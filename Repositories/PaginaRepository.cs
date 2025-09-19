using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using Naviguard.Models.Naviguard.Models;

namespace Naviguard.Repositories
{
    public class PaginaRepository
    {
        private readonly string _connectionString;

        public PaginaRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["NaviguardDB"].ConnectionString;
        }

        public List<Pagina> ObtenerPaginas()
        {
            var paginas = new List<Pagina>();
            using (var conn = new NpgsqlConnection(_connectionString))
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