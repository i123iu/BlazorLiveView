using BlazorLiveView.Core.Components;
using BlazorLiveView.Core.RenderTree;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.Tests.RenderTree;

public class RenderTreeMirrorTranslatorTests
{
    private const string CIRCUIT_ID = "test-circuit-id";
    private const int SEQ_MUL = RenderTreeMirrorTranslator.SEQ_MUL;

    class TestComponent : ComponentBase { }

    private static List<RenderTreeFrame> TranslateRoot(ReadOnlySpan<RenderTreeFrame> frames)
    {
        List<RenderTreeFrame> translated = new();
        RenderTreeMirrorTranslator translator = new(
            translated,
            CIRCUIT_ID
        );
        translator.TranslateRoot(frames);
        return translated;
    }

    [Fact]
    public void Empty()
    {
        var translated = TranslateRoot(ReadOnlySpan<RenderTreeFrame>.Empty);
        Assert.Empty(translated);
    }

    [Fact]
    public void Text()
    {
        var translated = TranslateRoot([
            RenderTreeFrameBuilder.Text(0, "text"),
        ]);
        var frame = Assert.Single(translated);
        Assert.Equal(RenderTreeFrameType.Text, frame.FrameType);
        Assert.Equal(0, frame.Sequence);
        Assert.Equal("text", frame.TextContent);
    }

    [Fact]
    public void Element_Text()
    {
        var translated = TranslateRoot([
            RenderTreeFrameBuilder.Element(0, 2, "div"),
            RenderTreeFrameBuilder.Text(1, "text"),
        ]);
        Assert.Collection(translated,
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Element, frame.FrameType);
                Assert.Equal(0 * SEQ_MUL, frame.Sequence);
                Assert.Equal("div", frame.ElementName);
                Assert.Equal(2, frame.ElementSubtreeLength);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Text, frame.FrameType);
                Assert.Equal(1 * SEQ_MUL, frame.Sequence);
                Assert.Equal("text", frame.TextContent);
            }
        );
    }

    [Fact]
    public void Element_AttributeAndText()
    {
        var translated = TranslateRoot([
            RenderTreeFrameBuilder.Element(0, 3, "div"),
            RenderTreeFrameBuilder.Attribute(1, "id", "the-id"),
            RenderTreeFrameBuilder.Text(2, "text"),
        ]);
        Assert.Collection(translated,
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Element, frame.FrameType);
                Assert.Equal(0 * SEQ_MUL, frame.Sequence);
                Assert.Equal("div", frame.ElementName);
                Assert.Equal(3, frame.ElementSubtreeLength);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Attribute, frame.FrameType);
                Assert.Equal(1 * SEQ_MUL, frame.Sequence);
                Assert.Equal("id", frame.AttributeName);
                Assert.Equal("the-id", frame.AttributeValue);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Text, frame.FrameType);
                Assert.Equal(2 * SEQ_MUL, frame.Sequence);
                Assert.Equal("text", frame.TextContent);
            }
        );
    }

    [Fact]
    public void Component_Attributes()
    {
        var translated = TranslateRoot([
            RenderTreeFrameBuilder.Component(0, 3, typeof(TestComponent)),
            RenderTreeFrameBuilder.Attribute(1, "Param1", "value1"),
            RenderTreeFrameBuilder.Attribute(2, "Param2", "value2"),
        ]);

        Assert.Collection(translated,
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Component, frame.FrameType);
                Assert.Equal(0 * SEQ_MUL, frame.Sequence);
                Assert.Equal(typeof(MirrorComponent), frame.ComponentType);
                Assert.Equal(4, frame.ComponentSubtreeLength);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Attribute, frame.FrameType);
                Assert.Equal(0 * SEQ_MUL + 1, frame.Sequence);
                Assert.Equal(nameof(MirrorComponent.CircuitId), frame.AttributeName);
                Assert.Equal(CIRCUIT_ID, frame.AttributeValue);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Attribute, frame.FrameType);
                Assert.Equal(0 * SEQ_MUL + 2, frame.Sequence);
                Assert.Equal(nameof(MirrorComponent.ComponentId), frame.AttributeName);
                Assert.Equal(0, frame.AttributeValue); // No id set
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Attribute, frame.FrameType);
                Assert.Equal(0 * SEQ_MUL + 3, frame.Sequence);
                Assert.Equal(nameof(MirrorComponent.DebugView), frame.AttributeName);
                Assert.Equal(false, frame.AttributeValue);
            }
        );
    }

    [Fact]
    public void Region_MultipleChildren()
    {
        var translated = TranslateRoot([
            RenderTreeFrameBuilder.Region(0, 5),
            RenderTreeFrameBuilder.Text(1, "text"),
            RenderTreeFrameBuilder.Element(2, 1, "div"),
            RenderTreeFrameBuilder.Component(3, 1, typeof(TestComponent)),
            RenderTreeFrameBuilder.Markup(4, "markup"),
        ]);

        Assert.Collection(translated,
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Region, frame.FrameType);
                Assert.Equal(0 * SEQ_MUL, frame.Sequence);
                Assert.Equal(8, frame.RegionSubtreeLength);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Text, frame.FrameType);
                Assert.Equal(1 * SEQ_MUL, frame.Sequence);
                Assert.Equal("text", frame.TextContent);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Element, frame.FrameType);
                Assert.Equal(2 * SEQ_MUL, frame.Sequence);
                Assert.Equal("div", frame.ElementName);
                Assert.Equal(1, frame.ElementSubtreeLength);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Component, frame.FrameType);
                Assert.Equal(3 * SEQ_MUL, frame.Sequence);
                Assert.Equal(typeof(MirrorComponent), frame.ComponentType);
                Assert.Equal(4, frame.ComponentSubtreeLength);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Attribute, frame.FrameType);
                Assert.Equal(3 * SEQ_MUL + 1, frame.Sequence);
                Assert.Equal(nameof(MirrorComponent.CircuitId), frame.AttributeName);
                Assert.Equal(CIRCUIT_ID, frame.AttributeValue);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Attribute, frame.FrameType);
                Assert.Equal(3 * SEQ_MUL + 2, frame.Sequence);
                Assert.Equal(nameof(MirrorComponent.ComponentId), frame.AttributeName);
                Assert.Equal(0, frame.AttributeValue); // No id set
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Attribute, frame.FrameType);
                Assert.Equal(3 * SEQ_MUL + 3, frame.Sequence);
                Assert.Equal(nameof(MirrorComponent.DebugView), frame.AttributeName);
                Assert.Equal(false, frame.AttributeValue);
            },
            frame =>
            {
                Assert.Equal(RenderTreeFrameType.Markup, frame.FrameType);
                Assert.Equal(4 * SEQ_MUL, frame.Sequence);
                Assert.Equal("markup", frame.MarkupContent);
            }
        );
    }
}
