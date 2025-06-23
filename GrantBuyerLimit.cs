using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Data;
using Core.Services;
using Core.Models;

namespace SupplyChainFinance
{
    class GrantBuyerLimit
    {
        static void Main(string[] args)
        {
            // Configure services
            var services = new ServiceCollection();

            // Database file path
            string dbFilePath = "supply_chain_finance.db";

            // Register database context
            services.AddDbContext<SupplyChainDbContext>(options =>
                options.UseSqlite($"Data Source={dbFilePath}"));

            // Register services
            services.AddScoped<MyLimitService>();

            var serviceProvider = services.BuildServiceProvider();

            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    // Get service
                    var limitService = scope.ServiceProvider.GetRequiredService<MyLimitService>();

                    // Get context
                    var context = scope.ServiceProvider.GetRequiredService<SupplyChainDbContext>();

                    // Get the seller organization (MegaCorp Industries, ID: 2)
                    int sellerId = 2;
                    var seller = context.Organizations.Find(sellerId);
                    if (seller == null)
                    {
                        Console.WriteLine("Seller not found!");
                        return;
                    }

                    // Find the buyer from transaction
                    var transaction = context.Transactions
                        .Include(t => t.Invoice)
                        .Where(t => t.Description.Contains("uploaded by buyer") && t.Invoice.SellerId == sellerId)
                        .FirstOrDefault();

                    if (transaction == null)
                    {
                        Console.WriteLine("No buyer transaction found!");
                        return;
                    }

                    int buyerId = transaction.Invoice.BuyerId ?? 0;
                    if (buyerId == 0)
                    {
                        Console.WriteLine("Invalid buyer ID!");
                        return;
                    }

                    var buyer = context.Organizations.Find(buyerId);
                    if (buyer == null)
                    {
                        Console.WriteLine("Buyer not found!");
                        return;
                    }

                    // Grant $250,000 limit
                    decimal limitAmount = 250000;
                    var result = limitService.AllocateBuyerLimit(sellerId, buyerId, FacilityType.InvoiceFinancing, limitAmount);

                    // Show result
                    Console.WriteLine(result.Success ? "Success!" : "Failed!");
                    Console.WriteLine(result.Message);

                    // Prompt to add facilities after granting buyer limit
                    Console.Write("\nAdd additional facilities to this buyer? (Y/N): ");
                    if (Console.ReadLine()?.Trim().ToUpper() == "Y")
                    {
                        AddFacilitiesToBuyer(context, limitService, buyerId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        static void AddFacilitiesToBuyer(SupplyChainDbContext context, MyLimitService limitService, int buyerId)
        {
            Console.WriteLine("\nAdding facilities to the buyer...");

            // Facility creation logic (similar to ManageFacilities.cs)
            Console.Write("Enter facility type (1: Invoice Financing, 2: Guarantee): ");
            if (!int.TryParse(Console.ReadLine(), out int facilityTypeChoice) || facilityTypeChoice < 1 || facilityTypeChoice > 2)
            {
                Console.WriteLine("Invalid facility type.");
                return;
            }

            FacilityType facilityType = facilityTypeChoice == 1 ? FacilityType.InvoiceFinancing : FacilityType.Guarantee;

            Console.Write("Enter facility limit: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal facilityLimit) || facilityLimit <= 0)
            {
                Console.WriteLine("Invalid facility limit.");
                return;
            }

            Console.Write("Enter review end date (yyyy-MM-dd): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime reviewEndDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            Console.Write("Enter grace period days (default 5): ");
            string gracePeriodInput = Console.ReadLine() ?? "";
            int gracePeriodDays = 5;
            if (!string.IsNullOrWhiteSpace(gracePeriodInput) && !int.TryParse(gracePeriodInput, out gracePeriodDays))
            {
                Console.WriteLine("Invalid grace period. Using default of 5 days.");
                gracePeriodDays = 5;
            }

            // Save facility to database
            var facility = new Facility
            {
                CreditLimitInfoId = buyerId, // Assuming buyerId maps to CreditLimitInfoId
                Type = facilityType,
                TotalLimit = facilityLimit,
                ReviewEndDate = reviewEndDate,
                GracePeriodDays = gracePeriodDays
            };

            context.Facilities.Add(facility);
            context.SaveChanges();

            Console.WriteLine("Facility added successfully.");
        }
    }
}
