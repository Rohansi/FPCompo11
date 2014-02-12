using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;
using Texter;

namespace GlitchGame.Gui.Widgets
{
    public class MenuBar : Widget
    {
        public List<MenuItem> Items { get; private set; }

        private MenuItem _selected;
        private MenuDrop _drop;

        public MenuBar()
        {
            Left = 0;
            Top = 0;
            Width = 1000; // assuming 1000 is impossibly wide
            Height = 1;

            Items = new List<MenuItem>();
            _drop = new MenuDrop();
        }

        public override void Initialize(IContainer parent)
        {
            base.Initialize(parent);
            Parent.Add(_drop);
        }

        public override void Draw(ITextRenderer renderer)
        {
            Width = Parent.SurfaceWidth;

            if (!_drop.Visible)
                _selected = null;

            renderer.Clear(GuiSettings.Menu);

            var x = 0;
            foreach (var i in Items)
            {
                var highlight = _selected != null && _selected == i;
                i.Draw(renderer.Region(x, 0, (uint)i.Caption.Length + 2, 1), highlight, false);
                x += i.Caption.Length + 2;
            }
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (button != Mouse.Button.Left || !pressed)
                return true;

            var xx = 0;
            foreach (var i in Items)
            {
                if (x >= xx && x < xx + i.Caption.Length + 2)
                {
                    i.Click();

                    if (i.Items.Count > 0)
                    {
                        _selected = i;
                        _drop.Show(xx, 1, i.Items);
                        _drop.Focus();
                        return true;
                    }
                }

                xx += i.Caption.Length + 2;
            }

            return true;
        }
    }

    class MenuDrop : Widget
    {
        private List<MenuItem> _items;

        private MenuDrop _drop;
        private MenuItem _selected;

        public MenuDrop()
        {
            Visible = false;
        }

        public void Show(int x, int y, List<MenuItem> items)
        {
            Left = x;
            Top = y;
            Width = (uint)Math.Max(items.Max(i => i.Caption.Length), 18) + 5;
            Height = (uint)items.Count + 2;
            Visible = true;

            if (_drop == null)
            {
                _drop = new MenuDrop();
                Parent.Add(_drop);
            }

            _selected = null;
            _items = items;
        }

        public override void Draw(ITextRenderer renderer)
        {
            if (_drop == null || !_drop.Visible)
                _selected = null;

            if (!Focussed && _selected == null)
                Visible = false;

            renderer.DrawBox(0, 0, Width, Height, TextExtensions.SingleBox, GuiSettings.MenuOutline);

            var y = 1;
            foreach (var i in _items)
            {
                var highlight = _selected != null && _selected == i;
                i.Draw(renderer.Region(1, y, Width - 2, 1), highlight, true);
                y++;
            }
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (button != Mouse.Button.Left || !pressed)
                return true;

            var yy = 1;
            foreach (var i in _items)
            {
                if ((x >= 1 && x < Width - 1) && yy == y)
                {
                    i.Click();

                    if (i.Items.Count > 0)
                    {
                        _selected = i;
                        _drop.Show(Left + 1, Top + yy + 1, i.Items);
                        _drop.Focus();
                    }
                    else
                    {
                        Visible = false;
                    }

                    return true;
                }

                yy++;
            }

            return true;
        }
    }

    public class MenuItem
    {
        public string Caption;
        public List<MenuItem> Items { get; private set; }
        public event Action Clicked;

        public MenuItem(string caption)
        {
            Items = new List<MenuItem>();
            Caption = caption;
        }

        public void Draw(ITextRenderer renderer, bool highlight, bool extended)
        {
            var col = highlight ? GuiSettings.MenuItemHighlight : GuiSettings.MenuItem;
            var caption = string.Format(" {0} ", Caption);

            renderer.Clear(col);
            renderer.DrawText(0, 0, caption, col);

            if (extended && Items.Count > 0)
                renderer.Set((int)renderer.Width - 1, 0, GuiSettings.MenuArrow);
        }

        // INTERNAL
        public void Click()
        {
            if (Clicked != null)
                Clicked();
        }
    }
}
