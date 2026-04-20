using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using CreditService.Infrastructure.Persistence;

namespace CreditService.Infrastructure.DesignTime
{
    public class CreditDbContextFactory : IDesignTimeDbContextFactory<CreditDbContext>
    {
        public CreditDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CreditDbContext>();

            builder.UseSqlServer("Server=DESKTOP-JPMC7TC\\SQLEXPRESS;Database=CreditDb;Trusted_Connection=True;TrustServerCertificate=True;");

            return new CreditDbContext(builder.Options);
        }
    }
}