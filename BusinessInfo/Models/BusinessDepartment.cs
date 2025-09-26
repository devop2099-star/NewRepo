using System.Xml.Linq;

namespace Naviguard.BusinessInfo.Models
{
    public class BusinessDepartment
    {
        public int id_bnsdpt { get; set; }
        public string name_department { get; set; }

        public override string ToString() => name_department;
    }
}
