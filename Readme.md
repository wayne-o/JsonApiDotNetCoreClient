# Json Api Client

##  What
Some classes to use JADNC as a client rather than a server. 

Checkout https://rs-finance.visualstudio.com/_git/nmbrs for more examples (next to those below) of usage

Todo: 
* M2F also has an extra piece in the JADNC that supports Bulk request. I did not yet extract that piece of code into this "package" (repo).
## How

### Clone the repo into your project
:point_up_2:

### Configure in startup
Secondly, configure JADNC as a client in `startup.cs`.
```c#
using  <your-namespace>.Extensions.JsonApiClient;
public void ConfigureServices(IServiceCollection services)
{
    ...
    _services = services;
    ConfigureJsonApiDotNetCore();
    ConfigureRsApiClients();
    ...
}
// This configures JADNC as a client. Note that there are two properties on the `options` object: core and client.
// Core is used for normal JADNC configuration. Client is used for the client (in this case we're setting a api root adres).
// Note that the AddJsonApiClient method takes care of some 
// overhead introduced when isolating some JADNC components from the usual environment in which it is used (see comments ScopedService provider file).
void ConfigureJsonApiDotNetCore()
{
    _services.AddJsonApiClient(options =>
    {
        options.Client.BaseAddress = new Uri(Configuration.GetSection("YourJadncApiTargetUrl").Value);
        options.Core.BuildResourceGraph((b) =>
        {
            b.AddResource<Client, Guid>("clients");
            b.AddResource<InfineTask, Guid>("infine-tasks");
        });
    });

}
void ConfigureApiClients()
{
    // register the Client Services that will consume your API
    _services.AddScoped<IJsonApiClient<Client, Guid>, ClientService>();
    _services.AddScoped<IJsonApiClient<InfineTask, Guid>, InfineTaskService>();
    _services.AddScoped<IJsonApiClient<Run, int>, RunService>();

    // We could do this this automatically based on the defined entities in the JANDC resource graph.
    _services.AddScoped<IJsonApiClientSerializer<Client>, JsonApiClientSerializer<Client>>();
    _services.AddScoped<IJsonApiClientSerializer<InfineTask>, JsonApiClientSerializer<InfineTask>>();
    _services.AddScoped<IJsonApiClientSerializer<Run>, JsonApiClientSerializer<Run>>();
}
```


### Implement deriving classes of `JsonApiClient<TEntity, TId>`

Then, implement the `YourModelService`  api client service/consumer you require for your app. It has to extend `JsonApiClient<TEntity, TId>`.  The idea is that this class will be the base class for a client for any model, just like `DefaultResourceService` is in JADNC.  Right now, this class is still pretty empty, so you will need to make a "custom" implementation to fill in the logic. Later, when we have seen all the ins and outs of what we need, we should  generalize that and put in the `JsonApiClient` base class.

As an example: check the implementation of `Client` in the `Nbmrs` microservice.

### no auth example
```c#
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using nmbrs.Models;
using System.Net.Http;
using Newtonsoft.Json;
using JsonApiDotNetCore.Serialization;
using nmbrs.Extensions.JsonApiClient;

namespace nmbrs.ApiClientServices
{
    public class ClientService : JsonApiClient<Client, Guid>, IJsonApiClient<Client, Guid>
    {
        readonly string _endPoint = "clients";
        public ClientService(
            HttpClient httpClient,
            IJsonApiDeSerializer deserializer,
            IJsonApiClientSerializer<Client> serializer
        ) : base(httpClient, deserializer, serializer) { }


        public async override Task<IEnumerable<Client>> GetAsync(Dictionary<string, string> qp = null)
        {
            var url = _endPoint + CreateQueryParams(qp); 
            string requestBody = await _httpClient.GetStringAsync(url);
            Documents documents = JsonConvert.DeserializeObject<Documents>(requestBody);
            return _deserializer.DeserializeList<Client>(requestBody);
        }
    }
}

```

### with auth example
```c#
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Models;
using nmbrs.Models;
using System.Net.Http;
using Newtonsoft.Json;
using JsonApiDotNetCore.Serialization;
using nmbrs.Extensions.JsonApiClient;

namespace nmbrs.ApiClientServices
{
    public class ClientService : JsonApiClient<Client, Guid>, IJsonApiClient<Client, Guid>
    {
        readonly string _endPoint = "clients";
        public ClientService(
            HttpClient httpClient,
            IJsonApiDeSerializer deserializer,
            IJsonApiClientSerializer<Client> serializer
            // IJsonApiClientAuthProvider should be registred in startup. You can eg use the DaemonAuthProvider or UserAuthProvider as defined in m2f project.
            IJsonApiClientAuthProvider authProvider 

        ) : base(httpClient, deserializer, serializer, authProvider ) { }
        //            ------------------------------->      ^^^^    add this here

        public async override Task<IEnumerable<Client>> GetAsync(Dictionary<string, string> qp = null)
        {
            var auth = await AddAuthIfAvailable() // adds a token internally to the request sent out.
            var url = _endPoint + CreateQueryParams(qp); 
            string requestBody = await _httpClient.GetStringAsync(url);
            Documents documents = JsonConvert.DeserializeObject<Documents>(requestBody);
            return _deserializer.DeserializeList<Client>(requestBody);
        }
    }
}

```

### example sending data
The above two examples only pulls data from the api, the example below also sends it.
```c#

public class RunService : JsonApiClient<Run, int>, IJsonApiClient<Run, int>
{  
    readonly string _endPoint = "runs";
    public RunService(
        HttpClient httpClient,
        IJsonApiDeSerializer deserializer,
        IJsonApiClientSerializer<Run> serializer
    ) : base (httpClient, deserializer, serializer)
    {

    }

    public override async Task<Run> CreateAsync(Run entity, Dictionary<string, string> qp = null)
    {
        //var auth = await AddAuthIfAvailable();
        string document = _serializer.Serialize(entity);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _endPoint)
        {
            Content = new StringContent(document)
        };
        SetContentType(request);
        var response = await _httpClient.SendAsync(request);
        return entity;
    }

}
```





