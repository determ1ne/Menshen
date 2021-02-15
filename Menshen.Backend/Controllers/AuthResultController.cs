using System.Linq;
using Menshen.Backend.Services;
using Menshen.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Menshen.Backend.Controllers
{
    [ApiController]
    [Route("authres")]
    public class AuthResultController : MenshenControllerBase
    {
        private readonly ILogger<AuthResultController> _logger;
        private readonly IMetaInfo _metaInfo;
        
        public AuthResultController(ILogger<AuthResultController> logger, ILocker locker, IMetaInfo metaInfo) : base(locker)
        {
            _logger = logger;
            _metaInfo = metaInfo;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var rpSecret = _metaInfo.GetValue(MetaKeys.ReverseProxySecretHeaderName);
            if (!string.IsNullOrWhiteSpace(rpSecret) &&
                string.IsNullOrWhiteSpace(HttpContext.Request.Headers[rpSecret]))
            {
                _logger.LogWarning($"Unauthorized: {HttpContext.Connection.RemoteIpAddress}");
                return BadRequest();
            }

            var ip = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            var host = HttpContext.Request.Host.Host;
            if (_locker.GetIpAllowedForHost(ip, host))
            {
                _logger.LogInformation($"Allowed: {ip} ({host})");
                return Ok();
            }

            _logger.LogInformation($"Blocked: {ip} ({host})");
            return StatusCode(403);
        }
    }
}