using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;
using Texter;
using Window = GlitchGame.Debugger.Widgets.Window;

namespace GlitchGame.Debugger.Windows
{
    public class FlappyBird : DebugWindow
    {
        private class Game : Widget
        {
            private class Player
            {
                public float X, Y, VelX, VelY;
            }

            private class Pipe
            {
                public float X, Y, W, H;

                public Pipe(float x, float y)
                {
                    X = x;
                    Y = y;
                    W = 8;
                    H = 10;
                }

                public bool Contains(float x, float y)
                {
                    if (x < X || x > X + W)
                        return false;

                    if (y >= Y && y <= Y + H)
                        return false;

                    return true;
                }
            }

            private bool _playing;
            private Player _player;
            private List<Pipe> _pipes; 
            private int _camera;
            private Random _random;

            public Game(int x, int y, uint w, uint h)
            {
                Left = x;
                Top = y;
                Width = w;
                Height = h;

                _player = new Player();
                _pipes = new List<Pipe>();
                _random = new Random();
            }

            public override void Draw(ITextRenderer renderer)
            {
                if (_playing)
                {
                    if (_player.Y >= Height - 1)
                        _playing = false;

                    if (_pipes.Any(p => p.Contains(_player.X, _player.Y)))
                        _playing = false;
                }

                if (_playing)
                {
                    var prevPipe = (int)_player.X / 32;

                    _player.VelY += 0.025f;
                    _player.X += _player.VelX;
                    _player.Y += _player.VelY;

                    if ((int)_player.X / 32 != prevPipe)
                        _pipes.Add(new Pipe(_camera + 100, _random.Next(2, (int)Height - 12)));

                    _pipes.RemoveAll(p => p.X < _camera - 10);
                }
                else
                {
                    _player.X = 10;
                    _player.Y = Height / 2f;
                    _player.VelX = 0;
                    _player.VelY = 0;

                    _pipes.Clear();
                }

                _camera = (int)_player.X - 15;

                renderer.Clear(new Character(0, 53, 53));

                Draw(renderer, _player.X, _player.Y, 3, 2, new Character(0, 43, 43));

                if (!_playing)
                    renderer.DrawText(_camera + 10, (int)Height / 2 + 5, "Press Space!", new Character(0, 0));

                foreach (var p in _pipes)
                {
                    Draw(renderer, p.X, 0, (uint)p.W, (uint)p.Y, new Character(0, 10, 10));
                    Draw(renderer, p.X, p.Y + p.H, (uint)p.W, 50, new Character(0, 10, 10));
                }

                Draw(renderer, _camera, Height - 1, Width, 2, new Character(0, 2, 2));
            }

            public override bool KeyPressed(Keyboard.Key key, string text)
            {
                if (key == Keyboard.Key.Space)
                {
                    if (_playing)
                    {
                        _player.VelY = -0.65f;
                    }
                    else
                    {
                        _player.VelX = 0.7f;
                        _playing = true;
                    }
                }

                return true;
            }
            
            private void Draw(ITextRenderer renderer, float x, float y, uint w, uint h, Character color)
            {
                renderer.DrawBox((int)x - _camera, (int)y, w, h, GuiSettings.SolidBox, color);
            }
        }

        private Window _window;
        private Game _game;

        public FlappyBird(DebugView view)
            : base(view)
        {
            _window = new Window(25, 25, 100, 30, "Flappy Square");
            View.Desktop.Add(_window);

            _game = new Game(0, 0, 98, 28);
            _window.Add(_game);
        }

        public override void Reset()
        {
            
        }

        public override void Update()
        {
            
        }

        public void Show()
        {
            _window.Visible = true;
            _window.Focus();
        }

        public void Hide()
        {
            _window.Visible = false;
        }
    }
}
