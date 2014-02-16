using System;
using SFML.Window;
using Texter;

namespace GlitchGame.Debugger.Widgets
{
    public class Scrollbar : Widget
    {
        private float _internalValue;
        private bool _dragging;

        public event Action Changed;
        public float Minimum, Maximum;
        public float Step = 1;

        public Scrollbar(int x, int y, uint height)
        {
            Left = x;
            Top = y;
            Height = height;
            Width = 1;
        }

        private float InternalValue
        {
            get { return _internalValue; }
            set
            {
                _internalValue = value < 0 ? 0 : (value > 1 ? 1 : value);
                if (Changed != null)
                    Changed();
            }
        }

        public float Value
        {
            get { return (InternalValue * (Maximum - Minimum)) + Minimum; }
            set { InternalValue = (value - Minimum) / (Maximum - Minimum); }
        }

        public override void Draw(ITextRenderer renderer)
        {
            renderer.Set(0, 0, GuiSettings.ScrollbarUp);
            for (var y = 1; y < Height - 1; y++)
            {
                renderer.Set(0, y, GuiSettings.Scrollbar);
            }
            renderer.Set(0, (int)Height - 1, GuiSettings.ScrollbarDown);

            var x = 1 + (InternalValue * (Height - 3));
            renderer.Set(0, (int)x, GuiSettings.ScrollbarThumb);
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (button != Mouse.Button.Left)
                return true;

            if (pressed && x == 0)
            {
                if (y == 0)
                {
                    Value -= Step;
                }
                else if (y == Height - 1)
                {
                    Value += Step;
                }
                else
                {
                    _dragging = true;
                }
            }

            if (!pressed)
                _dragging = false;

            return true;
        }

        public override void MouseMoved(int x, int y)
        {
            if (!_dragging)
                return;

            InternalValue = ((float)y) / (Height - 2);
        }

        public override bool MouseWheelMoved(int x, int y, int delta)
        {
            Value -= Step * delta;
            return true;
        }
    }
}
