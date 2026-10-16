using RaceDay222MVC.Models;

namespace RaceDay222MVC.Services
{
    public class RaceDayApiService
    {
        private readonly HttpClient _httpClient;

        public RaceDayApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Models.EventViewModel>> GetEventsAsync()
        {
            var events = await _httpClient
                .GetFromJsonAsync<List<RaceDay222MVC.Models.EventViewModel>>("api/Events");

            return events ?? [];
        }

        public async Task<EventViewModel?> GetEventByIdAsync(int id)
        {
            return await _httpClient
                .GetFromJsonAsync<EventViewModel>($"api/Events/{id}");
        }
    }
}