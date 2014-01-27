using System;
using Microsoft.Xna.Framework;

namespace GlitchGame.Entities.Projectiles
{
    public class NerfBall : Bullet
    {
        public NerfBall(Ship parent, Vector2 offset, Vector2 speed)
            : base(parent, offset, speed, "nerfball.png")
        {
            
        }

        public override void Hit(Ship ship)
        {
            base.Hit(ship);

            ship.Health -= 2.5f * Size * ship.DamageTakenMultiplier;

            var d = Program.Random.NextDouble() <= 0.75 ? 1f : -1f;
            var r = Program.Random.NextDouble();

            if (r <= 0.20f) // 20% chance to do nothing
            {
                return;
            }

            if (r <= 0.40f) // 20% chance to change damage
            {
                ship.DamageMultiplier = Math.Max(ship.DamageMultiplier - 0.05f * d, 0);
                return;
            }

            if (r <= 0.60f) // 20% chance to change speed
            {
                ship.SpeedMultiplier -= 0.05f * d;
                return;
            }

            if (r <= 0.80f) // 20% chance to change regen
            {
                ship.RegenRate -= 1 * d;
                return;
            }

            if (r <= 1.0f) // 20% chance to corrupt memory
            {
                if (ship is Enemy)
                {
                    (ship as Enemy).Corrupt();
                }
                
                return;
            }
        }
    }
}
