using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dahmira.Models
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public DateTime Date { get; set; }
        public List<Category> Categories { get; set; }
    }
}
