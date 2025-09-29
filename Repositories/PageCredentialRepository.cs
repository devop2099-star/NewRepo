using Naviguard.Connections;
using Naviguard.Models;
using Npgsql;
using System.Diagnostics;

namespace Naviguard.Repositories
{
    public class PageCredentialRepository
    {
        public async Task<PageCredential?> ObtenerCredencialPorPaginaAsync(long pageId)
        {
            // <--- DEBUG: Confirmamos que el método se llamó y con qué pageId
            Debug.WriteLine($"[PageCredentialRepo] 🔎 Buscando en BD credencial para PageId: {pageId}");

            using var conn = ConexionBD.ObtenerConexionNaviguard();
            await conn.OpenAsync(); // Es mejor usar la versión async

            var sql = "SELECT page_credential_id, page_id, username, password, state " +
                      "FROM browser_app.page_credentials " +
                      "WHERE page_id = @pageId AND state = 1 LIMIT 1;";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("pageId", pageId);

            using var reader = await cmd.ExecuteReaderAsync(); // Mejor async
            if (await reader.ReadAsync())
            {
                // <--- DEBUG: Confirmamos que la BD devolvió un registro
                Debug.WriteLine($"[PageCredentialRepo] ✅ Registro encontrado en BD para PageId: {pageId}. Usuario: '{reader["username"]}'");
                return new PageCredential
                {
                    PageCredentialId = Convert.ToInt64(reader["page_credential_id"]),
                    PageId = Convert.ToInt64(reader["page_id"]),
                    Username = reader["username"].ToString(),
                    Password = reader["password"].ToString(),
                    State = Convert.ToInt32(reader["state"]) == 1
                };
            }

            // <--- DEBUG: Si llegamos aquí, la BD no devolvió nada
            Debug.WriteLine($"[PageCredentialRepo] ❌ No se encontró registro en BD para PageId: {pageId}");
            return null;
        }
    }
}