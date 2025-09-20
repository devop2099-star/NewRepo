using Npgsql;
using System.Windows;
using Naviguard.Connections; // Importa el namespace de tu clase de conexión

namespace Naviguard.Proxy
{
    public class ProxyManager
    {
        // Clase interna para contener la información del proxy
        public class ProxyInfo
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }

            // Método para obtener la cadena de dirección del proxy
            public string GetProxyString() => $"{Host}:{Port}";
        }

        public ProxyInfo GetProxy()
        {
            try
            {
                // Usa tu clase de conexión estática para obtener una conexión
                using var connection = ConexionBD.ObtenerConexionNaviguard();
                connection.Open();

                // La consulta se actualiza para incluir el esquema de la tabla
                // y seleccionar las columnas correctas
                string query = "SELECT host, port, username, password FROM browser_app.proxies WHERE proxy_id IS NOT NULL LIMIT 1;";
                using var cmd = new NpgsqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    // Lee los valores de las columnas y los asigna a un nuevo objeto ProxyInfo
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
                // Captura y muestra errores de conexión o lectura
                MessageBox.Show($"Error al obtener proxy: {ex.Message}", "Error");
            }

            // Devuelve null si no se encuentra ningún proxy o si ocurre un error
            return null;
        }
    }
}