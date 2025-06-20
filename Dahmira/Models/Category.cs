using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class Category
    {
        public string Title { get; set; }
        public List<Subcategory> Subcategories { get; set; }
    }
}
