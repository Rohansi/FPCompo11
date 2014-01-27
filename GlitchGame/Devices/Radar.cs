using System;
using FarseerPhysics.Dynamics;
using LoonyVM;

namespace GlitchGame.Devices
{
    public enum RadarValue
    {
        Asteroid,
        Player,
        Bullet,
        Computer,

        Count
    }

    public class Radar : IDevice
    {
        private const float MaxDistance = 30;
        private const int UpdateEvery = Program.InstructionsPerSecond / 10;

        private Body _body;
        private int _radarPointer;
        private short[] _radarData;
        private int _timer;

        public byte Id { get { return 11; } }

        public bool InterruptRequest
        {
            get
            {
                if (_radarPointer == 0)
                    return false;

                _timer++;
                return _timer >= UpdateEvery;
            }
        }

        public Radar(Body body)
        {
            _radarPointer = 0;
            _radarData = new short[Program.RadarRays];
            _timer = UpdateEvery;

            _body = body;
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            _timer -= UpdateEvery;

            RayCast();

            for (var i = 0; i < _radarData.Length; i++)
            {
                machine.Memory.WriteShort(_radarPointer + (i * 2), _radarData[i]);
            }
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            _radarPointer = machine.Registers[7];
        }

        private void RayCast()
        {
            const float step = (float)(2 * Math.PI) / Program.RadarRays;

            var start = _body.Position;
            var i = 0;
            for (var dir = 0f; dir <= 2 * Math.PI; dir += step, i++)
            {
                byte type = 127;
                byte distance = 127;

                float min = 100;
                var point = start + Util.RadarLengthDir(dir, MaxDistance);

                Program.World.RayCast((f, p, n, fr) =>
                {
                    if (f.Body == _body)
                        return -1;

                    if (fr > min)
                        return 1;

                    min = fr;

                    var entity = (IEntity)f.Body.UserData;
                    type = (byte)entity.Radar;
                    distance = (byte)(fr * 126);
                    return fr;
                }, start, point);

                _radarData[i] = (short)(distance << 8 | type);
            }
        }
    }
}
