using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NorMall.Models;

namespace NorMall.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<HomePageConfig> HomePageConfigs => Set<HomePageConfig>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductInCart> ProductsInCart => Set<ProductInCart>();
    public DbSet<ProductOrdered> ProductsOrdered => Set<ProductOrdered>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
}