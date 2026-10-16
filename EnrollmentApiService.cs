
using System.Net.Http.Headers;
using RaceDay222MVC.Models;

namespace RaceDay222MVC.Services
{
    public class EnrollmentApiService
    {
        private readonly HttpClient _httpClient;

        public EnrollmentApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EnrollmentViewModel>>
            GetMyEnrollmentsAsync(string token)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, "api/Enrollments/my");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var enrollments =
                await response.Content
                    .ReadFromJsonAsync<List<EnrollmentViewModel>>();

            return enrollments ?? new List<EnrollmentViewModel>();
        }

        public async Task<bool> CancelEnrollmentAsync(
            int id, string token)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"api/Enrollments/{id}/cancel");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        internal async Task<bool> CreateEnrollmentAsync(int eventCategoryId, string token)
        {
            throw new NotImplementedException();
        }
    }
}