
using System.Threading.Channels;
using hng13Stage2.Data;
using hng13Stage2.DTOs;
using hng13Stage2.Entities;
using Microsoft.EntityFrameworkCore;

namespace hng13Stage2.Services
{
    public class CountryService(AppDbContext context, ExternalApiService externalApiService, Channel<ImageDto> channel)
    {
        private readonly AppDbContext _context = context;
        private readonly ExternalApiService _externalApiService = externalApiService;
        private readonly Channel<ImageDto> _imageService = channel;
        private static readonly Random random = new();
        public async Task<RefreshResultDto> RefreshDataAsync()
        {
            var countriesTask = _externalApiService.GetCountriesAsync();
            var exchangeRatesTask = _externalApiService.GetExchangeRatesAsync();
            
            var countriesDto = await countriesTask;
            var exchangeRatesDto = await exchangeRatesTask;

            List<Country> lc = [];

            foreach (var countryDto in countriesDto)
            {
                var country = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == countryDto.Name.ToLower());

                if (country is null)
                {
                    country = new Country();
                    lc.Add(country);
                }

                country.Name = countryDto.Name;
                country.Capital = countryDto.Capital;
                country.Region = countryDto.Region;
                country.Population = countryDto.Population;
                country.FlagUrl = countryDto.Flag;

                if (countryDto.Currencies.Count > 0)
                {
                    country.CurrencyCode = countryDto.Currencies.First().Code;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

                    if (country.CurrencyCode != null && exchangeRatesDto.Rates.TryGetValue(country.CurrencyCode, out var rate))
                    {
                        country.ExchangeRate = rate;
                    }
                    else
                    {
                        country.ExchangeRate = null;
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                }
                else
                {
                    country.CurrencyCode = null;
                    country.ExchangeRate = null;
                }

                if (country.ExchangeRate.HasValue && country.ExchangeRate > 0)
                {
                    country.EstimatedGdp = country.Population * (decimal)random.Next(1000, 2001) / country.ExchangeRate.Value;
                }
                else
                {
                    country.EstimatedGdp = 0;
                }

                country.LastRefreshedAt = DateTime.UtcNow;
            }
            await _context.Countries.AddRangeAsync(lc);
            await _context.SaveChangesAsync();

            var refreshTime = DateTime.UtcNow;

            await _imageService.Writer.WriteAsync(new ImageDto(lc, refreshTime));

            return new RefreshResultDto
            {
                Status = "success",
                Message = "Data refreshed successfully.",
                CountriesProcessed = countriesDto.Count,
                RefreshedAt = refreshTime
            };
        }


        public async Task<List<Country>> GetAllCountriesAsync(string? region, string? currency, string? sort)
        {
            var query = _context.Countries.AsQueryable();

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(c => c.Region.ToLower() == region.ToLower());
            }

            if (!string.IsNullOrEmpty(currency))
            {
                query = query.Where(c => c.CurrencyCode != null && c.CurrencyCode.ToLower() == currency.ToLower());
            }

            if (!string.IsNullOrEmpty(sort))
            {
                query = sort.ToLower() switch
                {
                    "gdp_asc" => query.OrderBy(c => c.EstimatedGdp),
                    "gdp_desc" => query.OrderByDescending(c => c.EstimatedGdp),
                    _ => query
                };
            }

            return await query.ToListAsync();
        }

        public async Task<Country?> GetCountryByNameAsync(string name)
        {
            return await _context.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> DeleteCountryAsync(string name)
        {
            var rowsAffected = await _context.Countries
                .Where(c => c.Name.ToLower() == name.ToLower())
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }

        public async Task<StatusDto> GetStatusAsync()
        {
            var totalCountries = await _context.Countries.CountAsync();
            var lastRefreshedAt = await _context.Countries.MaxAsync(c => (DateTime?)c.LastRefreshedAt) ?? DateTime.UtcNow.AddMinutes(-5);

            return new StatusDto
            {
                TotalCountries = totalCountries,
                LastRefreshedAt = lastRefreshedAt
            };
        }
    }
}
