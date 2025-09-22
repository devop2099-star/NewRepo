namespace Naviguard.Models
{
    public class PageCredential
    {
        public long PageCredentialId { get; set; }
        public long PageId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool State { get; set; }
    }
}