using System;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using GlitchGame.Devices;
using GlitchGame.Weapons;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Entities
{
    public abstract class Ship : Entity
    {
        private const int DamageSprites = 5;
        private const string SpriteFormat = "{0}{1}.png";

        private Sprite _forward;
        private Sprite _backward;
        private Sprite _left;
        private Sprite _right;

        public override RadarValue RadarType { get { return RadarValue.Count; } }

        public Sprite Sprite { get; protected set; }
        public float Size { get; protected set; }
        public Weapon Weapon { get; protected set; }

        public int Team { get; private set; }

        public float MaxHealth { get; protected set; }
        public float Health;
        public float MaxEnergy { get; protected set; }
        public float Energy;

        public float HealthRegenRate;
        public float EnergyRegenRate;
        public float DamageTakenMultiplier; // armor
        public float DamageMultiplier;
        public float SpeedMultiplier;
        public float NerfMultiplier;

        protected float Thruster;
        protected float AngularThruster;
        protected bool Shooting;

        protected Ship(State state, Vector2 position, float size, int team)
            : base(state)
        {
            Sprite = new Sprite(Assets.LoadTexture(string.Format(SpriteFormat, "ship", 0))).Center();

            if (team != 0)
                Sprite.Color = new Color(255, 200, 220);

            Scale = new Vector2f(size, size);

            #region Body Initialize
            InitializeBody(state.World, size);

            Body.UserData = this;
            Body.Position = position;
            Body.Rotation = (float)Program.Random.NextDouble() * Util.Pi2;
            #endregion

            Size = size;
            Team = team;

            DamageTakenMultiplier = 1;
            DamageMultiplier = 1;
            SpeedMultiplier = 1;
            NerfMultiplier = 1;

            _forward = new Sprite(Assets.LoadTexture("ship_forward.png"));
            _forward.Origin = new Vector2f(_forward.Texture.Size.X / 2f, 0) - new Vector2f(0, 65);

            _backward = new Sprite(Assets.LoadTexture("ship_backward.png"));
            _backward.Origin = new Vector2f(_backward.Texture.Size.X / 2f, _backward.Texture.Size.Y) - new Vector2f(0, -54);

            _left = new Sprite(Assets.LoadTexture("ship_left.png"));
            _left.Origin = new Vector2f(0, _left.Texture.Size.Y / 2f) - new Vector2f(15, -37);

            _right = new Sprite(Assets.LoadTexture("ship_right.png"));
            _right.Origin = new Vector2f(_right.Texture.Size.X, _right.Texture.Size.Y / 2f) - new Vector2f(-15, -37);
        }

        public virtual void InitializeBody(World world, float size)
        {
            Body = new Body(State.World);
            Body.BodyType = BodyType.Dynamic;
            Body.LinearDamping = 0.5f;
            Body.AngularDamping = 1.0f;

            // tip
            var rect1 = new PolygonShape(PolygonTools.CreateRectangle(0.23f * size, 0.55f * size, new Vector2(0, -0.45f) * size, 0), 1);

            // tail
            var rect2 = new PolygonShape(PolygonTools.CreateRectangle(0.725f * size, 0.45f * size, new Vector2(0, 0.55f) * size, 0), 3);

            Body.CreateFixture(rect1);
            Body.CreateFixture(rect2);
        }

        public override void Update(float dt)
        {
            /*if (Health <= 0)
            {
                Dead = true;
                return;
            }*/

            var a = Math.Abs(1 - NerfMultiplier);
            var s = Math.Sign(1 - NerfMultiplier);
            NerfMultiplier += Util.Clamp(a, -0.05f, 0.05f) * s * dt;
            NerfMultiplier = Util.Clamp(NerfMultiplier, 0.5f, 1.5f);

            Health = Util.Clamp(Health + HealthRegenRate * NerfMultiplier * dt, 0, MaxHealth);
            Energy = Util.Clamp(Energy + EnergyRegenRate * NerfMultiplier * dt, 0, MaxEnergy);

            if (Weapon != null && Shooting)
                Weapon.TryShoot();

            // TODO: see if we can make large ships turn slower
            var linearSpeed = 7.5f * Body.Mass * SpeedMultiplier * NerfMultiplier;
            var angularSpeed = 2.5f * Body.Inertia * SpeedMultiplier * NerfMultiplier;
            Body.ApplyForce(Body.GetWorldVector(new Vector2(0.0f, Util.Clamp(Thruster, -1.0f, 0.5f) * linearSpeed)));
            Body.ApplyTorque(AngularThruster * angularSpeed);
        }

        public override void Draw(RenderTarget target)
        {
            var damage = Health / MaxHealth;
            var damageIdx = (DamageSprites - 1) - (int)Util.Clamp(damage * DamageSprites, 0, DamageSprites - 1);
            Sprite.Texture = Assets.LoadTexture(string.Format(SpriteFormat, "ship", damageIdx));

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
