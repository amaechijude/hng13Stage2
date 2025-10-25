
namespace hng13Stage2.DTOs
{
    public class CountryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Capital { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public long Population { get; set; }
        public string Flag { get; set; } = string.Empty;
        public List<CurrencyDto> Currencies { get; set; } = [];
    }

    public class CurrencyDto
    {
        public string? Code { get; set; }
    }
}
