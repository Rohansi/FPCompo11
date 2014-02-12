using System;
using System.Text;

namespace LoonyVM
{
    public class Operand
    {
        private VirtualMachine _machine;
        public int Type { get; private set; }
        public int ValueType { get; private set; }
        public bool IsPointer { get; private set; }
        public bool IsOffset { get; private set; }
        public int OffsetRegister { get; private set; }
        public int Payload { get; private set; }

        public Operand(VirtualMachine machine)
        {
            _machine = machine;
        }
    
        public void Change(int type, int valueType, bool pointer, bool offset, int offsetRegister, int payload)
        {
            Type = type;
            ValueType = valueType;
            IsPointer = pointer;
            IsOffset = offset;
            OffsetRegister = offsetRegister;
            Payload = payload;
        }

        public bool IsValid
        {
            get
            {
                if (ValueType < 0x0 || ValueType > 0x2)
                    return false;
                return true;
            }
        }

        public int Get(bool resolvePointer = true, bool disableType = false)
        {
            int value = 0;
            if (Type <= 0xC)
                value = _machine.Registers[Type];
            if (Type >= 0xD && Type <= 0xF)
                value = Payload;

            if (IsOffset)
                value += _machine.Registers[OffsetRegister];

            if (IsPointer && resolvePointer)
                value = _machine.ReadInt(value);

            if (disableType)
                return value;

            switch (ValueType)
            {
                case 0x0:
                    value = (sbyte)value;
                    break;
                case 0x1:
                    value = (short)value;
                    break;
                case 0x2:
                    break;
                default:
                    throw new VirtualMachineInvalidOpcode("Invalid operand value type");
            }

            return value;
        }

        public void Set(int value)
        {
            if (!IsPointer)
            {
                if (Type <= 0xC)
                    _machine.Registers[Type] = PreserveUpper(value, _machine.Registers[Type], ValueType);
                return;
            }

            var val = PreserveUpper(value, Get(disableType: true), ValueType);
            var addr = Get(false, true);
            _machine.WriteInt(addr, val);
        }

        public static int PreserveUpper(int newValue, int originalValue, int type)
        {
            switch (type)
            {
                case 0x0:
                    return (int)(originalValue & 0xFFFFFF00) | ((sbyte)newValue) & 0xFF;
                case 0x1:
                    return (int)(originalValue & 0xFFFF0000) | ((short)newValue) & 0xFFFF;
                case 0x2:
                    return newValue;
                default:
                    throw new VirtualMachineInvalidOpcode("Invalid operand value type");
            }
        }

        private static readonly string[] RegisterNames =
        {
            "r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7", "r8", "r9",
            "bp", "sp", "ip"
        };

        public override string ToString()
        {
            var sb = new StringBuilder();

            switch (ValueType)
            {
                case 0x0:
                    sb.Append("byte ");
                    break;
                case 0x1:
                    sb.Append("word ");
                    break;
                case 0x2:
                    break;
                default:
                    throw new Exception("Invalid operand value type");
            }

            if (IsPointer)
                sb.Append('[');

            if (IsOffset)
            {
                if (OffsetRegister <= 0xC)
                    sb.Append(RegisterNames[OffsetRegister]);
                sb.Append(" + ");
            }

            if (Type <= 0xC)
                sb.Append(RegisterNames[Type]);
            if (Type >= 0xD && Type <= 0xF)
                sb.AppendFormat("0x{0:X}", Payload);

            if (IsPointer)
                sb.Append(']');

            return sb.ToString();
        }
    }
}
