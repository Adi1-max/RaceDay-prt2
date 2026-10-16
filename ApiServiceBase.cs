using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay222MVC.Services
{
    public abstract class ApiServiceBase
    {
        protected readonly HttpClient _httpClient;

    protected ApiServiceBase(
        HttpClient httpClient,
        IConfiguration configuration)
        {
            _httpClient = httpClient;

            var baseUrl = configuration["RaceDayApi:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "RaceDayApi:BaseUrl is missing from appsettings.json.");
            }

            _httpClient.BaseAddress = new Uri(
                baseUrl.EndsWith("/")
                    ? baseUrl
                    : baseUrl + "/");
        }

        protected HttpRequestMessage CreateRequest(
            HttpMethod method,
            string endpoint,
            string? token = null)
        {
            var request = new HttpRequestMessage(method, endpoint);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return request;
        }

        protected async Task<List<T>> GetListAsync<T>(
            string endpoint,
            string? token = null)
        {
            using var request = CreateRequest(
                HttpMethod.Get, endpoint, token);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<T>>()
                   ?? new List<T>();
        }

        protected async Task<T?> GetAsync<T>(
            string endpoint,
            string? token = null)
        {
            using var request = CreateRequest(
                HttpMethod.Get, endpoint, token);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data,
            string? token = null)
        {
            using var request = CreateRequest(
                HttpMethod.Post, endpoint, token);

            request.Content = JsonContent.Create(data);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentLength == 0)
            {
                return default;
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        protected async Task<TResponse?> PutAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data,
            string? token = null)
        {
            using var request = CreateRequest(
                HttpMethod.Put, endpoint, token);

            request.Content = JsonContent.Create(data);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentLength == 0)
            {
                return default;
            }

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        protected async Task<bool> DeleteAsync(
            string endpoint,
            string? token = null)
        {
            using var request = CreateRequest(
                HttpMethod.Delete, endpoint, token);

            using var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        protected async Task<bool> PostForSuccessAsync<TRequest>(
            string endpoint,
            TRequest data,
            string? token = null)
        {
            using var request = CreateRequest(
                HttpMethod.Post, endpoint, token);

            request.Content = JsonContent.Create(data);

            using var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        protected async Task<bool> PutForSuccessAsync<TRequest>(
            string endpoint,
            TRequest data,
            string? token = null)
        {
            using var request = CreateRequest(
                HttpMethod.Put, endpoint, token);

            request.Content = JsonContent.Create(data);

            using var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }


}
