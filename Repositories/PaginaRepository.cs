using Npgsql;
using Naviguard.Connections;
using Naviguard.Models;

namespace Naviguard.Repositories
{
    public class PaginaRepository
    {
        public List<Pagina> ObtenerPaginas() 
        {
            var paginas = new List<Pagina>(); 
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                conn.Open();
                var sql = "SELECT page_id, page_name, url, description, requires_proxy, requires_login, created_at, state FROM browser_app.pages ORDER BY page_name";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paginas.Add(new Pagina 
                        {
                            page_id = Convert.ToInt64(reader["page_id"]),
                            page_name = reader["page_name"].ToString(),
                            url = reader["url"].ToString(),
                            description = reader["description"].ToString(),
                            requires_proxy = Convert.ToBoolean(reader["requires_proxy"]),
                            requires_login = Convert.ToBoolean(reader["requires_login"]),
                            created_at = Convert.ToDateTime(reader["created_at"]),
                            state = Convert.ToInt16(reader["state"])
                        });
                    }
                }
            }
            return paginas;
        }
        public async Task<long> AddPageAsync(Pagina newPage)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = @"
                    INSERT INTO browser_app.pages 
                    (page_name, description, url, requires_proxy, requires_login, created_at, state) 
                    VALUES 
                    (@page_name, @description, @url, @requires_proxy, @requires_login, @created_at, @state)
                    RETURNING page_id;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@page_name", newPage.page_name);
                    cmd.Parameters.AddWithValue("@description", (object)newPage.description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@url", newPage.url);
                    cmd.Parameters.AddWithValue("@requires_proxy", newPage.requires_proxy);
                    cmd.Parameters.AddWithValue("@requires_login", newPage.requires_login);
                    cmd.Parameters.AddWithValue("@created_at", newPage.created_at);
                    cmd.Parameters.AddWithValue("@state", newPage.state);

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
    }
}