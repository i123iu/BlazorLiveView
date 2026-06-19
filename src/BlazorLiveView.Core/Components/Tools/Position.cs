using System.Numerics;

namespace BlazorLiveView.Core.Components.Tools;

public struct Position<T>(T x, T y)
    where T : struct, IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>
{
    public T x = x;
    public T y = y;

    public static Position<T> operator +(Position<T> a, Position<T> b)
    {
        return new Position<T>(a.x + b.x, a.y + b.y);
    }

    public static Position<T> operator -(Position<T> a, Position<T> b)
    {
        return new Position<T>(a.x - b.x, a.y - b.y);
    }
}
