using FarseerPhysics.Dynamics;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Entities.Projectiles
{
    public abstract class Projectile : Transformable, IEntity
    {
        public int DrawOrder { get { return 0; } }
        public byte RadarType { get; protected set; }
        public bool Dead { get; protected set; }

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

            RadarType = 2;
        }

        public void Destroyed()
        {
            Program.World.RemoveBody(Body);
        }

        public virtual void Update()
        {
        }

        public void Draw(RenderTarget target)
        {
            Position = Body.Position.ToSfml() * Program.PixelsPerMeter;
            Rotation = Util.Direction(Body.LinearVelocity) * Program.DegreesPerRadian + 90;
            target.Draw(Sprite, new RenderStates(Transform));
        }
    }
}
