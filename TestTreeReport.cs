using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Data;
using Core.Services;

class TestTreeReport
{
    static void Main()
    {
        // Configure services
        var services = new ServiceCollection();

        // Database file path
        string dbFilePath = Path.Combine(Directory.GetCurrentDirectory(), "supply_chain_finance.db");

        // Register database context
        services.AddDbContext<SupplyChainDbContext>(options =>
            options.UseSqlite($"Data Source={dbFilePath}"));

        // Register services
        services.AddScoped<MyLimitService>();

        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var limitService = scope.ServiceProvider.GetRequiredService<MyLimitService>();
            
            Console.WriteLine("Testing the new tree report with visibility rules...\n");
            
            // Test with organization ID 3 (Supply Solutions Ltd) which should exist
            try
            {
                string treeReport = limitService.GenerateLimitTreeReportWithVisibilityRules(3);
                Console.WriteLine("=== TREE FORMAT ===");
                Console.WriteLine(treeReport);
                Console.WriteLine("\n" + new string('=', 50) + "\n");
                
                string tabularReport = limitService.GenerateLimitReportWithVisibilityRules(3);
                Console.WriteLine("=== TABULAR FORMAT ===");
                Console.WriteLine(tabularReport);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
