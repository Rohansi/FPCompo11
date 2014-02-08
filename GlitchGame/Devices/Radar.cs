using System;
using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using LoonyVM;

namespace GlitchGame.Devices
{
    public enum RadarValue
    {
        Asteroid,
        Bullet,
        Ally,
        Enemy,

        Count
    }

    public class Radar : IDevice
    {
        private const float MaxDistance = 20;
        private const int UpdateEvery = Program.InstructionsPerSecond / 10;

        private readonly Computer _parent;
        private int _radarPointer;
        private int _timer;

        public short[] RadarData;

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

        public Radar(Computer parent)
        {
            _radarPointer = 0;
            RadarData = new short[Program.RadarRays];
            _timer = Program.Random.Next(UpdateEvery); // helps with stutter with lots of radars

            _parent = parent;
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            _timer -= UpdateEvery;

            RayCast();

            for (var i = 0; i < RadarData.Length; i++)
            {
                machine.Memory.WriteShort(_radarPointer + (i * 2), RadarData[i]);
            }
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            _radarPointer = machine.Registers[0];
        }

        private void RayCast()
        {
            const float step = (float)(2 * Math.PI) / Program.RadarRays;

            var start = _parent.Body.Position;
            var i = 0;
            for (var dir = 0f; dir <= 2 * Math.PI; dir += step, i++)
            {
                byte type = 127;
                byte distance = 127;

                float min = 100;
                var point = start + Util.RadarLengthDir(dir, MaxDistance);

                _parent.State.World.RayCast((f, p, n, fr) =>
                {
                    if (f.Body == _parent.Body)
                        return -1;

                    if (fr > min)
                        return 1;

                    min = fr;

                    var entity = (Entity)f.Body.UserData;
                    if (entity is Bullet)
                        return 1;

                    var ship = entity as Ship;

                    if (ship != null)
                    {
                        type = (byte)(ship.Team == _parent.Team ? RadarValue.Ally : RadarValue.Enemy);
                    }
                    else
                    {
                        type = (byte)entity.RadarType;
                    }
                    
                    distance = (byte)(fr * 126);
                    return fr;
                }, start, point);

                RadarData[i] = (short)(distance << 8 | type);
            }
        }
    }
}
