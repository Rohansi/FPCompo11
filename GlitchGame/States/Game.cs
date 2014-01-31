using System;
using System.Collections.Generic;
using GlitchGame.Entities;
using Microsoft.Xna.Framework;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.States
{
    public class Game : GameBase
    {
        private List<Enemy> _enemies;

        private Text _text;
        private int _wave;
        private float _timer;

        public Game()
            : base(35, 0.075f)
        {
            _enemies = new List<Enemy>();

            _text = new Text("", Program.Font, 32);
            _wave = 1;
            _timer = 5;
        }

        public override void Update()
        {
            var hadEnemies = _enemies.Count > 0;
            _enemies.RemoveAll(e => e.Dead);

            if (hadEnemies)
            {
                if (_timer <= 0 && _enemies.Count == 0)
                    _timer = 5;
            }
            else if (_timer <= 0)
            {
                var enemyCount = 2 * _wave;
                Spawn(enemyCount);
                _wave++;

                Player.Health = Player.MaxHealth;
                Player.Energy = Player.MaxEnergy;
            }

            _timer -= Program.FrameTime;

            base.Update();
        }

        public override void DrawHud(RenderTarget target)
        {
            base.DrawHud(target);

            if (_timer < -1)
                return;

            var bounds = Program.HudCamera.Bounds;
            _text.Position = new Vector2f(bounds.Width / 2, bounds.Height / 5);

            if (_timer >= 0)
                _text.DisplayedString = string.Format("Wave {0} in {1} ...", _wave, (int)Math.Ceiling(_timer));
            else
                _text.DisplayedString = "Fight!";

            _text.Center();
            target.Draw(_text);
        }

        private void Spawn(int n)
        {
            var size = new Vector2(1, 1);
            var failed = 0;

            for (var i = 0; i < n; i++)
            {
                var space = FindOpenSpace(new Vector2(0), Radius, size);

                if (!space.HasValue || Util.Distance(space.Value.ToSfml(), Player.Body.Position.ToSfml()) < 25)
                {
                    i--;
                    failed++;

                    if (failed < 100)
                        continue;

                    break;
                }

                var enemy = new Enemy(this, space.Value);
                Entities.AddLast(enemy);
                _enemies.Add(enemy);
            }
        }
    }
}
