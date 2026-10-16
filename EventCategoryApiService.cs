
using System.Net.Http.Json;
using RaceDay222MVC.Models;
using RaceDayMVC.Models;

namespace RaceDay222MVC.Services
{
    public class EventCategoryApiService
    {
        private readonly HttpClient _httpClient;

        public EventCategoryApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EventCategoryViewModel>>
            GetEventCategoriesAsync()
        {
            var categories =
                await _httpClient.GetFromJsonAsync<
                    List<EventCategoryViewModel>>(
                    "api/EventCategory");

            return categories ?? new List<EventCategoryViewModel>();
        }
    }
}
