using System;
using GlitchGame.Entities;
using LoonyVM;
using SFML.Graphics;

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

        public Radar(Computer parent)
        {
            _radarPointer = 0;
            _radarData = new short[Program.RadarRays];
            _timer = Program.Random.Next(UpdateEvery); // helps with stutter with lots of radars

            _parent = parent;
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

                _radarData[i] = (short)(distance << 8 | type);
            }
        }

        public void Draw(RenderTarget target)
        {
            const float step = Util.Pi2 / Program.RadarRays;
            var vertices = new VertexArray(PrimitiveType.Lines, Program.RadarRays * 2);
            var center = _parent.Position;
            float angle = 0;

            for (uint i = 0; i < vertices.VertexCount; i += 2)
            {
                var dist = (_radarData[i / 2] >> 8) / 126f;
                var type = (RadarValue)(_radarData[i / 2] & 0xFF);

                Color color = Color.White;

                switch (type)
                {
                    case RadarValue.Ally:
                        color = Color.Green;
                        break;
                    case RadarValue.Enemy:
                        color = Color.Red;
                        break;
                    case RadarValue.Asteroid:
                        color = Color.Yellow;
                        break;
                }

                if (dist <= 1)
                {
                    vertices[i + 0] = new Vertex(center, color);
                    vertices[i + 1] = new Vertex(center + Util.RadarLengthDir(angle, dist * MaxDistance * Program.PixelsPerMeter).ToSfml(), color);
                }

                angle += step;
            }

            target.Draw(vertices);
        }
    }
}
