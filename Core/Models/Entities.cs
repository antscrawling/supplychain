using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = ""; // In a real app, this would be hashed
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public UserRole Role { get; set; }
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }

    public class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string TaxId { get; set; } = "";
        public string Address { get; set; } = "";
        public string ContactPerson { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string ContactPhone { get; set; } = "";
        public bool IsBuyer { get; set; }
        public bool IsSeller { get; set; }
        public bool IsBank { get; set; }
        public List<User> Users { get; set; } = new();
        public List<CreditLimitInfo> CreditLimits { get; set; } = new();
    }

    public class Facility
    {
        public int Id { get; set; }
        public int CreditLimitInfoId { get; set; }
        public FacilityType Type { get; set; }
        public decimal TotalLimit { get; set; }
        public decimal CurrentUtilization { get; set; } = 0;
        public DateTime ReviewEndDate { get; set; }
        public int GracePeriodDays { get; set; } = 5; // Default grace period of 5 days
        public bool IsExpired => DateTime.Now > ReviewEndDate.AddDays(GracePeriodDays);
        
        // New fields for buyer-seller relationship
        public int? RelatedPartyId { get; set; }  // ID of the buyer/seller this facility is linked to
        public Organization? RelatedParty { get; set; }  // The buyer/seller this facility is linked to
        public decimal AllocatedLimit { get; set; } = 0;  // Portion of limit allocated to specific counterparty
        
        public decimal AvailableLimit 
        { 
            get 
            {
                if (IsExpired)
                    return 0; // No available limit if expired (beyond grace period)
                return TotalLimit - CurrentUtilization;
            }
        }
        public decimal InExcess => IsExpired ? CurrentUtilization : 0; // Amount in excess if expired
        public decimal UtilizationPercentage => TotalLimit > 0 ? (CurrentUtilization / TotalLimit) * 100 : 0;
        public CreditLimitInfo? CreditLimitInfo { get; set; }
    }

    public class CreditLimitInfo
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public decimal MasterLimit { get; set; }
        public List<Facility> Facilities { get; set; } = new();

        public decimal TotalUtilization => Facilities.Sum(f => f.CurrentUtilization);
        public decimal AvailableMasterLimit => MasterLimit - TotalUtilization;
        public decimal MasterUtilizationPercentage => MasterLimit > 0 ? (TotalUtilization / MasterLimit) * 100 : 0;

        // Add LastReviewDate and NextReviewDate properties to CreditLimitInfo
        public DateTime LastReviewDate { get; set; }
        public DateTime NextReviewDate { get; set; }
    }

    public class Counterparty
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string TaxId { get; set; } = "";
        public string Address { get; set; } = "";
        public string ContactPerson { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string ContactPhone { get; set; } = "";
        // Optionally, add a type: Buyer or Seller
        public bool IsBuyer { get; set; }
        public bool IsSeller { get; set; }
    }

    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = "";
        public int? SellerId { get; set; }
        public Organization? Seller { get; set; }
        public int? BuyerId { get; set; }
        public Organization? Buyer { get; set; }
        public int? CounterpartyId { get; set; } // New: for non-customer party
        public Counterparty? Counterparty { get; set; }
        public string Currency { get; set; } = "USD";
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Uploaded;
        public DateTime? FundingDate { get; set; }
        public decimal? FundedAmount { get; set; }
        public decimal? DiscountRate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? PaidAmount { get; set; }
        
        // New fields for approval workflow
        public bool BuyerApproved { get; set; }
        public DateTime? BuyerApprovalDate { get; set; }
        public int? BuyerApprovalUserId { get; set; }
        
        public bool SellerAccepted { get; set; }
        public DateTime? SellerAcceptanceDate { get; set; }
        public int? SellerAcceptanceUserId { get; set; }
        
        public string? RejectionReason { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public FacilityType FacilityType { get; set; }
        public int OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal? InterestOrDiscountRate { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public DateTime MaturityDate { get; set; }
        public bool IsPaid { get; set; } = false;
        public DateTime? PaymentDate { get; set; }
    }

    public class AccountStatement
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GenerationDate { get; set; } = DateTime.Now;
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<Transaction> Transactions { get; set; } = new();
        public string StatementNumber { get; set; } = "";
    }

    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public string Type { get; set; } = "Info"; // Info, Warning, Alert
        
        // New fields for approval workflow
        public int? InvoiceId { get; set; }
        public bool RequiresAction { get; set; } = false;
        public bool ActionTaken { get; set; } = false;
        public DateTime? ActionDate { get; set; }
        public string? ActionResponse { get; set; }
    }

    public class FundingDetails
    {
        public DateTime FundingDate { get; set; } = DateTime.Now;
        public decimal BaseRate { get; set; }
        public decimal MarginRate { get; set; }
        public decimal FinalDiscountRate { get; set; }
    }

    // Accounting Models
    public class Account
    {
        public int Id { get; set; }
        public string AccountCode { get; set; } = "";
        public string AccountName { get; set; } = "";
        public AccountType Type { get; set; }
        public AccountCategory Category { get; set; }
        public decimal Balance { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public List<JournalEntry> DebitEntries { get; set; } = new();
        public List<JournalEntry> CreditEntries { get; set; } = new();
    }

    public class JournalEntry
    {
        public int Id { get; set; }
        public string TransactionReference { get; set; } = "";
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string Description { get; set; } = "";
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        public List<JournalEntryLine> JournalEntryLines { get; set; } = new();
        public decimal TotalDebit => JournalEntryLines.Sum(l => l.DebitAmount);
        public decimal TotalCredit => JournalEntryLines.Sum(l => l.CreditAmount);
        public bool IsBalanced => TotalDebit == TotalCredit;
        public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Pending;
        public DateTime? PostedDate { get; set; }
        public int? PostedByUserId { get; set; }
        public User? PostedByUser { get; set; }
    }

    public class JournalEntryLine
    {
        public int Id { get; set; }
        public int JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
        public decimal DebitAmount { get; set; } = 0;
        public decimal CreditAmount { get; set; } = 0;
        public string Description { get; set; } = "";
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }

    public class AccountingPeriod
    {
        public int Id { get; set; }
        public string PeriodName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsOpen { get; set; } = true;
        public bool IsClosed { get; set; } = false;
        public DateTime? ClosedDate { get; set; }
        public int? ClosedByUserId { get; set; }
        public User? ClosedByUser { get; set; }
    }

    public class TrialBalance
    {
        public int Id { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public DateTime AsOfDate { get; set; }
        public int GeneratedByUserId { get; set; }
        public User? GeneratedByUser { get; set; }
        public List<TrialBalanceLine> Lines { get; set; } = new();
        public decimal TotalDebits => Lines.Sum(l => l.DebitBalance);
        public decimal TotalCredits => Lines.Sum(l => l.CreditBalance);
        public bool IsBalanced => TotalDebits == TotalCredits;
    }

    public class TrialBalanceLine
    {
        public int Id { get; set; }
        public int TrialBalanceId { get; set; }
        public TrialBalance? TrialBalance { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
        public decimal DebitBalance { get; set; } = 0;
        public decimal CreditBalance { get; set; } = 0;
    }
}
