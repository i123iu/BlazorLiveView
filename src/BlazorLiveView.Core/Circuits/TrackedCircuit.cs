namespace BlazorLiveView.Core.Circuits;

public class TrackedCircuit
{
    private readonly bool _isMirror;
    private readonly IMirrorCircuit? _mirrorCircuit;
    private readonly IRegularCircuit? _regularCircuit;

    public TrackedCircuit(IMirrorCircuit circuit)
    {
        _isMirror = true;
        _mirrorCircuit = circuit;
        _regularCircuit = null;
    }

    public TrackedCircuit(IRegularCircuit circuit)
    {
        _isMirror = false;
        _mirrorCircuit = null;
        _regularCircuit = circuit;
    }

    public bool IsMirror => _isMirror;
    public IMirrorCircuit MirrorCircuit => _mirrorCircuit
        ?? throw new InvalidOperationException("Not a mirror circuit.");
    public IRegularCircuit RegularCircuit => _regularCircuit
        ?? throw new InvalidOperationException("Not a regular circuit.");
}
