using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SWD_BLDONATION.Models.Generated;
using SWD_BLDONATION.Models.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SWD_BLDONATION.Provider;

namespace SWD_BLDONATION.Services
{
    public class BloodRequestFulfillmentService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BloodRequestFulfillmentService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

        public BloodRequestFulfillmentService(
            IServiceProvider serviceProvider,
            ILogger<BloodRequestFulfillmentService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BloodRequestFulfillmentService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingBloodRequests();
                    await ProcessCompletedDonationRequests();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during periodic blood request fulfillment.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("BloodRequestFulfillmentService stopped.");
        }

        private async Task ProcessPendingBloodRequests()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BloodDonationDbContext>();

            _logger.LogInformation("Processing pending BloodRequests.");

            var pendingRequests = await context.BloodRequests
                .Where(br => br.Status == (byte)BloodRequestStatus.Successful && br.Fulfilled == false)
                .ToListAsync();

            foreach (var bloodRequest in pendingRequests)
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var allInventories = await context.BloodInventories
                        .Where(inv =>
                            inv.BloodTypeId == bloodRequest.BloodTypeId &&
                            inv.BloodComponentId == bloodRequest.BloodComponentId &&
                            inv.Quantity >= bloodRequest.Quantity)
                        .ToListAsync();

                    BloodInventory? matchedInventory = null;

                    if (!string.IsNullOrWhiteSpace(bloodRequest.Location))
                    {
                        var requestLocationParts = bloodRequest.Location.ToLower().Split('_').Select(p => p.Trim()).ToList();

                        matchedInventory = allInventories.FirstOrDefault(inv =>
                            requestLocationParts.Any(part =>
                                inv.InventoryLocation != null &&
                                inv.InventoryLocation.ToLower().Contains(part)));
                    }

                    if (matchedInventory == null)
                    {
                        matchedInventory = allInventories.FirstOrDefault();
                    }

                    if (matchedInventory != null)
                    {
                        matchedInventory.Quantity -= bloodRequest.Quantity;
                        matchedInventory.LastUpdated = DateTime.UtcNow;
                        bloodRequest.Fulfilled = true;
                        bloodRequest.FulfilledSource = "Inventory";

                        var bloodRequestInventory = new BloodRequestInventory
                        {
                            BloodRequestId = bloodRequest.BloodRequestId,
                            InventoryId = matchedInventory.InventoryId,
                            QuantityUnit = bloodRequest.Quantity,
                            QuantityAllocated = bloodRequest.Quantity,
                            AllocatedAt = DateTime.UtcNow,
                            AllocatedBy = null
                        };
                        context.BloodRequestInventories.Add(bloodRequestInventory);

                        if (bloodRequest.UserId.HasValue)
                        {
                            var notification = new Notification
                            {
                                UserId = bloodRequest.UserId.Value,
                                Message = "Your request has been fulfilled. Please wait for our staff to call you to confirm the appointment.",
                                Type = "BloodRequest",
                                Status = "Unread",
                                SentAt = VietnamDateTimeProvider.Now
                            };
                            context.Notifications.Add(notification);
                        }

                        context.Entry(matchedInventory).State = EntityState.Modified;
                        context.Entry(bloodRequest).State = EntityState.Modified;

                        _logger.LogInformation("Blood request fulfilled from inventory: id={Id}, inventoryId={InventoryId}, quantity={Quantity}",
                            bloodRequest.BloodRequestId, matchedInventory.InventoryId, bloodRequest.Quantity);
                    }
                    else
                    {
                        var potentialDonations = await context.DonationRequests
                            .Where(dr =>
                                dr.BloodTypeId == bloodRequest.BloodTypeId &&
                                dr.BloodComponentId == bloodRequest.BloodComponentId &&
                                dr.Status == 1)
                            .ToListAsync();

                        foreach (var donation in potentialDonations)
                        {
                            var existingMatch = await context.RequestMatches
                                .AnyAsync(rm =>
                                    rm.BloodRequestId == bloodRequest.BloodRequestId &&
                                    rm.DonationRequestId == donation.DonateRequestId &&
                                    rm.MatchStatus == "pending");

                            if (!existingMatch)
                            {
                                var match = new RequestMatch
                                {
                                    BloodRequestId = bloodRequest.BloodRequestId,
                                    DonationRequestId = donation.DonateRequestId,
                                    MatchStatus = "pending",
                                    ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                                    Notes = "Auto-matched by periodic job",
                                    Type = "donation_to_request"
                                };
                                context.RequestMatches.Add(match);
                            }
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error processing BloodRequest {BloodRequestId}", bloodRequest.BloodRequestId);
                }
            }

