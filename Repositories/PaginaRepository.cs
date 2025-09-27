using Npgsql;
using Naviguard.Connections;
using Naviguard.Models;
using System.Diagnostics;

namespace Naviguard.Repositories
{
    public class PaginaRepository
    {
        public async Task<long> AddPageAsync(Pagina newPage)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = @"
                INSERT INTO browser_app.pages 
                (page_name, description, url, requires_proxy, requires_login, requires_custom_login, state, created_at) 
                VALUES 
                (@page_name, @description, @url, @requires_proxy, @requires_login, @requires_custom_login, @state, @created_at)
                RETURNING page_id;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@page_name", newPage.page_name);
                    cmd.Parameters.AddWithValue("@description", (object)newPage.description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@url", newPage.url);
                    cmd.Parameters.AddWithValue("@requires_proxy", newPage.requires_proxy);
                    cmd.Parameters.AddWithValue("@requires_login", newPage.requires_login);
                    cmd.Parameters.AddWithValue("@requires_custom_login", newPage.requires_custom_login);
                    cmd.Parameters.AddWithValue("@state", newPage.state);
                    cmd.Parameters.AddWithValue("@created_at", newPage.created_at);

                    var newId = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt64(newId);
                }
            }
        }
        public async Task AddCredentialAsync(long pageId, string username, string password)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = @"
                    INSERT INTO browser_app.page_credentials
                    (page_id, username, password, created_at, state)
                    VALUES
                    (@page_id, @username, @password, @created_at, @state);";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@page_id", pageId);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", (object)password ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@state", 1);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<long> AddGroupAsync(string groupName, string description)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = @"
                    INSERT INTO browser_app.page_groups
                    (group_name, description, created_at, state)
                    VALUES
                    (@group_name, @description, @created_at, @state)
                    RETURNING group_id;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@group_name", groupName);
                    cmd.Parameters.AddWithValue("@description", (object)description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    cmd.Parameters.AddWithValue("@state", 1);

                    var newId = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt64(newId);
                }
            }
        }
        public async Task AddPagesToGroupAsync(long groupId, List<long> pageIds)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();

                foreach (var pageId in pageIds)
                {
                    var sql = @"
                        INSERT INTO browser_app.group_pages
                        (group_id, page_id, state)
                        VALUES
                        (@group_id, @page_id, @state);";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@group_id", groupId);
                        cmd.Parameters.AddWithValue("@page_id", pageId);
                        cmd.Parameters.AddWithValue("@state", 1);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task UpdatePageAsync(Pagina page)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();

                var sql = @"
            UPDATE browser_app.pages
            SET 
                page_name = @page_name,
                description = @description,
                url = @url,
                requires_proxy = @requires_proxy,
                requires_login = @requires_login,
                requires_custom_login = @requires_custom_login
            WHERE 
                page_id = @page_id;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@page_name", page.page_name);
                    cmd.Parameters.AddWithValue("@description", (object)page.description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@url", page.url);
                    cmd.Parameters.AddWithValue("@requires_proxy", page.requires_proxy);
                    cmd.Parameters.AddWithValue("@requires_login", page.requires_login);
                    cmd.Parameters.AddWithValue("@requires_custom_login", page.requires_custom_login);

                    cmd.Parameters.AddWithValue("@page_id", page.page_id);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public List<Pagina> ObtenerPaginas()
        {
            var paginas = new List<Pagina>();
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                conn.Open();
                var sql = @"
            SELECT page_id, page_name, description, url, 
                   requires_proxy, requires_login, requires_custom_login, 
                   state, created_at 
            FROM browser_app.pages 
            WHERE state = 1 
            ORDER BY page_name;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paginas.Add(new Pagina
                        {
                            page_id = Convert.ToInt64(reader["page_id"]),
                            page_name = reader["page_name"].ToString(),
                            description = reader["description"]?.ToString(),
                            url = reader["url"].ToString(),
                            requires_proxy = Convert.ToBoolean(reader["requires_proxy"]),
                            requires_login = Convert.ToBoolean(reader["requires_login"]),
                            requires_custom_login = Convert.ToBoolean(reader["requires_custom_login"]),
                            state = Convert.ToInt16(reader["state"]),
                            created_at = Convert.ToDateTime(reader["created_at"])
                        });
                    }
                }
            }
            return paginas;
        }

        public async Task<PageCredential> GetCredentialForPageAsync(long pageId)
        {
            Debug.WriteLine($"--- [REPOSITORIO] Buscando credenciales para PageID: {pageId} ---");
            PageCredential credential = null;

            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = "SELECT username, password FROM browser_app.page_credentials WHERE page_id = @page_id AND state = 1 LIMIT 1";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@page_id", pageId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Debug.WriteLine("[REPOSITORIO] ¡Éxito! Se encontró una credencial en la base de datos.");
                            string dbUsername = reader["username"].ToString();
                            string dbPassword = reader["password"].ToString();
                            Debug.WriteLine($"[REPOSITORIO] -> Username de BD: '{dbUsername}', Password de BD: '{dbPassword}'");

                            credential = new PageCredential
                            {
                                Username = dbUsername,
                                Password = dbPassword
                            };
                        }
                        else
                        {
                            Debug.WriteLine("[REPOSITORIO] AVISO: La consulta no devolvió filas. No se encontró ninguna credencial para este PageID.");
                        }
                    }
                }
            }

            Debug.WriteLine($"--- [REPOSITORIO] El método retornará {(credential == null ? "NULL" : "un objeto PageCredential")} ---");
            return credential;
        }

        public async Task UpdateOrInsertCredentialAsync(long pageId, string username, string password)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = @"
                    INSERT INTO browser_app.page_credentials (page_id, username, password, created_at, state)
                    VALUES (@page_id, @username, @password, @created_at, 1)
                    ON CONFLICT (page_id) DO UPDATE SET
                        username = EXCLUDED.username,
                        password = EXCLUDED.password;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@page_id", pageId);
                    cmd.Parameters.AddWithValue("@username", (object)username ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@password", (object)password ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task SoftDeletePageAsync(long pageId)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = "UPDATE browser_app.pages SET state = 0 WHERE page_id = @page_id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@page_id", pageId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Pagina>> GetPagesRequiringCustomLoginAsync()
        {
            Debug.WriteLine("[PaginaRepository] Buscando páginas con login personalizado...");
            var paginas = new List<Pagina>();
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = @"SELECT page_id, page_name, description, url, 
                           requires_proxy, requires_login, requires_custom_login, 
                           state, created_at 
                    FROM browser_app.pages 
                    WHERE requires_custom_login = true AND state = 1 
                    ORDER BY page_name;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        paginas.Add(new Pagina
                        {
                            page_id = reader.GetInt64(reader.GetOrdinal("page_id")),
                            page_name = reader.GetString(reader.GetOrdinal("page_name")),
                            description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                            url = reader.GetString(reader.GetOrdinal("url")),
                            requires_proxy = reader.GetBoolean(reader.GetOrdinal("requires_proxy")),
                            requires_login = reader.GetBoolean(reader.GetOrdinal("requires_login")),
                            requires_custom_login = reader.GetBoolean(reader.GetOrdinal("requires_custom_login")),
                            state = reader.GetInt16(reader.GetOrdinal("state")),
                            created_at = reader.GetDateTime(reader.GetOrdinal("created_at"))
                        });
                    }
                }
            }
            Debug.WriteLine($"[PaginaRepository] Consulta finalizada. Se encontraron {paginas.Count} páginas."); // DEBUG
            return paginas;
        }

    }
}