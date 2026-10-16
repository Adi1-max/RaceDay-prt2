using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using RaceDay222MVC.Models;

namespace RaceDay222MVC.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;


    public AuthApiService(
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

            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<(LoginResponse? Response, string? Error)>
            LoginAsync(LoginViewModel model)
        {
            var request = new
            {
                Email = model.Email,
                Password = model.Password
            };

            try
            {
                var result = await _httpClient.PostAsJsonAsync(
                    "api/auth/login", request);

                if (!result.IsSuccessStatusCode)
                {
                    var details = await result.Content.ReadAsStringAsync();

                    return (null,
                        $"Login failed (HTTP {(int)result.StatusCode}). " +
                        "Check your credentials and API response. " +
                        details);
                }

                var response =
                    await result.Content.ReadFromJsonAsync<LoginResponse>();

                if (response == null ||
                    string.IsNullOrWhiteSpace(response.Token))
                {
                    return (null,
                        "The API response did not contain a valid login token. " +
                        "Check the login response format.");
                }

                return (response, null);
            }
            catch (HttpRequestException ex)
            {
                return (null,
                    $"Could not connect to the RaceDay API: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return (null,
                    "The RaceDay API request timed out. Check that the API is running.");
            }
            catch (System.Text.Json.JsonException ex)
            {
                return (null,
                    $"Could not read the login response from the API: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? Error)>
            RegisterAsync(RegisterViewModel model)
        {
            var request = new
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password
            };

            string endpoint = model.Role == "Organiser"
                ? "api/auth/register/organiser"
                : "api/auth/register/participant";

            try
            {
                var result = await _httpClient.PostAsJsonAsync(
                    endpoint, request);

                if (!result.IsSuccessStatusCode)
                {
                    var details = await result.Content.ReadAsStringAsync();

                    return (false,
                        $"Registration failed (HTTP {(int)result.StatusCode}). " +
                        details);
                }

                return (true, null);
            }
            catch (HttpRequestException ex)
            {
                return (false,
                    $"Could not connect to the RaceDay API: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                return (false,
                    "The RaceDay API request timed out.");
            }
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
    }


}
