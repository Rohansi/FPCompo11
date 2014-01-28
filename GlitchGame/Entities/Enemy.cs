using System.Collections.Generic;
using System.IO;
using GlitchGame.Devices;
using GlitchGame.Weapons;
using LoonyVM;
using Microsoft.Xna.Framework;
using SFML.Graphics;

namespace GlitchGame.Entities
{
    public sealed class Enemy : Ship
    {
        private VirtualMachine _vm;
        private bool _programDead;
        private int _programOffset;
        private int _programLen;
        private List<Variable> _variables;
        private Engines _engines;
        private Guns _guns;

        public override int Depth { get { return 2; } }
        public override RadarValue Radar { get { return RadarValue.Computer; } }

        public Enemy(Vector2 position)
            : base(position, "ship.png", 1)
        {
            Sprite.Color = new Color(255, 180, 200);

            #region VM Setup
            _vm = new VirtualMachine(4096);

            var code = File.ReadAllBytes("Data/bios.bin");
            for (var i = 0; i < code.Length; i++)
            {
                _vm.Memory[i] = code[i];
            }

            _variables = new List<Variable>();
            var ptr = 8;
            var varCount = _vm.Memory.ReadInt(ptr);
            ptr += sizeof(int);

            for (var i = 0; i < varCount; i++)
            {
                var varType = _vm.Memory.ReadSByte(ptr);
                ptr += sizeof(sbyte);

                var varAddr = _vm.Memory.ReadInt(ptr);
                ptr += sizeof(int);

                _variables.Add(new Variable((VariableType)varType, varAddr));
            }

            _programLen = code.Length;
            _programOffset = ptr;
            _programDead = false;

            _vm.Attach(new Navigation(Body));
            _vm.Attach(new Radar(Body));

#if DEBUG
            _vm.Attach(new Debug());
#endif

            _engines = new Engines();
            _vm.Attach(_engines);

            _guns = new Guns();
            _vm.Attach(_guns);

            #endregion

            var r = Program.Random.NextDouble();

            if (r <= 0.75f)
                Weapon = new LaserGun(this);
            else
                Weapon = new NerfGun(this);

            MaxHealth = 1000;
            Health = MaxHealth;
            RegenRate = 5;
            DamageTakenMultiplier = 1;
            DamageMultiplier = 1;
            SpeedMultiplier = 1;
        }

        public override void Update()
        {
            /*var instr = _vm.GetType().GetField("_instruction", BindingFlags.NonPublic | BindingFlags.Instance);
            var inter = _vm.GetType().GetField("_interrupted", BindingFlags.NonPublic | BindingFlags.Instance);*/

            if (!_programDead)
            {
                try
                {
                    for (var i = 0; i < Program.InstructionsPerFrame; i++)
                    {
                        _vm.Step();

                        /*var instrValue = (Instruction)instr.GetValue(_vm);
                        var interValue = (bool)inter.GetValue(_vm);

                        //if (!interValue)
                            Console.WriteLine(instrValue);*/
                    }
                }
                catch (VirtualMachineException e)
                {
                    _programDead = true;
                }
            }

            Weapon.Update();

            Shooting = _guns.Shooting;
            Thruster = _engines.Thruster;
            AngularThruster = _engines.AngularThruster;

            base.Update();
        }

        public void Corrupt()
        {
            var r = Program.Random.NextDouble();

            if (r <= 0.40f && _variables.Count > 0) // 40% chance to corrupt variable
            {
                var variable = _variables[Program.Random.Next(_variables.Count)];
                int newValue;

                switch (variable.Type)
                {
                    case VariableType.General:
                        newValue = Program.Random.Next(int.MinValue, int.MaxValue);
                        break;
                    case VariableType.RadarValue:
                        newValue = Program.Random.Next((int)RadarValue.Count);
                        break;
                    case VariableType.Speed:
                        newValue = Program.Random.Next(-100, 100);
                        break;
                    default:
                        return;
                }

                _vm.Memory.WriteInt(variable.Address, newValue);
                return;
            }

            if (r <= 0.80f) // 40% chance to corrupt register
            {
                var reg = Program.Random.Next(10);
                var bit = Program.Random.Next(32);

                _vm.Registers[reg] ^= 1 << bit;
            }

            if (r <= 1.00f) // 20% chance to corrupt random memory
            {
                var addr = Program.Random.Next(_programOffset, _programLen - _programOffset);
                var bit = Program.Random.Next(8);

                _vm.Memory[addr] ^= (byte)(1 << bit);
                return;
            }
        }

        private enum VariableType
        {
            General,
            RadarValue,
            Speed
        }

        private struct Variable
        {
            public readonly VariableType Type;
            public readonly int Address;

            public Variable(VariableType type, int address)
            {
                Type = type;
                Address = address;
            }
        }
    }
}
