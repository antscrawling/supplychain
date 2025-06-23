PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "Counterparties" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Counterparties" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "TaxId" TEXT NOT NULL,
    "Address" TEXT NOT NULL,
    "ContactPerson" TEXT NOT NULL,
    "ContactEmail" TEXT NOT NULL,
    "ContactPhone" TEXT NOT NULL,
    "IsBuyer" INTEGER NOT NULL,
    "IsSeller" INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS "Organizations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Organizations" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "TaxId" TEXT NOT NULL,
    "Address" TEXT NOT NULL,
    "ContactPerson" TEXT NOT NULL,
    "ContactEmail" TEXT NOT NULL,
    "ContactPhone" TEXT NOT NULL,
    "IsBuyer" INTEGER NOT NULL,
    "IsSeller" INTEGER NOT NULL,
    "IsBank" INTEGER NOT NULL
);
INSERT INTO Organizations VALUES(1,'Global Finance Bank','GB123456789','100 Wall Street, New York','John Finance','john@globalbank.com','555-1234',0,0,1);
INSERT INTO Organizations VALUES(2,'MegaCorp Industries','MC987654321','200 Industry Avenue, Chicago','Alice Purchasing','alice@megacorp.com','555-5678',1,0,0);
INSERT INTO Organizations VALUES(3,'Supply Solutions Ltd','SS123789456','300 Supplier Road, Boston','Bob Selling','bob@supplysolutions.com','555-9012',0,1,0);
INSERT INTO Organizations VALUES(4,'Milo Seller Corporation','S7484717Z','13 Holland Drive, #10-40 Singapore 271013','Jose Ibay','ourcatisfat@gmail.com','94747674',0,1,0);
INSERT INTO Organizations VALUES(5,'Ovaltine Buyer Corporation','SHJDHHD','2 H S Olympic Singapore 28474','Jay Ocampo','f@gmail.com','8484784',1,0,0);
CREATE TABLE IF NOT EXISTS "AccountStatements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AccountStatements" PRIMARY KEY AUTOINCREMENT,
    "OrganizationId" INTEGER NOT NULL,
    "StartDate" TEXT NOT NULL,
    "EndDate" TEXT NOT NULL,
    "GenerationDate" TEXT NOT NULL,
    "OpeningBalance" TEXT NOT NULL,
    "ClosingBalance" TEXT NOT NULL,
    "StatementNumber" TEXT NOT NULL,
    CONSTRAINT "FK_AccountStatements_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE CASCADE
);
INSERT INTO AccountStatements VALUES(1,3,'2025-06-01 00:00:00','2025-06-21 10:54:31.9341424','2025-06-21 10:54:31.9465039','0.0','0.0','STMT-2025-06-0003');
INSERT INTO AccountStatements VALUES(2,2,'2025-06-01 00:00:00','2025-06-21 10:55:34.1567484','2025-06-21 10:55:34.1901521','0.0','0.0','STMT-2025-06-0002');
CREATE TABLE IF NOT EXISTS "CreditLimits" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_CreditLimits" PRIMARY KEY AUTOINCREMENT,
    "OrganizationId" INTEGER NOT NULL,
    "MasterLimit" TEXT NOT NULL,
    CONSTRAINT "FK_CreditLimits_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE CASCADE
);
INSERT INTO CreditLimits VALUES(1,2,'1000000.0');
CREATE TABLE IF NOT EXISTS "Invoices" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Invoices" PRIMARY KEY AUTOINCREMENT,
    "InvoiceNumber" TEXT NOT NULL,
    "IssueDate" TEXT NOT NULL,
    "DueDate" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "SellerId" INTEGER NULL,
    "BuyerId" INTEGER NULL,
    "CounterpartyId" INTEGER NULL,
    "Currency" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "FundingDate" TEXT NULL,
    "FundedAmount" TEXT NULL,
    "DiscountRate" TEXT NULL,
    "PaymentDate" TEXT NULL,
    "PaidAmount" TEXT NULL,
    "BuyerApproved" INTEGER NOT NULL,
    "BuyerApprovalDate" TEXT NULL,
    "BuyerApprovalUserId" INTEGER NULL,
    "SellerAccepted" INTEGER NOT NULL,
    "SellerAcceptanceDate" TEXT NULL,
    "SellerAcceptanceUserId" INTEGER NULL,
    "RejectionReason" TEXT NULL,
    CONSTRAINT "FK_Invoices_Counterparties_CounterpartyId" FOREIGN KEY ("CounterpartyId") REFERENCES "Counterparties" ("Id"),
    CONSTRAINT "FK_Invoices_Organizations_BuyerId" FOREIGN KEY ("BuyerId") REFERENCES "Organizations" ("Id"),
    CONSTRAINT "FK_Invoices_Organizations_SellerId" FOREIGN KEY ("SellerId") REFERENCES "Organizations" ("Id")
);
INSERT INTO Invoices VALUES(1,'INV-2025-001','2025-06-11 10:53:18.5966497','2025-08-10 10:53:18.5966781','130000.0','Q2 Product Shipment',3,2,NULL,'USD',3,NULL,NULL,NULL,NULL,NULL,0,NULL,NULL,0,NULL,NULL,NULL);
INSERT INTO Invoices VALUES(2,'INV-2025-002','2025-06-16 10:53:18.5967733','2025-08-15 10:53:18.5967735','87500.0','Marketing Services',3,2,NULL,'USD',7,'2025-06-22 16:07:31.2609482','84000.0','4.0',NULL,NULL,0,NULL,NULL,0,NULL,NULL,NULL);
INSERT INTO Invoices VALUES(3,'INV-2025-003','2025-05-01 00:00:00','2025-07-09 00:00:00','12345.0',',muj',3,2,NULL,'USD',3,NULL,NULL,NULL,NULL,NULL,0,NULL,NULL,0,NULL,NULL,NULL);
INSERT INTO Invoices VALUES(4,'INV-2025-B01','2025-01-01 00:00:00','2025-09-09 00:00:00','22222.0','jj',3,2,NULL,'USD',1,NULL,NULL,NULL,NULL,NULL,0,NULL,NULL,0,NULL,NULL,NULL);
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Username" TEXT NOT NULL,
    "Password" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Role" INTEGER NOT NULL,
    "OrganizationId" INTEGER NULL,
    CONSTRAINT "FK_Users_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id")
);
INSERT INTO Users VALUES(1,'bankadmin','password','Bank Administrator','admin@globalbank.com',0,1);
INSERT INTO Users VALUES(2,'bankuser','password','Bank User','user@globalbank.com',1,1);
INSERT INTO Users VALUES(3,'buyeradmin','password','Buyer Administrator','admin@megacorp.com',2,2);
INSERT INTO Users VALUES(4,'buyeruser','password','Buyer User','user@megacorp.com',3,2);
INSERT INTO Users VALUES(5,'selleradmin','password','Seller Administrator','admin@supplysolutions.com',2,3);
INSERT INTO Users VALUES(6,'selleruser','password','Seller User','user@supplysolutions.com',3,3);
INSERT INTO Users VALUES(7,'miloadmin','password','James Virata','o@gmail.com',2,4);
INSERT INTO Users VALUES(8,'ovaltineadmin','password','Julius Lizardo','f@gmail.com',2,5);
CREATE TABLE IF NOT EXISTS "Facilities" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Facilities" PRIMARY KEY AUTOINCREMENT,
    "CreditLimitInfoId" INTEGER NOT NULL,
    "Type" INTEGER NOT NULL,
    "TotalLimit" TEXT NOT NULL,
    "CurrentUtilization" TEXT NOT NULL,
    "ReviewEndDate" TEXT NOT NULL,
    "GracePeriodDays" INTEGER NOT NULL,
    "RelatedPartyId" INTEGER NULL,
    "AllocatedLimit" TEXT NOT NULL,
    CONSTRAINT "FK_Facilities_CreditLimits_CreditLimitInfoId" FOREIGN KEY ("CreditLimitInfoId") REFERENCES "CreditLimits" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Facilities_Organizations_RelatedPartyId" FOREIGN KEY ("RelatedPartyId") REFERENCES "Organizations" ("Id")
);
INSERT INTO Facilities VALUES(1,1,0,'250000.0','0.0','2025-09-21 10:53:18.5766659',5,NULL,'0.0');
INSERT INTO Facilities VALUES(2,1,1,'250000.0','0.0','2025-12-21 10:53:18.5784812',5,NULL,'0.0');
INSERT INTO Facilities VALUES(3,1,2,'250000.0','0.0','2025-07-21 10:53:18.5784822',5,NULL,'0.0');
INSERT INTO Facilities VALUES(4,1,3,'250000.0','0.0','2026-06-21 10:53:18.5784823',5,NULL,'0.0');
INSERT INTO Facilities VALUES(5,1,0,'100000.0','0.0','2026-06-22 23:33:23.276394',5,NULL,'0.0');
CREATE TABLE IF NOT EXISTS "Transactions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY AUTOINCREMENT,
    "Type" INTEGER NOT NULL,
    "FacilityType" INTEGER NOT NULL,
    "OrganizationId" INTEGER NOT NULL,
    "InvoiceId" INTEGER NULL,
    "Description" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "InterestOrDiscountRate" TEXT NULL,
    "TransactionDate" TEXT NOT NULL,
    "MaturityDate" TEXT NOT NULL,
    "IsPaid" INTEGER NOT NULL,
    "PaymentDate" TEXT NULL,
    "AccountStatementId" INTEGER NULL,
    CONSTRAINT "FK_Transactions_AccountStatements_AccountStatementId" FOREIGN KEY ("AccountStatementId") REFERENCES "AccountStatements" ("Id"),
    CONSTRAINT "FK_Transactions_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id"),
    CONSTRAINT "FK_Transactions_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE CASCADE
);
INSERT INTO Transactions VALUES(1,0,0,3,3,'Invoice INV-2025-003 uploaded','12345.0',NULL,'2025-06-21 10:54:07.3798112','2025-07-09 00:00:00',0,NULL,1);
INSERT INTO Transactions VALUES(2,0,0,2,4,'Invoice INV-2025-B01 uploaded by buyer','22222.0',NULL,'2025-06-21 10:55:17.9511996','2025-09-09 00:00:00',0,NULL,2);
CREATE TABLE IF NOT EXISTS "Notifications" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Notifications" PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "Title" TEXT NOT NULL,
    "Message" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "IsRead" INTEGER NOT NULL,
    "Type" TEXT NOT NULL,
    "InvoiceId" INTEGER NULL,
    "RequiresAction" INTEGER NOT NULL,
    "ActionTaken" INTEGER NOT NULL,
    "ActionDate" TEXT NULL,
    "ActionResponse" TEXT NULL,
    CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);
