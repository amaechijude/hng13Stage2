
using hng13Stage2.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using System.Threading.Channels;

namespace hng13Stage2.Services;

public class ImageService
{
    public async Task GenerateSummaryImageAsync(List<Country> countries, DateTime refreshTime)
    {
        var top5Gdp = countries.OrderByDescending(c => c.EstimatedGdp).Take(5).ToList();

        var image = new Image<Rgba32>(800, 600);
        image.Mutate(x => x.BackgroundColor(Color.White));

        var font = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);

        image.Mutate(x =>
        {
            x.DrawText($"Total Countries: {countries.Count}", font, Color.Black, new PointF(10, 10));
            x.DrawText("Top 5 Countries by Estimated GDP:", font, Color.Black, new PointF(10, 40));

            var y = 70;
            foreach (var country in top5Gdp)
            {
                x.DrawText($"- {country.Name}: {country.EstimatedGdp:N2}", font, Color.Black, new PointF(20, y));
                y += 30;
            }

            x.DrawText($"Last Refreshed: {refreshTime:yyyy-MM-dd HH:mm:ss} UTC", font, Color.Black, new PointF(10, 550));
        });

        var path = Path.Combine(Directory.GetCurrentDirectory(), "cache");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        await image.SaveAsync(Path.Combine(path, "summary.png"));
    }
}

public class ImageBackgroundWorker(ImageService imageService, Channel<ImageDto> imageChannel) : BackgroundService
{
    private readonly ImageService _imageService = imageService;
    Channel<ImageDto> _imageChannel = imageChannel;


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var imageDto in _imageChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try

            {

                await _imageService.GenerateSummaryImageAsync(imageDto.Countries, imageDto.RefreshTime);
            }
            catch

            {
                continue;
            }
        }
    }

}

public record ImageDto(List<Country> Countries, DateTime RefreshTime);