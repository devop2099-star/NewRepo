using Npgsql;
using Naviguard.Connections;
using Naviguard.Models;
using System.Text.RegularExpressions;

namespace Naviguard.Repositories
{
    public class GrupoRepository
    {
        public List<Group> ObtenerGruposConPaginas()
        {
            var grupos = new Dictionary<long, Group>();

            using (var conn = ConexionBD.ObtenerConexionNaviguard())
            {
                conn.Open();
                var sql = @"
                    SELECT pg.group_id, pg.group_name, pg.description,
                           p.page_id, p.page_name, p.url, p.requires_proxy, p.requires_login, p.state
                    FROM browser_app.page_groups pg
                    LEFT JOIN browser_app.group_pages gp ON pg.group_id = gp.group_id
                    LEFT JOIN browser_app.pages p ON gp.page_id = p.page_id
                    WHERE pg.state = 1 AND (p.state = 1 OR p.page_id IS NULL)
                    ORDER BY pg.group_name, p.page_name;";

                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long groupId = Convert.ToInt64(reader["group_id"]);

                        // Si el grupo no existe aún, lo agregamos
                        if (!grupos.ContainsKey(groupId))
                        {
                            grupos[groupId] = new Group
                            {
                                group_id = groupId,
                                group_name = reader["group_name"].ToString()!,
                                description = reader["description"]?.ToString(),
                                Paginas = new System.Collections.ObjectModel.ObservableCollection<Pagina>()
                            };
                        }

                        if (!(reader["page_id"] is DBNull))
                        {
                            var pagina = new Pagina
                            {
                                page_id = Convert.ToInt64(reader["page_id"]),
                                page_name = reader["page_name"].ToString(),
                                url = reader["url"].ToString(),
                                requires_proxy = Convert.ToBoolean(reader["requires_proxy"]),
                                requires_login = Convert.ToBoolean(reader["requires_login"]),
                                state = Convert.ToInt16(reader["state"])
                            };

                            grupos[groupId].Paginas.Add(pagina);
                        }
                    }
                }
            }

            return grupos.Values.ToList();
        }
    }
}
