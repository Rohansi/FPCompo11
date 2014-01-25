using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.Entities
{
    public class Bullet : Transformable, IEntity
    {
        private Sprite _sprite;
        private Body _body;

        public int DrawOrder { get { return 0; } }
        public byte RadarType { get { return 2; } }

        public bool Dead { get; private set; }

        public Bullet(Body parent, Vector2 offset)
        {
            _sprite = new Sprite(Assets.LoadTexture("bullet.png"));
            _sprite.Origin = new Vector2f(_sprite.Texture.Size.X / 2f, 0);

            _body = Util.CreateBullet(parent, offset, 7.5f, (a, b, contact) =>
            {
                Program.Entities.Remove(this);
                return true;
            });

            _body.UserData = this;
        }

        public void Destroyed()
        {
            Program.World.RemoveBody(_body);
        }

        public void Update()
        {
            
        }

        public void Draw(RenderTarget target)
        {
            Position = _body.Position.ToSfml() * Program.PixelsPerMeter;
            Rotation = _body.Rotation * Program.DegreesPerRadian;

            target.Draw(_sprite, new RenderStates(Transform));
        }
    }
}
