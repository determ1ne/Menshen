using System;
using System.Linq;
using Menshen.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Menshen.Backend.Utils
{
    public class MenshenControllerBase : ControllerBase
    {
        protected readonly ILocker _locker;

        protected MenshenControllerBase(ILocker locker)
        {
            _locker = locker;
        }

        protected string GetUserIp()
        {
            return HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault(); 
        }
        
        protected bool GetIsIpBlocked((int, long)? blockDetails)
        {
            return blockDetails?.Item1 > 3 &&
                   blockDetails?.Item2 > DateTimeOffset.UtcNow
                       .AddMinutes(-(int) Math.Pow(2, (double) blockDetails?.Item1)).ToUnixTimeSeconds();
        }
    }
}