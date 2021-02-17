using System;
using System.Text;
using OtpNet;

namespace Menshen.Backend.Services
{
    public class _2FA
    {
        public static string Get2FaOtp(string secretKey, DateTime timeStamp)
        {
            var totp = new Totp(Encoding.UTF8.GetBytes(secretKey));
            return totp.ComputeTotp(timeStamp);
        }
    }
}