using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay222MVC.Services
{
    public class RaceDayApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _context;
        private readonly IConfiguration _configuration;

        public RaceDayApiClient(
            HttpClient http,
            IHttpContextAccessor context,
            IConfiguration configuration)
        {
            _http = http;
            _context = context;
            _configuration = configuration;

            _http.BaseAddress = new Uri(
                _configuration["RaceDayApi:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "RaceDayApi:BaseUrl is missing from appsettings.json."));
        }

        private void AddToken()
        {
            var token = _context.HttpContext?
                .Session.GetString("JwtToken");

            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(token)
                    ? null
                    : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<T?> GetAsync<T>(string route)
        {
            AddToken();
            return await _http.GetFromJsonAsync<T>(route);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(
            string route, T data)
        {
            AddToken();
            return await _http.PostAsJsonAsync(route, data);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string route)
        {
            AddToken();
            return await _http.DeleteAsync(route);
        }
    }
}