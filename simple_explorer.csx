#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 7.0.0"
#r "nuget: Microsoft.Extensions.DependencyInjection, 7.0.0"

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

// This script provides basic functionality to explore data in the supply chain system
// Run with `dotnet-script simple_explorer.csx`

Console.WriteLine("Supply Chain Finance Explorer Script");
Console.WriteLine("===================================\n");

// Load Core assemblies dynamically
var coreAssembly = System.Reflection.Assembly.LoadFrom("Core/bin/Debug/net8.0/Core.dll");
var dbContextType = coreAssembly.GetType("Core.Data.SupplyChainDbContext");

// Setup services
var services = new ServiceCollection();
services.AddDbContext(dbContextType, options => 
{
    var sqliteMethod = typeof(SqliteDbContextOptionsBuilderExtensions)
        .GetMethod("UseSqlite", new[] { typeof(DbContextOptionsBuilder), typeof(string), typeof(Action<>).MakeGenericType(typeof(SqliteDbContextOptionsBuilder)) });
    
    sqliteMethod.Invoke(null, new object[] { 
        options, 
        "Data Source=supply_chain_finance.db", 
        null 
    });
});

var provider = services.BuildServiceProvider();

// Get context
dynamic context = provider.GetRequiredService(dbContextType);

// Display Organizations
Console.WriteLine("Organizations:");
Console.WriteLine("--------------------------------------------------");
dynamic organizations = context.Organizations;
foreach (var org in organizations)
{
    string type = "";
    if (org.IsBuyer && org.IsSeller) type = " (Buyer & Seller)";
    else if (org.IsBuyer) type = " (Buyer)";
    else if (org.IsSeller) type = " (Seller)";
    else if (org.IsBank) type = " (Bank)";
    else type = " (Other)";
    
    Console.WriteLine($"{org.Id}. {org.Name}{type}");
}

// Display Users
Console.WriteLine("\nUsers:");
Console.WriteLine("--------------------------------------------------");
dynamic users = context.Users;
foreach (var user in users)
{
    Console.WriteLine($"{user.Id}. {user.Name} ({user.Username}) - Organization: {user.Organization?.Name}");
}

// Display Credit Limits
Console.WriteLine("\nCredit Limits:");
Console.WriteLine("--------------------------------------------------");
dynamic creditLimits = context.CreditLimits;
foreach (var limit in creditLimits)
{
    Console.WriteLine($"Limit ID: {limit.Id}, Organization: {limit.Organization?.Name}, Master Limit: ${limit.MasterLimit}");
    
    // Display facilities
    dynamic facilities = context.Facilities.Where(f => f.CreditLimitId == limit.Id);
    foreach (var facility in facilities)
    {
        Console.WriteLine($"  - Facility Type: {facility.Type}, Limit: ${facility.TotalLimit}, Available: ${facility.AvailableLimit}");
    }
}

// Display Invoices
Console.WriteLine("\nInvoices:");
Console.WriteLine("--------------------------------------------------");
dynamic invoices = context.Invoices;
foreach (var invoice in invoices.Take(10)) // Only show first 10 invoices to avoid overwhelming the output
{
    Console.WriteLine($"Invoice: {invoice.InvoiceNumber}, Amount: ${invoice.Amount}, Status: {invoice.Status}");
    Console.WriteLine($"  Seller: {invoice.Seller?.Name}, Buyer: {invoice.Buyer?.Name}");
    Console.WriteLine($"  Issue Date: {invoice.IssueDate.ToShortDateString()}, Due Date: {invoice.DueDate.ToShortDateString()}");
    Console.WriteLine();
}

Console.WriteLine("\nExplorer script completed.");
