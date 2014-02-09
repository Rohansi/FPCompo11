using System;
using GlitchGame.Entities;
using LoonyVM;
using SFML.Graphics;

namespace GlitchGame.Devices
{
    public class Debug : IDevice
    {
        public byte Id { get { return 3; } }
        public bool InterruptRequest { get { return false; } }

        private readonly Computer _parent;

        public Debug(Computer parent)
        {
            _parent = parent;
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {

        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            var debugInfo = _parent.Code.DebugInfo;

            if (debugInfo == null)
                return;

            var splitter = new string('-', 40);

            Console.WriteLine();
            Console.WriteLine(" REGISTERS");
            Console.WriteLine(splitter);

            for (var i = 0; i <= 9; i++)
            {
                Console.WriteLine("R{0}={1}", i, machine.Registers[i]);
            }

            Console.WriteLine("BP={0:X8}", machine.Registers[10]);
            Console.WriteLine("SP={0:X8}", machine.SP);

            var ip = machine.IP;
            var line = debugInfo.FindLine(ip);
            if (line.HasValue)
                Console.WriteLine("IP={0:X8} <{1}:{2}>", ip, line.Value.FileName, line.Value.LineNumber);
            else
                Console.WriteLine("IP={0:X8}", ip);

            Console.WriteLine();
            Console.WriteLine(" STACK TRACE");
            Console.WriteLine(splitter);

            var addr = machine.IP;
            var bp = machine.Registers[10];

            while (addr != 0 && bp != 0)
            {
                try
                {
                    var sym = debugInfo.FindSymbol(addr);

                    if (sym.HasValue)
                    {
                        var sline = debugInfo.FindLine(addr);

                        if (sline.HasValue)
                            Console.WriteLine("{0}+{1:X} <{2}:{3}>", sym.Value.Name, addr - sym.Value.Address, sline.Value.FileName, sline.Value.LineNumber);
                        else
                            Console.WriteLine("{0}+{1:X}", sym.Value.Name, addr - sym.Value.Address);
                    }
                    else
                    {
                        Console.WriteLine("{0:X8}", addr);
                    }

                    addr = machine.Memory.ReadInt(bp + 4);
                    bp = machine.Memory.ReadInt(bp);
                }
                catch
                {
                    break;
                }
            }
        }

        public void Draw(RenderTarget target)
        {
            
        }
    }
}
