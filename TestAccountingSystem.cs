using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Data;
using Core.Services;
using Core.Models;

namespace SupplyChainFinance
{
    class AccountingTestProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Supply Chain Finance - Accounting System Demo");
            Console.WriteLine("=============================================\n");

            // Configure services
            var services = new ServiceCollection();
            services.AddDbContext<SupplyChainDbContext>(options =>
                options.UseSqlite("Data Source=supply_chain_finance.db"));
            services.AddScoped<MyLimitService>();

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SupplyChainDbContext>();
                var accountingService = new AccountingService(context);
                var transactionService = new TransactionService(context);

                try
                {
                    // Initialize chart of accounts
                    Console.WriteLine("1. Initializing Chart of Accounts...");
                    accountingService.InitializeChartOfAccounts();
                    Console.WriteLine("   ✓ Chart of accounts created");

                    // Display chart of accounts
                    DisplayChartOfAccounts(context);

                    // Create a sample organization and invoice
                    var seller = GetOrCreateOrganization(context, "ACME Corp", true, false);
                    var buyer = GetOrCreateOrganization(context, "BigBuy Inc", false, true);

                    // Create sample invoice
                    var invoice = new Invoice
                    {
                        InvoiceNumber = "INV-2025-001",
                        Amount = 10000,
                        SellerId = seller.Id,
                        BuyerId = buyer.Id,
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(30),
                        Description = "Sample invoice for accounting demo",
                        Status = InvoiceStatus.Approved
                    };
                    context.Invoices.Add(invoice);
                    context.SaveChanges();

                    Console.WriteLine("\n2. Creating Sample Invoice Funding Transaction...");
                    
                    // Create invoice funding transaction
                    var fundingTransaction = new Transaction
                    {
                        Type = TransactionType.InvoiceFunding,
                        FacilityType = FacilityType.InvoiceFinancing,
                        OrganizationId = seller.Id,
                        InvoiceId = invoice.Id,
                        Description = $"Funding for invoice {invoice.InvoiceNumber}",
                        Amount = 10000,
                        InterestOrDiscountRate = 5.0m, // 5% discount rate
                        TransactionDate = DateTime.Now,
                        MaturityDate = invoice.DueDate
                    };

                    // Record transaction (this will automatically create journal entries)
                    var recordedTransaction = transactionService.RecordTransaction(fundingTransaction);
                    Console.WriteLine($"   ✓ Transaction {recordedTransaction.Id} recorded");

                    Console.WriteLine("\n3. Creating Payment Transaction...");
                    
                    // Create payment transaction
                    var paymentTransaction = new Transaction
                    {
                        Type = TransactionType.Payment,
                        FacilityType = FacilityType.InvoiceFinancing,
                        OrganizationId = buyer.Id,
                        InvoiceId = invoice.Id,
                        Description = $"Payment for invoice {invoice.InvoiceNumber}",
                        Amount = 10000,
                        TransactionDate = DateTime.Now.AddDays(30),
                        MaturityDate = DateTime.Now.AddDays(30)
                    };

                    var recordedPayment = transactionService.RecordTransaction(paymentTransaction);
                    Console.WriteLine($"   ✓ Payment transaction {recordedPayment.Id} recorded");

                    Console.WriteLine("\n4. Creating Fee Charge Transaction...");
                    
                    // Create fee charge transaction
                    var feeTransaction = new Transaction
                    {
                        Type = TransactionType.FeeCharge,
                        FacilityType = FacilityType.InvoiceFinancing,
                        OrganizationId = seller.Id,
                        Description = "Processing fee for invoice financing",
                        Amount = 100,
                        TransactionDate = DateTime.Now,
                        MaturityDate = DateTime.Now
                    };

                    var recordedFee = transactionService.RecordTransaction(feeTransaction);
                    Console.WriteLine($"   ✓ Fee transaction {recordedFee.Id} recorded");

                    // Display journal entries
                    DisplayJournalEntries(context);

                    // Display account balances
                    DisplayAccountBalances(context);

                    // Generate and display trial balance
                    Console.WriteLine("\n7. Generating Trial Balance...");
                    var trialBalance = accountingService.GenerateTrialBalance(DateTime.Now, 1);
                    DisplayTrialBalance(trialBalance);

                    Console.WriteLine("\n✓ Accounting system demonstration completed successfully!");
                    Console.WriteLine("\nKey Features Demonstrated:");
                    Console.WriteLine("- Automatic journal entry creation for all transactions");
                    Console.WriteLine("- Double-entry bookkeeping with balanced entries");
                    Console.WriteLine("- Chart of accounts with proper account types");
                    Console.WriteLine("- Real-time account balance updates");
                    Console.WriteLine("- Trial balance generation");
                    Console.WriteLine("- Proper accounting treatment for:");
                    Console.WriteLine("  * Invoice funding (with discount/factoring fee)");
                    Console.WriteLine("  * Customer payments");
                    Console.WriteLine("  * Fee charges");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
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
                    Address = "123 Business St",
                    ContactPerson = "John Doe",
                    ContactEmail = "contact@" + name.Replace(" ", "").ToLower() + ".com",
                    ContactPhone = "555-0100",
                    IsSeller = isSeller,
                    IsBuyer = isBuyer,
                    IsBank = false
                };
                context.Organizations.Add(org);
                context.SaveChanges();
            }
            return org;
        }

        static void DisplayChartOfAccounts(SupplyChainDbContext context)
        {
            Console.WriteLine("\n3. Chart of Accounts:");
            Console.WriteLine("   Code  | Account Name                     | Type      | Category");
            Console.WriteLine("   ------|----------------------------------|-----------|------------------");
            
            var accounts = context.Accounts.OrderBy(a => a.AccountCode).ToList();
            foreach (var account in accounts)
            {
                Console.WriteLine($"   {account.AccountCode,-5} | {account.AccountName,-32} | {account.Type,-9} | {account.Category}");
            }
        }

        static void DisplayJournalEntries(SupplyChainDbContext context)
        {
            Console.WriteLine("\n5. Journal Entries Created:");
            var entries = context.JournalEntries
                .Include(j => j.JournalEntryLines)
                .ThenInclude(l => l.Account)
                .Include(j => j.Organization)
                .OrderBy(j => j.TransactionDate)
                .ToList();

            foreach (var entry in entries)
            {
                Console.WriteLine($"\n   Entry #{entry.Id} - {entry.TransactionReference}");
                Console.WriteLine($"   Date: {entry.TransactionDate:yyyy-MM-dd} | Status: {entry.Status}");
                Console.WriteLine($"   Description: {entry.Description}");
                Console.WriteLine($"   Organization: {entry.Organization?.Name ?? "Bank"}");
                Console.WriteLine($"   Total Debit: ${entry.TotalDebit:N2} | Total Credit: ${entry.TotalCredit:N2} | Balanced: {entry.IsBalanced}");
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
        }

        static void DisplayAccountBalances(SupplyChainDbContext context)
        {
            Console.WriteLine("\n6. Account Balances:");
            Console.WriteLine("   Code  | Account Name                     | Balance      | Type");
            Console.WriteLine("   ------|----------------------------------|--------------|----------");
            
            var accounts = context.Accounts
                .Where(a => a.Balance != 0)
                .OrderBy(a => a.AccountCode)
                .ToList();
                
            foreach (var account in accounts)
            {
                Console.WriteLine($"   {account.AccountCode,-5} | {account.AccountName,-32} | ${account.Balance,10:N2} | {account.Type}");
            }
        }

        static void DisplayTrialBalance(TrialBalance trialBalance)
        {
            Console.WriteLine($"   As of: {trialBalance.AsOfDate:yyyy-MM-dd}");
            Console.WriteLine("   Account Name                     | Debit        | Credit");
            Console.WriteLine("   ----------------------------------|--------------|------------");
            
            foreach (var line in trialBalance.Lines.OrderBy(l => l.Account?.AccountCode))
            {
                var debitStr = line.DebitBalance > 0 ? $"${line.DebitBalance:N2}" : "";
                var creditStr = line.CreditBalance > 0 ? $"${line.CreditBalance:N2}" : "";
                Console.WriteLine($"   {line.Account?.AccountName,-33} | {debitStr,12} | {creditStr,12}");
            }
            
            Console.WriteLine("   ----------------------------------|--------------|------------");
            Console.WriteLine($"   {"TOTALS",-33} | ${trialBalance.TotalDebits,10:N2} | ${trialBalance.TotalCredits,10:N2}");
            Console.WriteLine($"   Balanced: {trialBalance.IsBalanced}");
        }
    }
}
