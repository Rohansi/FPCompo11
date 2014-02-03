using System.Collections.Generic;
using System.Reflection;
using GlitchGame.Devices;
using LoonyVM;
using Microsoft.Xna.Framework;

namespace GlitchGame.Entities
{
    public abstract class Computer : Ship
    {
        public readonly VirtualMachine Vm;
        public readonly ShipCode Code;
        public readonly Navigation Navigation;
        public readonly Radar Radar;
        public readonly Engines Engines;
        public readonly Guns Guns;

        private bool _programDead;
        private int _programOffset;
        private int _programLen;
        private List<Variable> _variables;

        public override int Depth { get { return 2; } }

        protected Computer(State state, Vector2 position, float size, int team, string codeFile)
            : base(state, position, size, team)
        {
            Vm = new VirtualMachine(4096);

            Code = Assets.LoadProgram(string.Format("{0}.bin", codeFile));
            for (var i = 0; i < Code.Length; i++)
            {
                Vm.Memory[i] = Code[i];
            }

            _variables = new List<Variable>();
            var ptr = 8;
            var varCount = Vm.Memory.ReadInt(ptr);
            ptr += sizeof(int);

            for (var i = 0; i < varCount; i++)
            {
                var varType = Vm.Memory.ReadSByte(ptr);
                ptr += sizeof(sbyte);

                var varAddr = Vm.Memory.ReadInt(ptr);
                ptr += sizeof(int);

                _variables.Add(new Variable((VariableType)varType, varAddr));
            }

            _programLen = Code.Length;
            _programOffset = ptr;
            _programDead = false;

            Navigation = new Navigation(Body);
            Vm.Attach(Navigation);

            Radar = new Radar(this);
            Vm.Attach(Radar);

            Engines = new Engines();
            Vm.Attach(Engines);

            Guns = new Guns();
            Vm.Attach(Guns);

            Vm.Attach(new Timer());
            Vm.Attach(new Debug(this));
        }

        public override void Update()
        {
            if (_programDead)
                return;

            /*var instr = _vm.GetType().GetField("_instruction", BindingFlags.NonPublic | BindingFlags.Instance);
            var inter = _vm.GetType().GetField("_interrupted", BindingFlags.NonPublic | BindingFlags.Instance);*/

            try
            {
                for (var i = 0; i < Program.InstructionsPerFrame; i++)
                {
                    Vm.Step();

                    /*var instrValue = (Instruction)instr.GetValue(_vm);
                    var interValue = (bool)inter.GetValue(_vm);

                    //if (!interValue)
                        Console.WriteLine(instrValue);*/
                }
            }
            catch (VirtualMachineException)
            {
                _programDead = true;
                return;
            }

            Weapon.Update();

            Shooting = Guns.Shooting;
            Thruster = Engines.Thruster;
            AngularThruster = Engines.AngularThruster;

            base.Update();
        }

        public void Corrupt()
        {
            var r = Program.Random.NextDouble();

            if (r <= 0.60f && _variables.Count > 0) // 60% chance to corrupt variable
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

                Vm.Memory.WriteInt(variable.Address, newValue);
                return;
            }

            if (r <= 0.80f) // 20% chance to corrupt register
            {
                var reg = Program.Random.Next(10);
                var bit = Program.Random.Next(32);

                Vm.Registers[reg] ^= 1 << bit;
            }

            if (r <= 1.00f) // 20% chance to corrupt random memory
            {
                var addr = Program.Random.Next(_programOffset, _programLen - _programOffset);
                var bit = Program.Random.Next(8);

                Vm.Memory[addr] ^= (byte)(1 << bit);
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
