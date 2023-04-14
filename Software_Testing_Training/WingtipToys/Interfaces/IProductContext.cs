using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using WingtipToys.Models;

namespace WingtipToys.Interfaces
{
    public interface IProductContext : IDisposable, IObjectContextAdapter
    {
        DbSet<Category> Categories { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<CartItem> ShoppingCartItems { get; set; }

        int SaveChanges();
    }
}
