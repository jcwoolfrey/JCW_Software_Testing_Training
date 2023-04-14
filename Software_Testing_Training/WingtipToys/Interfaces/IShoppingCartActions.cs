using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingtipToys.Interfaces
{
    public interface IShoppingCartActions : IDisposable
    {
        void AddToCart(int id);
        string GetCartId();
    }
}