INSERT INTO Notifications VALUES(1,5,'New Invoice Received','Buyer MegaCorp Industries has uploaded invoice INV-2025-B01 for $22,222.00 due on 09/09/2025','2025-06-21 10:55:17.9188565',0,'Info',4,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(2,6,'New Invoice Received','Buyer MegaCorp Industries has uploaded invoice INV-2025-B01 for $22,222.00 due on 09/09/2025','2025-06-21 10:55:17.9276724',0,'Info',4,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(3,1,'New Buyer-Uploaded Invoice','Buyer MegaCorp Industries has uploaded invoice INV-2025-B01 for seller Supply Solutions Ltd for $22,222.00. You can now offer early payment to the seller.','2025-06-21 10:55:17.9483693',1,'Info',4,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(4,2,'New Buyer-Uploaded Invoice','Buyer MegaCorp Industries has uploaded invoice INV-2025-B01 for seller Supply Solutions Ltd for $22,222.00. You can now offer early payment to the seller.','2025-06-21 10:55:17.9484534',0,'Info',4,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(5,5,'Invoice Validated','Invoice INV-2025-003 has been validated and is pending approval.','2025-06-22 15:59:24.4932863',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(6,6,'Invoice Validated','Invoice INV-2025-003 has been validated and is pending approval.','2025-06-22 15:59:24.5038949',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(7,5,'Invoice Validated','Invoice INV-2025-001 has been validated and is pending approval.','2025-06-22 15:59:42.8430857',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(8,6,'Invoice Validated','Invoice INV-2025-001 has been validated and is pending approval.','2025-06-22 15:59:42.8461871',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(9,5,'Invoice Validated','Invoice INV-2025-002 has been validated and is pending approval.','2025-06-22 15:59:52.648675',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(10,6,'Invoice Validated','Invoice INV-2025-002 has been validated and is pending approval.','2025-06-22 15:59:52.6503887',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(11,5,'Invoice Approved','Invoice INV-2025-003 has been approved and is ready for funding.','2025-06-22 16:00:10.2475193',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(12,6,'Invoice Approved','Invoice INV-2025-003 has been approved and is ready for funding.','2025-06-22 16:00:10.2486468',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(13,5,'Invoice Approved','Invoice INV-2025-002 has been approved and is ready for funding.','2025-06-22 16:00:20.7307913',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(14,6,'Invoice Approved','Invoice INV-2025-002 has been approved and is ready for funding.','2025-06-22 16:00:20.7329368',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(15,5,'Invoice Approved','Invoice INV-2025-001 has been approved and is ready for funding.','2025-06-22 16:00:32.1012568',0,'Info',NULL,0,0,NULL,NULL);
INSERT INTO Notifications VALUES(16,6,'Invoice Approved','Invoice INV-2025-001 has been approved and is ready for funding.','2025-06-22 16:00:32.1018323',0,'Info',NULL,0,0,NULL,NULL);
CREATE TABLE IF NOT EXISTS "__EFMigrationsLock" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK___EFMigrationsLock" PRIMARY KEY,
    "Timestamp" TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);
DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('Organizations',5);
INSERT INTO sqlite_sequence VALUES('Users',8);
INSERT INTO sqlite_sequence VALUES('CreditLimits',1);
INSERT INTO sqlite_sequence VALUES('Facilities',5);
INSERT INTO sqlite_sequence VALUES('Invoices',4);
INSERT INTO sqlite_sequence VALUES('Transactions',2);
INSERT INTO sqlite_sequence VALUES('AccountStatements',2);
INSERT INTO sqlite_sequence VALUES('Notifications',16);
CREATE INDEX "IX_AccountStatements_OrganizationId" ON "AccountStatements" ("OrganizationId");
CREATE INDEX "IX_CreditLimits_OrganizationId" ON "CreditLimits" ("OrganizationId");
CREATE INDEX "IX_Facilities_CreditLimitInfoId" ON "Facilities" ("CreditLimitInfoId");
CREATE INDEX "IX_Facilities_RelatedPartyId" ON "Facilities" ("RelatedPartyId");
CREATE INDEX "IX_Invoices_BuyerId" ON "Invoices" ("BuyerId");
CREATE INDEX "IX_Invoices_CounterpartyId" ON "Invoices" ("CounterpartyId");
CREATE INDEX "IX_Invoices_SellerId" ON "Invoices" ("SellerId");
CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");
CREATE INDEX "IX_Transactions_AccountStatementId" ON "Transactions" ("AccountStatementId");
CREATE INDEX "IX_Transactions_InvoiceId" ON "Transactions" ("InvoiceId");
CREATE INDEX "IX_Transactions_OrganizationId" ON "Transactions" ("OrganizationId");
CREATE INDEX "IX_Users_OrganizationId" ON "Users" ("OrganizationId");
COMMIT;
