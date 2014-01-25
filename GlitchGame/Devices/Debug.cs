using System;
using LoonyVM;

namespace GlitchGame.Devices
{
    public class Debug : IDevice
    {
        public byte Id { get { return 14; } }
        public bool InterruptRequest { get { return false; } }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            Console.WriteLine("DEBUG r1={0}", machine.Registers[1]);
        }
    }
}
