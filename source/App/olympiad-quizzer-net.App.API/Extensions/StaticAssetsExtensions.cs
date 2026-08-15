using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Infrastructure.SQLite;

namespace OlympiadQuizzer.App.Api.Extensions;

internal static class StaticAssetsExtensions
{
    internal static WebApplication UseQuestionImages(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<QuestionBankOptions>>().Value;
        string configured = options.ImagesPath;
        string imagesPath = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);

        if (Directory.Exists(imagesPath))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(imagesPath),
                RequestPath = "/images"
            });
        }
        return app;
    }
}
