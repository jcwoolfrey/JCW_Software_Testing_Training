using System.Data.Entity;
using WingtipToys.Interfaces;

namespace WingtipToys.Models
{
    public class ProductContext : DbContext, IProductContext
    {
        public ProductContext() : base("WingtipToys")
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> ShoppingCartItems { get; set; }
    }
}