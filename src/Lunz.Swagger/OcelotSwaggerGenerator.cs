using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lunz.Swagger.Models;
using Lunz.Swagger.Models.Ocelot;
using NSwag;

namespace Lunz.Swagger
{
    public class OcelotSwaggerGenerator
    {
        public bool ThrowException { get; set; }

        public async Task<SwaggerDocument> Generate(OcelotSwaggerSettings settings, Configuration ocelotConfiguration)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (ocelotConfiguration == null)
                throw new ArgumentNullException(nameof(ocelotConfiguration));

            var swaggerDocument = new SwaggerDocument
            {
                Info = settings.Info
            };

            var documents = await GetDownstreamSwaggerDocuments(settings, ocelotConfiguration);

            var definitionKeys = new Dictionary<string, int>();

            foreach (var definition in documents.Values.SelectMany(x => x.Definitions))
            {

                if (swaggerDocument.Definitions.ContainsKey(definition.Key))
                {
                    var keyCount = definitionKeys[definition.Key];
                    swaggerDocument.Definitions.Add($"{definition.Key}_{keyCount}", definition.Value);
                    definitionKeys[definition.Key] = keyCount + 1;
                }
                else
                {
                    swaggerDocument.Definitions.Add(definition.Key, definition.Value);
                    definitionKeys.Add(definition.Key, 1);
                }
            }

            foreach (var reRoute in ocelotConfiguration.ReRoutes)
            {
                AddPath(swaggerDocument, reRoute, documents);
            }

            if (settings.SecurityDefinitions != null)
            {
                foreach (var item in settings.SecurityDefinitions)
                {
                    swaggerDocument.SecurityDefinitions.Add(item);
                }
            }

            return swaggerDocument;
        }

        private async Task<Dictionary<string, SwaggerDocument>> GetDownstreamSwaggerDocuments(
            OcelotSwaggerSettings settings, Configuration ocelotConfiguration)
        {
            var dic = new Dictionary<string, SwaggerDocument>();

            var downstreamHosts = ocelotConfiguration.ReRoutes
                .SelectMany(x => x.DownstreamHostAndPorts);

            foreach (var host in downstreamHosts)
            {
                if (dic.ContainsKey(host.ToUrl()))
                    continue;

                // var swaggerDocumentUrl = $"{host.ToUrl()}/swagger/v1/swagger.json";
                var swaggerDocumentUrl = string.Format(settings.Settings.SwaggerUrl, host.ToUrl());

                var downstreamSwaggerDocument = settings.DownstreamSwaggers
                    .FirstOrDefault(x => string.Compare(x.Host, host.ToUrl(), StringComparison.OrdinalIgnoreCase) == 0);
                if (downstreamSwaggerDocument != null)
                    swaggerDocumentUrl = downstreamSwaggerDocument.SwaggerUrl;

                try
                {
                    var document = await SwaggerDocument.FromUrlAsync(swaggerDocumentUrl);
                    dic.Add(host.ToUrl(), document);
                }
                catch (Exception ex)
                {
                    Log(ex);
                }
            }

            return dic;
        }

        private void AddPath(SwaggerDocument swaggerDocument,
            ReRoute reRoute, IReadOnlyDictionary<string, SwaggerDocument> documents)
        {
            var host = reRoute.DownstreamHostAndPorts.First().ToUrl();
            
            if (!documents.ContainsKey(host))
            {
                Log(new Exception($"不能找到 '{host}' 相对应的 Swagger 文档。"));
                return;
            }

            var document = documents[host];
            var pathTemplate = reRoute.DownstreamPathTemplate;
            var httpMethods = reRoute.UpstreamHttpMethod;

            var pathKey = document.Paths.Keys
                .FirstOrDefault(x => string.Compare(x, pathTemplate, StringComparison.OrdinalIgnoreCase) == 0);
            if (string.IsNullOrWhiteSpace(pathKey))
            {
                Log(new Exception($"在 '{host}' 中不能找到 '{pathTemplate}' 相对应的值。"));
                return;
            }

            var pathItem = document.Paths[pathKey];

            SwaggerPathItem newPathItem = null;

            if (swaggerDocument.Paths.ContainsKey(reRoute.UpstreamPathTemplate))
            {
                newPathItem = swaggerDocument.Paths[reRoute.UpstreamPathTemplate];
            }
            else
            {
                newPathItem = new SwaggerPathItem()
                {
                    Summary = pathItem.Summary,
                    Description = pathItem.Description,
                    Servers = pathItem.Servers,
                    Parameters = pathItem.Parameters,
                };

                swaggerDocument.Paths.Add(reRoute.UpstreamPathTemplate, newPathItem);
            }

            foreach (var httpMethod in httpMethods)
            {
                var operationMethod = ConvertToSwaggerOperationMethod(httpMethod);
                if (!pathItem.ContainsKey(operationMethod))
                {
                    Log(new Exception($"在 '{host}' 中不能找到 '({httpMethod}){pathKey}' 相对应的值。"));
                    continue;
                }

                newPathItem.Add(operationMethod, pathItem[operationMethod]);
            }
        }

        private static SwaggerOperationMethod ConvertToSwaggerOperationMethod(string httpMethod)
        {
            var method = httpMethod.ToUpper();
            switch (method)
            {
                case "GET":
                    return SwaggerOperationMethod.Get;
                case "POST":
                    return SwaggerOperationMethod.Post;
                case "PUT":
                    return SwaggerOperationMethod.Put;
                case "PATCH":
                    return SwaggerOperationMethod.Patch;
                case "DELETE":
                    return SwaggerOperationMethod.Delete;
                case "HEAD":
                    return SwaggerOperationMethod.Head;
                case "OPTIONS":
                    return SwaggerOperationMethod.Options;
                default:
                    throw new NotImplementedException(httpMethod);
            }
        }

        private void Log(Exception ex)
        {
            if (ThrowException)
                throw ex;
            else
            {
                // TODO: LOG
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }
    }
}