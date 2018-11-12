namespace Lunz.Swagger.Models.Ocelot
{
    public class DownstreamHostAndPort
    {
        public string Host { get; set; }
        public int? Port { get; set; }

        public DownstreamHostAndPort()
        {
        }

        public DownstreamHostAndPort(string host, int? port = null)
        {
            Host = host;
            Port = port;
        }

        public string ToUrl()
        {
            return Port == null || Port == 80 ? $"{Host}" : $"{Host}:{Port}";
        }
    }
}