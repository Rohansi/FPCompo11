using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;
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
                Program.Entities.Remove(this);
                return true;
            });

            Body.ApplyForce(Body.GetWorldVector(speed));
            Body.UserData = this;
        }
    }
}
