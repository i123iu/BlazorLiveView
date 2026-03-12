namespace BlazorLiveView.Dashboard.Models;

internal class CircuitSortOptions
{
    public SortColumn Column { get; set; } = SortColumn.ConnectedAt;
    public SortDirection Direction { get; set; } = SortDirection.Descending;
}

internal enum SortColumn
{
    None,
    CircuitId,
    Location,
    ConnectedAt,
    Status,
    User
}

internal enum SortDirection
{
    Ascending,
    Descending
}
