using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Npgsql;
using System.Windows;

namespace Naviguard.Connections
{
    public static class ConexionBD
    {
        private static IConfiguration _config;
        static ConexionBD()
        {
            try
            {
                _config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("Connections/appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar configuración: " + ex.Message);
                throw;
            }
        }

        public static NpgsqlConnection ObtenerConexionNaviguard()
        {
            string connStr = _config.GetConnectionString("naviguard");
            if (string.IsNullOrEmpty(connStr))
            {
                throw new InvalidOperationException("La cadena de conexión 'naviguard' no se encontró en appsettings.json.");
            }
            return new NpgsqlConnection(connStr);
        }

        public static MySqlConnection ObtenerConexionNxEcosystem()
        {
            string connStr = _config.GetConnectionString("BD_NexusEcosystem");
            if (string.IsNullOrEmpty(connStr))
            {
                throw new InvalidOperationException("La cadena de conexión 'BD_NexusEcosystem' no se encontró en appsettings.json.");
            }
            return new MySqlConnection(connStr);
        }
    }
}