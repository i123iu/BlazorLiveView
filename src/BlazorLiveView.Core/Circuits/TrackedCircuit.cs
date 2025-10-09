namespace BlazorLiveView.Core.Circuits;

public class TrackedCircuit
{
    private readonly bool _isMirror;
    private readonly IMirrorCircuit? _mirrorCircuit;
    private readonly IUserCircuit? _userCircuit;

    public TrackedCircuit(IMirrorCircuit circuit)
    {
        _isMirror = true;
        _mirrorCircuit = circuit;
        _userCircuit = null;
    }

    public TrackedCircuit(IUserCircuit circuit)
    {
        _isMirror = false;
        _mirrorCircuit = null;
        _userCircuit = circuit;
    }

    public bool IsMirror => _isMirror;
    public IMirrorCircuit MirrorCircuit => _mirrorCircuit
        ?? throw new InvalidOperationException("Not a mirror circuit.");
    public IUserCircuit UserCircuit => _userCircuit
        ?? throw new InvalidOperationException("Not a user circuit.");
}
