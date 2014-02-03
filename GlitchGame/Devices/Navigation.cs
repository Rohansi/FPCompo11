using System;
using FarseerPhysics.Dynamics;
using LoonyVM;

namespace GlitchGame.Devices
{
    public class Navigation : IDevice
    {
        private Body _body;

        public Navigation(Body body)
        {
            _body = body;
        }

        public byte Id { get { return 10; } }
        public bool InterruptRequest { get { return false; } }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            switch (machine.Registers[7])
            {
                case 0: // get speed
                    machine.Registers[7] = (int)(_body.LinearVelocity.X * Program.PixelsPerMeter);
                    machine.Registers[8] = (int)(_body.LinearVelocity.Y * Program.PixelsPerMeter);
                    break;
                case 1: // get angular speed
                    machine.Registers[7] = (int)(_body.AngularVelocity % Program.RadarRays);
                    break;
                case 2: // get heading
                    machine.Registers[7] = Util.ToMachineRotation(_body.Rotation - ((float)Math.PI / 2));
                    break;
            }
        }
    }
}
