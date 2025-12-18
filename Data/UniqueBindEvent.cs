using System;

namespace Needleforge.Data;

/// <summary>
/// Represents a bind event which occurs when the player inputs a specific direction at
/// the same time as they bind.
/// </summary>
/// <param name="Direction"><inheritdoc cref="Direction" path="//summary"/></param>
/// <param name="lambdaMethod"><inheritdoc cref="lambdaMethod" path="//summary"/></param>
public class UniqueBindEvent(UniqueBindDirection Direction, Action<Action> lambdaMethod)
{
    /// <summary>
    /// The directional input that triggers the unique bind.
    /// </summary>
    public UniqueBindDirection Direction = Direction;

    /// <summary>
    /// The logic of the unique bind.
    /// The function passed to the parameter must be called when the unique bind
    /// is completed.
    /// </summary>
    public Action<Action> lambdaMethod = lambdaMethod;
}
