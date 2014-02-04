using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Entities.Projectiles
{
    public abstract class Projectile : Entity
    {
        private const float MaxLifeTime = 15;

        private float _lifeTime;
        private readonly float _healthDamageMultiplier;
        private readonly float _energyDamageMultiplier;

        public override int Depth { get { return 0; } }

        public Ship Parent { get; protected set; }
        public Sprite Sprite { get; protected set; }
        public float Size { get; protected set; }

        public abstract bool DirectionalRotation { get; }

        protected Projectile(Ship parent, string texture, float size)
            : base(Program.State)
        {
            Parent = parent;
            Sprite = new Sprite(Assets.LoadTexture(texture)).Center();
            Size = size;
            Scale = new Vector2f(size, size);

            _healthDamageMultiplier = Size * Parent.DamageMultiplier * Parent.NerfMultiplier;
            _energyDamageMultiplier = Size * Parent.NerfMultiplier;

            _lifeTime = 0;
        }

        public override void Update()
        {
            _lifeTime += Program.FrameTime;

            if (_lifeTime >= MaxLifeTime)
                Dead = true;
        }

        public virtual void Hit(Ship ship)
        {
            Assets.PlaySound("hit.wav", ship.Position);
        }

        public override void Draw(RenderTarget target)
        {
            Position = Body.Position.ToSfml() * Program.PixelsPerMeter;

            if (DirectionalRotation)
                Rotation = Util.Direction(Body.LinearVelocity) * Program.DegreesPerRadian + 90;
            else
                Rotation = Body.Rotation * Program.DegreesPerRadian;

            target.Draw(Sprite, new RenderStates(Transform));
        }

        protected float HealthDamageMultiplier(Ship ship)
        {
            return _healthDamageMultiplier * ship.DamageTakenMultiplier * ship.NerfMultiplier;
        }

        protected float EnergyDamageMultiplier(Ship ship)
        {
            return _energyDamageMultiplier * ship.NerfMultiplier;
        }
    }
}
