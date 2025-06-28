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
                .Where(br => br.Status == (byte)BloodRequestStatus.Pending && br.Fulfilled == false)
                .ToListAsync();

            foreach (var bloodRequest in pendingRequests)
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var matchedInventory = await context.BloodInventories
                        .FirstOrDefaultAsync(inv =>
                            inv.BloodTypeId == bloodRequest.BloodTypeId &&
                            inv.BloodComponentId == bloodRequest.BloodComponentId &&
                            inv.Quantity >= bloodRequest.Quantity);

                    if (matchedInventory != null)
                    {
                        matchedInventory.Quantity -= bloodRequest.Quantity;
                        bloodRequest.Fulfilled = true;
                        bloodRequest.FulfilledSource = "Inventory";

                        context.Entry(matchedInventory).State = EntityState.Modified;
                        context.Entry(bloodRequest).State = EntityState.Modified;
                    }
                    else
                    {
                        var potentialDonations = await context.DonationRequests
                            .Where(dr =>
                                dr.BloodTypeId == bloodRequest.BloodTypeId &&
                                dr.BloodComponentId == bloodRequest.BloodComponentId &&
                                dr.Status == "Available")
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
                                    Type = "Auto"
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

            // Retrieve completed DonationRequests
            var completedDonations = await context.DonationRequests
                .Where(dr => dr.Status == "Completed")
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
                        continue;
                    }

                    var pendingRequests = await context.BloodRequests
                        .Where(br =>
                            br.BloodTypeId == donation.BloodTypeId &&
                            br.BloodComponentId == donation.BloodComponentId &&
                            br.Status == (byte)BloodRequestStatus.Pending &&
                            br.Fulfilled == false)
                        .ToListAsync();

                    bool usedForRequest = false;
                    int remainingQuantity = donation.Quantity.Value;

                    foreach (var bloodRequest in pendingRequests)
                    {
                        if (remainingQuantity >= bloodRequest.Quantity)
                        {
                            bloodRequest.Fulfilled = true;
                            bloodRequest.FulfilledSource = "Donation";

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
                            context.Entry(bloodRequest).State = EntityState.Modified;

                            remainingQuantity -= bloodRequest.Quantity.Value;
                            usedForRequest = true;
                        }
                    }

                    if (remainingQuantity > 0)
                    {
                        var inventory = await context.BloodInventories
                            .FirstOrDefaultAsync(inv =>
                                inv.BloodTypeId == donation.BloodTypeId &&
                                inv.BloodComponentId == donation.BloodComponentId);

                        if (inventory == null)
                        {
                            inventory = new BloodInventory
                            {
                                BloodTypeId = donation.BloodTypeId,
                                BloodComponentId = donation.BloodComponentId,
                                Quantity = remainingQuantity,
                                LastUpdated = DateTime.UtcNow
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
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                }
            }

            _logger.LogInformation("Finished processing {Count} completed DonationRequests.", completedDonations.Count);
        }
    }
}