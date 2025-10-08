namespace BlazorLiveView.Core.Reflection;

internal abstract class WrapperBase(object wrapped)
{
    public object Inner { get; private init; } = wrapped;
}

internal abstract class WrapperBase<T>(T wrapped) : WrapperBase(wrapped)
    where T : class
{
    public new T Inner => (T)base.Inner;
}