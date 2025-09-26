using Naviguard.Connections;
using Naviguard.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Naviguard.Repositories
{
    public class UserAssignmentRepository
    {
        public async Task<List<Group>> GetGroupsByUserIdAsync(int userId)
        {
            var groups = new List<Group>();
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();

                var sql = @"
            SELECT pg.group_id, pg.group_name, pg.description, pg.pin -- <-- 1. Añadimos la columna 'pin'
            FROM browser_app.page_groups pg
            INNER JOIN browser_app.user_page_groups upg ON pg.group_id = upg.group_id
            WHERE upg.external_user_id = @user_id AND upg.state = 1 AND pg.state = 1
            ORDER BY pg.pin DESC, pg.group_name; -- <-- 2. Ordenamos por 'pin' primero
        ";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            groups.Add(new Group
                            {
                                group_id = Convert.ToInt64(reader["group_id"]),
                                group_name = reader["group_name"].ToString(),
                                description = reader["description"]?.ToString(),
                                pin = reader["pin"] == DBNull.Value ? (short)0 : Convert.ToInt16(reader["pin"]) // <-- 3. Asignamos el valor de 'pin'
                            });
                        }
                    }
                }
            }
            return groups;
        }

        public async Task AssignGroupsToUserAsync(int userId, List<long> groupIds)
        {
            if (!groupIds.Any()) return;

            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        var sql = @"
                            INSERT INTO browser_app.user_page_groups (external_user_id, group_id, assigned_at, state)
                            VALUES (@user_id, @group_id, NOW(), 1)
                            ON CONFLICT (external_user_id, group_id) DO NOTHING;
                        ";

                        foreach (var groupId in groupIds)
                        {
                            using (var cmd = new NpgsqlCommand(sql, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@user_id", userId);
                                cmd.Parameters.AddWithValue("@group_id", groupId);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task RemoveGroupFromUserAsync(int userId, long groupId)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                var sql = @"
            UPDATE browser_app.user_page_groups 
            SET state = 0 
            WHERE external_user_id = @user_id AND group_id = @group_id;
        ";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    cmd.Parameters.AddWithValue("@group_id", groupId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}