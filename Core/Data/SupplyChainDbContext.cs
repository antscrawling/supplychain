using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Core.Models;

namespace Core.Data
{
    public class SupplyChainDbContext : DbContext
    {
        public SupplyChainDbContext(DbContextOptions<SupplyChainDbContext> options) : base(options)
        {
            // Removed EnsureCreated to avoid conflicts with migrations
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<CreditLimitInfo> CreditLimits { get; set; } = null!;
        public DbSet<Facility> Facilities { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<AccountStatement> AccountStatements { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Counterparty> Counterparties { get; set; } = null!;

        // Accounting DbSets
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; } = null!;
        public DbSet<AccountingPeriod> AccountingPeriods { get; set; } = null!;
        public DbSet<TrialBalance> TrialBalances { get; set; } = null!;
        public DbSet<TrialBalanceLine> TrialBalanceLines { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId);

            modelBuilder.Entity<CreditLimitInfo>()
                .HasOne(c => c.Organization)
                .WithMany(o => o.CreditLimits)
                .HasForeignKey(c => c.OrganizationId);

            modelBuilder.Entity<Facility>()
                .HasOne(f => f.CreditLimitInfo)
                .WithMany(c => c.Facilities)
                .HasForeignKey(f => f.CreditLimitInfoId);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Seller)
                .WithMany()
                .HasForeignKey(i => i.SellerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Buyer)
                .WithMany()
                .HasForeignKey(i => i.BuyerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Organization)
                .WithMany()
                .HasForeignKey(t => t.OrganizationId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Invoice)
                .WithMany()
                .HasForeignKey(t => t.InvoiceId);

            modelBuilder.Entity<AccountStatement>()
                .HasOne(a => a.Organization)
                .WithMany()
                .HasForeignKey(a => a.OrganizationId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId);

            // Configure accounting entities
            modelBuilder.Entity<JournalEntry>()
                .HasOne(j => j.Organization)
                .WithMany()
                .HasForeignKey(j => j.OrganizationId);

            modelBuilder.Entity<JournalEntry>()
                .HasOne(j => j.Invoice)
                .WithMany()
                .HasForeignKey(j => j.InvoiceId);

            modelBuilder.Entity<JournalEntry>()
                .HasOne(j => j.Transaction)
                .WithMany()
                .HasForeignKey(j => j.TransactionId);

            modelBuilder.Entity<JournalEntry>()
                .HasOne(j => j.PostedByUser)
                .WithMany()
                .HasForeignKey(j => j.PostedByUserId);

            modelBuilder.Entity<JournalEntryLine>()
                .HasOne(l => l.JournalEntry)
                .WithMany(j => j.JournalEntryLines)
                .HasForeignKey(l => l.JournalEntryId);

            modelBuilder.Entity<JournalEntryLine>()
                .HasOne(l => l.Account)
                .WithMany()
                .HasForeignKey(l => l.AccountId);

            modelBuilder.Entity<JournalEntryLine>()
                .HasOne(l => l.Organization)
                .WithMany()
                .HasForeignKey(l => l.OrganizationId);

            // Account relationships are already configured through JournalEntryLine

            modelBuilder.Entity<AccountingPeriod>()
                .HasOne(p => p.ClosedByUser)
                .WithMany()
                .HasForeignKey(p => p.ClosedByUserId);

            modelBuilder.Entity<TrialBalance>()
                .HasOne(t => t.GeneratedByUser)
                .WithMany()
                .HasForeignKey(t => t.GeneratedByUserId);

            modelBuilder.Entity<TrialBalanceLine>()
                .HasOne(l => l.TrialBalance)
                .WithMany(t => t.Lines)
                .HasForeignKey(l => l.TrialBalanceId);

            modelBuilder.Entity<TrialBalanceLine>()
                .HasOne(l => l.Account)
                .WithMany()
                .HasForeignKey(l => l.AccountId);
        }
    }

