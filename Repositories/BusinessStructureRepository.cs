using MySqlConnector;
using Naviguard.BusinessInfo.Models;
using Naviguard.Connections;
using Naviguard.Models;
using System.Text;

namespace Naviguard.Repositories
{
    public class BusinessStructureRepository
    {
        public async Task<List<BusinessDepartment>> GetDepartmentsAsync()
        {
            var departments = new List<BusinessDepartment>();
            using (var conn = ConexionBD.ObtenerConexionNxEcosystem())
            {
                await conn.OpenAsync();
                var sql = "SELECT id_bnsdpt, name_department FROM business_department WHERE state = 1 ORDER BY name_department;";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        departments.Add(new BusinessDepartment
                        {
                            id_bnsdpt = Convert.ToInt32(reader["id_bnsdpt"]),
                            name_department = reader["name_department"].ToString()
                        });
                    }
                }
            }
            return departments;
        }

        public async Task<List<BusinessArea>> GetAreasByDepartmentAsync(int departmentId)
        {
            var areas = new List<BusinessArea>();
            using (var conn = ConexionBD.ObtenerConexionNxEcosystem())
            {
                await conn.OpenAsync();
                var sql = "SELECT id_bnsarea, name_area FROM business_area WHERE state = 1 AND id_bnsdpt = @id_bnsdpt ORDER BY name_area;";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id_bnsdpt", departmentId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            areas.Add(new BusinessArea
                            {
                                id_bnsarea = Convert.ToInt32(reader["id_bnsarea"]),
                                name_area = reader["name_area"].ToString()
                            });
                        }
                    }
                }
            }
            return areas;
        }

        public async Task<List<BusinessSubarea>> GetSubareasByAreaAsync(int areaId)
        {
            var subareas = new List<BusinessSubarea>();
            using (var conn = ConexionBD.ObtenerConexionNxEcosystem())
            {
                await conn.OpenAsync();
                var sql = "SELECT id_bnsbar, name_subarea FROM business_subarea WHERE state = 1 AND id_bnsarea = @id_bnsarea ORDER BY name_subarea;";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id_bnsarea", areaId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            subareas.Add(new BusinessSubarea
                            {
                                id_bnsbar = Convert.ToInt32(reader["id_bnsbar"]),
                                name_subarea = reader["name_subarea"].ToString()
                            });
                        }
                    }
                }
            }
            return subareas;
        }

        public async Task<List<FilteredUser>> FilterUsersAsync(string name, int? departmentId, int? areaId, int? subareaId)
        {
            var users = new List<FilteredUser>();
            using (var conn = ConexionBD.ObtenerConexionNxEcosystem())
            {
                await conn.OpenAsync();

                var sqlBuilder = new StringBuilder(@"
            SELECT md.id_user, 
                   CONCAT_WS(' ', md.name, COALESCE(NULLIF(md.paternal_surname, ''), md.maternal_surname)) AS full_name
            FROM mother_data md
            INNER JOIN relational_database rd ON md.id_user = rd.id_user
            INNER JOIN business_information bi ON rd.id_bsninfo = bi.id_bsninfo
        ");

                var conditions = new List<string>();
                var parameters = new List<MySqlParameter>();

                conditions.Add("md.state = 1");

                if (!string.IsNullOrWhiteSpace(name))
                {
                    conditions.Add("CONCAT_WS(' ', md.name, md.paternal_surname, md.maternal_surname) LIKE @name");
                    parameters.Add(new MySqlParameter("@name", $"%{name}%"));
                }
                if (departmentId.HasValue)
                {
                    conditions.Add("bi.id_bnsdpt = @departmentId");
                    parameters.Add(new MySqlParameter("@departmentId", departmentId.Value));
                }
                if (areaId.HasValue)
                {
                    conditions.Add("bi.id_bnsarea = @areaId");
                    parameters.Add(new MySqlParameter("@areaId", areaId.Value));
                }
                if (subareaId.HasValue)
                {
                    conditions.Add("bi.id_bnsbar = @subareaId");
                    parameters.Add(new MySqlParameter("@subareaId", subareaId.Value));
                }

                if (conditions.Any())
                {
                    sqlBuilder.Append(" WHERE ").Append(string.Join(" AND ", conditions));
                }

                sqlBuilder.Append(" ORDER BY full_name;");

                using (var cmd = new MySqlCommand(sqlBuilder.ToString(), conn))
                {
                    if (parameters.Any())
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new FilteredUser
                            {
                                id_user = Convert.ToInt32(reader["id_user"]),
                                full_name = reader["full_name"].ToString()
                            });
                        }
                    }
                }
            }
            return users;
        }
    }
}