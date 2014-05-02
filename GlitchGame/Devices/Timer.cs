using LoonyVM;

namespace GlitchGame.Devices
{
    public class Timer : IDevice
    {
        public byte Id { get { return 1; } }

        private bool _enabled;
        private int _updateEvery;
        private int _timer;

        public bool InterruptRequest
        {
            get
            {
                if (!_enabled)
                    return false;

                _timer++;
                return _timer >= _updateEvery;
            }
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            _timer -= _updateEvery;
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            switch (machine.Registers[0])
            {
                case 0: // enable
                    _enabled = machine.Registers[1] != 0;
                    break;
                case 1: // set frequency
                    _updateEvery = Program.InstructionsPerSecond / Util.Clamp(machine.Registers[1], 1, 1000);
                    break;
            }
        }
    }
}
