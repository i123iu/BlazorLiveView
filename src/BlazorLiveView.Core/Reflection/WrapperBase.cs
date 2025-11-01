namespace BlazorLiveView.Core.Reflection;

internal abstract class WrapperBase(object inner)
{
    public object Inner { get; private init; } = inner;
}

internal abstract class WrapperBase<T>(T inner) : WrapperBase(inner)
    where T : class
{
    public new T Inner => (T)base.Inner;
}