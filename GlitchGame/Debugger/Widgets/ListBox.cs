﻿using System;
using System.Collections.Generic;
using SFML.Window;
using Texter;

namespace GlitchGame.Debugger.Widgets
{
    public class ListBoxItem
    {
        public string Text;
        public object Tag;

        public ListBoxItem(string text, object tag = null)
        {
            Text = text;
            Tag = tag;
        }
    }

    public class ListBox : Widget
    {
        public List<ListBoxItem> Items { get; private set; }
        public int Selected;
        public bool SelectEnabled;
        public event Action Changed;

        private Scrollbar _scrollbar;

        public ListBox(int x, int y, uint w, uint h)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = h;

            Items = new List<ListBoxItem>();

            Selected = -1;
            SelectEnabled = false;

            _scrollbar = new Scrollbar((int)Width - 1, 1, Height - 2);
            _scrollbar.Minimum = 0;
        }

        public override void Draw(ITextRenderer renderer)
        {
            renderer.DrawBox(0, 0, Width, Height, TextExtensions.SingleBox, GuiSettings.ListBox);

            _scrollbar.Maximum = Math.Max(Items.Count - (Height - 2), 0);
            var startIndex = (int)_scrollbar.Value;

            for (var i = 0; i < Height - 2; i++)
            {
                var index = startIndex + i;
                if (index >= Items.Count)
                    break;

                var reg = renderer.Region(1, i + 1, Width - 2, 1);
                var col = (SelectEnabled && Selected == index) ? GuiSettings.ListBoxItemSelected : GuiSettings.ListBoxItem;

                reg.Clear(col, true);
                reg.DrawText(0, 0, Items[index].Text, col);
            }

            _scrollbar.Draw(renderer.Region(_scrollbar.Left, _scrollbar.Top, _scrollbar.Width, _scrollbar.Height));
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (SelectEnabled && pressed && (x > 0 && y > 0 && x < Width - 1 && y < Height - 1))
            {
                var index = (int)_scrollbar.Value + (y - 1);

                if (index < Items.Count)
                {
                    Selected = index;

                    if (Changed != null)
                        Changed();
                }

                return true;
            }

            _scrollbar.MousePressed(x - _scrollbar.Left, y - _scrollbar.Top, button, pressed);
            return true;
        }

        public override void MouseMoved(int x, int y)
        {
            _scrollbar.MouseMoved(x - _scrollbar.Left, y - _scrollbar.Top);
        }

        public override bool MouseWheelMoved(int x, int y, int delta)
        {
            _scrollbar.MouseWheelMoved(x, y, delta);
            return true;
        }
    }
}
