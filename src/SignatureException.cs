using JWT.Exceptions;

namespace ModernMT
{
    public class SignatureException : ModernMTException
    {
        public SignatureException(SignatureVerificationException e)
            : base(0, "SignatureException", e.Message)
        {
            
        }
    }
}