using System;
using System.Collections.Generic;
using SFML.Window;
using Texter;

namespace GlitchGame.Debugger.Widgets
{
    public class ListView<TItem> : Widget
    {
        public class Column
        {
            public readonly string Name;
            public readonly uint Width;
            public readonly bool Sortable;
            public readonly Func<TItem, IComparable> ValueSelector;
            public readonly Func<TItem, string> Formatter;
             
            public Column(string name, uint width, Func<TItem, IComparable> valueSelector, bool sortable = true, Func<TItem, string> formatter = null)
            {
                Name = name;
                Width = width;
                Sortable = sortable;
                ValueSelector = valueSelector;
                Formatter = formatter ?? (i => ValueSelector(i).ToString());
            } 
        }

        private Scrollbar _scrollbar;
        private int _sortColumn;
        private bool _sortAscending;

        public readonly List<Column> Columns;
        public readonly List<TItem> Items;

        public event Action<TItem, Mouse.Button> Clicked;
         
        public ListView(int x, int y, uint w, uint h)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = h;

            Columns = new List<Column>();
            Items = new List<TItem>();

            _scrollbar = new Scrollbar((int)Width - 1, 0, Height);
            _scrollbar.Minimum = 0;
            _scrollbar.Maximum = 1;
            _scrollbar.Step = 1;

            _sortColumn = -1;
            _sortAscending = false;
        }

        public override void Draw(ITextRenderer renderer)
        {
            _scrollbar.Maximum = Math.Max(Items.Count - Height, 0);
            var startIndex = (int)_scrollbar.Value;

            renderer.Clear(new Character(0, 0, 7), true);

            var view = renderer.Region(0, 0, Width - 1, Height);

            for (var i = 0; i < Height; i++)
            {
                var index = startIndex + i - 1;
                if (index >= Items.Count)
                    break;

                var x = 0;
                var dark = false;

                for (var j = 0; j < Columns.Count; j++)
                {
                    var column = Columns[j];
                    var reg = view.Region(x, i, column.Width, 1);

                    if (i == 0)
                    {
                        reg.Clear(new Character(0, 15, 24));
                        reg.DrawText(1, 0, column.Name, new Character(0, 15, 24));

                        if (_sortColumn == j)
                            reg.Set(0, 0, new Character(_sortAscending ? 24 : 25));
                    }
                    else
                    {
                        var item = Items[index];
                        var col = dark ? new Character(0, 0, 27) : new Character(0, 0, 7);
                        reg.Clear(col);
                        reg.DrawText(0, 0, column.Formatter(item), col);
                    }

                    x += (int)column.Width;
                    dark = !dark;
                }
            }

            _scrollbar.Draw(renderer.Region(_scrollbar.Left, _scrollbar.Top, _scrollbar.Width, _scrollbar.Height));
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            var columnIndex = -1;
            var columnX = 0;

            for (var i = 0; i < Columns.Count; i++)
            {
                var c = Columns[i];

                if (x >= columnX && x < columnX + c.Width)
                {
                    columnIndex = i;
                    break;
                }

                columnX += (int)c.Width;
            }

            if (pressed && y == 0 && columnIndex != -1)
            {
                var column = Columns[columnIndex];

                if (!column.Sortable)
                    return true;

                if (_sortColumn == columnIndex)
                    _sortAscending = !_sortAscending;
                else
                    _sortAscending = true;

                _sortColumn = columnIndex;

                var dir = _sortAscending ? 1 : -1;
                Items.Sort(new GenericComparer<TItem>(
                    (a, b) => column.ValueSelector(a).CompareTo(column.ValueSelector(b)) * dir));

                return true;
            }

            if (pressed && x >= 0 && x < Width - 1 && y >= 1 && y < Height)
            {
                var startIndex = (int)_scrollbar.Value;
                var index = startIndex + y;

                if (index > Items.Count)
                    return true;

                var item = Items[index - 1];

                if (Clicked != null)
                    Clicked(item, button);

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
