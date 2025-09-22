using Naviguard.Connections;
using Naviguard.Models;
using Npgsql;

namespace Naviguard.Repositories
{
    public class PageCredentialRepository
    {
        public PageCredential? ObtenerCredencialPorPagina(long pageId)
        {
            using var conn = ConexionBD.ObtenerConexionNaviguard();
            conn.Open();

            var sql = "SELECT page_credential_id, page_id, username, password, state " +
                      "FROM browser_app.page_credentials " +
                      "WHERE page_id = @pageId AND state = 1 LIMIT 1;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("pageId", pageId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new PageCredential
                {
                    PageCredentialId = Convert.ToInt64(reader["page_credential_id"]),
                    PageId = Convert.ToInt64(reader["page_id"]),
                    Username = reader["username"].ToString(),
                    Password = reader["password"].ToString(),
                    State = Convert.ToInt32(reader["state"]) == 1
                };
            }

            return null;
        }
    }
}