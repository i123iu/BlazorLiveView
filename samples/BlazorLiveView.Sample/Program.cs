using BlazorLiveView.Core.Extensions;
using BlazorLiveView.Sample.Common;
using BlazorLiveView.Sample.Components;

namespace BlazorLiveView.Sample;

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
                options.ShowDebugOptions = false;
                options.RequireUserAgreement = false;
            })
            .InterceptIJSRuntime(options =>
            {
                options.DotnetToJsForwardingRules.Rules.Add(
                    new DebugDotnetToJsForwardingRule()
                );
                options.DotnetToJsForwardingRules.Rules.Add(
                    new DebugDotnetToJsForwardingRule()
                );
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapLiveViewMirrorEndpoint();

        app.Run();
    }
}
