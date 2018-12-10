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
{    /// <summary>
     /// This custom implementation of IScopedServiceProvider is required to use JADNC as a client.
     /// Normally, ie when using JADNC as a server, JsonApiSerializer acquires IScopedServiceProvider through DI. The default implementation then depends on httpContextAccessor.HttpContext.
     /// However, when using JADNC as a client, we extract the JsonApiSerializer and use it outside the JADNC environment, and therefore httpContextAccessor.HttpContext will not be set.
     /// This will thus throw an error. We fix this with the custom implementation below. As a reference, see
     /// https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/src/JsonApiDotNetCore/Services/ScopedServiceProvider.cs
     /// </summary>
    public class JsonApiClientScopedServiceProvider : IScopedServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public JsonApiClientScopedServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            // just return the service that is asked for.
            return _serviceProvider.GetService(serviceType);

        }
    }
}
