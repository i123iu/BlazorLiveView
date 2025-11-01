using Microsoft.AspNetCore.Components.Rendering;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RenderTreeBuilderWrapper(RenderTreeBuilder inner)
    : WrapperBase<RenderTreeBuilder>(inner)
{
    private static readonly Type InnerType = Types.RenderTreeBuilder;
    private static readonly FieldInfo _entries;

    static RenderTreeBuilderWrapper()
    {
        _entries = InnerType.GetRequiredField("_entries", isPublic: false);
    }

    public RenderTreeFrameArrayBuilderWrapper Entries
        => new(_entries.GetValue(Inner)!);
}
