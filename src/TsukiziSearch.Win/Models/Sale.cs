using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TsukiziSearch.Win.Models
{
    class Sale
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Section { get; set; }
        public string Kind { get; set; }
        public string Market { get; set; }
        public string Name { get; set; }
        public string Method { get; set; }
        public double Weight { get; set; }
    }
}
