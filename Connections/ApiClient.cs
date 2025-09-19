using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace Naviguard.Connections
{
    public class ApiClient
    {
        private static readonly HttpClient client = new HttpClient();
        private static string _jwtToken = "";

        static ApiClient()
        {
            client.BaseAddress = new Uri("https://localhost:7164/");
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var loginData = new { Username = username, Password = password };
            var jsonContent = JsonSerializer.Serialize(loginData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("api/Auth/login", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("JSON Recibido de la API:");
                    Debug.WriteLine(responseBody);

                    using (JsonDocument jsonDoc = JsonDocument.Parse(responseBody))
                    {
                        JsonElement root = jsonDoc.RootElement;

                        if (root.TryGetProperty("token", out var tokenElement) &&
                            root.TryGetProperty("userId", out var userIdElement) &&
                            root.TryGetProperty("username", out var usernameElement) &&
                            tokenElement.GetString() is not null)
                        {
                            _jwtToken = tokenElement.GetString()!;

                            long apiUserId = userIdElement.GetInt32();
                            string user = usernameElement.GetString()!;


                            UserSession.StartSession(apiUserId, user);


                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                            return true;
                        }
                    }

                    MessageBox.Show("Error: La respuesta del servidor no es válida o no tiene el formato esperado.");
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    MessageBox.Show("Error: Credenciales inválidas.");
                    return false;
                }
                else
                {
                    MessageBox.Show($"Error en el servidor: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión con la API: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            _jwtToken = "";
            client.DefaultRequestHeaders.Authorization = null;
            UserSession.EndSession();
        }
    }
}
