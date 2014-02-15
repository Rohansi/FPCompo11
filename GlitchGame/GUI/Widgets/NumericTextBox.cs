using System;
using SFML.Window;
using Texter;

namespace GlitchGame.Gui.Widgets
{
    public class NumericTextBox : Widget
    {
        private string _value;
        private int _intValue;
        private int _selectedIndex;
        private int _view;
        private int _frames;
        private bool _caretVisible;

        public int Minimum;
        public int Maximum;
        public event Action Changed;

        public NumericTextBox(int x, int y, uint w)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = 1;

            Value = 0;
        }

        public int Value
        {
            get { return _intValue; }
            set
            {
                _intValue = value;
                _value = _intValue.ToString("D");

                if (Changed != null)
                    Changed();
            }
        }

        public int SelectedIndex
        {
            get { return Math.Max(Math.Min(_selectedIndex, _value.Length), 0); }
            set { _selectedIndex = value; }
        }

        public override void Draw(ITextRenderer renderer)
        {
            if (SelectedIndex > _view + Width - 1)
                _view = SelectedIndex - (int)Width + 1;
            else if (SelectedIndex <= _view)
                _view = SelectedIndex - (int)Width - 1;
            if (_view < 0)
                _view = 0;
            if (_view > _value.Length)
                _view = _value.Length - 1;
            if (_value.Length < Width)
                _view = 0;

            var text = _value.Substring(_view);

            renderer.Clear(GuiSettings.TextBox);
            renderer.DrawText(0, 0, text, GuiSettings.TextBox);

            _frames++;
            if (_frames >= 30)
            {
                _caretVisible = !_caretVisible;
                _frames = 0;
            }

            if (Focussed && _caretVisible)
                renderer.Set(SelectedIndex - _view, 0, GuiSettings.TextBoxCaret);
        }

        public override bool KeyPressed(Keyboard.Key key, string text)
        {
            if (text == null)
            {
                switch (key)
                {
                    case Keyboard.Key.Left:
                        SelectedIndex -= 1;
                        break;
                    case Keyboard.Key.Right:
                        SelectedIndex += 1;
                        break;
                    case Keyboard.Key.Delete:
                        if (_value.Length > 0 && SelectedIndex < _value.Length)
                            _value = _value.Remove(SelectedIndex, 1);
                        break;
                    default:
                        return true;
                }
            }
            else if (text == "\b")
            {
                if (_value.Length == 0 || SelectedIndex - 1 < 0)
                    return true;

                _value = _value.Remove(SelectedIndex - 1, 1);
                _selectedIndex -= 1;
            }
            else
            {
                if (_value.Length > 12 || text.Length > 1)
                    return true;

                var c = text[0];
                if (!char.IsDigit(c) && c != '-')
                    return true;

                _value = _value.Insert(SelectedIndex, text);
                SelectedIndex += text.Length;
            }

            _caretVisible = true;
            _frames = 0;

            if (_value.Length == 0)
            {
                _intValue = 0;
                return true;
            }

            long v;
            if (!long.TryParse(_value, out v))
                return true;

            v = Math.Max(Math.Min(v, Maximum), Minimum);
            _intValue = (int)v;
            _value = _intValue.ToString("D");

            if (Changed != null)
                Changed();

            return true;
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (button != Mouse.Button.Left || !pressed)
                return true;

            SelectedIndex = _view + x;
            return true;
        }
    }
}
