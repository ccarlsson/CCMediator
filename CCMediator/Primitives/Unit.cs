namespace SimpleMediator.Primitives;

/// <summary>
/// Represents a unit (void) value for request/response flows that do not return data.
/// Useful with <c>IRequest&lt;Unit&gt;</c> to model commands without a result.
/// </summary>
public struct Unit
{
    /// <summary>
    /// Gets the unit value.
    /// </summary>
    public static Unit Value => new();
}
