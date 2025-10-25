using hng13Stage2.DTOs;

namespace hng13Stage2.Services
{
    public class ExternalApiService(IHttpClientFactory httpClientFactory)
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<List<CountryDto>> GetCountriesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            return await client.GetFromJsonAsync<List<CountryDto>>("https://restcountries.com/v2/all?fields=name,capital,region,population,flag,currencies")
            ?? [];
        }

        public async Task<ExchangeRateDto?> GetExchangeRatesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            return await client.GetFromJsonAsync<ExchangeRateDto>("https://open.er-api.com/v6/latest/USD");

        }
    }
}
