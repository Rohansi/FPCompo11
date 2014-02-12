﻿using System.Collections.Generic;
using System.Text;

namespace LoonyVM
{
    public enum Opcode
    {
        Mov, Add, Sub, Mul, Div, Rem, Inc, Dec, Not, And, Or,
        Xor, Shl, Shr, Push, Pop, Jmp, Call, Ret, Cmp, Jz, Jnz,
        Je, Jne, Ja, Jae, Jb, Jbe, Rand, Int, Iret, Ivt, Abs, 
        Retn, Xchg, Cmpxchg, Pusha, Popa, Sti, Cli, Neg, Sorg,
        None
    }

    public class Instruction
    {
        public Opcode Opcode { get; private set; }
        public Operand Left { get; private set; }
        public Operand Right { get; private set; }
        public int Length { get; private set; }

        private VirtualMachine _machine;

        public Instruction(VirtualMachine machine)
        {
            _machine = machine;

            Left = new Operand(machine);
            Right = new Operand(machine);
        }

        public void Decode()
        {
            var originalIp = _machine.IP;

            Opcode = (Opcode)_machine.Memory[_machine.IP++];

            var b1 = _machine.Memory[_machine.IP++];
            var b2 = _machine.Memory[_machine.IP++];
            var b3 = _machine.Memory[_machine.IP++];

            var left = b1 >> 4;
            var leftPtr = (b3 & 0x80) != 0;
            var leftOffset = (b3 & 0x40) != 0;
            var leftOffsetReg = b2 >> 4;
            var leftType = (b3 >> 4) & 0x03;
            var leftPayload = ReadPayload(left);

            var right = b1 & 0x0F;
            var rightPtr = (b3 & 0x08) != 0;
            var rightOffset = (b3 & 0x04) != 0;
            var rightOffsetReg = b2 & 0x0F;
            var rightType = b3 & 0x03;
            var rightPayload = ReadPayload(right);

            Left.Change(left, leftType, leftPtr, leftOffset, leftOffsetReg, leftPayload);
            Right.Change(right, rightType, rightPtr, rightOffset, rightOffsetReg, rightPayload);

            Length = _machine.IP - originalIp;
            _machine.IP = originalIp;
        }

        public bool IsValid
        {
            get
            {
                int operands;
                if (!OperandCounts.TryGetValue(Opcode, out operands))
                    return false;
                if (operands >= 1 && !Left.IsValid)
                    return false;
                if (operands >= 2 && !Right.IsValid)
                    return false;
                return true;
            }
        }

        private int ReadPayload(int operandType)
        {
            var payload = 0;

            switch (operandType)
            {
                case 0xD:
                    payload = _machine.ReadSByte(_machine.IP);
                    _machine.IP += sizeof(sbyte);
                    break;
                case 0xE:
                    payload = _machine.ReadShort(_machine.IP);
                    _machine.IP += sizeof(short);
                    break;
                case 0xF:
                    payload = _machine.ReadInt(_machine.IP);
                    _machine.IP += sizeof(int);
                    break;
            }

            return payload;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var operands = OperandCounts[Opcode];

            sb.AppendFormat("{0,-8}", Opcode.ToString().ToLower());

            if (operands >= 1)
                sb.AppendFormat(" {0}", Left);

            if (operands >= 2)
                sb.AppendFormat(", {0}", Right);

            return sb.ToString();
        }

        public static readonly Dictionary<Opcode, int> OperandCounts = new Dictionary<Opcode, int>
        {
            { Opcode.Mov,     2 },
            { Opcode.Add,     2 },
            { Opcode.Sub,     2 },
            { Opcode.Mul,     2 },
            { Opcode.Div,     2 },
            { Opcode.Rem,     2 },
            { Opcode.Inc,     1 },
            { Opcode.Dec,     1 },
            { Opcode.Not,     1 },
            { Opcode.And,     2 },
            { Opcode.Or,      2 },
            { Opcode.Xor,     2 },
            { Opcode.Shl,     2 },
            { Opcode.Shr,     2 },
            { Opcode.Push,    1 },
            { Opcode.Pop,     1 },
            { Opcode.Jmp,     1 },
            { Opcode.Call,    1 },
            { Opcode.Ret,     0 },
            { Opcode.Cmp,     2 },
            { Opcode.Jz,      1 },
            { Opcode.Jnz,     1 },
            { Opcode.Je,      1 },
            { Opcode.Jne,     1 },
            { Opcode.Ja,      1 },
            { Opcode.Jae,     1 },
            { Opcode.Jb,      1 },
            { Opcode.Jbe,     1 },
            { Opcode.Rand,    1 },
            { Opcode.Int,     1 },
            { Opcode.Iret,    0 },
            { Opcode.Ivt,     1 },
            { Opcode.Abs,     1 },
            { Opcode.Retn,    1 },
            { Opcode.Xchg,    2 },
            { Opcode.Cmpxchg, 2 },
            { Opcode.Pusha,   0 },
            { Opcode.Popa,    0 }, 
            { Opcode.Sti,     0 },
            { Opcode.Cli,     0 },
            { Opcode.Neg,     1 },
            { Opcode.Sorg,    1 }
        };
    }
}