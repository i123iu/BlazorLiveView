# Custom Dashboard

The pre-built dashboard components provided by `BlazorLiveView.Dashboard` only use services from the `BlazorLiveView.Core` package.

See the current [Dashboard Implementation](https://github.com/i123iu/BlazorLiveView/tree/main/src/BlazorLiveView.Dashboard) for reference.

## Used Services

Some of the more important services used by the dashboard are:

- `ICircuitTracker`: tracks and lists all active circuits (connections)
- `ICurrentCircuit`: provides context (id) of the current circuit
- `ILiveViewMirrorUriBuilder`: builds URIs for the mirror endpoint

Note that `LiveViewScreen` and the _mirror endpoint_ are different. `LiveViewScreen` is a component placed in a Razor page with a top status bar and an `iframe` pointing to the mirror endpoint.
