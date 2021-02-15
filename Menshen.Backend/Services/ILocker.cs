using System;
using System.Collections.Generic;

namespace Menshen.Backend.Services
{
    public interface ILocker
    {
        List<(int, string)> GetSiteContent(string host, int type);
        void AddSiteConfig(string host, int type, string content);

        (int, long)? GetIpBlockDetails(string ip);
        void BlockIp(string ip, bool permanent);

        bool GetIpAllowedForHost(string ip, string host);
        void AllowIpForHost(string ip, int siteId, string host, TimeSpan timeSpan);
    }

    public static class UserRole
    {
        public const int Admin = 1;
        public const int NormalUser = 2;
    }
}