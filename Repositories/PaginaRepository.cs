using Npgsql;
using Naviguard.Connections;
using Naviguard.Models.Naviguard.Models;

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
                var sql = "SELECT id_pagina, nombre_pagina, url, descripcion FROM browser_app.paginas ORDER BY id_pagina";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paginas.Add(new Pagina
                        {
                            IdPagina = Convert.ToInt64(reader["id_pagina"]),
                            NombrePagina = reader["nombre_pagina"].ToString(),
                            Url = reader["url"].ToString(),
                            Descripcion = reader["descripcion"].ToString()
                        });
                    }
                }
            }
            return paginas;
        }
    }
}