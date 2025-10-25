
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hng13Stage2.Entities
{
    public class Country
    {
        private static Random Random() => new();


        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Capital { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public long Population { get; set; }
        public string? CurrencyCode { get; set; } = string.Empty;
        public decimal? ExchangeRate { get; set; }
        public decimal EstimatedGdp { get; set; }
        public string FlagUrl { get; set; } = string.Empty;
        public DateTime LastRefreshedAt { get; set; }
    }
}
