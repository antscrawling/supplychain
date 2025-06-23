using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Data;
using Core.Services;
using Core.Models;

namespace SupplyChainFinance
{
    class AccountingModule
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===================================================");
            Console.WriteLine("   SUPPLY CHAIN FINANCE - ACCOUNTING MODULE");
            Console.WriteLine("===================================================\n");

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

                var module = new AccountingModuleApp(context, accountingService, invoiceService, transactionService);
                module.Run();
            }

            Console.WriteLine("\nThank you for using the Accounting Module. Goodbye!");
        }
    }

    class AccountingModuleApp
    {
        private readonly SupplyChainDbContext _context;
        private readonly AccountingService _accountingService;
        private readonly InvoiceService _invoiceService;
        private readonly TransactionService _transactionService;

        public AccountingModuleApp(SupplyChainDbContext context, AccountingService accountingService, 
            InvoiceService invoiceService, TransactionService transactionService)
        {
            _context = context;
            _accountingService = accountingService;
            _invoiceService = invoiceService;
            _transactionService = transactionService;
        }

        public void Run()
        {
            // Initialize chart of accounts if needed
            _accountingService.InitializeChartOfAccounts();

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                ShowMainMenu();
                
                Console.Write("\nSelect an option: ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ViewChartOfAccounts();
                        break;
                    case "2":
                        ViewJournalEntries();
                        break;
                    case "3":
                        CreateManualJournalEntry();
                        break;
                    case "4":
                        PostJournalEntry();
                        break;
                    case "5":
                        ViewAccountBalances();
                        break;
                    case "6":
                        GenerateTrialBalance();
                        break;
                    case "7":
                        CreateInvoiceFinancingEntry();
                        break;
                    case "8":
                        CreatePaymentEntry();
                        break;
                    case "9":
                        ViewTransactionHistory();
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("\nInvalid option. Please try again.");
                        WaitForEnterKey();
                        break;
                }
            }
        }

        private void ShowMainMenu()
        {
            Console.WriteLine("ACCOUNTING ENTRIES MANAGEMENT");
            Console.WriteLine("============================");
            Console.WriteLine();
            Console.WriteLine("Chart of Accounts & Setup:");
            Console.WriteLine("1. View Chart of Accounts");
            Console.WriteLine();
            Console.WriteLine("Journal Entries:");
            Console.WriteLine("2. View Journal Entries");
            Console.WriteLine("3. Create Manual Journal Entry");
            Console.WriteLine("4. Post Journal Entry");
            Console.WriteLine();
            Console.WriteLine("Reports:");
            Console.WriteLine("5. View Account Balances");
            Console.WriteLine("6. Generate Trial Balance");
            Console.WriteLine();
            Console.WriteLine("Transaction Processing:");
            Console.WriteLine("7. Create Invoice Financing Entry");
            Console.WriteLine("8. Create Payment Entry");
            Console.WriteLine("9. View Transaction History");
            Console.WriteLine();
            Console.WriteLine("0. Exit");
        }

        private void ViewChartOfAccounts()
        {
            Console.Clear();
            Console.WriteLine("CHART OF ACCOUNTS");
            Console.WriteLine("=================\n");

            var accounts = _context.Accounts
                .OrderBy(a => a.AccountCode)
                .ToList();

            if (!accounts.Any())
            {
                Console.WriteLine("No accounts found. Initializing chart of accounts...");
                _accountingService.InitializeChartOfAccounts();
                accounts = _context.Accounts.OrderBy(a => a.AccountCode).ToList();
            }

            Console.WriteLine($"{"Code",-8} {"Account Name",-35} {"Type",-12} {"Category",-20} {"Balance",-15} {"Status",-8}");
            Console.WriteLine(new string('-', 100));

            foreach (var account in accounts)
            {
                string status = account.IsActive ? "Active" : "Inactive";
                Console.WriteLine($"{account.AccountCode,-8} {account.AccountName,-35} {account.Type,-12} {account.Category,-20} ${account.Balance,-13:N2} {status,-8}");
            }

            Console.WriteLine($"\nTotal Accounts: {accounts.Count}");
            WaitForEnterKey();
        }

        private void ViewJournalEntries()
        {
            Console.Clear();
            Console.WriteLine("JOURNAL ENTRIES");
            Console.WriteLine("===============\n");

            Console.WriteLine("Filter by:");
            Console.WriteLine("1. All Entries");
            Console.WriteLine("2. Pending Entries");
            Console.WriteLine("3. Posted Entries");
            Console.WriteLine("4. Today's Entries");
            Console.Write("\nSelect filter: ");
            
            string? filter = Console.ReadLine();
            
            var query = _context.JournalEntries
                .Include(j => j.JournalEntryLines)
                .ThenInclude(l => l.Account)
                .Include(j => j.Organization)
                .AsQueryable();

            switch (filter)
            {
                case "2":
                    query = query.Where(j => j.Status == JournalEntryStatus.Pending);
                    break;
                case "3":
                    query = query.Where(j => j.Status == JournalEntryStatus.Posted);
                    break;
                case "4":
                    query = query.Where(j => j.TransactionDate.Date == DateTime.Today);
                    break;
            }

            var entries = query.OrderByDescending(j => j.TransactionDate).Take(20).ToList();

            if (!entries.Any())
            {
                Console.WriteLine("No journal entries found.");
                WaitForEnterKey();
                return;
            }

            Console.WriteLine($"\n{"ID",-5} {"Date",-12} {"Reference",-15} {"Description",-30} {"Debit",-12} {"Credit",-12} {"Status",-8}");
            Console.WriteLine(new string('-', 95));

            foreach (var entry in entries)
            {
                Console.WriteLine($"{entry.Id,-5} {entry.TransactionDate:MM/dd/yyyy,-12} {entry.TransactionReference,-15} {entry.Description?.Substring(0, Math.Min(29, entry.Description.Length)),-30} ${entry.TotalDebit,-10:N2} ${entry.TotalCredit,-10:N2} {entry.Status,-8}");
            }

            Console.Write("\nEnter Journal Entry ID to view details (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int entryId) && entryId > 0)
            {
                ViewJournalEntryDetails(entryId);
            }
        }

        private void ViewJournalEntryDetails(int entryId)
        {
            var entry = _context.JournalEntries
                .Include(j => j.JournalEntryLines)
                .ThenInclude(l => l.Account)
                .Include(j => j.Organization)
                .FirstOrDefault(j => j.Id == entryId);

            if (entry == null)
            {
                Console.WriteLine("Journal entry not found.");
                WaitForEnterKey();
                return;
            }

            Console.Clear();
            Console.WriteLine($"JOURNAL ENTRY DETAILS - #{entry.Id}");
            Console.WriteLine("===============================\n");

            Console.WriteLine($"Reference: {entry.TransactionReference}");
            Console.WriteLine($"Date: {entry.TransactionDate:yyyy-MM-dd}");
            Console.WriteLine($"Description: {entry.Description}");
            Console.WriteLine($"Organization: {entry.Organization?.Name ?? "Bank"}");
            Console.WriteLine($"Status: {entry.Status}");
            Console.WriteLine($"Total Debit: ${entry.TotalDebit:N2}");
            Console.WriteLine($"Total Credit: ${entry.TotalCredit:N2}");
            Console.WriteLine($"Balanced: {entry.IsBalanced}");

            if (entry.PostedDate.HasValue)
            {
                Console.WriteLine($"Posted Date: {entry.PostedDate:yyyy-MM-dd HH:mm}");
            }

            Console.WriteLine("\nJournal Entry Lines:");
            Console.WriteLine($"{"Account Code",-12} {"Account Name",-30} {"Debit",-12} {"Credit",-12} {"Organization",-15}");
            Console.WriteLine(new string('-', 85));

            foreach (var line in entry.JournalEntryLines.OrderBy(l => l.Account?.AccountCode))
            {
                var orgName = line.OrganizationId.HasValue ? 
                    _context.Organizations.Find(line.OrganizationId)?.Name ?? "Unknown" : "Bank";
                
                Console.WriteLine($"{line.Account?.AccountCode,-12} {line.Account?.AccountName,-30} ${line.DebitAmount,-10:N2} ${line.CreditAmount,-10:N2} {orgName,-15}");
            }

            WaitForEnterKey();
        }

        private void CreateManualJournalEntry()
        {
            Console.Clear();
            Console.WriteLine("CREATE MANUAL JOURNAL ENTRY");
            Console.WriteLine("===========================\n");

            Console.Write("Description: ");
            string description = Console.ReadLine() ?? "";

            Console.Write("Transaction Date (MM/dd/yyyy, default today): ");
            string? dateInput = Console.ReadLine();
            DateTime transactionDate = string.IsNullOrWhiteSpace(dateInput) ? DateTime.Now : DateTime.Parse(dateInput);

            var journalEntry = new JournalEntry
            {
                TransactionReference = $"MAN-{DateTime.Now:yyyyMMdd}-{DateTime.Now.Ticks.ToString()[^4..]}",
                TransactionDate = transactionDate,
                Description = description,
                Status = JournalEntryStatus.Pending
            };

            var lines = new List<JournalEntryLine>();
            bool addingLines = true;
            decimal totalDebits = 0, totalCredits = 0;

            Console.WriteLine("\nAdd journal entry lines (enter empty line to finish):");
            Console.WriteLine("Format: AccountCode DebitAmount CreditAmount Description");
            Console.WriteLine("Example: 1100 1000 0 Cash received");

            while (addingLines)
            {
                Console.Write($"\nLine {lines.Count + 1}: ");
                string? lineInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(lineInput))
                {
                    addingLines = false;
                    continue;
                }

                var parts = lineInput.Split(' ', 4);
                if (parts.Length < 3)
                {
                    Console.WriteLine("Invalid format. Please use: AccountCode DebitAmount CreditAmount [Description]");
                    continue;
                }

                string accountCode = parts[0];
                if (!decimal.TryParse(parts[1], out decimal debitAmount) || 
                    !decimal.TryParse(parts[2], out decimal creditAmount))
                {
                    Console.WriteLine("Invalid amounts. Please enter numeric values.");
                    continue;
                }

                var account = _context.Accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account == null)
                {
                    Console.WriteLine($"Account {accountCode} not found.");
                    continue;
                }

                string lineDescription = parts.Length > 3 ? parts[3] : description;

                var line = new JournalEntryLine
                {
                    AccountId = account.Id,
                    DebitAmount = debitAmount,
                    CreditAmount = creditAmount,
                    Description = lineDescription
                };

                lines.Add(line);
                totalDebits += debitAmount;
                totalCredits += creditAmount;

                Console.WriteLine($"Added: {account.AccountName} DR:{debitAmount:C} CR:{creditAmount:C}");
                Console.WriteLine($"Running totals - Debits: {totalDebits:C}, Credits: {totalCredits:C}");
            }

            if (lines.Count == 0)
            {
                Console.WriteLine("No lines added. Entry cancelled.");
                WaitForEnterKey();
                return;
            }

            if (totalDebits != totalCredits)
            {
                Console.WriteLine($"\nWARNING: Entry is not balanced! Debits: {totalDebits:C}, Credits: {totalCredits:C}");
            }

            Console.Write("\nSave this journal entry? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                journalEntry.JournalEntryLines = lines;
                _context.JournalEntries.Add(journalEntry);
                _context.SaveChanges();

                Console.WriteLine($"Journal entry #{journalEntry.Id} created successfully.");
            }
            else
            {
                Console.WriteLine("Journal entry cancelled.");
            }

            WaitForEnterKey();
        }

        private void PostJournalEntry()
        {
            Console.Clear();
            Console.WriteLine("POST JOURNAL ENTRY");
            Console.WriteLine("==================\n");

            // Show pending entries
            var pendingEntries = _context.JournalEntries
                .Where(j => j.Status == JournalEntryStatus.Pending)
                .OrderByDescending(j => j.TransactionDate)
                .Take(10)
                .ToList();

            if (!pendingEntries.Any())
            {
                Console.WriteLine("No pending journal entries found.");
                WaitForEnterKey();
                return;
            }

            Console.WriteLine("Pending Journal Entries:");
            Console.WriteLine($"{"ID",-5} {"Date",-12} {"Reference",-15} {"Description",-30} {"Balanced",-8}");
            Console.WriteLine(new string('-', 75));

            foreach (var entry in pendingEntries)
            {
                Console.WriteLine($"{entry.Id,-5} {entry.TransactionDate:MM/dd/yyyy,-12} {entry.TransactionReference,-15} {entry.Description?.Substring(0, Math.Min(29, entry.Description?.Length ?? 0)),-30} {entry.IsBalanced,-8}");
            }

            Console.Write("\nEnter Journal Entry ID to post: ");
            if (int.TryParse(Console.ReadLine(), out int entryId))
            {
                try
                {
                    _accountingService.PostJournalEntry(entryId, 1); // Using user ID 1 as default
                    Console.WriteLine($"Journal entry #{entryId} posted successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error posting journal entry: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Invalid entry ID.");
            }

            WaitForEnterKey();
        }

        private void ViewAccountBalances()
        {
            Console.Clear();
            Console.WriteLine("ACCOUNT BALANCES");
            Console.WriteLine("================\n");

            var accounts = _context.Accounts
                .Where(a => a.Balance != 0 || a.AccountCode.StartsWith("1") || a.AccountCode.StartsWith("2") || a.AccountCode.StartsWith("3") || a.AccountCode.StartsWith("4"))
                .OrderBy(a => a.AccountCode)
                .ToList();

            Console.WriteLine($"{"Code",-8} {"Account Name",-35} {"Type",-12} {"Balance",-15}");
            Console.WriteLine(new string('-', 75));

            decimal totalAssets = 0, totalLiabilities = 0, totalEquity = 0, totalRevenue = 0, totalExpenses = 0;

            foreach (var account in accounts)
            {
                Console.WriteLine($"{account.AccountCode,-8} {account.AccountName,-35} {account.Type,-12} ${account.Balance,-13:N2}");
                
                switch (account.Type)
                {
                    case AccountType.Asset:
                        totalAssets += account.Balance;
                        break;
                    case AccountType.Liability:
                        totalLiabilities += account.Balance;
                        break;
                    case AccountType.Equity:
                        totalEquity += account.Balance;
                        break;
                    case AccountType.Revenue:
                        totalRevenue += account.Balance;
                        break;
                    case AccountType.Expense:
                        totalExpenses += account.Balance;
                        break;
                }
            }

            Console.WriteLine(new string('-', 75));
            Console.WriteLine($"{"TOTALS:",-55}");
            Console.WriteLine($"{"Assets:",-55} ${totalAssets,-13:N2}");
            Console.WriteLine($"{"Liabilities:",-55} ${totalLiabilities,-13:N2}");
            Console.WriteLine($"{"Equity:",-55} ${totalEquity,-13:N2}");
            Console.WriteLine($"{"Revenue:",-55} ${totalRevenue,-13:N2}");
            Console.WriteLine($"{"Expenses:",-55} ${totalExpenses,-13:N2}");
            Console.WriteLine();
            Console.WriteLine($"{"Assets - Liabilities - Equity:",-55} ${totalAssets - totalLiabilities - totalEquity,-13:N2}");
            Console.WriteLine($"{"Revenue - Expenses:",-55} ${totalRevenue - totalExpenses,-13:N2}");

            WaitForEnterKey();
        }

        private void GenerateTrialBalance()
        {
            Console.Clear();
            Console.WriteLine("TRIAL BALANCE");
            Console.WriteLine("=============\n");

            Console.Write("As of date (MM/dd/yyyy, default today): ");
            string? dateInput = Console.ReadLine();
            DateTime asOfDate = string.IsNullOrWhiteSpace(dateInput) ? DateTime.Now : DateTime.Parse(dateInput);

            try
            {
                var trialBalance = _accountingService.GenerateTrialBalance(asOfDate, 1);

                Console.WriteLine($"Trial Balance as of {trialBalance.AsOfDate:yyyy-MM-dd}");
                Console.WriteLine($"{"Account Name",-35} {"Debit",-15} {"Credit",-15}");
                Console.WriteLine(new string('-', 70));

                foreach (var line in trialBalance.Lines.OrderBy(l => l.Account?.AccountCode))
                {
                    string debitStr = line.DebitBalance > 0 ? $"${line.DebitBalance:N2}" : "";
                    string creditStr = line.CreditBalance > 0 ? $"${line.CreditBalance:N2}" : "";
                    Console.WriteLine($"{line.Account?.AccountName,-35} {debitStr,-15} {creditStr,-15}");
                }

                Console.WriteLine(new string('-', 70));
                Console.WriteLine($"{"TOTALS",-35} ${trialBalance.TotalDebits,-13:N2} ${trialBalance.TotalCredits,-13:N2}");
                Console.WriteLine($"\nBalanced: {trialBalance.IsBalanced}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating trial balance: {ex.Message}");
            }

            WaitForEnterKey();
        }

        private void CreateInvoiceFinancingEntry()
        {
            Console.Clear();
            Console.WriteLine("CREATE INVOICE FINANCING ENTRY");
            Console.WriteLine("==============================\n");

            // Show available approved invoices
            var approvedInvoices = _context.Invoices
                .Include(i => i.Seller)
                .Include(i => i.Buyer)
                .Where(i => i.Status == InvoiceStatus.Approved)
                .ToList();

            if (!approvedInvoices.Any())
            {
                Console.WriteLine("No approved invoices found for financing.");
                WaitForEnterKey();
                return;
            }

            Console.WriteLine("Available Invoices for Financing:");
            Console.WriteLine($"{"ID",-5} {"Invoice #",-15} {"Amount",-12} {"Seller",-20} {"Buyer",-20}");
            Console.WriteLine(new string('-', 80));

            foreach (var invoice in approvedInvoices)
            {
                Console.WriteLine($"{invoice.Id,-5} {invoice.InvoiceNumber,-15} ${invoice.Amount,-10:N2} {invoice.Seller?.Name,-20} {invoice.Buyer?.Name,-20}");
            }

            Console.Write("\nSelect Invoice ID: ");
            if (!int.TryParse(Console.ReadLine(), out int invoiceId))
            {
                Console.WriteLine("Invalid invoice ID.");
                WaitForEnterKey();
                return;
            }

            var selectedInvoice = approvedInvoices.FirstOrDefault(i => i.Id == invoiceId);
            if (selectedInvoice == null)
            {
                Console.WriteLine("Invoice not found.");
                WaitForEnterKey();
                return;
            }

            Console.Write("Discount Rate (%): ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal discountRate) || discountRate < 0)
            {
                Console.WriteLine("Invalid discount rate.");
                WaitForEnterKey();
                return;
            }

            decimal discountAmount = selectedInvoice.Amount * (discountRate / 100m);
            decimal fundedAmount = selectedInvoice.Amount - discountAmount;

            Console.WriteLine($"\nInvoice Amount: ${selectedInvoice.Amount:N2}");
            Console.WriteLine($"Discount Amount: ${discountAmount:N2}");
            Console.WriteLine($"Funded Amount: ${fundedAmount:N2}");

            Console.Write("\nCreate financing transaction and journal entry? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                try
                {
                    // Create the transaction
                    var transaction = new Transaction
                    {
                        Type = TransactionType.InvoiceFunding,
                        FacilityType = FacilityType.InvoiceFinancing,
                        OrganizationId = selectedInvoice.SellerId ?? 0,
                        InvoiceId = selectedInvoice.Id,
                        Description = $"Invoice financing for {selectedInvoice.InvoiceNumber}",
                        Amount = selectedInvoice.Amount,
                        InterestOrDiscountRate = discountRate,
                        TransactionDate = DateTime.Now,
                        MaturityDate = selectedInvoice.DueDate
                    };

                    var recordedTransaction = _transactionService.RecordTransaction(transaction);
                    
                    // Update invoice status
                    selectedInvoice.Status = InvoiceStatus.Funded;
                    selectedInvoice.FundedAmount = fundedAmount;
                    selectedInvoice.DiscountRate = discountRate;
                    selectedInvoice.FundingDate = DateTime.Now;
                    
                    _context.SaveChanges();

                    Console.WriteLine($"Invoice financing transaction created successfully. Transaction ID: {recordedTransaction.Id}");
                    Console.WriteLine("Journal entries have been automatically created and are pending posting.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating financing entry: {ex.Message}");
                }
            }

            WaitForEnterKey();
        }

        private void CreatePaymentEntry()
        {
            Console.Clear();
            Console.WriteLine("CREATE PAYMENT ENTRY");
            Console.WriteLine("====================\n");

            // Show funded invoices
            var fundedInvoices = _context.Invoices
                .Include(i => i.Seller)
                .Include(i => i.Buyer)
                .Where(i => i.Status == InvoiceStatus.Funded || i.Status == InvoiceStatus.PartiallyPaid)
                .ToList();

            if (!fundedInvoices.Any())
            {
                Console.WriteLine("No funded invoices found for payment.");
                WaitForEnterKey();
                return;
            }

            Console.WriteLine("Funded Invoices Available for Payment:");
            Console.WriteLine($"{"ID",-5} {"Invoice #",-15} {"Amount",-12} {"Paid",-12} {"Due",-12} {"Buyer",-20}");
            Console.WriteLine(new string('-', 85));

            foreach (var invoice in fundedInvoices)
            {
                decimal invoiceAmountDue = invoice.Amount - (invoice.PaidAmount ?? 0);
                Console.WriteLine($"{invoice.Id,-5} {invoice.InvoiceNumber,-15} ${invoice.Amount,-10:N2} ${invoice.PaidAmount ?? 0,-10:N2} ${invoiceAmountDue,-10:N2} {invoice.Buyer?.Name,-20}");
            }

            Console.Write("\nSelect Invoice ID: ");
            if (!int.TryParse(Console.ReadLine(), out int invoiceId))
            {
                Console.WriteLine("Invalid invoice ID.");
                WaitForEnterKey();
                return;
            }

            var selectedInvoice = fundedInvoices.FirstOrDefault(i => i.Id == invoiceId);
            if (selectedInvoice == null)
            {
                Console.WriteLine("Invoice not found.");
                WaitForEnterKey();
                return;
            }

            decimal amountDue = selectedInvoice.Amount - (selectedInvoice.PaidAmount ?? 0);
            Console.Write($"Payment Amount (max ${amountDue:N2}): $");
            if (!decimal.TryParse(Console.ReadLine(), out decimal paymentAmount) || paymentAmount <= 0 || paymentAmount > amountDue)
            {
                Console.WriteLine("Invalid payment amount.");
                WaitForEnterKey();
                return;
            }

            Console.Write("\nCreate payment transaction and journal entry? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                try
                {
                    // Process the payment using InvoiceService
                    var result = _invoiceService.ProcessPayment(selectedInvoice.Id, paymentAmount);
                    Console.WriteLine($"Payment processed successfully: {result.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing payment: {ex.Message}");
                }
            }

            WaitForEnterKey();
        }

        private void ViewTransactionHistory()
        {
            Console.Clear();
            Console.WriteLine("TRANSACTION HISTORY");
            Console.WriteLine("===================\n");

            var transactions = _context.Transactions
                .Include(t => t.Organization)
                .Include(t => t.Invoice)
                .OrderByDescending(t => t.TransactionDate)
                .Take(20)
                .ToList();

            if (!transactions.Any())
            {
                Console.WriteLine("No transactions found.");
                WaitForEnterKey();
                return;
            }

            Console.WriteLine($"{"ID",-5} {"Date",-12} {"Type",-15} {"Amount",-12} {"Organization",-20} {"Invoice",-15}");
            Console.WriteLine(new string('-', 90));

            foreach (var transaction in transactions)
            {
                Console.WriteLine($"{transaction.Id,-5} {transaction.TransactionDate:MM/dd/yyyy,-12} {transaction.Type,-15} ${transaction.Amount,-10:N2} {transaction.Organization?.Name ?? "Bank",-20} {transaction.Invoice?.InvoiceNumber ?? "N/A",-15}");
            }

            Console.Write("\nEnter Transaction ID for details (or 0 to return): ");
            if (int.TryParse(Console.ReadLine(), out int transactionId) && transactionId > 0)
            {
                ViewTransactionDetails(transactionId);
            }
        }

        private void ViewTransactionDetails(int transactionId)
        {
            var transaction = _context.Transactions
                .Include(t => t.Organization)
                .Include(t => t.Invoice)
                .ThenInclude(i => i.Seller)
                .Include(t => t.Invoice)
                .ThenInclude(i => i.Buyer)
                .FirstOrDefault(t => t.Id == transactionId);

            if (transaction == null)
            {
                Console.WriteLine("Transaction not found.");
                WaitForEnterKey();
                return;
            }

            Console.Clear();
            Console.WriteLine($"TRANSACTION DETAILS - #{transaction.Id}");
            Console.WriteLine("==============================\n");

            Console.WriteLine($"Type: {transaction.Type}");
            Console.WriteLine($"Facility Type: {transaction.FacilityType}");
            Console.WriteLine($"Amount: ${transaction.Amount:N2}");
            Console.WriteLine($"Date: {transaction.TransactionDate:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"Organization: {transaction.Organization?.Name ?? "Bank"}");
            Console.WriteLine($"Description: {transaction.Description}");

            if (transaction.Invoice != null)
            {
                Console.WriteLine($"\nInvoice Information:");
                Console.WriteLine($"  Invoice Number: {transaction.Invoice.InvoiceNumber}");
                Console.WriteLine($"  Seller: {transaction.Invoice.Seller?.Name ?? "Unknown"}");
                Console.WriteLine($"  Buyer: {transaction.Invoice.Buyer?.Name ?? "Unknown"}");
                Console.WriteLine($"  Invoice Amount: ${transaction.Invoice.Amount:N2}");
                Console.WriteLine($"  Due Date: {transaction.Invoice.DueDate:yyyy-MM-dd}");
            }

            if (transaction.InterestOrDiscountRate.HasValue)
            {
                Console.WriteLine($"Interest/Discount Rate: {transaction.InterestOrDiscountRate:P2}");
            }

            if (transaction.MaturityDate != default(DateTime))
            {
                Console.WriteLine($"Maturity Date: {transaction.MaturityDate:yyyy-MM-dd}");
            }

            // Show related journal entries
            var journalEntries = _context.JournalEntries
                .Where(j => j.TransactionId == transactionId)
                .Include(j => j.JournalEntryLines)
                .ThenInclude(l => l.Account)
                .ToList();

            if (journalEntries.Any())
            {
                Console.WriteLine($"\nRelated Journal Entries:");
                foreach (var entry in journalEntries)
                {
                    Console.WriteLine($"  Journal Entry #{entry.Id} - {entry.Status}");
                    Console.WriteLine($"  Total Debit: ${entry.TotalDebit:N2}, Total Credit: ${entry.TotalCredit:N2}");
                }
            }

            WaitForEnterKey();
        }

        private void WaitForEnterKey()
        {
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }
}
