namespace BlazorLiveView.Dashboard.Models;

public class CircuitSortOptions
{
    public SortColumn Column { get; set; } = SortColumn.ConnectedAt;
    public SortDirection Direction { get; set; } = SortDirection.Descending;
}

public enum SortColumn
{
    None,
    CircuitId,
    Location,
    ConnectedAt,
    Status,
    User
}

public enum SortDirection
{
    Ascending,
    Descending
}
