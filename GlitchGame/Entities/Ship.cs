using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Entities
{
    public abstract class Ship : Transformable, IEntity
    {
        private float _timer;

        internal Sprite Sprite;
        internal Body Body;

        internal float Thruster;
        internal float AngularThruster;
        internal bool Shooting;

        protected virtual float GunCooldown { get { return 0.15f; } }

        protected Ship(Vector2 position, string sprite)
        {
            Sprite = new Sprite(Assets.LoadTexture(sprite)).Center();

            Body = Util.CreateShip();
            Body.UserData = this;
            Body.Position = position;
        }

        public abstract int DrawOrder { get; }
        public abstract byte RadarType { get; }
        public bool Dead { get; protected set; }

        public void Destroyed()
        {
            Program.World.RemoveBody(Body);
        }

        public virtual void Update()
        {
            _timer = Math.Max(_timer - Program.FrameTime, 0);

            if (Shooting && _timer <= 0)
            {
                Shoot();
                _timer = GunCooldown;
            }

            Body.ApplyForce(Body.GetWorldVector(new Vector2(0.0f, Thruster * 25)));
            Body.ApplyTorque(AngularThruster * 10);
        }

        public virtual void Shoot()
        {
            var b1 = new Bullet(Body, new Vector2(-0.575f, -0.20f));
            var b2 = new Bullet(Body, new Vector2(0.575f, -0.20f));

            Program.Entities.AddLast(b1);
            Program.Entities.AddLast(b2);
        }

        public void Draw(RenderTarget target)
        {
            Position = Body.Position.ToSfml() * Program.PixelsPerMeter;
            Rotation = Body.Rotation * Program.DegreesPerRadian;

            target.Draw(Sprite, new RenderStates(Transform));
        }
    }
}
