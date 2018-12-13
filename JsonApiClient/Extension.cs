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
using Microsoft.EntityFrameworkCore;
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
        public static IServiceCollection AddJsonApiCoreWithClient<TDbContext>(this IServiceCollection services, Action<JsonApiCoreWithClientOptions> action) where TDbContext : DbContext
        {
            var options = new JsonApiCoreWithClientOptions();
            var coreOptions = options.Core;

            services.AddJsonApiInternals<TDbContext>(coreOptions);
            action(options);
            AddClientInternals(services, options.Client);
            return services;
        }
        public static IServiceCollection AddJsonApiCoreWithClient(this IServiceCollection services, Action<JsonApiCoreWithClientOptions> action)
        {
            var options = new JsonApiCoreWithClientOptions();
            var coreOptions = options.Core;

            services.AddJsonApiInternals(coreOptions);
            action(options);
            AddClientInternals(services, options.Client);
            return services;
        }

        public static IServiceCollection AddJsonApiClient(this IServiceCollection services, Action<ClientOptions> action)
        {
            var options = new ClientOptions();
            action(options);
            AddClientInternals(services, options);
            return services;
        }
        private static void AddClientInternals(IServiceCollection services, ClientOptions options)
        {
            HttpClient httpClient = new HttpClient()
            {
                BaseAddress = options.BaseAddress
            };
            ServicePointManager.FindServicePoint(options.BaseAddress).ConnectionLeaseTimeout = 60000;
            services.AddSingleton(httpClient);
            services.AddScoped<IScopedServiceProvider, JsonApiClientScopedServiceProvider>();
        }
    }

    public class ClientOptions
    {
        public Uri BaseAddress { get; set; }
        public int ConnecitonLeaseTimeout { get; set; } = 60000;

    }
    public class JsonApiCoreWithClientOptions
    {
        public JsonApiOptions Core { get; set; }
        public ClientOptions Client { get; set; }
        public JsonApiCoreWithClientOptions()
        {
            Core = new JsonApiOptions();
            Client = new ClientOptions();
        }

    }

}
