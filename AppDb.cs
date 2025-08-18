using Microsoft.EntityFrameworkCore;
// DbContext
public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }
    public DbSet<Product> Products => Set<Product>();
}
