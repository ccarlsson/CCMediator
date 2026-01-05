namespace CCMediator;

/// <summary>
/// Represents a unit (void) value for request/response flows that do not return data.
/// </summary>
public struct Unit
{
    public static Unit Value => new();
}
