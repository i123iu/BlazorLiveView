# Utilities

These are the attributes and components used to control how your application behaves when being mirrored in BlazorLiveView.

## Hiding Components in Mirror Views

When working with sensitive data (like personal information) you may want to hide them from being visible in the mirrored views.

### Using the Component

For hiding specific parts of the UI, you can wrap the content in a `LiveViewHideInMirror` component. For example:

```razor
<div class="user-dashboard">
    <h1>Welcome, @UserName</h1>

    Email: @UserEmail

    <LiveViewHideInMirror>
        Social Security Number: @UserSSN
    </LiveViewHideInMirror>
</div>
```

### Using the Attribute

For hiding entire components from being mirrored on any page, you can apply the `LiveViewHideInMirrorAttribute` to the component class. For example:

```razor
@* TwoFactorAuthComponent.razor *@

@using BlazorLiveView.Core.Attributes;
@attribute [LiveViewHideInMirror]

Your authentication code is: @AuthCode
```
