using Naviguard.Connections;
using Npgsql;

namespace Naviguard.Repositories
{
    public class AuthRepository
    {
        public async Task<bool> UserHasRolesAsync(long userId)
        {
            using (var conn = ConexionBD.ObtenerConexionNaviguard()) 
            {
                await conn.OpenAsync();
                var sql = @"
                    SELECT 1 
                    FROM browser_app.roles_user 
                    WHERE external_user_id = @user_id AND state = 1 
                    LIMIT 1;
                ";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    var result = await cmd.ExecuteScalarAsync();

                    return result != null;
                }
            }
        }
    }
}