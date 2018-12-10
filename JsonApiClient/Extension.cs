using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace nmbrs.Extensions.JsonApiClient
{
    /// <summary>
    /// TODO make generically usable version of JsonApiSerializer  (ClientSerialize)
    /// </summary>
    public static class AddJsonApiClientExtension
    {
        public static IServiceCollection AddJsonApiClient(this IServiceCollection services, Action<JsonApiClientOptions> action)
        {
            var options = new JsonApiClientOptions();
            var coreOptions = options.Core;

            services.AddJsonApiInternals(coreOptions);
            action(options);
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = options.Client.BaseAddress
            };
            ServicePointManager.FindServicePoint(options.Client.BaseAddress).ConnectionLeaseTimeout = 60000;
            services.AddSingleton(httpClient);
            services.AddScoped<IScopedServiceProvider, JsonApiClientScopedServiceProvider>();

            return services;
        }
    }

    public class ClientOptions
    {
        public Uri BaseAddress { get; set; }
        public int ConnecitonLeaseTimeout { get; set; } = 60000;

    }
    public class JsonApiClientOptions
    {
        public JsonApiOptions Core { get; set; }
        public ClientOptions Client { get; set; }
        public JsonApiClientOptions()
        {
            Core = new JsonApiOptions();
            Client = new ClientOptions();
        }

    }

}
