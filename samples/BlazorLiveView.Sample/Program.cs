using BlazorLiveView.Core.Extensions;
using BlazorLiveView.Core.JSInterop;
using BlazorLiveView.Sample.Components;
using BlazorLiveView.Sample.Components.Pages;

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
                options.DotnetToJsForwardingRules.Rules.Add(new ExactForwardingRule(
                    "not_mirrored_function", ForwardingBehavior.SkipForwarding
                ));

                options.JsToDotnetForwardingRules.Rules.Add(new ExactForwardingRule(
                    nameof(Counter.CallFromJS), ForwardingBehavior.Forward
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
