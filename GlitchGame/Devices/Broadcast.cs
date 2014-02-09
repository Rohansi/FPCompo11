using System.Collections.Generic;
using System.Linq;
using GlitchGame.Entities;
using LoonyVM;
using Microsoft.Xna.Framework;

namespace GlitchGame.Devices
{
    public class Broadcast : IDevice
    {
        public byte Id { get { return 14; } }

        private const float PacketDelay = Program.InstructionsPerSecond / 30f; // instructions per meter
        private const int PacketSize = 32;
        private const int PacketSendCooldown = Program.InstructionsPerSecond;

        private class Packet
        {
            public int Delay;
            public readonly Vector2 SenderPosition;
            public readonly byte[] Data;

            public Packet(float distance, Vector2 senderPosition, byte[] data)
            {
                Delay = (int)(distance * PacketDelay);
                SenderPosition = senderPosition;
                Data = data;
            }
        }

        private Computer _parent;
        private List<Packet> _packetQueue;
        private int _packetPointer;
        private int _sendTimer;

        public Broadcast(Computer parent)
        {
            _parent = parent;
            _packetQueue = new List<Packet>();
            _packetPointer = 0;
            _sendTimer = 0;
        }

        public bool InterruptRequest
        {
            get
            {
                if (_sendTimer > 0)
                    _sendTimer--;

                if (_packetPointer == 0)
                    return false;

                var result = false;

                for (var i = 0; i < _packetQueue.Count; i++)
                {
                    var p = _packetQueue[i];

                    p.Delay--;

                    if (p.Delay <= 0)
                        result = true;
                }

                return result;
            }
        }

        public void HandleInterruptRequest(VirtualMachine machine)
        {
            var packet = _packetQueue.First(p => p.Delay <= 0);
            _packetQueue.Remove(packet);

            machine.Registers[0] = Util.ToMachineRotation(Util.Direction(_parent.Body.Position, packet.SenderPosition));

            for (var i = 0; i < packet.Data.Length; i++)
            {
                machine.Memory[_packetPointer + i] = packet.Data[i];
            }
        }

        public void HandleInterrupt(VirtualMachine machine)
        {
            switch (machine.Registers[0])
            {
                case 0: // get status
                    machine.Registers[0] = _sendTimer == 0 ? 1 : 0;
                    break;
                case 1: // set packet pointer
                    _packetPointer = machine.Registers[1];
                    break;
                case 2: // send packet
                    if (_sendTimer != 0)
                        break;

                    _sendTimer = PacketSendCooldown;

                    var data = new byte[PacketSize];
                    for (var i = 0; i < data.Length; i++)
                    {
                        data[i] = machine.Memory[machine.Registers[1] + i];
                    }

                    foreach (var c in _parent.State.Entities.Iterate().OfType<Computer>())
                    {
                        if (c == _parent)
                            continue;

                        c.Broadcast.Enqueue(_parent, data);
                    }
                    break;
            }
        }

        public void Enqueue(Computer sender, byte[] data)
        {
            if (_packetPointer == 0 || _packetQueue.Count >= 32)
                return;

            var distance = Util.Distance(sender.Body.Position, _parent.Body.Position);
            _packetQueue.Add(new Packet(distance, sender.Body.Position, data));
        }
    }
}
