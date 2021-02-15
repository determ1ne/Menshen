using System;
using System.Linq;
using Menshen.Backend.Services;
using Menshen.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Menshen.Backend.Controllers
{
    [ApiController]
    [Route("knock")]
    public class KnockController : MenshenControllerBase
    {
        private readonly ILogger<KnockController> _logger;
        
        public KnockController(ILogger<KnockController> logger, ILocker locker) : base(locker)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult SecretKnock(string host, string secret)
        {
            var ip = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip)) return BadRequest();
            
            var blockedDetails = _locker.GetIpBlockDetails(ip);
            if (GetIsIpBlocked(blockedDetails))
            {
                _logger.LogInformation($"StillBlocking: {ip} for {host}");
                return StatusCode(403);
            }

            var secrets = _locker.GetSiteContent(host, 0);
            var secretIdPair = secrets.FirstOrDefault(x => x.Item2 == secret);
            if (secretIdPair.Item1 != 0)
            {
                _logger.LogInformation($"Knock: {ip} for {host}");
                _locker.AllowIpForHost(ip, secretIdPair.Item1, host, TimeSpan.FromDays(730));
                return Ok();
            }
            
            _locker.BlockIp(ip, false);
            _logger.LogInformation($"KnockFailed: {ip} for {host}");
            return StatusCode(403);
        }
    }
}