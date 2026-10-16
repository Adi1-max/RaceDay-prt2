namespace RaceDay222MVC.Services
{
    public class ResultApiService : ApiServiceBase
    {
        private const string Endpoint = "api/Results";


    public ResultApiService(
        HttpClient httpClient,
        IConfiguration configuration)
        : base(httpClient, configuration)
        {
        }

        public Task<List<T>> GetAllAsync<T>(string? token = null)
        {
            return GetListAsync<T>(Endpoint, token);
        }

        public Task<T?> GetByIdAsync<T>(int id, string? token = null)
        {
            return GetAsync<T>($"{Endpoint}/{id}", token);
        }
    }


}

