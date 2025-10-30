using BlazorLiveView.Core.RenderTree;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.Tests.RenderTree;

public class RenderTreeFrameBuilderTests
{
    [Fact]
    public void Create_None()
    {
        var none = RenderTreeFrameBuilder.None();
        Assert.Equal(RenderTreeFrameType.None, none.FrameType);
        Assert.Equal(0, none.Sequence);
    }

    [Fact]
    public void Create_Element()
    {
        object key = new();
        var element = RenderTreeFrameBuilder.Element(3, 5, "div", key);
        Assert.Equal(RenderTreeFrameType.Element, element.FrameType);
        Assert.Equal(3, element.Sequence);
        Assert.Equal(5, element.ElementSubtreeLength);
        Assert.Equal("div", element.ElementName);
        Assert.Equal(key, element.ElementKey);
    }

    class TestComponent : ComponentBase { }

    [Fact]
    public void Create_Component()
    {
        object key = new();
        var component = RenderTreeFrameBuilder.Component(2, 4, typeof(TestComponent), null, key);
        Assert.Equal(RenderTreeFrameType.Component, component.FrameType);
        Assert.Equal(2, component.Sequence);
        Assert.Equal(4, component.ComponentSubtreeLength);
        Assert.Equal(typeof(TestComponent), component.ComponentType);
        Assert.Equal(key, component.ComponentKey);
    }

    [Fact]
    public void Create_Region()
    {
        var region = RenderTreeFrameBuilder.Region(7, 10);
        Assert.Equal(RenderTreeFrameType.Region, region.FrameType);
        Assert.Equal(7, region.Sequence);
        Assert.Equal(10, region.RegionSubtreeLength);
    }

    [Fact]
    public void Create_Text()
    {
        var text = RenderTreeFrameBuilder.Text(1, "text");
        Assert.Equal(RenderTreeFrameType.Text, text.FrameType);
        Assert.Equal(1, text.Sequence);
        Assert.Equal("text", text.TextContent);
    }

    [Fact]
    public void Create_Markup()
    {
        var markup = RenderTreeFrameBuilder.Markup(4, "<b>bold</b>");
        Assert.Equal(RenderTreeFrameType.Markup, markup.FrameType);
        Assert.Equal(4, markup.Sequence);
        Assert.Equal("<b>bold</b>", markup.MarkupContent);
    }

    [Fact]
    public void Create_Attribute()
    {
        var attribute = RenderTreeFrameBuilder.Attribute(6, "class", "my-class", 5, "x");
        Assert.Equal(RenderTreeFrameType.Attribute, attribute.FrameType);
        Assert.Equal(6, attribute.Sequence);
        Assert.Equal("class", attribute.AttributeName);
        Assert.Equal("my-class", attribute.AttributeValue);
        Assert.Equal(5ul, attribute.AttributeEventHandlerId);
        Assert.Equal("x", attribute.AttributeEventUpdatesAttributeName);
    }

    [Fact]
    public void Create_ElementReferenceCapture()
    {
        static void Action(ElementReference _) { }
        var elementRefCapture = RenderTreeFrameBuilder.ElementReferenceCapture(8, Action, "id");
        Assert.Equal(RenderTreeFrameType.ElementReferenceCapture, elementRefCapture.FrameType);
        Assert.Equal(8, elementRefCapture.Sequence);
        Assert.Equal(Action, elementRefCapture.ElementReferenceCaptureAction);
        Assert.Equal("id", elementRefCapture.ElementReferenceCaptureId);
    }

    [Fact]
    public void Create_ComponentReferenceCapture()
    {
        static void Action(object _) { }
        var componentRefCapture = RenderTreeFrameBuilder.ComponentReferenceCapture(9, Action, 3);
        Assert.Equal(RenderTreeFrameType.ComponentReferenceCapture, componentRefCapture.FrameType);
        Assert.Equal(9, componentRefCapture.Sequence);
        Assert.Equal(Action, componentRefCapture.ComponentReferenceCaptureAction);
        Assert.Equal(3, componentRefCapture.ComponentReferenceCaptureParentFrameIndex);
    }
}
