using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NSwag;

namespace Lunz.Swagger.Models
{
    public class OcelotSwaggerSettings
    {
        public Settings Settings { get; set; }
        public IEnumerable<DownstreamSwaggerDocument> DownstreamSwaggers { get; set; }
        public SwaggerInfo Info { get; set; }
        public IDictionary<string, SwaggerSecurityScheme> SecurityDefinitions { get; set; }

        public OcelotSwaggerSettings()
        {
            DownstreamSwaggers = new List<DownstreamSwaggerDocument>();
        }
        
        public static OcelotSwaggerSettings FromFile(string file)
        {
            using (var sr = new StreamReader(file, Encoding.UTF8))
            {
                var json = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<OcelotSwaggerSettings>(json);
            }
        }
    }

    public class DownstreamSwaggerDocument
    {
        public string Host { get; set; }
        public string SwaggerUrl { get; set; }
    }

    public class Settings
    {
        public string SwaggerUrl { get; set; }

        public Settings()
        {
            if (string.IsNullOrWhiteSpace(SwaggerUrl))
            {
                SwaggerUrl = "http://{0}:{1}/swagger/v1/swagger.json";
            }
        }
    }
}