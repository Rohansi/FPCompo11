using System;
using SFML.Window;
using Texter;

namespace GlitchGame.GUI.Widgets
{
    public class Window : Widget, IContainer
    {
        private Container _children;
        private bool _dragging;
        private int _dragOffset;

        public string Caption;
        public Func<bool> Closing = null;
        public event Action Closed;

        public Window(int x, int y, int w, int h, string caption)
        {
            Left = x;
            Top = y;
            Width = (uint)w;
            Height = (uint)h;

            _children = new Container(Width - 2, Height - 2);
            Caption = caption;
        }

        public override void Draw(ITextRenderer renderer)
        {
            renderer.DrawBox(0, 0, Width, Height, TextExtensions.DoubleBox, GuiSettings.Window);
            renderer.DrawText(2, 0, "[\xFE]", GuiSettings.Window);

            if (!string.IsNullOrEmpty(Caption))
                renderer.DrawText(6, 0, string.Format(" {0} ", Caption), GuiSettings.WindowCaption);

            _children.Draw(renderer.Region(1, 1, Width - 2, Height - 2));
        }

        public override bool KeyPressed(Keyboard.Key key, string text)
        {
            return _children.KeyPressed(key, text);
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (button == Mouse.Button.Left && y == 0)
            {
                if (_dragging && !pressed)
                    _dragging = false;

                if (x == 3)
                {
                    var close = true;
                    if (Closing != null)
                        close = Closing();

                    if (close)
                    {
                        Visible = false;

                        if (Closed != null)
                            Closed();
                    }

                    return true;
                }

                _dragging = pressed;
                _dragOffset = x;
            }

            _children.MousePressed(x - 1, y - 1, button, pressed);
            return true;
        }

        public override void MouseMoved(int x, int y)
        {
            if (_dragging)
            {
                Left += x - _dragOffset;
                Top += y;

                Left = Clamp(Left, 0, (int)Parent.SurfaceWidth - 8);
                Top = Clamp(Top, 0, (int)Parent.SurfaceHeight - 1);
            }

            _children.MouseMoved(x - 1, y - 1);
        }

        private static int Clamp(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        #region IContainer Methods
        public uint SurfaceWidth { get { return _children.SurfaceWidth; } }
        public uint SurfaceHeight { get { return _children.SurfaceHeight; } }

        public void Add(Widget widget)
        {
            _children.Add(widget);
            widget.Initialize(this);
        }

        public void Remove(Widget widget)
        {
            _children.Remove(widget);
        }

        public void Focus(Widget widget)
        {
            _children.Focus(widget);
        }

        public void BringToFront(Widget widget)
        {
            _children.BringToFront(widget);
        }

        public void RemoveFocus()
        {
            _children.RemoveFocus();
        }
        #endregion
    }
}
