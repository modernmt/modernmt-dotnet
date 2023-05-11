using System;

namespace ModernMT
{
    // ReSharper disable once InconsistentNaming
    public class ModernMTException : Exception
    {
        public readonly int Code;
        public readonly string Type;
        public readonly dynamic Metadata;

        public ModernMTException(int code, string type, string message, dynamic metadata = null) : base(message)
        {
            Code = code;
            Type = type;
            Metadata = metadata;
        }

        public override string ToString()
        {
            return Message + " (" + Code + " - " + Type + ")";
        }

    }
}
