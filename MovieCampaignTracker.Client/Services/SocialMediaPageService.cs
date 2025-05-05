using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using MovieCampaignTracker.Shared;

namespace MovieCampaignTracker.Client.Services
{
    public class SocialMediaPageService
    {
        private readonly HttpClient _http;
        private const string apiUrl = "api/SocialMediaPage";

        public SocialMediaPageService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<SocialMediaPage>> GetAllPagesAsync()
        {
            return await _http.GetFromJsonAsync<List<SocialMediaPage>>(apiUrl);
        }

        public async Task<SocialMediaPage> GetPageByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<SocialMediaPage>($"{apiUrl}/{id}");
        }

        public async Task CreatePageAsync(SocialMediaPage page)
        {
            await _http.PostAsJsonAsync(apiUrl, page);
        }

        public async Task UpdatePageAsync(SocialMediaPage page)
        {
            await _http.PutAsJsonAsync(apiUrl, page);
        }

        public async Task DeletePageAsync(int id)
        {
            await _http.DeleteAsync($"{apiUrl}/{id}");
        }
    }
}
