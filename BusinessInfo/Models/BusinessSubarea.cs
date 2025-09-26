namespace Naviguard.BusinessInfo.Models
{
    public class BusinessSubarea
    {
        public int id_bnsbar { get; set; }
        public string name_subarea { get; set; }
        public int id_bnsarea { get; set; }

        public override string ToString() => name_subarea;

    }
}
