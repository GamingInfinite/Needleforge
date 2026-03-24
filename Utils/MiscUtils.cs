using System;
using UnityEngine;

namespace Needleforge.Utils;

internal static class MiscUtils {

    /// <summary>
    /// Scales a polygon defined by an array of points around its own center point,
    /// and returns the result as a new array.
    /// </summary>
    internal static Vector2[] ScalePolygon(Vector2[] poly, float mult) {
        if (poly.Length <= 0)
            return [];

        Vector2[] pts = [.. poly];

        Vector2 center = Vector2.zero;
        foreach (var p in pts)
            center += p;
        center /= pts.Length;

        for (int i = 0; i < pts.Length; i++)
            pts[i] = center + (pts[i] - center) * mult;

        return pts;
    }

    /// <summary>
    /// Calls a zero-parameter method named <paramref name="fnName"/> on the component,
    /// if such a method exists; optionally runs a fallback if it doesn't exist.
    /// </summary>
    /// <remarks>
    /// Team Cherry not defining StartSlash() and CancelAttack() on the base class for
    /// attacks is a problem. This is mostly for solving that problem.
    /// </remarks>
    internal static void CallMethod<T>(
        this T component, string fnName, Action<T>? fallbackFn = null
    ) where T : Component
    {
        var type = component.GetType();
        var fn = type.GetMethod(fnName, []);

        if (fn != null)
            fn.Invoke(component, []);
        else
            fallbackFn?.Invoke(component);
    }

}
