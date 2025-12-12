using UnityEngine;

namespace Needleforge.Utils;

internal static class MathUtils {

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

}
