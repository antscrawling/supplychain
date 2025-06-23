using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Data;
using Core.Models;

namespace SupplyChainFinance
{
    class QueryDatabase
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

                    // Query organizations
                    Console.WriteLine("===== Organizations =====");
                    var orgs = context.Organizations.ToList();
                    foreach (var org in orgs)
                    {
                        Console.WriteLine($"ID: {org.Id}, Name: {org.Name}, IsBuyer: {org.IsBuyer}, IsSeller: {org.IsSeller}");
                    }
                    
                    // Query credit limits
                    Console.WriteLine("\n===== Credit Limits =====");
                    var limits = context.CreditLimits
                        .Include(cl => cl.Organization)
                        .ToList();
                    
                    foreach (var limit in limits)
                    {
                        Console.WriteLine($"ID: {limit.Id}, OrgID: {limit.OrganizationId}, OrgName: {limit.Organization?.Name}, MasterLimit: {limit.MasterLimit:N2}");
                    }
                    
                    // Query facilities
                    Console.WriteLine("\n===== Facilities =====");
                    var facilities = context.Facilities
                        .Include(f => f.CreditLimitInfo)
                            .ThenInclude(cl => cl!.Organization)
                        .ToList();
                    
                    foreach (var facility in facilities)
                    {
                        Console.WriteLine($"ID: {facility.Id}, Type: {facility.Type}, Limit: {facility.TotalLimit:N2}, " +
                            $"CreditLimitID: {facility.CreditLimitInfoId}, " +
                            $"OrgID: {facility.CreditLimitInfo?.OrganizationId}, " +
                            $"OrgName: {facility.CreditLimitInfo?.Organization?.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
