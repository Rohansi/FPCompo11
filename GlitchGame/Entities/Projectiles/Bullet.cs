using Microsoft.Xna.Framework;
using SFML.Window;

namespace GlitchGame.Entities.Projectiles
{
    public class Bullet : Projectile
    {
        public Bullet(Ship parent, Vector2 offset, Vector2 speed, string texture = "bullet.png")
            : base(parent, texture, parent.Size)
        {
            Sprite.Origin = new Vector2f(Sprite.Texture.Size.X / 2f, 0);

            Body = Util.CreateBullet(Parent.Body, offset, (a, b, contact) =>
            {
                var ship = b.Body.UserData as Ship;
                if (ship != null && !Dead)
                    Hit(ship);

                Dead = true;
                return true;
            });

            Body.LinearVelocity = Parent.Body.LinearVelocity + Body.GetWorldVector(speed);
            Body.UserData = this;
        }

        public override void Hit(Ship ship)
        {
            ship.Health -= 10;
        }
    }
}
