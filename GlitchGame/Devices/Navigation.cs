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
            switch (machine.Registers[0])
            {
                case 0: // get coordinates
                    machine.Registers[0] = (int)(_body.Position.X * Program.PixelsPerMeter);
                    machine.Registers[1] = (int)(_body.Position.Y * Program.PixelsPerMeter);
                    break;
                case 1: // get speed
                    machine.Registers[0] = (int)(_body.LinearVelocity.X * Program.PixelsPerMeter);
                    machine.Registers[1] = (int)(_body.LinearVelocity.Y * Program.PixelsPerMeter);
                    break;
                case 2: // get angular speed
                    machine.Registers[0] = (int)(_body.AngularVelocity * ((Program.RadarRays / 2) / Math.PI));
                    break;
                case 3: // get heading
                    machine.Registers[0] = Util.ToMachineRotation(_body.Rotation - ((float)Math.PI / 2));
                    break;
            }
        }
    }
}
