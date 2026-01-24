using SecretLabAPI.Features.Actions.Enums;

namespace SecretLabAPI.Features.Actions.API
{
    /// <summary>
    /// Represents a method that performs an action using the specified context and returns a set of flags indicating
    /// the result.
    /// </summary>
    /// <param name="context">A reference to the context in which the action is performed. The context may be modified by the delegate to
    /// reflect changes resulting from the action.</param>
    /// <returns>A value of type ActionResultFlags that specifies the outcome of the action. The returned flags indicate success,
    /// failure, or other result states as defined by the ActionResultFlags enumeration.</returns>
    public delegate ActionResultFlags ActionDelegate(ref ActionContext context);
}