using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using GlitchGame.Devices;
using Microsoft.Xna.Framework;
using SFML.Window;

namespace GlitchGame.Entities.Projectiles
{
    public abstract class Bullet : Projectile
    {
        public override RadarValue RadarType { get { return RadarValue.Bullet; } }

        protected Bullet(Ship parent, Vector2 offset, Vector2 speed, string texture)
            : base(parent, texture, parent.Size)
        {
            Sprite.Origin = new Vector2f(Sprite.Texture.Size.X / 2f, 0);

            #region Body Initialize
            Body = new Body(State.World);
            Body.BodyType = BodyType.Dynamic;
            Body.IsBullet = true;

            var shape = new CircleShape((0.15f / 4) * parent.Size, 0);
            Body.CreateFixture(shape);
            Body.Position = parent.Body.Position + parent.Body.GetWorldVector(offset);
            Body.Rotation = parent.Body.Rotation;

            Body.OnCollision += (a, b, contact) =>
            {
                if (!Collision(a, b, contact))
                    return false;

                Dead = true;
                return false;
            };

            Body.LinearVelocity = Parent.Body.LinearVelocity + Parent.Body.GetWorldVector(speed);
            Body.UserData = this;
            #endregion
        }

        private bool Collision(Fixture a, Fixture b, Contact contact)
        {
            if (b.Body.UserData is Bullet)
                return false;

            var ship = b.Body.UserData as Ship;

            if (ship == Parent)
                return false;

            if (ship != null && !Dead)
                Hit(ship);

            Dead = true;
            return true;
        }
    }
}
