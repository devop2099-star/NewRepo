// Naviguard/Models/Pagina.cs
using System;

namespace Naviguard.Models
{
    public class Pagina
    {
        public long page_id { get; set; }
        public string page_name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public bool requires_proxy { get; set; }
        public bool requires_login { get; set; }
        public DateTime created_at { get; set; }
        public short state { get; set; }
        public short pin_in_group { get; set; }

        public string PageName => page_name;
    }
}