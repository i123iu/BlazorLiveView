using Microsoft.AspNetCore.Components.RenderTree;

namespace BlazorLiveView.Core.RenderTree;

internal sealed class RenderTreeMirrorTranslator(
    List<RenderTreeFrame> result,
    string circuitId
) : RenderTreeTranslatorBase(
    result
)
{
    private readonly string _circuitId = circuitId;

    protected override void TranslateNone(RenderTreeFrame none)
    {
        // Skip
    }

    protected override void TranslateElement(
        RenderTreeFrame element,
        ReadOnlySpan<RenderTreeFrame> attributes,
        ReadOnlySpan<RenderTreeFrame> childFrames
    )
    {
        // Replace the Element, but only change its ElementSubtreeLength, because
        // it could have contained Components which would be replaced with
        // MirrorComponents and so the subtree length would change. 

        SubtreeLengthCapture newElement = new(_result, subtreeLength =>
            RenderTreeFrameBuilder.Element(
                element.Sequence * SEQ_MUL,
                subtreeLength,
                element.ElementName,
                element.ElementKey
            )
        );

        bool isAnchor = element.ElementName == "a";
        bool hasTargetBlank = false;
        if (isAnchor)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                var attribute = attributes[i];
                if (attribute.AttributeName == "target" &&
                    attribute.AttributeValue is string strAttrValue &&
                    strAttrValue == "_blank")
                {
                    hasTargetBlank = true;
                    break;
                }
            }
        }

        // Translate attributes
        for (int i = 0; i < attributes.Length; i++)
        {
            var attribute = attributes[i];
            var attributeValue = attribute.AttributeValue;

            if (isAnchor && attribute.AttributeName == "href")
            {
                // Remove the "href" attributes from anchor elements (<a href="...">),
                // so that mirror circuits cannot change location by clicking on links. 

                // If the attribute was to be removed completely, the link would
                // not be displayed as "clickable" ("cursor: pointer" in css). 

                if (!hasTargetBlank)
                {
                    // Also ignore anchor elements with "target" set to "_blank",
                    // because those links do not change the location. 
                    attributeValue = "javascript:void(0)";
                }

                // TODO?: redirect the original user circuit
            }

            _result.Add(RenderTreeFrameBuilder.Attribute(
                attribute.Sequence * SEQ_MUL,
                attribute.AttributeName,
                attributeValue,
                attribute.AttributeEventHandlerId,
                attribute.AttributeEventUpdatesAttributeName
            ));
        }

        TranslateAll(childFrames);

        newElement.Dispose();
    }

    protected override void TranslateText(
        RenderTreeFrame text
    )
    {
        _result.Add(RenderTreeFrameBuilder.Text(
            text.Sequence * SEQ_MUL,
            text.TextContent
        ));
    }

    protected override void TranslateComponent(
        RenderTreeFrame component,
        ReadOnlySpan<RenderTreeFrame> attributes
    )
    {
        // Ignore original Component and its Attributs and replace it with custom MirrorComponent

        AddMirrorComponent(
            component.Sequence * SEQ_MUL,
            component.ComponentKey,
            _circuitId,
            component.ComponentId,
            false
        );
    }

    protected override void TranslateRegion(
        RenderTreeFrame region,
        ReadOnlySpan<RenderTreeFrame> childFrames
    )
    {
        // Change RegionSubtreeLength - same as with Elements

        SubtreeLengthCapture newRegion = new(_result, subtreeLength =>
            RenderTreeFrameBuilder.Region(
                region.Sequence * SEQ_MUL,
                subtreeLength
            )
        );

        TranslateAll(childFrames);

        newRegion.Dispose();
    }

    protected override void TranslateMarkup(
        RenderTreeFrame markup
    )
    {
        // TODO: markup can contain other elements which cannot be translated simply,
        //       e.g. anchor elements (<a href>). 

        _result.Add(RenderTreeFrameBuilder.Markup(
            markup.Sequence * SEQ_MUL,
            markup.MarkupContent
        ));
    }
}
