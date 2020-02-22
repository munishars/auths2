using System;
using System.Diagnostics.CodeAnalysis;

namespace Auth.Api.Infrastructure.ServiceDiscovery
{
    [ExcludeFromCodeCoverage]
    public class ServiceConfig
    {
        public Uri ServiceDiscoveryAddress { get; set; }
        public Uri ServiceAddress { get; set; }
        public string ServiceName { get; set; }
        public string ServiceId { get; set; }
    }
}
