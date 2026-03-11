using Microsoft.Extensions.DependencyInjection;

namespace BlazorLiveView.Core.Extensions;

internal class LiveViewBuilder(IServiceCollection services)
    : ILiveViewBuilder
{
    public IServiceCollection Services { get; } = services;
}
