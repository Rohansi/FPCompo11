using System;
using SFML.Graphics;
using SFML.Window;

namespace GlitchGame.GUI
{
    public class GuiSystem : Container
    {
        private RenderWindow _target;

        public GuiSystem(uint w, uint h)
            : base(w, h)
        {
            
        }

        public void Attach(RenderWindow window)
        {
            if (_target != null)
                throw new Exception("Must detach first");

            _target = window;

            _target.TextEntered += WindowTextEntered;
            _target.KeyPressed += WindowKeyPressed;
            _target.MouseButtonPressed += WindowMousePressed;
            _target.MouseButtonReleased += WindowMouseReleased;
            _target.MouseMoved += WindowMouseMoved;
        }

        public void Detach()
        {
            if (_target == null)
                throw new Exception("Must attach first");

            _target.TextEntered += WindowTextEntered;
            _target.KeyPressed += WindowKeyPressed;
            _target.MouseButtonPressed += WindowMousePressed;
            _target.MouseButtonReleased += WindowMouseReleased;
            _target.MouseMoved += WindowMouseMoved;

            _target = null;
        }

        private void WindowTextEntered(object sender, TextEventArgs args)
        {
            KeyPressed(Keyboard.Key.Unknown, args.Unicode);
        }

        private void WindowKeyPressed(object sender, KeyEventArgs args)
        {
            KeyPressed(args.Code, null);
        }

        private void WindowMousePressed(object sender, MouseButtonEventArgs args)
        {
            WindowMouseButtonImpl(args.X, args.Y, args.Button, true);
        }

        private void WindowMouseReleased(object sender, MouseButtonEventArgs args)
        {
            WindowMouseButtonImpl(args.X, args.Y, args.Button, false);
        }

        private void WindowMouseButtonImpl(int x, int y, Mouse.Button button, bool pressed)
        {
            if (pressed)
                RemoveFocus();

            var pos = _target.MapPixelToCoords(new Vector2i(x, y), Program.HudCamera.View);
            MousePressed((int)pos.X / GuiSettings.CharWidth, (int)pos.Y / GuiSettings.CharHeight, button, pressed);
        }

        private void WindowMouseMoved(object sender, MouseMoveEventArgs args)
        {
            var pos = _target.MapPixelToCoords(new Vector2i(args.X, args.Y), Program.HudCamera.View);
            MouseMoved((int)pos.X / GuiSettings.CharWidth, (int)pos.Y / GuiSettings.CharHeight);
        }
    }
}
