using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Data;
using Core.Models;

namespace SupplyChainFinance
{
    class TestFacilityInsertion
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

            var serviceProvider = services.BuildServiceProvider();

            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<SupplyChainDbContext>();

                    // Create a test facility
                    var facility = new Facility
                    {
                        CreditLimitInfoId = 1, // Assuming CreditLimitInfoId 1 exists
                        Type = FacilityType.InvoiceFinancing,
                        TotalLimit = 100000,
                        ReviewEndDate = DateTime.Now.AddYears(1),
                        GracePeriodDays = 5
                    };

                    context.Facilities.Add(facility);
                    context.SaveChanges();

                    Console.WriteLine("Facility added successfully with ID: " + facility.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
