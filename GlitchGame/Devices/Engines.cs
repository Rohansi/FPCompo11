using LoonyVM;

namespace GlitchGame.Devices
{
    public class Engines : IDevice
    {
        public byte Id { get { return 12; } }
        public bool InterruptRequest { get { return false; } }

        public float Thruster { get; private set; }
        public float AngularThruster { get; private set; }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            switch (machine.Registers[7])
            {
                case 0: // set thruster speed
                    Thruster = Util.Clamp(machine.Registers[8] / 100f, -1, 1) * -1;
                    break;

                case 1: // set angular thrusters
                    AngularThruster = Util.Clamp(machine.Registers[8] / 100f, -1, 1);
                    break;
            }
        }
    }
}
