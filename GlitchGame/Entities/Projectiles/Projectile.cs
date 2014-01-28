using FarseerPhysics.Dynamics;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Entities.Projectiles
{
    public abstract class Projectile : Entity
    {
        private const float MaxLifeTime = 15;

        private float _lifeTime;

        public override int Depth { get { return 0; } }

        public Ship Parent { get; protected set; }
        public Sprite Sprite { get; protected set; }
        public Body Body { get; protected set; }
        public float Size { get; protected set; }

        protected Projectile(Ship parent, string texture, float size)
        {
            Parent = parent;
            Sprite = new Sprite(Assets.LoadTexture(texture)).Center();
            Size = size;
            Scale = new Vector2f(size, size);

            _lifeTime = 0;
        }

        public override void Destroyed()
        {
            Program.World.RemoveBody(Body);
        }

        public override void Update()
        {
            _lifeTime += Program.FrameTime;

            if (_lifeTime >= MaxLifeTime)
                Dead = true;
        }

        public virtual void Hit(Ship ship)
        {
            Assets.PlaySound("hit.wav");
        }

        public override void Draw(RenderTarget target)
        {
            Position = Body.Position.ToSfml() * Program.PixelsPerMeter;
            Rotation = Util.Direction(Body.LinearVelocity) * Program.DegreesPerRadian + 90;
            target.Draw(Sprite, new RenderStates(Transform));
        }
    }
}
