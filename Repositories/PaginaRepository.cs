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
                var sql = "SELECT page_id, page_name, url, description, requires_proxy FROM browser_app.pages ORDER BY page_id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paginas.Add(new Pagina
                        {
                            IdPagina = Convert.ToInt64(reader["page_id"]),
                            NombrePagina = reader["page_name"].ToString(),
                            Url = reader["url"].ToString(),
                            Descripcion = reader["description"].ToString(),
                            RequiresProxy = reader["requires_proxy"] != DBNull.Value &&
                                            Convert.ToBoolean(reader["requires_proxy"])
                        });
                    }
                }
            }
            return paginas;
        }
    }
}