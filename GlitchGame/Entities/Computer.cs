﻿using System;
using System.Collections.Generic;
using GlitchGame.Devices;
using LoonyVM;
using Microsoft.Xna.Framework;

namespace GlitchGame.Entities
{
    public abstract class Computer : Ship
    {
        public VirtualMachine Vm { get; private set; }
        public Navigation Navigation { get; private set; }
        public Radar Radar { get; private set; }
        public Engines Engines { get; private set; }
        public Guns Guns { get; private set; }
        public Broadcast Broadcast { get; private set; }

        public ShipCode Code { get; private set; }

        public readonly List<int> Breakpoints; 
        public bool Paused { get; set; }
        public bool Step { get; set; }
        public bool SkipInterrupts { get; set; }
        public string Error { get; private set; }

        private bool _programDead;
        private int _programOffset;
        private int _programLen;
        private readonly List<Variable> _variables;

        public override int Depth { get { return 2; } }

        protected Computer(State state, Vector2 position, float size, int team, string codeFile)
            : base(state, position, size, team)
        {
            Breakpoints = new List<int>();

            _variables = new List<Variable>();

            var code = Assets.LoadCode(string.Format("{0}.bin", codeFile));
            Load(code);
        }

        public override void Update(float dt)
        {
            if (!_programDead && (!Paused || Step))
            {
                try
                {
                    var instructions = (int)Math.Ceiling(Program.InstructionsPerSecond * dt);

                    if (Step)
                        instructions++;

                    for (var i = 0; i < instructions; i++)
                    {
                        if (!Step && HasBreakpoint(Vm.IP))
                        {
                            Paused = true;
                            break;
                        }

                        Vm.Step();

                        if (SkipInterrupts && Vm.Interrupted)
                            continue;

                        if (Step)
                        {
                            Step = false;
                            break;
                        }
                    }
                }
                catch (VirtualMachineException e)
                {
                    _programDead = true;
                    Error = e.Message.Trim(' ', '\t', '\n', '\r');
                    return;
                }
            }

            Weapon.Update(dt);

            Shooting = Guns.Shooting;
            Thruster = Engines.Thruster;
            AngularThruster = Engines.AngularThruster;

            base.Update(dt);
        }

        public void Load(ShipCode code)
        {
            Vm = new VirtualMachine(4096);

            Vm.Attach(new Timer());

            Navigation = new Navigation(Body);
            Vm.Attach(Navigation);

            Radar = new Radar(this);
            Vm.Attach(Radar);

            Engines = new Engines();
            Vm.Attach(Engines);

            Guns = new Guns();
            Vm.Attach(Guns);

            Broadcast = new Broadcast(this);
            Vm.Attach(Broadcast);

            Code = code;

            for (var i = 0; i < Code.Length; i++)
            {
                Vm.Memory[i] = Code[i];
            }

            var ptr = 8;
            var varCount = Vm.Memory.ReadInt(ptr);
            ptr += sizeof(int);

            _variables.Clear();

            for (var i = 0; i < varCount; i++)
            {
                var varAddr = ptr;
                ptr += sizeof(int);

                var varType = Vm.Memory.ReadSByte(ptr);
                ptr += sizeof(sbyte);

                _variables.Add(new Variable((VariableType)varType, varAddr));
            }

            _programLen = Code.Length;
            _programOffset = ptr;
            _programDead = false;

            for (var i = 0; i < Vm.Registers.Length; i++)
            {
                Vm.Registers[i] = 0;
            }

            Vm.SP = Vm.Memory.Length;
        }

        #region Breakpoint Methods
        public void ResetBreakpoints()
        {
            Breakpoints.Clear();
            Paused = false;
            Step = false;
            SkipInterrupts = false;
        }

        public void AddBreakpoint(int address)
        {
            var i = Breakpoints.BinarySearch(address);
            if (i >= 0)
                return;
            Breakpoints.Insert(~i, address);
        }

        public void RemoveBreakpoint(int address)
        {
            var i = Breakpoints.BinarySearch(address);
            if (i < 0)
                return;
            Breakpoints.RemoveAt(i);
        }

        public bool HasBreakpoint(int address)
        {
            return Breakpoints.BinarySearch(address) >= 0;
        }
        #endregion

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
