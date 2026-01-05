namespace CCMediator;

/// <summary>
/// Represents a unit (void) value for request/response flows that do not return data.
/// </summary>
public readonly struct Unit
{
    /// <summary>
    /// Gets the singleton unit value.
    /// </summary>
    public static Unit Value => new();
}
