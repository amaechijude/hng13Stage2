
namespace hng13Stage2.DTOs
{
    public class ExchangeRateDto
    {
        public required string Result { get; set; }
        public required Dictionary<string, decimal> Rates { get; set; }
    }
}
