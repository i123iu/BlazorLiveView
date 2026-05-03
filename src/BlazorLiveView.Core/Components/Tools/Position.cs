namespace BlazorLiveView.Core.Components.Tools;

public struct Position(int x, int y)
{
    public int x = x;
    public int y = y;

    public static Position operator +(Position a, Position b)
    {
        return new Position(a.x + b.x, a.y + b.y);
    }

    public static Position operator -(Position a, Position b)
    {
        return new Position(a.x - b.x, a.y - b.y);
    }
}
