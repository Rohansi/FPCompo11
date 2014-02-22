using System;
using GlitchGame.Entities;
using GlitchGame.Entities.Projectiles;
using LoonyVM;

namespace GlitchGame.Devices
{
    public enum RadarValue : sbyte
    {
        Asteroid,
        Bullet,
        Ally,
        Enemy,

        Count,
        Invalid = 127
    }

    public struct RadarRay
    {
        public readonly RadarValue Type;
        public readonly short Distance;

        public RadarRay(RadarValue type, short distance)
        {
            Type = type;
            Distance = distance;
        }
    }

    public class Radar : IDevice
    {
        public byte Id { get { return 11; } }

        public const float MaxDistanceM = 20;
        public const float MaxDistanceP = MaxDistanceM * Program.PixelsPerMeter;
        private const int UpdateEvery = Program.InstructionsPerSecond / 10;

        private readonly Computer _parent;
        private int _radarPointer;
        private int _timer;

        public RadarRay[] RadarData;

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
            RadarData = new RadarRay[Program.RadarRays];
            _timer = Program.Random.Next(UpdateEvery); // helps with stutter with lots of radars

            _parent = parent;
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            _timer -= UpdateEvery;

            RayCast();

            for (var i = 0; i < RadarData.Length; i++)
            {
                machine.Memory.WriteSByte(_radarPointer + (i * 3) + 0, (sbyte)RadarData[i].Type);
                machine.Memory.WriteShort(_radarPointer + (i * 3) + 1, RadarData[i].Distance);
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
                var type = RadarValue.Invalid;
                var dist = short.MaxValue;

                float min = 100;
                var point = start + Util.RadarLengthDir(dir, MaxDistanceM);

                _parent.State.World.RayCast((f, p, n, fr) =>
                {
                    if (fr > min)
                        return 1;

                    if (f.Body == _parent.Body)
                        return 1;

                    var entity = (Entity)f.Body.UserData;
                    if (entity is Bullet)
                        return 1;

                    min = fr;

                    var ship = entity as Ship;

                    if (ship != null)
                    {
                        type = ship.Team == _parent.Team ? RadarValue.Ally : RadarValue.Enemy;
                    }
                    else
                    {
                        type = entity.RadarType;
                    }
                    
                    dist = (short)(fr * MaxDistanceP);
                    return fr;
                }, start, point);

                RadarData[i] = new RadarRay(type, dist);
            }
        }
    }
}
