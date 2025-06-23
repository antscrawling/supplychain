using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Core.Data
{
    public class SupplyChainDbContextFactory : IDesignTimeDbContextFactory<SupplyChainDbContext>
    {
        public SupplyChainDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SupplyChainDbContext>();
            optionsBuilder.UseSqlite("Data Source=../supply_chain_finance.db");

            return new SupplyChainDbContext(optionsBuilder.Options);
        }
    }
}
