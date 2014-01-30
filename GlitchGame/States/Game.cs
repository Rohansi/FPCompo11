using GlitchGame.Entities;
using Microsoft.Xna.Framework;

namespace GlitchGame.States
{
    public class Game : GameBase
    {
        public Game()
            : base(30, 0.1f)
        {
            const int enemies = 10;

            for (var i = 0; i < enemies; i++)
            {
                var size = new Vector2(2, 2);
                var space = FindOpenSpace(new Vector2(), Radius, size);

                if (!space.HasValue)
                    continue;

                Entities.AddLast(new Enemy(this, space.Value));
            }
        }
    }
}
