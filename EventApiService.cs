using RaceDay222MVC.Models;

namespace RaceDay222MVC.Services
{
    public class EventApiService : ApiServiceBase
    {
        private const string Endpoint = "api/Event";

        public EventApiService(
            HttpClient httpClient,
            IConfiguration configuration)
            : base(httpClient, configuration)
        {
        }

        public Task<List<T>> GetAllAsync<T>(string? token = null)
        {
            return GetListAsync<T>(Endpoint, token);
        }

        public Task<T?> GetByIdAsync<T>(
            int id,
            string? token = null)
        {
            return GetAsync<T>($"{Endpoint}/{id}", token);
        }

        public Task<TResponse?> CreateAsync<TRequest, TResponse>(
            TRequest eventData,
            string token)
        {
            return PostAsync<TRequest, TResponse>(
                Endpoint, eventData, token);
        }

        public Task<TResponse?> UpdateAsync<TRequest, TResponse>(
            int id,
            TRequest eventData,
            string token)
        {
            return PutAsync<TRequest, TResponse>(
                $"{Endpoint}/{id}", eventData, token);
        }

        public Task<bool> DeleteAsync(int id, string token)
        {
            return DeleteAsync($"{Endpoint}/{id}", token);
        }
    }
}