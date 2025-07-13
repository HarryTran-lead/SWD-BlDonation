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
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

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
                                    rm.MatchStatus == "Pending");

                            if (!existingMatch)
                            {
                                var match = new RequestMatch
                                {
                                    BloodRequestId = bloodRequest.BloodRequestId,
                                    DonationRequestId = donation.DonateRequestId,
                                    MatchStatus = "Pending",
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

            _logger.LogInformation("Processing completed DonationRequests.");

            var completedDonations = await context.DonationRequests
                .Where(dr => dr.Status == 2)
                .ToListAsync();

            foreach (var donation in completedDonations)
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var hasMatches = await context.RequestMatches
                        .AnyAsync(rm => rm.DonationRequestId == donation.DonateRequestId);

                    var inventoryUpdated = await context.BloodInventories
                        .AnyAsync(inv =>
                            inv.BloodTypeId == donation.BloodTypeId &&
                            inv.BloodComponentId == donation.BloodComponentId);

                    if (hasMatches || inventoryUpdated)
                    {
                        _logger.LogInformation("DonationRequest {DonationRequestId} already processed.", donation.DonateRequestId);
                        continue;
                    }

                    var inventory = await context.BloodInventories
                        .FirstOrDefaultAsync(inv =>
                            inv.BloodTypeId == donation.BloodTypeId &&
                            inv.BloodComponentId == donation.BloodComponentId);

                    int remainingQuantity = donation.Quantity.Value;

                    if (inventory == null)
                    {
                        inventory = new BloodInventory
                        {
                            BloodTypeId = donation.BloodTypeId,
                            BloodComponentId = donation.BloodComponentId,
                            Quantity = remainingQuantity,
                            Unit = "mL",
                            LastUpdated = DateTime.UtcNow,
                            InventoryLocation = "Default Location"
                        };
                        context.BloodInventories.Add(inventory);
                        _logger.LogInformation(
                            "Created new BloodInventory for BloodTypeId={BloodTypeId}, BloodComponentId={BloodComponentId}, Quantity={Quantity}",
                            inventory.BloodTypeId, inventory.BloodComponentId, inventory.Quantity);
                    }
                    else
                    {
                        inventory.Quantity += remainingQuantity;
                        inventory.LastUpdated = DateTime.UtcNow;
                        context.Entry(inventory).State = EntityState.Modified;
                    }

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
                                MatchStatus = "Completed",
                                ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow),
                                Notes = "Fulfilled by completed donation",
                                Type = "Auto"
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
                                _logger.LogInformation("Fulfillment notification created for userId={UserId} for blood request id={BloodRequestId}", bloodRequest.UserId, bloodRequest.BloodRequestId);
                            }

                            context.Entry(bloodRequest).State = EntityState.Modified;

                            remainingQuantity -= bloodRequest.Quantity.Value;
                            inventory.Quantity -= bloodRequest.Quantity.Value;
                            context.Entry(inventory).State = EntityState.Modified;

                            _logger.LogInformation("Blood request fulfilled from donation: bloodRequestId={BloodRequestId}, donationRequestId={DonationRequestId}, quantity={Quantity}",
                                bloodRequest.BloodRequestId, donation.DonateRequestId, bloodRequest.Quantity);
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error processing DonationRequest {DonationRequestId}", donation.DonateRequestId);
                }
            }

            _logger.LogInformation("Finished processing {Count} completed DonationRequests.", completedDonations.Count);
        }
    }
}