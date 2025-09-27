using Naviguard.Connections;
using Naviguard.Models;
using Npgsql;
using System.Diagnostics;

namespace Naviguard.Repositories
{
    public class CredentialRepository
    {
        public async Task<UserPageCredential> GetCredentialAsync(long userId, long pageId)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = @"SELECT pagreqlg_id, external_user_id, page_id, username, password 
                            FROM browser_app.pages_requires_login 
                            WHERE external_user_id = @userId AND page_id = @pageId AND state = 1";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@pageId", pageId);

                    Debug.WriteLine("--- Ejecutando Consulta SQL ---");
                    Debug.WriteLine(cmd.CommandText); 
                    foreach (NpgsqlParameter p in cmd.Parameters)
                    {
                        Debug.WriteLine($"--> {p.ParameterName}: {p.Value}");
                    }
                    Debug.WriteLine("-----------------------------");

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new UserPageCredential
                            {
                                pagreqlg_id = reader.GetInt64(0),
                                external_user_id = reader.GetInt64(1),
                                page_id = reader.GetInt64(2),
                                username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                password = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task UpdateOrInsertCredentialAsync(long userId, long pageId, string username, string password)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = @"
                    INSERT INTO browser_app.pages_requires_login (external_user_id, page_id, username, password, state)
                    VALUES (@userId, @pageId, @username, @password, 1)
                    ON CONFLICT (external_user_id, page_id) DO UPDATE SET
                        username = EXCLUDED.username,
                        password = EXCLUDED.password;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@pageId", pageId);
                    cmd.Parameters.AddWithValue("@username", (object)username ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@password", (object)password ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}