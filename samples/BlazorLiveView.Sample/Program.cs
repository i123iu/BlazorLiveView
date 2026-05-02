using BlazorLiveView.Core.Extensions;
using BlazorLiveView.Core.JSInterop;
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
                options.ShowDebugOptions = true;
            })
            .InterceptIJSRuntime(options =>
            {
                options.DotnetToJsForwardingRules.Default = ForwardingBehavior.Forward;
                options.DotnetToJsForwardingRules.Rules.Add(new ExactJSInteropForwardingRule(
                    "not_forwarded_function", ForwardingBehavior.SkipForwarding
                ));
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
