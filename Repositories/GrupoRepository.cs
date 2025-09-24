using Npgsql;
using Naviguard.Connections;
using Naviguard.Models;
using System.Collections.ObjectModel;

namespace Naviguard.Repositories
{
    public class GrupoRepository
    {
        public List<Pagina> ObtenerPaginasPorGrupo(long groupId)
        {
            var paginas = new List<Pagina>();
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                conn.Open();
                var sql = @"
            SELECT p.page_id, p.page_name, p.url, p.requires_proxy, p.requires_login, p.state
            FROM browser_app.pages p
            INNER JOIN browser_app.group_pages gp ON p.page_id = gp.page_id
            WHERE gp.group_id = @group_id AND p.state = 1
            ORDER BY p.page_name;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@group_id", groupId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            paginas.Add(new Pagina
                            {
                                page_id = Convert.ToInt64(reader["page_id"]),
                                page_name = reader["page_name"].ToString(),
                                url = reader["url"].ToString(),
                                requires_proxy = Convert.ToBoolean(reader["requires_proxy"]),
                                requires_login = Convert.ToBoolean(reader["requires_login"]),
                                state = Convert.ToInt16(reader["state"])
                            });
                        }
                    }
                }
            }
            return paginas;
        }

        public List<Group> ObtenerGrupos()
        {
            var grupos = new List<Group>();
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                conn.Open();
                // Esta consulta es más simple: no une con las páginas
                var sql = @"
            SELECT group_id, group_name, description 
            FROM browser_app.page_groups 
            WHERE state = 1 
            ORDER BY group_name;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        grupos.Add(new Group
                        {
                            group_id = Convert.ToInt64(reader["group_id"]),
                            group_name = reader["group_name"].ToString()!,
                            description = reader["description"]?.ToString()
                        });
                    }
                }
            }
            return grupos;
        }

        public List<Group> ObtenerGruposConPaginas()
        {
            var grupos = new Dictionary<long, Group>();
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                conn.Open();
                var sql = @"
            SELECT pg.group_id, pg.group_name, pg.description, pg.pin,
                   p.page_id, p.page_name, p.url, p.requires_proxy, p.requires_login
            FROM browser_app.page_groups pg
            LEFT JOIN browser_app.group_pages gp ON pg.group_id = gp.group_id
            LEFT JOIN browser_app.pages p ON gp.page_id = p.page_id
            WHERE pg.state = 1 AND (p.state = 1 OR p.page_id IS NULL)
            ORDER BY pg.pin DESC, pg.group_name, p.page_name;";

                using (var cmd = new NpgsqlCommand(sql, conn))

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long groupId = Convert.ToInt64(reader["group_id"]);
                        if (!grupos.ContainsKey(groupId))
                        {
                            grupos[groupId] = new Group
                            {
                                group_id = groupId,
                                group_name = reader["group_name"].ToString(),
                                description = reader["description"]?.ToString(),
                                pin = reader["pin"] == DBNull.Value ? (short)0 : Convert.ToInt16(reader["pin"]), 
                                Paginas = new ObservableCollection<Pagina>()
                            };
                        }

                        if (reader["page_id"] != DBNull.Value)
                        {
                            grupos[groupId].Paginas.Add(new Pagina
                            {
                                page_id = Convert.ToInt64(reader["page_id"]),
                                page_name = reader["page_name"].ToString(),
                                url = reader["url"].ToString(),
                                requires_proxy = Convert.ToBoolean(reader["requires_proxy"]),
                                requires_login = Convert.ToBoolean(reader["requires_login"])
                            });
                        }
                    }
                }
            }
            return grupos.Values.ToList();
        }

        public async Task UpdateGroupAsync(Group groupToUpdate, List<long> newPageIds)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                await conn.OpenAsync();
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        var updateGroupSql = @"
                        UPDATE browser_app.page_groups
                        SET group_name = @group_name, description = @description, pin = @pin
                        WHERE group_id = @group_id;";

                        using (var cmdUpdate = new NpgsqlCommand(updateGroupSql, conn, transaction))
                        {
                            cmdUpdate.Parameters.AddWithValue("@group_name", groupToUpdate.group_name);
                            cmdUpdate.Parameters.AddWithValue("@description", (object)groupToUpdate.description ?? DBNull.Value);
                            cmdUpdate.Parameters.AddWithValue("@pin", groupToUpdate.pin); 
                            cmdUpdate.Parameters.AddWithValue("@group_id", groupToUpdate.group_id);
                            await cmdUpdate.ExecuteNonQueryAsync();
                        }

                        var deletePagesSql = "DELETE FROM browser_app.group_pages WHERE group_id = @group_id;";
                        using (var cmdDelete = new NpgsqlCommand(deletePagesSql, conn, transaction))
                        {
                            cmdDelete.Parameters.AddWithValue("@group_id", groupToUpdate.group_id);
                            await cmdDelete.ExecuteNonQueryAsync();
                        }

                        if (newPageIds.Any())
                        {
                            var insertPageSql = "INSERT INTO browser_app.group_pages (group_id, page_id, state) VALUES (@group_id, @page_id, 1);";
                            foreach (var pageId in newPageIds)
                            {
                                using (var cmdInsert = new NpgsqlCommand(insertPageSql, conn, transaction))
                                {
                                    cmdInsert.Parameters.AddWithValue("@group_id", groupToUpdate.group_id);
                                    cmdInsert.Parameters.AddWithValue("@page_id", pageId);
                                    await cmdInsert.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
    }
}
