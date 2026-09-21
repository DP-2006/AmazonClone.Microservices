using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Storage.Infrastructure.Data;

public sealed class StorageDbContextFactory : IDesignTimeDbContextFactory<StorageDbContext>
{
    public StorageDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StorageDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=storage_db;Username=postgres;Password=postgres")
            .Options;

        return new StorageDbContext(options);
    }
}
