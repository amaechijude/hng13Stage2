using hng13Stage2.Services;
using Microsoft.AspNetCore.Mvc;

namespace hng13Stage2.Controllers;

[ApiController]
[Route("[controller]")]
public class CountriesController(CountryService countryService) : ControllerBase
{
    private readonly CountryService _countryService = countryService;

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshData()
    {
        try
        {
            var result = await _countryService.RefreshDataAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { error = "External data source unavailable", details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCountries(string? region, string? currency, string? sort)
    {
        var countries = await _countryService.GetAllCountriesAsync(region, currency, sort);
        return Ok(countries);
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetCountry(string name)
    {
        var country = await _countryService.GetCountryByNameAsync(name);
        if (country == null)
        {
            return NotFound(new { error = "Country not found" });
        }
        return Ok(country);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteCountry(string name)
    {
        var result = await _countryService.DeleteCountryAsync(name);
        if (!result)
        {
            return NotFound(new { error = "Country not found" });
        }
        return NoContent();
    }

    [HttpGet("/status")]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _countryService.GetStatusAsync();
        return Ok(status);
    }

    [HttpGet("image")]
    public IActionResult GetSummaryImage()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "cache", "summary.png");
        if (!System.IO.File.Exists(path))
        {
            return NotFound(new { error = "Summary image not found" });
        }

        var bytes = System.IO.File.ReadAllBytes(path);
        return File(bytes, "image/png");
    }
}
