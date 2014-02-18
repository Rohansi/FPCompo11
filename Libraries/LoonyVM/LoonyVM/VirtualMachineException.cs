using System;

namespace LoonyVM
{
    public class VirtualMachineException : Exception
    {
        public VirtualMachineException(int ip, string message)
            : base(string.Format("{0:X8}: {1}", ip, message))
        {
            
        }

        public VirtualMachineException(int ip, string message, Exception innerException)
            : base(string.Format("{0:X8}: {1}", ip, message), innerException)
        {

        }
    }

    internal class VirtualMachineInvalidOpcode : Exception
    {
        public VirtualMachineInvalidOpcode(string message)
            : base(message)
        {

        }

        public VirtualMachineInvalidOpcode(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
