#nullable enable
using System;
using Microsoft.EntityFrameworkCore;

namespace BatchProcessor.Functions.Infrastructure
{
    public static class DbContextFactory
    {
        public static LocalCreditDbContext Create(string? connectionString = null)
        {
            var builder = new DbContextOptionsBuilder();
            var conn = connectionString ?? Environment.GetEnvironmentVariable("SqlConnection")
                       ?? "Server=localhost;Database=CreditDb;Trusted_Connection=True;TrustServerCertificate=True;";
            builder.UseSqlServer(conn);
            return new LocalCreditDbContext(builder.Options);
        }
    }
}