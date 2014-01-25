using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Entities
{
    public abstract class Ship : Transformable, IEntity
    {
        private float _timer;

        private Sprite _forward;
        private Sprite _backward;
        private Sprite _left;
        private Sprite _right;

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

            _forward = new Sprite(Assets.LoadTexture("ship_forward.png"));
            _forward.Origin = new Vector2f(_forward.Texture.Size.X / 2f, 0) - new Vector2f(0, 65);

            _backward = new Sprite(Assets.LoadTexture("ship_backward.png"));
            _backward.Origin = new Vector2f(_backward.Texture.Size.X / 2f, _backward.Texture.Size.Y) - new Vector2f(0, -54);

            _left = new Sprite(Assets.LoadTexture("ship_left.png"));
            _left.Origin = new Vector2f(0, _left.Texture.Size.Y / 2f) - new Vector2f(15, -37);

            _right = new Sprite(Assets.LoadTexture("ship_right.png"));
            _right.Origin = new Vector2f(_right.Texture.Size.X, _right.Texture.Size.Y / 2f) - new Vector2f(-15, -37);
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

            Body.ApplyForce(Body.GetWorldVector(new Vector2(0.0f, Util.Clamp(Thruster, -1.0f, 0.5f) * 25)));
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

            var states = new RenderStates(Transform);
            target.Draw(Sprite, states);

            if (Thruster < 0)
            {
                target.Draw(_forward, states);
            }
            else if (Thruster > 0)
            {
                target.Draw(_backward, states);
            }

            if (AngularThruster < 0)
            {
                target.Draw(_left, states);
            }
            else if (AngularThruster > 0)
            {
                target.Draw(_right, states);
            }
        }
    }
}
