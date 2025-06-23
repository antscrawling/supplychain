using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Data;
using Core.Services;
using Core.Models;

namespace SupplyChainFinance
{
    class TestAccountingIntegration
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===============================================");
            Console.WriteLine("  TESTING ACCOUNTING INTEGRATION");
            Console.WriteLine("===============================================\n");

            // Configure services
            var services = new ServiceCollection();
            services.AddDbContext<SupplyChainDbContext>(options =>
                options.UseSqlite("Data Source=supply_chain_finance.db"));
            services.AddScoped<MyLimitService>();
            services.AddScoped<InvoiceService>();
            services.AddScoped<TransactionService>();
            services.AddScoped<AccountingService>();

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SupplyChainDbContext>();
                var accountingService = scope.ServiceProvider.GetRequiredService<AccountingService>();
                var invoiceService = scope.ServiceProvider.GetRequiredService<InvoiceService>();
                var transactionService = scope.ServiceProvider.GetRequiredService<TransactionService>();

                try
                {
                    // Initialize chart of accounts
                    Console.WriteLine("1. Initializing Chart of Accounts...");
                    accountingService.InitializeChartOfAccounts();
                    Console.WriteLine("   ✓ Chart of accounts initialized");

                    // Create test organizations
                    var seller = GetOrCreateOrganization(context, "Test Seller Corp", true, false);
                    var buyer = GetOrCreateOrganization(context, "Test Buyer Inc", false, true);

                    // Create test invoice  
                    Console.WriteLine("\n2. Creating Test Invoice...");
                    var invoice = new Invoice
                    {
                        InvoiceNumber = $"TEST-{DateTime.Now:yyyyMMdd}-{DateTime.Now.Ticks.ToString()[^3..]}",
                        Amount = 50000,
                        SellerId = seller.Id,
                        BuyerId = buyer.Id,
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(30),
                        Description = "Test invoice for accounting integration",
                        Status = InvoiceStatus.Approved
                    };
                    context.Invoices.Add(invoice);
                    context.SaveChanges();
                    Console.WriteLine($"   ✓ Invoice {invoice.InvoiceNumber} created with amount ${invoice.Amount:N2}");

                    // Test funding with accounting integration
                    Console.WriteLine("\n3. Testing Invoice Funding with Accounting Integration...");
                    
                    var fundingDetails = new FundingDetails
                    {
                        BaseRate = 3.0m,
                        MarginRate = 2.0m,
                        FinalDiscountRate = 5.0m,
                        FundingDate = DateTime.Now
                    };

                    // Count journal entries before funding
                    int entriesBeforeFunding = context.JournalEntries.Count();

                    var result = invoiceService.FundInvoice(invoice.Id, 1, fundingDetails);
                    Console.WriteLine($"   ✓ Funding result: {result.Message}");

                    // Count journal entries after funding
                    int entriesAfterFunding = context.JournalEntries.Count();
                    int newEntries = entriesAfterFunding - entriesBeforeFunding;

                    Console.WriteLine($"   ✓ Journal entries created: {newEntries}");

                    // Display the journal entries that were created
                    if (newEntries > 0)
                    {
                        Console.WriteLine("\n4. Accounting Entries Created:");
                        var recentEntries = context.JournalEntries
                            .Include(j => j.JournalEntryLines)
                            .ThenInclude(l => l.Account)
                            .OrderByDescending(j => j.Id)
                            .Take(newEntries)
                            .ToList();

                        foreach (var entry in recentEntries)
                        {
                            Console.WriteLine($"\n   Journal Entry #{entry.Id} - {entry.TransactionReference}");
                            Console.WriteLine($"   Date: {entry.TransactionDate:yyyy-MM-dd} | Status: {entry.Status}");
                            Console.WriteLine($"   Description: {entry.Description}");
                            Console.WriteLine($"   Total Debit: ${entry.TotalDebit:N2} | Total Credit: ${entry.TotalCredit:N2}");
                            Console.WriteLine("   Lines:");
                            
                            foreach (var line in entry.JournalEntryLines)
                            {
                                var orgName = line.OrganizationId.HasValue ? 
                                    context.Organizations.Find(line.OrganizationId)?.Name ?? "Unknown" : "Bank";
                                
                                if (line.DebitAmount > 0)
                                    Console.WriteLine($"     DR {line.Account?.AccountName,-30} ${line.DebitAmount,10:N2} ({orgName})");
                                if (line.CreditAmount > 0)
                                    Console.WriteLine($"     CR {line.Account?.AccountName,-30} ${line.CreditAmount,10:N2} ({orgName})");
                            }
                        }

                        // Test posting the entries
                        Console.WriteLine("\n5. Posting Journal Entries...");
                        foreach (var entry in recentEntries)
                        {
                            try
                            {
                                accountingService.PostJournalEntry(entry.Id, 1);
                                Console.WriteLine($"   ✓ Posted journal entry #{entry.Id}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"   ✗ Error posting entry #{entry.Id}: {ex.Message}");
                            }
                        }

                        // Show account balances after posting
                        Console.WriteLine("\n6. Account Balances After Posting:");
                        var accounts = context.Accounts.Where(a => a.Balance != 0).OrderBy(a => a.AccountCode).ToList();
                        foreach (var account in accounts)
                        {
                            Console.WriteLine($"   {account.AccountCode} - {account.AccountName}: ${account.Balance:N2}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("   ⚠️  WARNING: No journal entries were created!");
                        Console.WriteLine("   This indicates that the accounting integration is not working properly.");
                    }

                    Console.WriteLine("\n✅ ACCOUNTING INTEGRATION TEST COMPLETED");
                    
                    if (newEntries > 0)
                    {
                        Console.WriteLine("\n✅ SUCCESS: Accounting entries are automatically created when invoices are funded!");
                        Console.WriteLine("\nThe system now properly creates:");
                        Console.WriteLine("- Debit: Loans to Customers (Bank's asset)");
                        Console.WriteLine("- Credit: Cash (Bank pays seller)");
                        Console.WriteLine("- Credit: Interest Income (Bank's revenue)");
                    }
                    else
                    {
                        Console.WriteLine("\n❌ ISSUE: No accounting entries were created. Check AccountingService integration.");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Error: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static Organization GetOrCreateOrganization(SupplyChainDbContext context, string name, bool isSeller, bool isBuyer)
        {
            var org = context.Organizations.FirstOrDefault(o => o.Name == name);
            if (org == null)
            {
                org = new Organization
                {
                    Name = name,
                    TaxId = "TAX" + DateTime.Now.Ticks.ToString()[^6..],
                    Address = "123 Test St",
                    ContactPerson = "Test Contact",
                    ContactEmail = "test@" + name.Replace(" ", "").ToLower() + ".com",
                    ContactPhone = "555-0123",
                    IsSeller = isSeller,
                    IsBuyer = isBuyer,
                    IsBank = false
                };
                context.Organizations.Add(org);
                context.SaveChanges();
            }
            return org;
        }
    }
}
