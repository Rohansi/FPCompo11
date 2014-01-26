using System;
using System.IO;
using System.Reflection;
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
        private Engines _engines;
        private Guns _guns;
        private bool _vmDead;

        public override int DrawOrder { get { return 2; } }
        public override byte RadarType { get { return 3; } }

        public Enemy(Vector2 position)
            : base(position, "ship.png", 1)
        {
            Sprite.Color = new Color(255, 180, 200);

            _vm = new VirtualMachine(2048);
            _vm.Attach(new Navigation(Body));
            _vm.Attach(new Radar(Body));

#if DEBUG
            _vm.Attach(new Debug());
#endif

            _engines = new Engines();
            _vm.Attach(_engines);

            _guns = new Guns();
            _vm.Attach(_guns);

            var code = File.ReadAllBytes("Data/bios.bin");
            for (var i = 0; i < code.Length; i++)
            {
                _vm.Memory[i] = code[i];
            }

            _vmDead = false;

            Weapon = new LaserGun(this);
        }

        public override void Update()
        {
            var instr = _vm.GetType().GetField("_instruction", BindingFlags.NonPublic | BindingFlags.Instance);
            var inter = _vm.GetType().GetField("_interrupted", BindingFlags.NonPublic | BindingFlags.Instance);

            if (!_vmDead)
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
                    Console.WriteLine(e);
                    _vmDead = true;
                }
            }

            Weapon.Update();

            Shooting = _guns.Shooting;
            Thruster = _engines.Thruster;
            AngularThruster = _engines.AngularThruster;

            base.Update();
        }
    }
}
