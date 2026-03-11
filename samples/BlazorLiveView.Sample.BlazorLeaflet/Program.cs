using BlazorLiveView.Core.Extensions;
using BlazorLiveView.Sample.BlazorLeaflet.Components;

namespace BlazorLiveView.Sample.BlazorLeaflet;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents(options =>
            {
                options.DetailedErrors = true;
            })
            .AddLiveView(options =>
            {
                options.ShowDebugOptions = true;
                options.UseScreenOverlay = false;
            })
            .InterceptJSInteropInvocations();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapLiveViewMirrorEndpoint();

        app.Run();
    }
}