    public class DataSeeder
    {
        public static void SeedData(SupplyChainDbContext context)
        {
            if (context.Organizations.Any())
                return; // Data already seeded

            // Create bank
            var bank = new Organization
            {
                Name = "Global Finance Bank",
                TaxId = "GB123456789",
                Address = "100 Wall Street, New York",
                ContactPerson = "John Finance",
                ContactEmail = "john@globalbank.com",
                ContactPhone = "555-1234",
                IsBank = true
            };
            context.Organizations.Add(bank);

            // Create bank users
            var bankAdmin = new User
            {
                Username = "bankadmin",
                Password = "password", // In real app should be hashed
                Name = "Bank Administrator",
                Email = "admin@globalbank.com",
                Role = UserRole.BankAdmin,
                Organization = bank
            };
            var bankUser = new User
            {
                Username = "bankuser",
                Password = "password", // In real app should be hashed
                Name = "Bank User",
                Email = "user@globalbank.com",
                Role = UserRole.BankUser,
                Organization = bank
            };
            context.Users.AddRange(bankAdmin, bankUser);

            // Create buyer organization
            var buyer = new Organization
            {
                Name = "MegaCorp Industries",
                TaxId = "MC987654321",
                Address = "200 Industry Avenue, Chicago",
                ContactPerson = "Alice Purchasing",
                ContactEmail = "alice@megacorp.com",
                ContactPhone = "555-5678",
                IsBuyer = true
            };
            context.Organizations.Add(buyer);

            // Create buyer users
            var buyerAdmin = new User
            {
                Username = "buyeradmin",
                Password = "password", // In real app should be hashed
                Name = "Buyer Administrator",
                Email = "admin@megacorp.com",
                Role = UserRole.ClientAdmin,
                Organization = buyer
            };
            var buyerUser = new User
            {
                Username = "buyeruser",
                Password = "password", // In real app should be hashed
                Name = "Buyer User",
                Email = "user@megacorp.com",
                Role = UserRole.ClientUser,
                Organization = buyer
            };
            context.Users.AddRange(buyerAdmin, buyerUser);

            // Create seller organization
            var seller = new Organization
            {
                Name = "Supply Solutions Ltd",
                TaxId = "SS123789456",
                Address = "300 Supplier Road, Boston",
                ContactPerson = "Bob Selling",
                ContactEmail = "bob@supplysolutions.com",
                ContactPhone = "555-9012",
                IsSeller = true
            };
            context.Organizations.Add(seller);

            // Create seller users
            var sellerAdmin = new User
            {
                Username = "selleradmin",
                Password = "password", // In real app should be hashed
                Name = "Seller Administrator",
                Email = "admin@supplysolutions.com",
                Role = UserRole.ClientAdmin,
                Organization = seller
            };
            var sellerUser = new User
            {
                Username = "selleruser",
                Password = "password", // In real app should be hashed
                Name = "Seller User",
                Email = "user@supplysolutions.com",
                Role = UserRole.ClientUser,
                Organization = seller
            };
            context.Users.AddRange(sellerAdmin, sellerUser);

            context.SaveChanges();

            // Set up credit limits for buyer
            var buyerCreditLimit = new CreditLimitInfo
            {
                OrganizationId = buyer.Id,
                MasterLimit = 1000000
            };
            context.CreditLimits.Add(buyerCreditLimit);
            context.SaveChanges();

            var facilities = new List<Facility>
            {
                new Facility 
                { 
                    CreditLimitInfoId = buyerCreditLimit.Id,
                    Type = FacilityType.InvoiceFinancing,
                    TotalLimit = 250000,
                    ReviewEndDate = DateTime.Now.AddMonths(3)
                },
                new Facility 
                { 
                    CreditLimitInfoId = buyerCreditLimit.Id,
                    Type = FacilityType.TermLoan,
                    TotalLimit = 250000,
                    ReviewEndDate = DateTime.Now.AddMonths(6)
                },
                new Facility 
                { 
                    CreditLimitInfoId = buyerCreditLimit.Id,
                    Type = FacilityType.Overdraft,
                    TotalLimit = 250000,
                    ReviewEndDate = DateTime.Now.AddMonths(1)
                },
                new Facility 
                { 
                    CreditLimitInfoId = buyerCreditLimit.Id,
                    Type = FacilityType.Guarantee,
                    TotalLimit = 250000,
                    ReviewEndDate = DateTime.Now.AddMonths(12)
                }
            };
            context.Facilities.AddRange(facilities);

            // Create some invoices
            var invoices = new List<Invoice>
            {
                new Invoice
                {
                    InvoiceNumber = "INV-2025-001",
                    IssueDate = DateTime.Now.AddDays(-10),
                    DueDate = DateTime.Now.AddDays(50),
                    SellerId = seller.Id,
                    BuyerId = buyer.Id,
                    Amount = 130000,
                    Status = InvoiceStatusValues.Uploaded,
                    Description = "Q2 Product Shipment"
                },
                new Invoice
                {
                    InvoiceNumber = "INV-2025-002",
                    IssueDate = DateTime.Now.AddDays(-5),
                    DueDate = DateTime.Now.AddDays(55),
                    SellerId = seller.Id,
                    BuyerId = buyer.Id,
                    Amount = 87500,
                    Status = InvoiceStatusValues.Uploaded,
                    Description = "Marketing Services"
                }
            };
            context.Invoices.AddRange(invoices);

            context.SaveChanges();
        }
    }
}
