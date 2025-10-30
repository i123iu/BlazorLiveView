using HarmonyLib;

namespace BlazorLiveView.Core.Patching;

/// <summary>
/// Performs patching (intercepting method calls) of Blazor's internal methods.
/// </summary>
internal interface IPatcher
{
    void Patch(Harmony harmony);
}
