namespace Naviguard.BusinessInfo.Models
{
    public class BusinessArea
    {
        public int id_bnsarea { get; set; }
        public string name_area { get; set; }
        public int id_bnsdpt { get; set; }

        public override string ToString() => name_area;

    }
}
