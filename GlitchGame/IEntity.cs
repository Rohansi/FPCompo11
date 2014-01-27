using GlitchGame.Devices;
using SFML.Graphics;

namespace GlitchGame
{
    public interface IEntity
    {
        int Depth { get; }
        RadarValue Radar { get; }

        bool Dead { get; }

        void Destroyed();

        void Update();
        void Draw(RenderTarget target);
    }
}
