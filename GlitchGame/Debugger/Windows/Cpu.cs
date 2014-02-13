using GlitchGame.Debugger.Widgets;
using GlitchGame.Gui.Widgets;
using LoonyVM;

namespace GlitchGame.Debugger.Windows
{
    public class Cpu : DebugWindow
    {
        private Window _window;
        private Label[] _registers;
        private Label[] _flags;
        private Disassembly _disassembly;

        public Cpu(DebugView view)
            : base(view)
        {
            #region Widget Creation
            _window = new Window(10, 10, 100, 40, "CPU");

            _disassembly = new Disassembly(1, 1, 69, 36);
            _window.Add(_disassembly);

            _registers = new Label[RegisterNames.Length];
            for (var i = 0; i < _registers.Length; i++)
            {
                _registers[i] = new Label(72, 1 + i, 25, 1, "");
                _window.Add(_registers[i]);
            }

            _flags = new Label[FlagNames.Length];
            for (var i = 0; i < _flags.Length; i++)
            {
                _flags[i] = new Label(72, 2 + _registers.Length + i, 25, 1, "");
                _window.Add(_flags[i]);
            }

            var gotoInput = new TextBox(72, 36, 17);
            _window.Add(gotoInput);

            var skipInterrupt = new Label(72, 28, 25, 1, "[ ]  Skip Interrupts");
            _window.Add(skipInterrupt);

            var pauseButton = new Button(72, 30, 25, "Pause");
            _window.Add(pauseButton);

            var stepButton = new Button(72, 33, 25, "Step");
            _window.Add(stepButton);

            var gotoButton = new Button(90, 36, 7, "Goto");
            _window.Add(gotoButton);

            view.Desktop.Add(_window);
            #endregion
        }

        public override void Reset()
        {
            _disassembly.Reset();
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

            _disassembly.Update(Target);
        }

        public override void Show()
        {
            _window.Visible = true;
            _window.Focus();
        }

        public override void Hide()
        {
            _window.Visible = false;
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
