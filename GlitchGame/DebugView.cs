using System;
using System.Linq;
using GlitchGame.Entities;
using GlitchGame.GUI;
using SFML.Graphics;
using SFML.Window;
using Texter;

namespace GlitchGame
{
    public class DebugView : Transformable, Drawable
    {
        private TextDisplay _display;
        private GuiSystem _gui;
        private State _state;
        private VertexArray _radar;
        private Computer _target;

        public DebugView()
        {
            TextDisplay.DataFolder = "Data/Texter/";
            _radar = new VertexArray(PrimitiveType.LinesStrip, Program.RadarRays + 1);

            _gui = new GuiSystem(1000, 250);

            var desktop = new Container(1000, 250);
            desktop.Top = 1;
            _gui.Add(desktop);

            var menu = new GUI.Widgets.MenuBar();
            var menuItem = new GUI.Widgets.MenuItem("hello???");
            var menuItem2 = new GUI.Widgets.MenuItem("wowow!");
            var menuItem3 = new GUI.Widgets.MenuItem("hi :)");
            menuItem2.Items.Add(menuItem3);
            menuItem.Items.Add(menuItem2);
            menu.Items.Add(menuItem);
            _gui.Add(menu);

            var window = new GUI.Widgets.Window(10, 10, 100, 30, "hello world");

            var text = new GUI.Widgets.TextBox(2, 1, 70);
            window.Add(text);

            var scroll = new GUI.Widgets.Scrollbar(2, 3, 10);
            window.Add(scroll);

            var list = new GUI.Widgets.ListBox(4, 3, 68, 10);
            list.SelectEnabled = true;
            list.Items.Add(new GUI.Widgets.ListBoxItem("sup"));
            list.Items.Add(new GUI.Widgets.ListBoxItem("nerd"));
            list.Items.Add(new GUI.Widgets.ListBoxItem("shoar"));
            window.Add(list);

            var label = new GUI.Widgets.Label(2, 14, 10, 1, "how r u");
            window.Add(label);

            var button = new GUI.Widgets.Button(13, 14, 10, "boop me");
            window.Add(button);

            desktop.Add(window);
        }

        public void Attach(State state)
        {
            if (_state != null)
                throw new Exception("Must detach first");

            _state = state;
            _target = null;

            Program.Window.MouseButtonPressed += MousePressed;
            _gui.Attach(Program.Window);
        }

        public void Detatch()
        {
            if (_state == null)
                throw new Exception("Must attach first");

            _gui.Detach();
            Program.Window.MouseButtonPressed -= MousePressed;

            _state = null;
            _target = null;
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

            CheckResize();
            _display.Clear(new Character(background: 255));
            _gui.Draw(_display);

            states.Transform *= Transform;
            target.Draw(_display, states);

            target.Draw(_radar, states);
        }

        private void CheckResize()
        {
            var bounds = Program.HudCamera.Bounds;
            var width = (uint)bounds.Width / 8 + 1;
            var height = (uint)bounds.Height / 12 + 1;

            if (_display == null || _display.Width != width || _display.Height != height)
            {
                if (_display != null)
                    _display.Dispose();

                _display = new TextDisplay(width, height);
            }
        }

        private void MousePressed(object sender, MouseButtonEventArgs args)
        {
            var pos = Program.Window.MapPixelToCoords(new Vector2i(args.X, args.Y), Program.Camera.View);
            var ents = _state.EntitiesInRegion(new FloatRect(pos.X - 32, pos.Y - 32, 64, 64));
            _target = ents.OfType<Computer>().FirstOrDefault();
        }
    }
}
