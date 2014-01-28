using System;
using FarseerPhysics.Dynamics;
using GlitchGame.Devices;
using GlitchGame.Weapons;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Entities
{
    public abstract class Ship : Transformable, IEntity
    {
        private Sprite _forward;
        private Sprite _backward;
        private Sprite _left;
        private Sprite _right;

        public abstract int Depth { get; }
        public abstract RadarValue Radar { get; }
        public bool Dead { get; protected set; }

        public Sprite Sprite { get; protected set; }
        public Body Body { get; protected set; }
        public float Size { get; protected set; }
        public Weapon Weapon { get; protected set; }

        public float MaxHealth { get; protected set; }
        public float Health;
        public float MaxEnergy { get; protected set; }
        public float Energy;

        public float HealthRegenRate;
        public float EnergyRegenRate;
        public float DamageTakenMultiplier; // armor
        public float DamageMultiplier;
        public float SpeedMultiplier;

        protected float Thruster;
        protected float AngularThruster;
        protected bool Shooting;

        protected Ship(Vector2 position, string texture, float size)
        {
            Sprite = new Sprite(Assets.LoadTexture(texture)).Center();
            Scale = new Vector2f(size, size);

            Body = Util.CreateShip(size);
            Body.UserData = this;
            Body.Position = position;

            Size = size;

            _forward = new Sprite(Assets.LoadTexture("ship_forward.png"));
            _forward.Origin = new Vector2f(_forward.Texture.Size.X / 2f, 0) - new Vector2f(0, 65);

            _backward = new Sprite(Assets.LoadTexture("ship_backward.png"));
            _backward.Origin = new Vector2f(_backward.Texture.Size.X / 2f, _backward.Texture.Size.Y) - new Vector2f(0, -54);

            _left = new Sprite(Assets.LoadTexture("ship_left.png"));
            _left.Origin = new Vector2f(0, _left.Texture.Size.Y / 2f) - new Vector2f(15, -37);

            _right = new Sprite(Assets.LoadTexture("ship_right.png"));
            _right.Origin = new Vector2f(_right.Texture.Size.X, _right.Texture.Size.Y / 2f) - new Vector2f(-15, -37);
        }

        public void Destroyed()
        {
            Program.World.RemoveBody(Body);
        }

        public virtual void Update()
        {
            if (Health <= 0)
            {
                Dead = true;
                return;
            }

            Health = Math.Min(Health + HealthRegenRate * Program.FrameTime, MaxHealth);
            Energy = Math.Min(Energy + EnergyRegenRate * Program.FrameTime, MaxEnergy);

            HealthRegenRate = Math.Max(HealthRegenRate, 0);
            DamageMultiplier = Util.Clamp(DamageMultiplier, 0.05f, 2.00f);
            DamageTakenMultiplier = Util.Clamp(DamageTakenMultiplier, 0.50f, 1.50f);
            SpeedMultiplier = Math.Max(SpeedMultiplier, 0.80f);

            if (Weapon != null && Shooting)
                Weapon.TryShoot();

            // TODO: speed doesnt scale properly
            var linearSpeed = 25 * (float)Math.Pow(Size, 2.5) * SpeedMultiplier;
            var angularSpeed = 8 * (float)Math.Pow(Size, 3) * SpeedMultiplier;
            Body.ApplyForce(Body.GetWorldVector(new Vector2(0.0f, Util.Clamp(Thruster, -1.0f, 0.5f) * linearSpeed)));
            Body.ApplyTorque(AngularThruster * angularSpeed);
        }

        public virtual void Draw(RenderTarget target)
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
