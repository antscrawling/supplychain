using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Data;
using Core.Models;

namespace SupplyChainFinance
{
    class CheckMiloFacility
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
                Console.WriteLine("Checking database for Milo Buyer Corporation facilities...");
                
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<SupplyChainDbContext>();

                    // Check for Milo Buyer Corporation
                    Console.WriteLine("\n===== Organization Info =====");
                    var miloBuyer = context.Organizations
                        .FirstOrDefault(o => o.Name.Contains("Milo") && o.IsBuyer);
                    
                    if (miloBuyer != null)
                    {
                        Console.WriteLine($"Found Milo Buyer Corporation: ID={miloBuyer.Id}, IsBuyer={miloBuyer.IsBuyer}, IsSeller={miloBuyer.IsSeller}");
                    }
                    else
                    {
                        Console.WriteLine("Milo Buyer Corporation not found in the database!");
                        
                        // Get all organizations
                        var orgs = context.Organizations.ToList();
                        Console.WriteLine("Available organizations:");
                        foreach (var org in orgs)
                        {
                            Console.WriteLine($"ID: {org.Id}, Name: {org.Name}, IsBuyer: {org.IsBuyer}, IsSeller: {org.IsSeller}, IsBank: {org.IsBank}");
                        }
                        return;
                    }
                    
                    // Check credit limits
                    Console.WriteLine("\n===== Credit Limit Info =====");
                    var creditLimit = context.CreditLimits
                        .Include(cl => cl.Organization)
                        .FirstOrDefault(cl => cl.OrganizationId == miloBuyer.Id);
                    
                    if (creditLimit != null)
                    {
                        Console.WriteLine($"Found Credit Limit: ID={creditLimit.Id}, MasterLimit={creditLimit.MasterLimit:N2}");
                    }
                    else
                    {
                        Console.WriteLine("No credit limit found for Milo Buyer Corporation!");
                    }
                    
                    // Check facilities
                    Console.WriteLine("\n===== Facilities =====");
                    var facilities = context.Facilities
                        .Include(f => f.CreditLimitInfo)
                        .Where(f => f.CreditLimitInfo != null && f.CreditLimitInfo.OrganizationId == miloBuyer.Id)
                        .ToList();
                    
                    if (facilities.Any())
                    {
                        Console.WriteLine($"Found {facilities.Count} facilities for Milo Buyer Corporation:");
                        foreach (var facility in facilities)
                        {
                            Console.WriteLine($"ID: {facility.Id}, Type: {facility.Type}, Limit: {facility.TotalLimit:N2}, " +
                                $"CreditLimitID: {facility.CreditLimitInfoId}, ReviewEndDate: {facility.ReviewEndDate.ToShortDateString()}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No facilities found for Milo Buyer Corporation!");
                        
                        // Check if there's any type of facility linked to any organization
                        var allFacilities = context.Facilities
                            .Include(f => f.CreditLimitInfo)
                                .ThenInclude(cl => cl!.Organization)
                            .ToList();
                                
                        Console.WriteLine($"\nTotal facilities in database: {allFacilities.Count}");
                        foreach (var facility in allFacilities)
                        {
                            Console.WriteLine($"Facility ID {facility.Id}, Type {facility.Type}, Limit {facility.TotalLimit:N2}, " +
                                $"CreditLimitInfoId {facility.CreditLimitInfoId}, " +
                                $"OrgID: {facility.CreditLimitInfo?.OrganizationId}, " +
                                $"OrgName: {facility.CreditLimitInfo?.Organization?.Name}");
                        }
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
