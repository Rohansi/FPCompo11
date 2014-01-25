using SFML.Graphics;

namespace GlitchGame
{
    public interface IEntity
    {
        int DrawOrder { get; }
        byte RadarType { get; }

        bool Dead { get; }

        void Destroyed();

        void Update();
        void Draw(RenderTarget target);
    }
}
