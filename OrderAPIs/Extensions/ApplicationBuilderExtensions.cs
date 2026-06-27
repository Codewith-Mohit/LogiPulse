namespace OrderAPIs.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseOrderApiMiddleware(this WebApplication app)
    {
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
