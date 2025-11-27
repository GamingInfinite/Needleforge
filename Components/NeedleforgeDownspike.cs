using UnityEngine;

namespace Needleforge.Components;

public class NeedleforgeDownspike : Downspike
{
    public Vector2 acceleration;
    public HeroSlashBounceConfig bounceConfig;

    public Vector2 Velocity
    {
        get
        {
            /*
            For vanilla downspikes, the "speed" in the HeroConfig is used as both
            components of a velocity vector, which creates a 45 degree angle of a greater
            speed than is actually written in the config.
            We do the same thing to ensure these attacks behave the same way when given
            their same config.
            */
            var speed = hc.Config.DownspikeSpeed;
            var magnitude = new Vector2(speed, speed).magnitude;

            var angleRad = slashAngle * Mathf.Deg2Rad;

            var velocity = new Vector2(
                magnitude * Mathf.Cos(angleRad),
                magnitude * Mathf.Sin(angleRad)
            );

            if (hc.cState.facingRight)
            {
                velocity *= new Vector2(-1, 1);
            }

            return velocity;
        }
    }
}
