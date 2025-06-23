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
    class CreateSellerCustomer
    {
        // Move the organization variable to a broader scope
        private static Organization organization = new Organization { Id = 1, Name = "Default Organization" };

        static void Main(string[] args)
        {
            Console.WriteLine("NOTICE: Organization creation is now restricted to BankPortal only.");
            Console.WriteLine("Starting BankPortal for organization creation...\n");

            // Initialize service provider
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<SupplyChainDbContext>(options => options.UseSqlite("Data Source=supply_chain_finance.db"));
            serviceCollection.AddTransient<MyLimitService>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Launch BankPortal application
            try
            {
                Console.WriteLine("Please use the BankPortal application to create a new Seller organization.");
                Console.WriteLine("Instructions:");
                Console.WriteLine("1. Log in to the BankPortal");
                Console.WriteLine("2. Select 'Manage Organizations' from the main menu");
                Console.WriteLine("3. Select 'Add New Organization'");
                Console.WriteLine("4. Follow the prompts to create a new Seller organization");
                Console.WriteLine("5. Ensure you select 'Seller' as the role\n");

                Console.WriteLine("Press Enter to launch BankPortal, or Ctrl+C to cancel...");
                Console.ReadLine();

                // Start the BankPortal application
                // In a real implementation, we'd launch the actual executable
                Process.Start(new ProcessStartInfo { 
                    FileName = "dotnet", 
                    Arguments = "run --project BankPortal/BankPortal.csproj",
                    UseShellExecute = true
                });
                
                Console.WriteLine("\nBankPortal has been launched. Please follow the instructions to create the seller organization.");
                Console.WriteLine("Once you've created the organization in BankPortal, you can return here to continue.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error launching BankPortal: {ex.Message}");
            }

            ShowExampleCode(serviceProvider);
        }

        static void ShowExampleCode(ServiceProvider serviceProvider)
        {
            Console.WriteLine("\nAfter you've created the seller organization in BankPortal, you can use the following");
            Console.WriteLine("code as a reference for setting up credit limits and facilities:");
            
            Console.WriteLine(@"
// Example code for setting up credit limits and facilities
// Replace 'sellerId' with the actual seller ID you created in BankPortal

using (var scope = serviceProvider.CreateScope())
{
    var limitService = scope.ServiceProvider.GetRequiredService<MyLimitService>();
    
    // Create credit limit for the seller (bank customer)
    var creditLimit = new CreditLimitInfo
    {
        OrganizationId = sellerId,
        MasterLimit = 500000 // $500,000 master limit
    };
    
    // Create facilities for seller
    var facilities = new List<Facility>
    {
        new Facility
        {
            Type = FacilityType.InvoiceFinancing,
            TotalLimit = 300000, // $300,000 for invoice financing
            ReviewEndDate = DateTime.Now.AddYears(1),
            GracePeriodDays = 10
        },
        new Facility
        {
            Type = FacilityType.TermLoan,
            TotalLimit = 100000, // $100,000 term loan
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

                    // Call facility creation logic
                    AddFacilitiesToOrganization(context, limitService, organization.Id);
                }
            }

        static void AddFacilitiesToOrganization(SupplyChainDbContext context, MyLimitService limitService, int organizationId)
        {
            Console.WriteLine("\nAdding facilities to the organization...");

            // Facility creation logic (similar to ManageFacilities.cs)
            Console.Write("Enter facility type (1: Invoice Financing, 2: Term Loan): ");
            if (!int.TryParse(Console.ReadLine(), out int facilityTypeChoice) || facilityTypeChoice < 1 || facilityTypeChoice > 2)
            {
                Console.WriteLine("Invalid facility type.");
                return;
            }

            FacilityType facilityType = facilityTypeChoice == 1 ? FacilityType.InvoiceFinancing : FacilityType.TermLoan;

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

            Console.WriteLine("Debug: Adding facility to database...");
            context.Facilities.Add(facility);
            context.SaveChanges();
            Console.WriteLine("Debug: Facility added successfully with ID: " + facility.Id);

            Console.WriteLine("Facility added successfully.");
        }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
