using Npgsql;
using System.Windows;
using Naviguard.Connections;
using Naviguard.Models;

namespace Naviguard.Proxy
{
    public class ProxyManager
    {
        public ProxyInfo GetProxy()
        {
            try
            {
                using var connection = ConexionBD.ObtenerConexionNaviguard();
                connection.Open();

                string query = "SELECT host, port, username, password FROM browser_app.proxies WHERE proxy_id IS NOT NULL LIMIT 1;";
                using var cmd = new NpgsqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new ProxyInfo
                    {
                        Host = reader["host"].ToString(),
                        Port = Convert.ToInt32(reader["port"]),
                        Username = reader["username"].ToString(),
                        Password = reader["password"].ToString()
                    };

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener proxy: {ex.Message}", "Error");
            }

            return null;
        }
    }
}