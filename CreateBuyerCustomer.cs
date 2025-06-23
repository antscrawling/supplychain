using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Data;
using Core.Services;
using Core.Models;
using System.Diagnostics;

namespace SupplyChainFinance
{
    class CreateBuyerCustomer
    {
        static void Main(string[] args)
        {
            Console.WriteLine("NOTICE: Organization creation is now restricted to BankPortal only.");
            Console.WriteLine("Starting BankPortal for organization creation...\n");

            // Launch BankPortal application
            try
            {
                Console.WriteLine("Please use the BankPortal application to create a new Buyer organization.");
                Console.WriteLine("Instructions:");
                Console.WriteLine("1. Log in to the BankPortal");
                Console.WriteLine("2. Select 'Manage Organizations' from the main menu");
                Console.WriteLine("3. Select 'Add New Organization'");
                Console.WriteLine("4. Follow the prompts to create a new Buyer organization");
                Console.WriteLine("5. Ensure you select 'Buyer' as the role\n");

                Console.WriteLine("Press Enter to launch BankPortal, or Ctrl+C to cancel...");
                Console.ReadLine();

                // Start the BankPortal application
                ProcessStartInfo psi = new ProcessStartInfo { 
                    FileName = "dotnet", 
                    Arguments = "run --project BankPortal/BankPortal.csproj",
                    UseShellExecute = true
                };
                Process.Start(psi);
                
                Console.WriteLine("\nBankPortal has been launched. Please follow the instructions to create the buyer organization.");
                Console.WriteLine("Once you've created the organization in BankPortal, you can return here to continue.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error launching BankPortal: {ex.Message}");
            }

            // Configure services for reference only (not for organization creation)
            var services = new ServiceCollection();
            string dbFilePath = "supply_chain_finance.db";
            services.AddDbContext<SupplyChainDbContext>(options =>
                options.UseSqlite($"Data Source={dbFilePath}"));
            services.AddScoped<MyLimitService>();
            services.AddScoped<InvoiceService>();
            var serviceProvider = services.BuildServiceProvider();
            
            ShowExampleCode(serviceProvider);
        }
        
        static void ShowExampleCode(IServiceProvider serviceProvider)
        {
            Console.WriteLine("\nAfter you've created the buyer organization in BankPortal, you can use the following");
            Console.WriteLine("code as a reference for setting up credit limits and facilities:");
            
            Console.WriteLine(@"
// Example code for setting up credit limits and facilities
// Replace 'buyerId' with the actual buyer ID you created in BankPortal

using (var scope = serviceProvider.CreateScope())
{
    var limitService = scope.ServiceProvider.GetRequiredService<MyLimitService>();
    
    // Create credit limit for the buyer (bank customer)
    var creditLimit = new CreditLimitInfo
    {
        OrganizationId = buyerId,
        MasterLimit = 750000 // $750,000 master limit
    };
    
    // Create facilities for buyer
    var facilities = new List<Facility>
    {
        new Facility
        {
            Type = FacilityType.InvoiceFinancing,
            TotalLimit = 400000, // $400,000 for invoice financing
            ReviewEndDate = DateTime.Now.AddYears(1),
            GracePeriodDays = 10
        },
        new Facility
        {
            Type = FacilityType.Guarantee,
            TotalLimit = 100000, // $100,000 guarantee
            ReviewEndDate = DateTime.Now.AddYears(2),
            GracePeriodDays = 5
        }
    };
    
    limitService.CreateCreditLimitWithFacilities(creditLimit, facilities);
}");

            // Prompt to add facilities after organization creation
            Console.Write("\nAdd credit facilities to this organization now? (Y/N): ");
            if (Console.ReadLine()?.Trim().ToUpper() == "Y")
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var limitService = scope.ServiceProvider.GetRequiredService<MyLimitService>();
                    var context = scope.ServiceProvider.GetRequiredService<SupplyChainDbContext>();

                    // Fetch the organization using buyerId
                    Console.Write("Enter the Buyer ID: ");
                    if (!int.TryParse(Console.ReadLine(), out int buyerId))
                    {
                        Console.WriteLine("Invalid Buyer ID.");
                        return;
                    }

                    var organization = context.Organizations.FirstOrDefault(o => o.Id == buyerId);
                    if (organization == null)
                    {
                        Console.WriteLine("Organization not found. Please ensure the buyer organization exists in the database.");
                        return;
                    }

                    // Call facility creation logic
                    AddFacilitiesToOrganization(context, limitService, organization.Id);
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();

        }

        static void AddFacilitiesToOrganization(SupplyChainDbContext context, MyLimitService limitService, int organizationId)
        {
            Console.WriteLine("\nAdding facilities to the organization...");

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
                CreditLimitInfoId = organizationId, // Assuming organizationId maps to CreditLimitInfoId
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
