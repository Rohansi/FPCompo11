using SFML.Graphics;
using SFML.Window;

namespace GlitchGame
{
    public class Background : Drawable
    {
        private Sprite _sprite;
        private Texture _texture;

        public Background(string texture)
        {
            _texture = Assets.LoadTexture(texture);
            _texture.Repeated = true;
            _texture.Smooth = true;

            _sprite = new Sprite(_texture);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            var bounds = Program.Camera.Bounds;
            var pos = Program.Camera.Position;

            _sprite.Position = pos - new Vector2f(bounds.Width / 2, bounds.Height / 2);
            _sprite.TextureRect = new IntRect((int)(pos.X % _texture.Size.X), (int)(pos.Y % _texture.Size.Y), (int)bounds.Width, (int)bounds.Height);
            target.Draw(_sprite);
        }
    }
}
