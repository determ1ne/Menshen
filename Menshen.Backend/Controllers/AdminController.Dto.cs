namespace Menshen.Backend.Controllers
{
    public partial class AdminController
    {
        public record NewSiteConfigDto(string host, int type, string content, string secret);
    }
}