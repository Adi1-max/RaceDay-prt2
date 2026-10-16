namespace RaceDay222MVC.Services
{
    public class ParticipantApiService : ApiServiceBase
    {
        private const string Endpoint = "api/Participant";


    public ParticipantApiService(
        HttpClient httpClient,
        IConfiguration configuration)
        : base(httpClient, configuration)
        {
        }

        public Task<List<T>> GetAllAsync<T>(string token)
        {
            return GetListAsync<T>(Endpoint, token);
        }

        public Task<T?> GetByIdAsync<T>(int id, string token)
        {
            return GetAsync<T>($"{Endpoint}/{id}", token);
        }

        public Task<TResponse?> CreateAsync<TRequest, TResponse>(
            TRequest participant,
            string token)
        {
            return PostAsync<TRequest, TResponse>(
                Endpoint, participant, token);
        }

        public Task<TResponse?> UpdateAsync<TRequest, TResponse>(
            int id,
            TRequest participant,
            string token)
        {
            return PutAsync<TRequest, TResponse>(
                $"{Endpoint}/{id}", participant, token);
        }

        public Task<bool> DeleteAsync(int id, string token)
        {
            return base.DeleteAsync($"{Endpoint}/{id}", token);
        }
    }


}