            _logger.LogInformation("Finished processing {Count} pending BloodRequests.", pendingRequests.Count);
        }

        private async Task ProcessCompletedDonationRequests()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BloodDonationDbContext>();

            _logger.LogInformation("Processing completed DonationRequests...");

            var completedDonations = await context.DonationRequests
                .Where(dr => dr.Status == 3)
                .ToListAsync();

            _logger.LogInformation("Processing completed DonationRequests..." + completedDonations.Count);

            foreach (var donation in completedDonations)
            {
                _logger.LogInformation("Start completed DonationRequests...");
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    if (!donation.Quantity.HasValue)
                    {
                        continue;
                    }

                    int remainingQuantity = donation.Quantity.Value;

                    var inventory = await context.BloodInventories
                 .FirstOrDefaultAsync(inv =>
                     inv.BloodTypeId == donation.BloodTypeId &&
                     inv.BloodComponentId == donation.BloodComponentId &&
                     inv.InventoryLocation == donation.Location);
                    if (inventory == null)
                    {
                        inventory = new BloodInventory
                        {
                            BloodTypeId = donation.BloodTypeId,
                            BloodComponentId = donation.BloodComponentId,
                            Quantity = remainingQuantity,
                            Unit = "ml",
                            LastUpdated = VietnamDateTimeProvider.Now,
                            InventoryLocation = donation.Location
                        };
                        context.BloodInventories.Add(inventory);
                        await context.SaveChangesAsync();
                        _logger.LogInformation("Created new inventory entry for blood type={BloodTypeId}, component={BloodComponentId}, quantity={Quantity}",
                            donation.BloodTypeId, donation.BloodComponentId, remainingQuantity);
                    }
                    else
                    {
                        Console.WriteLine("Inventory: " + inventory.InventoryId);
                        inventory.Quantity += remainingQuantity;
                        inventory.LastUpdated = VietnamDateTimeProvider.Now;
                        context.Entry(inventory).State = EntityState.Modified;
                        _logger.LogInformation("Updated inventory for blood type={BloodTypeId}, component={BloodComponentId}, added quantity={Quantity}",
                            donation.BloodTypeId, donation.BloodComponentId, remainingQuantity);
                    }

                    // Chuyển trạng thái donation sang Stocked (4)
                    donation.Status = 4;
                    context.Entry(donation).State = EntityState.Modified;

                    // Fulfill blood requests
                    var pendingRequests = await context.BloodRequests
                        .Where(br =>
                            br.BloodTypeId == donation.BloodTypeId &&
                            br.BloodComponentId == donation.BloodComponentId &&
                            br.Status == (byte)BloodRequestStatus.Successful &&
                            br.Fulfilled == false)
                        .ToListAsync();

                    foreach (var bloodRequest in pendingRequests)
                    {
                        if (remainingQuantity >= bloodRequest.Quantity)
                        {
                            bloodRequest.Fulfilled = true;
                            bloodRequest.FulfilledSource = "Donation";

                            var bloodRequestInventory = new BloodRequestInventory
                            {
                                BloodRequestId = bloodRequest.BloodRequestId,
                                InventoryId = inventory.InventoryId,
                                QuantityUnit = bloodRequest.Quantity,
                                QuantityAllocated = bloodRequest.Quantity,
                                AllocatedAt = DateTime.UtcNow,
                                AllocatedBy = null
                            };
                            context.BloodRequestInventories.Add(bloodRequestInventory);

                            var match = new RequestMatch
                            {
                                BloodRequestId = bloodRequest.BloodRequestId,
                                DonationRequestId = donation.DonateRequestId,
                                MatchStatus = "matched",
                                ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow),
                                Notes = "Fulfilled by completed donation",
                                Type = "donation_to_request"
                            };
                            context.RequestMatches.Add(match);

                            if (bloodRequest.UserId.HasValue)
                            {
                                var notification = new Notification
                                {
                                    UserId = bloodRequest.UserId.Value,
                                    Message = "Your request has been fulfilled. Please wait for our staff to call you to confirm the appointment.",
                                    Type = "BloodRequest",
                                    Status = "Unread",
                                    SentAt = VietnamDateTimeProvider.Now
                                };
                                context.Notifications.Add(notification);
                                _logger.LogInformation("Notification sent for fulfilled blood request ID={BloodRequestId} to user ID={UserId}",
                                    bloodRequest.BloodRequestId, bloodRequest.UserId);
                            }

                            inventory.Quantity -= bloodRequest.Quantity.Value;
                            remainingQuantity -= bloodRequest.Quantity.Value;

                            context.Entry(bloodRequest).State = EntityState.Modified;
                            context.Entry(inventory).State = EntityState.Modified;
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    try
                    {
                        await transaction.RollbackAsync();
                    }
                    catch (Exception rollbackEx)
                    {
                    }
                }
            }

            _logger.LogInformation("Finished processing {Count} completed donation requests.", completedDonations.Count);
        }
    }
}
