using LoonyVM;

namespace GlitchGame.Devices
{
    public class Guns : IDevice
    {
        public byte Id { get { return 13; } }
        public bool InterruptRequest { get { return false; } }

        public bool Shooting { get; private set; }

        public void HandleInterrupt(VirtualMachine machine)
        {
            Shooting = machine.Registers[7] != 0;
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {

        }

        /*public bool Update()
        {
            _timer = Math.Max(_timer - Program.FrameTime, 0);

            if (_shooting && _timer <= 0)
            {
                _timer = Cooldown;
                return true;
            }

            return false;
        }*/
    }
}
