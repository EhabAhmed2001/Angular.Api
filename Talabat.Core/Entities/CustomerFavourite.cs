using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities
{
    public class CustomerFavourite
    {
        public CustomerFavourite(string Id)
        {
            this.Id = Id;
        }
        public string Id { get; set; }
        public List<FavouriteItem> Items { get; set; } = new List<FavouriteItem>();
    }
}
