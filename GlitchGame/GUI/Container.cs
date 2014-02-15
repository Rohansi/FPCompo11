using System.Collections.Generic;
using SFML.Window;
using Texter;

namespace GlitchGame.Gui
{
    public class Container : Widget, IContainer
    {
        private LinkedList<Widget> _children;
        private Widget _focus;

        public uint SurfaceWidth { get; private set; }
        public uint SurfaceHeight { get; private set; }

        public Container(uint w, uint h)
        {
            SurfaceWidth = w;
            SurfaceHeight = h;
            Width = w;
            Height = h;

            _children = new LinkedList<Widget>();
            _focus = null;
        }

        public void Add(Widget widget)
        {
            widget.Initialize(this);
            _children.AddFirst(widget);
        }

        public void Remove(Widget widget)
        {
            _children.Remove(widget);

            if (_focus == widget)
                _focus = null;
        }

        // INTERNAL ONLY
        public void Focus(Widget widget)
        {
            if (!_children.Contains(widget))
                return;

            BringToFront(widget);

            if (_focus != null)
                _focus.Focussed = false;

            _focus = widget;

            if (Parent != null)
                Parent.Focus(this);

            widget.Focussed = true;
        }

        // INTERNAL ONLY
        public void BringToFront(Widget widget)
        {
            var node = _children.Find(widget);
            if (node == null)
                return;

            _children.Remove(node);
            _children.AddFirst(widget);

            if (Parent != null)
                Parent.BringToFront(this);
        }

        public void RemoveFocus()
        {
            if (_focus == null)
                return;

            _focus.Focussed = false;

            var focusContainer = _focus as IContainer;
            if (focusContainer != null)
            {
                focusContainer.RemoveFocus();
            }
        }

        public override void Draw(ITextRenderer renderer)
        {
            var node = _children.Last;
            while (node != null)
            {
                var widget = node.Value;
                var next = node.Previous;

                if (widget.Visible)
                {
                    var region = renderer.Region(widget.Left, widget.Top, widget.Width, widget.Height);
                    node.Value.Draw(region);
                }

                node = next;
            }
        }

        public override bool KeyPressed(Keyboard.Key key, string text)
        {
            if (_focus == null)
                return false;

            return _focus.KeyPressed(key, text);
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (pressed)
            {
                if (_focus != null)
                {
                    _focus.Focussed = false;
                    _focus = null;
                }

                var node = _children.First;
                while (node != null)
                {
                    var widget = node.Value;
                    var next = node.Next;

                    if (widget.Visible && ContainsPoint(widget, x, y))
                    {
                        widget.Focus();
                        return widget.MousePressed(x - widget.Left, y - widget.Top, button, true);
                    }

                    node = next;
                }
            }
            else
            {
                var node = _children.First;
                while (node != null)
                {
                    var widget = node.Value;
                    var next = node.Next;

                    widget.MousePressed(x - widget.Left, y - widget.Top, button, false);

                    node = next;
                }
            }

            return false;
        }

        public override void MouseMoved(int x, int y)
        {
            var node = _children.First;
            while (node != null)
            {
                var widget = node.Value;
                var next = node.Next;

                widget.MouseMoved(x - widget.Left, y - widget.Top);

                node = next;
            }
        }

        public override bool MouseWheelMoved(int x, int y, int delta)
        {
            var node = _children.First;
            while (node != null)
            {
                var widget = node.Value;
                var next = node.Next;

                if (widget.Visible && ContainsPoint(widget, x, y))
                {
                    return widget.MouseWheelMoved(x - widget.Left, y - widget.Top, delta);
                }

                node = next;
            }

            return false;
        }

        private static bool ContainsPoint(Widget widget, int x, int y)
        {
            return x >= widget.Left && y >= widget.Top && x <= (widget.Left + widget.Width) && y <= (widget.Top + widget.Height);
        }
    }
}
