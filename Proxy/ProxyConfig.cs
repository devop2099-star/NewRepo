using Naviguard.Proxy; // Asegúrate de que el namespace sea el correcto

namespace Naviguard.Models
{
    public static class ProxyConfig
    {
        public static ProxyManager.ProxyInfo CurrentProxy { get; set; }
    }
}