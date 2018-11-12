using System.Collections.Generic;

namespace Lunz.Swagger.Models.Ocelot
{
    public class ReRoute
    {
        public string DownstreamPathTemplate { get; set; }
        public string DownstreamScheme { get; set; }
        public IEnumerable<DownstreamHostAndPort> DownstreamHostAndPorts { get; set; }
        public string UpstreamPathTemplate { get; set; }
        public IEnumerable<string> UpstreamHttpMethod { get; set; }
        public string ServiceName { get; set; }

        public ReRoute()
        {
            DownstreamHostAndPorts = new List<DownstreamHostAndPort>();
            UpstreamHttpMethod = new List<string>();
        }
    }
}
