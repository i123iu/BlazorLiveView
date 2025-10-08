using Microsoft.AspNetCore.Components.Rendering;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection.Wrappers;

internal class RenderTreeBuilderWrapper(RenderTreeBuilder wrapped)
    : WrapperBase<RenderTreeBuilder>(wrapped)
{
    private static readonly Type WrappedType = Types.RenderTreeBuilder;
    private static readonly FieldInfo _entries;

    static RenderTreeBuilderWrapper()
    {
        _entries = WrappedType.GetRequiredField("_entries", isPublic: false);
    }

    public RenderTreeFrameArrayBuilderWrapper Entries
        => new(_entries.GetValue(Inner)!);
}
