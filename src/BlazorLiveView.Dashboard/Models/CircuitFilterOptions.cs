using BlazorLiveView.Core.Circuits;

namespace BlazorLiveView.Dashboard.Models;

public class CircuitFilterOptions
{
    public string? CircuitId { get; set; }
    public string? Location { get; set; }
    public string? User { get; set; }
    public CircuitStatus? Status { get; set; }
}
