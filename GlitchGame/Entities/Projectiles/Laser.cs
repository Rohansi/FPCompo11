using Microsoft.Xna.Framework;

namespace GlitchGame.Entities.Projectiles
{
    public class Laser : Bullet
    {
        public Laser(Ship parent, Vector2 offset, Vector2 speed)
            : base(parent, offset, speed, "laser.png")
        {
            
        }

        public override void Hit(Ship ship)
        {
            base.Hit(ship);

            ship.Health -= 10 * Size * ship.DamageTakenMultiplier;

            var r = Program.Random.NextDouble();

            if (r <= 0.95f) // 95% chance to do nothing
            {
                return;
            }

            if (r <= 1.00f) // 5% chance to damage hull
            {
                ship.DamageTakenMultiplier += 0.01f * Size;
            }
        }
    }
}
