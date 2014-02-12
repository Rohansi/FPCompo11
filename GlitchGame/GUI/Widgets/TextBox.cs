using System;
using SFML.Window;
using Texter;

namespace GlitchGame.Gui.Widgets
{
    public class TextBox : Widget
    {
        private string _value;
        private int _selectedIndex;
        private int _view;
        private int _frames;
        private bool _caretVisible;

        public char? PasswordCharacter;
        public event Action Changed;

        public TextBox(int x, int y, uint w)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = 1;

            Value = "";
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (Changed != null)
                    Changed();
            }
        }

        public int SelectedIndex
        {
            get { return Math.Max(Math.Min(_selectedIndex, Value.Length), 0); }
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
            if (_view > Value.Length)
                _view = Value.Length - 1;
            if (Value.Length < Width)
                _view = 0;

            var text = Value.Substring(_view);
            if (PasswordCharacter.HasValue)
                text = new string(PasswordCharacter.Value, text.Length);

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
            _caretVisible = true;

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
                        if (Value.Length > 0 && SelectedIndex < Value.Length)
                            Value = Value.Remove(SelectedIndex, 1);
                        break;
                }

                return true;
            }

            if (text == "\b")
            {
                if (Value.Length == 0 || SelectedIndex - 1 < 0)
                    return true;

                Value = Value.Remove(SelectedIndex - 1, 1);
                _selectedIndex -= 1;
            }
            else
            {
                var c = (int)text[0];
                if (c < 32 || c > 126)
                    return true;

                Value = Value.Insert(SelectedIndex, text);
                SelectedIndex += text.Length;
            }

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
