using System;
using System.Linq;
using GlitchGame.Entities;
using GlitchGame.GUI;
using SFML.Graphics;
using Texter;

namespace GlitchGame
{
    public partial class DebugView : Transformable, Drawable
    {
        private TextDisplay _display;
        private GuiSystem _gui;
        private State _state;
        private Computer _target;

        public DebugView()
        {
            TextDisplay.DataFolder = "Data/Texter/";

            Initialize();
        }

        public void Attach(State state)
        {
            if (_state != null)
                throw new Exception("Must detach first");

            _state = state;
            _target = null;
        }

        public void Detatch()
        {
            if (_state == null)
                throw new Exception("Must attach first");

            _state = null;
            _target = null;
        }

        public bool ProcessEvent(InputArgs args)
        {
            if (_gui.ProcessEvent(args))
                return true;

            var mousePressedArgs = args as MouseButtonInputArgs;
            if (mousePressedArgs != null && mousePressedArgs.Pressed)
            {
                var mousePos = Program.Window.MapPixelToCoords(mousePressedArgs.Position, Program.Camera.View);
                var ents = _state.EntitiesInRegion(new FloatRect(mousePos.X - 32, mousePos.Y - 32, 64, 64));
                _target = ents.OfType<Computer>().FirstOrDefault();
                return true;
            }

            return false;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            /*if (_state == null)
                return;

            if (_target == null || _target.Dead)
            {
                _target = null;
                return;
            }*/

            Update();

            CheckResize();
            _display.Clear(new Character(background: 255));
            _gui.Draw(_display);

            states.Transform *= Transform;
            target.Draw(_display, states);
        }

        private void CheckResize()
        {
            var bounds = Program.HudCamera.Bounds;
            var width = (uint)bounds.Width / GuiSettings.CharWidth + 1;
            var height = (uint)bounds.Height / GuiSettings.CharHeight + 1;

            if (_display == null || _display.Width != width || _display.Height != height)
            {
                if (_display != null)
                    _display.Dispose();

                _display = new TextDisplay(width, height, "font.png", GuiSettings.CharWidth, GuiSettings.CharHeight);
            }
        }
    }
}
