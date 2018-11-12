using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Lunz.Swagger.Models.Ocelot
{
    public class Configuration
    {
        public IEnumerable<ReRoute> ReRoutes { get; set; }

        public Configuration()
        {
            ReRoutes = new List<ReRoute>();
        }

        public static Configuration FromFile(string file)
        {
            using (var sr = new StreamReader(file, Encoding.UTF8))
            {
                var json = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Configuration>(json);
            }
        }
    }
}