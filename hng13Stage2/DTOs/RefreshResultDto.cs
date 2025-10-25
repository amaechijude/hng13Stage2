
namespace hng13Stage2.DTOs
{
    public class RefreshResultDto
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public int CountriesProcessed { get; set; }
        public DateTime RefreshedAt { get; set; }
    }
}
