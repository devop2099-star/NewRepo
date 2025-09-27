namespace Naviguard.Models
{
    public class UserPageCredential
    {
        public long pagreqlg_id { get; set; }
        public long external_user_id { get; set; }
        public long page_id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}