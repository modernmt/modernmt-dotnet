using System;

namespace ModernMT
{
    public class ModernMTException : Exception
    {
        public readonly int Code;
        public readonly string Type;

        public ModernMTException(int code, string type, string message) : base(message)
        {
            Code = code;
            Type = type;
        }

        public ModernMTException(int code, string type, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
            Type = type;
        }

        public override string ToString()
        {
            return Message + " (" + Code + " - " + Type + ")";
        }

    }
}
