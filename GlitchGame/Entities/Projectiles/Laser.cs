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

            ship.Health -= 10.0f * HealthDamageMultiplier(ship);
        }
    }
}
