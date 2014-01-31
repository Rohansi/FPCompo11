using GlitchGame.Entities;
using Microsoft.Xna.Framework;

namespace GlitchGame.States
{
    public class Game : GameBase
    {
        public Game()
            : base(40, 0.075f)
        {
            const int enemies = 20;
            const float distFromPlayer = 30;

            var size = new Vector2(2.5f, 2.5f);
            var failed = 0;

            for (var i = 0; i < enemies; i++)
            {
                var dir = (float)Program.Random.NextDouble() * Util.Pi2;
                var pos = Player.Body.Position + Util.LengthDir(dir, distFromPlayer).ToFarseer();
                var space = FindOpenSpace(pos, 1, size);

                if (!space.HasValue)
                {
                    i--;
                    failed++;

                    if (failed < 25)
                        continue;

                    break;
                }

                Entities.AddLast(new Enemy(this, space.Value));
            }

            /*for (var i = 0; i < enemies; i++)
            {
                var size = new Vector2(2, 2);
                var space = FindOpenSpace(new Vector2(), Radius, size);

                if (!space.HasValue)
                    continue;

                Entities.AddLast(new Enemy(this, space.Value));
            }*/
        }
    }
}
