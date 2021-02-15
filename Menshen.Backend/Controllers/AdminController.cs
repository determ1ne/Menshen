using Menshen.Backend.Services;
using Menshen.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Menshen.Backend.Controllers
{
    [ApiController]
    [Route("admin")]
    public partial class AdminController : MenshenControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IMetaInfo _metaInfo;
        
        public AdminController(ILogger<AdminController> logger, IMetaInfo metaInfo, ILocker locker) : base(locker)
        {
            _logger = logger;
            _metaInfo = metaInfo;
        }

        [HttpPost("newConfig")]
        public IActionResult NewSiteConfig([FromBody] NewSiteConfigDto data)
        {
            if (BCrypt.Net.BCrypt.EnhancedVerify(data.secret, _metaInfo.GetValue(MetaKeys.AdminPassword)))
            {
                _locker.AddSiteConfig(data.host, data.type, data.content);
                return Ok();
            }

            return StatusCode(403);
        }
    }
}