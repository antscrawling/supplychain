using System;

namespace Core.Models
{
    public enum FacilityType
    {
        InvoiceFinancing,
        TermLoan,
        Overdraft,
        Guarantee
    }

    public enum UserRole
    {
        BankAdmin,
        BankUser,
        ClientAdmin,
        ClientUser
    }

    public enum InvoiceStatus
    {
        Uploaded,
        BuyerUploaded,   // New: Uploaded by buyer rather than seller
        Validated,
        Approved,
        BuyerApprovalPending, // New: Waiting for buyer's approval of liability transfer
        SellerAcceptancePending, // New: Waiting for seller's acceptance of early payment
        Rejected,
        Funded,
        PartiallyPaid,
        FullyPaid,
        Overdue
    }

    // Static class for Invoice Status string constants
    public static class InvoiceStatusValues
    {
        public const string Uploaded = "Uploaded";
        public const string BuyerUploaded = "BuyerUploaded";
        public const string Validated = "Validated";
        public const string Approved = "Approved";
        public const string BuyerApprovalPending = "BuyerApprovalPending";
        public const string SellerAcceptancePending = "SellerAcceptancePending";
        public const string Rejected = "Rejected";
        public const string Funded = "Funded";
        public const string PartiallyPaid = "PartiallyPaid";
        public const string FullyPaid = "FullyPaid";
        public const string Overdue = "Overdue";
    }

    public enum TransactionType
    {
        InvoiceUpload,
        InvoiceFunding,
        Payment,
        LimitAdjustment,
        FeeCharge,
        TreasuryFunding     // New: Funding from treasury to loans department
    }
    
    public enum NotificationType
    {
        Info,
        Warning,
        Alert,
        BuyerApprovalRequest,   // New: Request for buyer to approve liability transfer
        SellerAcceptanceRequest // New: Request for seller to accept early payment
    }

    // Accounting Enums
    public enum AccountType
    {
        Asset,
        Liability,
        Equity,
        Revenue,
        Expense
    }

    public enum AccountCategory
    {
        // Asset categories
        CurrentAssets,
        FixedAssets,
        IntangibleAssets,
        
        // Liability categories
        CurrentLiabilities,
        LongTermLiabilities,
        
        // Equity categories
        ShareCapital,
        RetainedEarnings,
        OtherEquity,
        
        // Revenue categories
        OperatingRevenue,
        NonOperatingRevenue,
        
        // Expense categories
        OperatingExpenses,
        FinancingExpenses,
        NonOperatingExpenses
    }

    public enum JournalEntryStatus
    {
        Pending,
        Posted,
        Reversed,
        Cancelled
    }
}
