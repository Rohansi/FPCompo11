using System;
using GlitchGame.Debugger.Widgets;
using LoonyVM;
using SFML.Window;
using Window = GlitchGame.Debugger.Widgets.Window;

namespace GlitchGame.Debugger.Windows
{
    public class Cpu : DebugWindow
    {
        private Window _window;
        private Label[] _registers;
        private Label[] _flags;
        private Label _ivt;
        private Label _org;
        private Disassembly _disassembly;
        private Button _pause;
        private Button _step;
        private Checkbox _skipInterrupt;
        private Label _error;
        private bool _autoMove;
        private string _gotoError;

        public Cpu(DebugView view)
            : base(view)
        {
            #region Widget Creation
            _window = new Window(10, 10, 110, 41, "CPU");
            View.Desktop.Add(_window);

            _disassembly = new Disassembly(1, 1, 79, 37);
            _disassembly.Clicked += (addr, button) =>
            {
                if (Target == null)
                    return;

                if (button == Mouse.Button.Left)
                    Target.AddBreakpoint(addr);
                else if (button == Mouse.Button.Right)
                    Target.RemoveBreakpoint(addr);
            };
            _window.Add(_disassembly);

            var y = 1;

            _registers = new Label[RegisterNames.Length];
            for (var i = 0; i < _registers.Length; i++, y++)
            {
                _registers[i] = new Label(82, y, 25, 1, "");
                _window.Add(_registers[i]);
            }

            y++;

            _flags = new Label[FlagNames.Length];
            for (var i = 0; i < _flags.Length; i++, y++)
            {
                _flags[i] = new Label(82, y, 25, 1, "");
                _window.Add(_flags[i]);
            }

            y++;
            _ivt = new Label(82, y++, 25, 1, "");
            _window.Add(_ivt);

            _org = new Label(82, y++, 25, 1, "");
            _window.Add(_org);

            _error = new Label(82, ++y, 25, 4, "");
            _window.Add(_error);

            _skipInterrupt = new Checkbox(82, 29, 25, "Skip Interrupts");
            _skipInterrupt.Changed += () =>
            {
                if (Target != null)
                    Target.SkipInterrupts = _skipInterrupt.Checked;
            };
            _window.Add(_skipInterrupt);

            _step = new Button(82, 31, 25, "Step");
            _step.Clicked += () =>
            {
                if (Target == null || !Target.Paused)
                    return;

                Target.Step = true;
                _autoMove = true;
            };
            _window.Add(_step);

            _pause = new Button(82, 34, 25, "Pause");
            _pause.Clicked += () =>
            {
                if (Target == null)
                    return;

                if (!Target.Paused)
                    _autoMove = true;

                Target.Paused = !Target.Paused;
                Target.Step = true;
            };
            _window.Add(_pause);

            var gotoInput = new TextBox(82, 37, 17);
            _window.Add(gotoInput);

            var gotoButton = new Button(100, 37, 7, "Goto");
            gotoButton.Clicked += () =>
            {
                if (Target == null || gotoInput.Value.Length == 0)
                    return;

                try
                {
                    _gotoError = null;
                    var expr = WatchExpression.Compile(gotoInput.Value);
                    Goto(expr(Target));
                }
                catch (Exception e)
                {
                    _gotoError = string.Format("Goto: {0}", e.Message);
                }
            };
            _window.Add(gotoButton);
            #endregion
        }

        public override void Reset()
        {
            _disassembly.Reset();
            _skipInterrupt.Checked = false;
            _step.Visible = false;
            _skipInterrupt.Visible = false;

            if (Target != null)
                Target.ResetBreakpoints();
        }

        public override void Update()
        {
            if (Target == null || !_window.Visible)
                return;

            for (var i = 0; i < _registers.Length; i++)
            {
                _registers[i].Caption = string.Format("{0} = {1:X8} {1,11}", RegisterNames[i], Target.Vm.Registers[i]);
            }

            for (var i = 0; i < _flags.Length; i++)
            {
                var set = (Target.Vm.Flags & FlagValues[i]) != 0;
                _flags[i].Caption = string.Format("{0} = {1}", FlagNames[i], set);
            }

            _ivt.Caption = string.Format("IVT= {0:X8}", Target.Vm.IVT);
            _org.Caption = string.Format("ORG= {0:X8}", Target.Vm.Origin);

            if (Target.Error != null)
                _error.Caption = Target.Error;
            else if (_gotoError != null)
                _error.Caption = _gotoError;
            else
                _error.Caption = "";

            if (_autoMove)
            {
                _disassembly.Offset = Target.Vm.IP - 48;
                _autoMove = false;
            }

            _disassembly.Update(Target);

            _pause.Caption = Target.Paused ? "Continue" : "Pause";
            _step.Visible = Target.Paused;
            _skipInterrupt.Visible = Target.Paused;

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

        public void Goto(int address)
        {
            _disassembly.Offset = address - 16;
            Show();
        }

        private static readonly string[] RegisterNames =
        {
            "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9",
            "BP", "SP", "IP"
        };

        private static readonly string[] FlagNames =
        {
            "Z ", "E ", "A ", "B "
        };

        private static readonly VmFlags[] FlagValues =
        {
            VmFlags.Zero, VmFlags.Equal, VmFlags.Above, VmFlags.Below
        };
    }
}
