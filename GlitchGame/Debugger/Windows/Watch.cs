using System;
using GlitchGame.Debugger.Widgets;
using GlitchGame.Entities;
using SFML.Window;
using Window = GlitchGame.Debugger.Widgets.Window;

namespace GlitchGame.Debugger.Windows
{
    public class Watch : DebugWindow
    {
        private class WatchItem
        {
            public string Name;
            public string Expression;
            public Func<Computer, int> Function;

            public string Error = null;
        }

        private Window _window;
        private ListView<WatchItem> _watchView;
        private TextBox _name;
        private TextBox _expression;
        private Button _button;
        private WatchItem _editing;

        public Watch(DebugView view)
            : base(view)
        {
            _window = new Window(30, 30, 80, 25, "Watch");
            _window.Visible = false;
            view.Desktop.Add(_window);

            _watchView = new ListView<WatchItem>(1, 1, 76, 19);
            _watchView.Columns.Add(new ListView<WatchItem>.Column("Name", 14, w => w.Name));
            _watchView.Columns.Add(new ListView<WatchItem>.Column("Expression", 40, w => w.Expression, false));
            _watchView.Columns.Add(new ListView<WatchItem>.Column("Value", 20, null, false, w =>
            {
                if (Target == null)
                    return "No Target";

                if (w.Error != null)
                    return w.Error;

                try
                {
                    var value = w.Function(Target);
                    return string.Format("{0:X8} {0,11}", value);
                }
                catch (WatchException e)
                {
                    return e.Message;
                }
                catch
                {
                    return "Error";
                }
            }));

            _watchView.Clicked += (item, button) =>
            {
                if (button == Mouse.Button.Right)
                {
                    _watchView.Items.Remove(item);
                }
                else if (button == Mouse.Button.Middle)
                {
                    _editing = item;
                    _name.Value = _editing.Name;
                    _expression.Value = _editing.Expression;
                }
            };

            _window.Add(_watchView);

            _name = new TextBox(1, 21, 13);
            _window.Add(_name);

            _expression = new TextBox(15, 21, 50);
            _window.Add(_expression);

            _button = new Button(66, 21, 11, "Add");
            _button.Clicked += () =>
            {
                if (_expression.Value.Length == 0)
                    return;

                WatchItem item;

                if (_editing == null)
                {
                    item = new WatchItem();
                    _watchView.Items.Add(item);
                }
                else
                {
                    item = _editing;
                }

                item.Name = _name.Value;
                item.Expression = _expression.Value;

                try
                {
                    item.Function = WatchExpression.Compile(item.Expression);
                }
                catch (WatchException e)
                {
                    item.Error = e.Message;
                }

                _name.Value = "";
                _expression.Value = "";
                _editing = null;
            };
            _window.Add(_button);
        }

        public override void Reset()
        {
            _watchView.Items.Clear();
        }

        public override void Update()
        {
            if (!_window.Visible)
                return;

            if (_editing == null)
                _button.Caption = "Add";
            else
                _button.Caption = "Update";
        }

        public void Show()
        {
            _window.Visible = true;
            _window.Focus();
        }

        public void Hide()
        {
            _window.Visible = false;
        }
    }
}
