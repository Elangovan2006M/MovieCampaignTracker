using System.Net.Http.Json;
using MovieCampaignTracker.Shared;

namespace MovieCampaignTracker.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> Register(User user)
        {
            var response = await _http.PostAsJsonAsync("api/User/register", user);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync()
                : "Registration failed.";
        }

        public async Task<string> Login(User user)
        {
            var response = await _http.PostAsJsonAsync("api/User/login", user);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync()
                : "Invalid credentials.";
        }
    }
}
