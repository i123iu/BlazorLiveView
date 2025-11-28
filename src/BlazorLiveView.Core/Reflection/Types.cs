using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Reflection;

namespace BlazorLiveView.Core.Reflection;

internal static class Types
{
    #region Microsoft.AspNetCore.Components.Server.Circuits
    public static readonly Type Circuit = typeof(Circuit);
    public static readonly Type CircuitHost = Circuit.GetTypeFromSameNamespace(nameof(CircuitHost));
    public static readonly Type ICircuitFactory = Circuit.GetTypeFromSameNamespace(nameof(ICircuitFactory));
    public static readonly Type CircuitFactory = Circuit.GetTypeFromSameNamespace(nameof(CircuitFactory));
    public static readonly Type RemoteRenderer = Circuit.GetTypeFromSameNamespace(nameof(RemoteRenderer));
    public static readonly Type RemoteNavigationManager = Circuit.GetTypeFromSameNamespace(nameof(RemoteNavigationManager));
    public static readonly Type WebRootComponentManager = Circuit.GetTypeFromSameNamespace($"{nameof(RemoteRenderer)}+{nameof(WebRootComponentManager)}");
    public static readonly Type WebRootComponent = Circuit.GetTypeFromSameNamespace($"{nameof(RemoteRenderer)}+{nameof(WebRootComponentManager)}+{nameof(WebRootComponent)}");
    public static readonly Type CircuitIdFactory = Circuit.GetTypeFromSameNamespace(nameof(CircuitIdFactory));
    public static readonly Type CircuitRegistry = Circuit.GetTypeFromSameNamespace(nameof(CircuitRegistry));
    public static readonly Type ICircuitHandleRegistry = Circuit.GetTypeFromSameNamespace(nameof(ICircuitHandleRegistry));

    public static readonly Type ILoggerOfComponentHub;

    /// <summary>
    /// <see cref="ValueTask{CircuitHost}"></see>
    /// </summary>
    public static readonly Type ValueTaskOfCircuitHost = typeof(ValueTask<>).MakeGenericType(CircuitHost);
    /// <summary>
    /// <see cref="Task{CircuitHost}"></see>
    /// </summary>
    public static readonly Type TaskOfCircuitHost = typeof(Task<>).MakeGenericType(CircuitHost);
    /// <summary>
    /// <see cref="Func{Task{CircuitHost}, CircuitHost}"></see>
    /// </summary>
    public static readonly Type FuncOfTaskOfCircuitHostAndCircuitHost =
        typeof(Func<,>).MakeGenericType(TaskOfCircuitHost, CircuitHost);
    #endregion

    #region Microsoft.AspNetCore.Components.RenderTree
    public static readonly Type Renderer = typeof(Renderer);
    public static readonly Type WebRenderer = typeof(WebRenderer);
    public static readonly Type RenderTreeFrameArrayBuilder = Renderer.GetTypeFromSameNamespace(nameof(RenderTreeFrameArrayBuilder));
    public static readonly Type ArrayBuilderOfRenderTreeFrame = RenderTreeFrameArrayBuilder?.BaseType
        ?? throw new Exception("ArrayBuilderOfRenderTreeFrame not found.");
    #endregion

    #region Microsoft.AspNetCore.Components.Rendering
    public static readonly Type ComponentState = typeof(ComponentState);
    public static readonly Type RenderTreeBuilder = typeof(RenderTreeBuilder);
    #endregion

    #region Microsoft.AspNetCore.Components.Server
    public static readonly Type CircuitOptions = typeof(CircuitOptions);
    public static readonly Type ComponentHub = CircuitOptions.GetTypeFromSameNamespace(nameof(ComponentHub));
    public static readonly Type IServerComponentDeserializer = CircuitOptions.GetTypeFromSameNamespace(nameof(IServerComponentDeserializer));
    #endregion

    static Types()
    {
        ILoggerOfComponentHub = typeof(Microsoft.Extensions.Logging.ILogger<>).MakeGenericType([ComponentHub]);
    }

    private static Type GetTypeFromSameNamespace(
        this Type typeFromSameNamespace,
        string name
    )
    {
        var @namespace = typeFromSameNamespace.Namespace;
        var assembly = typeFromSameNamespace.Assembly;
        return assembly.GetType($"{@namespace}.{name}")
            ?? throw new Exception($"{name} type not found.");
    }

    private static readonly BindingFlags DefaultPublic = BindingFlags.Public | BindingFlags.Instance;
    private static readonly BindingFlags DefaultPrivate = BindingFlags.NonPublic | BindingFlags.Instance;

    public static FieldInfo GetRequiredField(
        this Type type,
        string name,
        bool isPublic
    )
    {
        var flags = isPublic ? DefaultPublic : DefaultPrivate;
        return type.GetField(name, flags)
            ?? throw new Exception($"Field '{name}' not found on type '{type.FullName}'.");
    }

    public static PropertyInfo GetRequiredProperty(
        this Type type,
        string name,
        bool isPublic
    )
    {
        var flags = isPublic ? DefaultPublic : DefaultPrivate;
        return type.GetProperty(name, flags)
            ?? throw new Exception($"Property '{name}' not found on type '{type.FullName}'.");
    }

    public static MethodInfo GetRequiredMethod(
        this Type type,
        string name,
        bool isPublic,
        Type[]? parameters = null
    )
    {
        var flags = isPublic ? DefaultPublic : DefaultPrivate;
        var method = parameters is not null
            ? type.GetMethod(name, flags, parameters)
            : type.GetMethod(name, flags);
        return method
            ?? throw new Exception($"Method '{name}' not found on type '{type.FullName}'.");
    }

    public static void AssertIsInstanceOfType(Type expected, object actual)
    {
        if (!expected.IsInstanceOfType(actual))
        {
            throw new ArgumentException(
                $"Expected type {expected}, but got {actual.GetType()}."
            );
        }
    }
}
