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
    class ManageFacilities
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
                    // Get required services
                    var limitService = scope.ServiceProvider.GetRequiredService<MyLimitService>();
                    var context = scope.ServiceProvider.GetRequiredService<SupplyChainDbContext>();

                    // Main menu
                    bool exit = false;
                    while (!exit)
                    {
                        Console.Clear();
                        Console.WriteLine("===== Facility Management System =====");
                        Console.WriteLine("1. List all organizations");
                        Console.WriteLine("2. View organization facilities");
                        Console.WriteLine("3. Add new facility to organization");
                        Console.WriteLine("4. Modify existing facility");
                        Console.WriteLine("5. Allocate limit to counterparty");
                        Console.WriteLine("0. Exit");
                        Console.Write("Select option: ");
                        
                        string? choice = Console.ReadLine();
                        
                        switch (choice)
                        {
                            case "1":
                                ListAllOrganizations(context);
                                WaitForEnterKey();
                                break;
                            case "2":
                                ViewOrganizationFacilities(context, limitService);
                                WaitForEnterKey();
                                break;
                            case "3":
                                AddNewFacility(context, limitService);
                                WaitForEnterKey();
                                break;
                            case "4":
                                ModifyExistingFacility(context, limitService);
                                WaitForEnterKey();
                                break;
                            case "5":
                                AllocateLimitToCounterparty(context, limitService);
                                WaitForEnterKey();
                                break;
                            case "0":
                                exit = true;
                                break;
                            default:
                                Console.WriteLine("Invalid option. Press Enter to continue...");
                                Console.ReadLine();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                WaitForEnterKey();
            }
        }
        
        static void ListAllOrganizations(SupplyChainDbContext context)
        {
            var organizations = context.Organizations
                .OrderBy(o => o.Name)
                .ToList();
                
            Console.WriteLine("\n===== Organizations =====");
            Console.WriteLine("ID | Name | Type | Has Credit Limit");
            Console.WriteLine("--------------------------------");
            
            foreach (var org in organizations)
            {
                string orgType = "";
                if (org.IsBank) orgType = "Bank";
                else if (org.IsBuyer && org.IsSeller) orgType = "Buyer/Seller";
                else if (org.IsBuyer) orgType = "Buyer";
                else if (org.IsSeller) orgType = "Seller";
                
                bool hasCreditLimit = context.CreditLimits.Any(cl => cl.OrganizationId == org.Id);
                
                Console.WriteLine($"{org.Id} | {org.Name} | {orgType} | {(hasCreditLimit ? "Yes" : "No")}");
            }
        }
        
        static void ViewOrganizationFacilities(SupplyChainDbContext context, MyLimitService limitService)
        {
            Console.Write("\nEnter Organization ID: ");
            if (!int.TryParse(Console.ReadLine(), out int orgId))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }
            
            var organization = context.Organizations.Find(orgId);
            if (organization == null)
            {
                Console.WriteLine("Organization not found.");
                return;
            }
            
            try
            {
                var creditInfo = limitService.GetOrganizationCreditLimitInfo(orgId);
                
                Console.WriteLine($"\n===== Facilities for {organization.Name} =====");
                Console.WriteLine("ID | Type | Total Limit | Utilization | Available | Expires | Counterparty");
                Console.WriteLine("--------------------------------------------------------------");
                
                foreach (var facility in creditInfo.Facilities)
                {
                    string counterpartyName = "N/A";
                    if (facility.RelatedPartyId.HasValue)
                    {
                        var counterparty = context.Organizations.Find(facility.RelatedPartyId.Value);
                        if (counterparty != null)
                        {
                            counterpartyName = counterparty.Name;
                        }
                    }
                    
                    Console.WriteLine($"{facility.Id} | {facility.Type} | {facility.TotalLimit:C} | " +
                                    $"{facility.CurrentUtilization:C} | {facility.AvailableLimit:C} | " +
                                    $"{facility.ReviewEndDate.ToShortDateString()} | {counterpartyName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        static void AddNewFacility(SupplyChainDbContext context, MyLimitService limitService)
        {
            Console.Write("\nEnter Organization ID: ");
            if (!int.TryParse(Console.ReadLine(), out int orgId))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }
            
            var organization = context.Organizations.Find(orgId);
            if (organization == null)
            {
                Console.WriteLine("Organization not found.");
                return;
            }
            
            // Check if credit limit exists, create if not
            var creditLimit = context.CreditLimits.FirstOrDefault(cl => cl.OrganizationId == orgId);
            if (creditLimit == null)
            {
                Console.WriteLine("No credit limit record found. Creating new credit limit record.");
                creditLimit = new CreditLimitInfo
                {
                    OrganizationId = orgId,
                    Organization = organization,
                    LastReviewDate = DateTime.Now,
                    NextReviewDate = DateTime.Now.AddMonths(12)
                };
                context.CreditLimits.Add(creditLimit);
                context.SaveChanges();
            }
            
            // Select facility type
            Console.WriteLine("\nSelect Facility Type:");
            var facilityTypes = Enum.GetValues(typeof(FacilityType)).Cast<FacilityType>();
            int i = 1;
            foreach (var type in facilityTypes)
            {
                Console.WriteLine($"{i++}. {type}");
            }
            
            Console.Write("Enter selection: ");
            if (!int.TryParse(Console.ReadLine(), out int typeSelection) || 
                typeSelection < 1 || typeSelection > facilityTypes.Count())
            {
                Console.WriteLine("Invalid selection.");
                return;
            }
            
            FacilityType selectedType = facilityTypes.ElementAt(typeSelection - 1);
            
            // Get facility details
            Console.Write("Enter total limit amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal totalLimit) || totalLimit <= 0)
            {
                Console.WriteLine("Invalid amount.");
                return;
            }
            
            Console.Write("Enter expiry date (MM/DD/YYYY): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime expiryDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }
            
            Console.Write("Enter grace period days (default 5): ");
            string? gracePeriod = Console.ReadLine();
            int gracePeriodDays = 5;
            if (!string.IsNullOrEmpty(gracePeriod))
            {
                int.TryParse(gracePeriod, out gracePeriodDays);
            }
            
            // Create the new facility
            var facility = new Facility
            {
                CreditLimitInfoId = creditLimit.Id,
                Type = selectedType,
                TotalLimit = totalLimit,
                CurrentUtilization = 0,
                ReviewEndDate = expiryDate,
                GracePeriodDays = gracePeriodDays
            };
            
            // Check if this should be linked to a counterparty
            Console.Write("Link to a counterparty? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                ListPotentialCounterparties(context, organization);
                
                Console.Write("Enter counterparty ID: ");
                if (int.TryParse(Console.ReadLine(), out int counterpartyId))
                {
                    var counterparty = context.Organizations.Find(counterpartyId);
                    if (counterparty != null && counterparty.Id != orgId)
                    {
                        facility.RelatedPartyId = counterpartyId;
                        facility.RelatedParty = counterparty;
                        
                        Console.Write("Enter allocated limit for this counterparty: ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal allocatedLimit) &&
                            allocatedLimit > 0 && allocatedLimit <= totalLimit)
                        {
                            facility.AllocatedLimit = allocatedLimit;
                        }
                    }
                }
            }
            
            // Save changes
            context.Facilities.Add(facility);
            context.SaveChanges();
            Console.WriteLine("Facility added successfully.");
        }
        
        static void ModifyExistingFacility(SupplyChainDbContext context, MyLimitService limitService)
        {
            Console.Write("\nEnter Facility ID: ");
            if (!int.TryParse(Console.ReadLine(), out int facilityId))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }
            
            var facility = context.Facilities
                .Include(f => f.CreditLimitInfo)
                .Include(f => f.CreditLimitInfo.Organization)
                .FirstOrDefault(f => f.Id == facilityId);
                
            if (facility == null)
            {
                Console.WriteLine("Facility not found.");
                return;
            }
            
            var organization = facility.CreditLimitInfo.Organization;
            Console.WriteLine($"\nModifying {facility.Type} facility for {organization.Name}");
            Console.WriteLine($"Current total limit: {facility.TotalLimit:C}");
            Console.WriteLine($"Current utilization: {facility.CurrentUtilization:C}");
            Console.WriteLine($"Current expiry date: {facility.ReviewEndDate.ToShortDateString()}");
            Console.WriteLine($"Current grace period: {facility.GracePeriodDays} days");
            
            // Modify facility details
            Console.Write("Enter new total limit amount (or press Enter to keep current): ");
            string? newLimitStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(newLimitStr) && decimal.TryParse(newLimitStr, out decimal newLimit) && newLimit >= facility.CurrentUtilization)
            {
                facility.TotalLimit = newLimit;
            }
            
            Console.Write("Enter new expiry date (MM/DD/YYYY) (or press Enter to keep current): ");
            string? newExpiryStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(newExpiryStr) && DateTime.TryParse(newExpiryStr, out DateTime newExpiry))
            {
                facility.ReviewEndDate = newExpiry;
            }
            
            Console.Write("Enter new grace period days (or press Enter to keep current): ");
            string? newGraceStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(newGraceStr) && int.TryParse(newGraceStr, out int newGrace) && newGrace >= 0)
            {
                facility.GracePeriodDays = newGrace;
            }
            
            // Save changes
            context.SaveChanges();
            Console.WriteLine("Facility updated successfully.");
        }
        
        static void AllocateLimitToCounterparty(SupplyChainDbContext context, MyLimitService limitService)
        {
            Console.Write("\nEnter Organization ID: ");
            if (!int.TryParse(Console.ReadLine(), out int orgId))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }
            
            var organization = context.Organizations.Find(orgId);
            if (organization == null)
            {
                Console.WriteLine("Organization not found.");
                return;
            }
            
            try
            {
                // Get credit limit info
                var creditInfo = limitService.GetOrganizationCreditLimitInfo(orgId);
                
                // List facilities
                Console.WriteLine($"\n===== Facilities for {organization.Name} =====");
                var facilities = creditInfo.Facilities
                    .Where(f => !f.RelatedPartyId.HasValue) // Only show facilities not already tied to counterparties
                    .ToList();
                    
                if (!facilities.Any())
                {
                    Console.WriteLine("No available facilities for allocation.");
                    return;
                }
                
                Console.WriteLine("ID | Type | Total Limit | Available | Expires");
                Console.WriteLine("-------------------------------------------");
                foreach (var facility in facilities)
                {
                    Console.WriteLine($"{facility.Id} | {facility.Type} | {facility.TotalLimit:C} | " +
                                    $"{facility.AvailableLimit:C} | {facility.ReviewEndDate.ToShortDateString()}");
                }
                
                // Select facility to allocate from
                Console.Write("\nEnter Facility ID to allocate from: ");
                if (!int.TryParse(Console.ReadLine(), out int facilityId))
                {
                    Console.WriteLine("Invalid ID format.");
                    return;
                }
                
                var selectedFacility = facilities.FirstOrDefault(f => f.Id == facilityId);
                if (selectedFacility == null)
                {
                    Console.WriteLine("Invalid facility selection.");
                    return;
                }
                
                // Select counterparty
                ListPotentialCounterparties(context, organization);
                
                Console.Write("Enter counterparty ID: ");
                if (!int.TryParse(Console.ReadLine(), out int counterpartyId))
                {
                    Console.WriteLine("Invalid ID format.");
                    return;
                }
                
                var counterparty = context.Organizations.Find(counterpartyId);
                if (counterparty == null || counterparty.Id == organization.Id)
                {
                    Console.WriteLine("Invalid counterparty selection.");
                    return;
                }
                
                // Get allocation amount
                Console.Write($"Enter allocation amount (max {selectedFacility.AvailableLimit:C}): ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal allocatedAmount) || 
                    allocatedAmount <= 0 || allocatedAmount > selectedFacility.AvailableLimit)
                {
                    Console.WriteLine("Invalid amount.");
                    return;
                }
                
                // Create a new facility for the counterparty relationship
                var counterpartyFacility = new Facility
                {
                    CreditLimitInfoId = creditInfo.Id,
                    Type = selectedFacility.Type,
                    TotalLimit = allocatedAmount,
                    CurrentUtilization = 0,
                    ReviewEndDate = selectedFacility.ReviewEndDate,
                    GracePeriodDays = selectedFacility.GracePeriodDays,
                    RelatedPartyId = counterpartyId,
                    RelatedParty = counterparty,
                    AllocatedLimit = allocatedAmount
                };
                
                context.Facilities.Add(counterpartyFacility);
                selectedFacility.AllocatedLimit += allocatedAmount;
                
                context.SaveChanges();
                Console.WriteLine($"Successfully allocated {allocatedAmount:C} to {counterparty.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        static void ListPotentialCounterparties(SupplyChainDbContext context, Organization org)
        {
            IQueryable<Organization> query = context.Organizations.Where(o => o.Id != org.Id && !o.IsBank);
            
            // If the organization is a buyer, show sellers; if a seller, show buyers
            if (org.IsBuyer && !org.IsSeller)
            {
                query = query.Where(o => o.IsSeller);
            }
            else if (org.IsSeller && !org.IsBuyer)
            {
                query = query.Where(o => o.IsBuyer);
            }
            
            var counterparties = query.OrderBy(o => o.Name).ToList();
            
            Console.WriteLine("\n===== Potential Counterparties =====");
            Console.WriteLine("ID | Name | Type");
            Console.WriteLine("------------------");
            
            foreach (var counterparty in counterparties)
            {
                string orgType = "";
                if (counterparty.IsBuyer && counterparty.IsSeller) orgType = "Buyer/Seller";
                else if (counterparty.IsBuyer) orgType = "Buyer";
                else if (counterparty.IsSeller) orgType = "Seller";
                
                Console.WriteLine($"{counterparty.Id} | {counterparty.Name} | {orgType}");
            }
        }
        
        static void WaitForEnterKey()
        {
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }
}
