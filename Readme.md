
# Json Api Client  
  
## What  
Some classes to use JADNC as a client rather than as, or next to a server.  
 
Some examples are described down here. A live implementation can be found [here](https://rs-finance.visualstudio.com/_git/nmbrs) (used as a client running in parallel to a server).
  
Todo:  
* M2F also has an extra piece in the JADNC that supports Bulk request. I did not yet extract that piece of code into this "package".  

## How

### 1. Clone the repo into your project
:point_up_2:

### 2. Configure in startup
Secondly, configure JADNC as a client in `startup.cs`. There is two ways of doing this

#### 2a. Use JADNC as both a client and server
```c#
void ConfigureJsonApiDotNetCore()
{
    // configuring the server as usual - nothing changed
    _services.AddJsonApi<NmbrsDb>(options =>
    {
        options.BuildResourceGraph((b) => {
            b.AddResource<Client, Guid>("clients");
            b.AddResource<InfineTask, Guid>("infine-tasks");
        });
    });
    // this adds the client on top
    _services.AddJsonApiClient(options =>
   {
       options.BaseAddress = new Uri(Configuration.GetSection("RsApiUrl").Value);
   });
}
```
#### 2b. Use JADNC as a standalone client
```c#

void ConfigureJsonApiDotNetCore()
{
    // this initializes the server (core) as far as is required,
    // taking into account that it some .net core features 
    // might not be available, eg the MVC features when using a 
    // console app.
    _services.AddJsonApiClientStandAlone<NmbrsDb>(options =>
    {
        options.Core.BuildResourceGraph((b) => {
            b.AddResource<Client, Guid>("clients");
            b.AddResource<InfineTask, Guid>("infine-tasks");
        });
        options.Client.BaseAddress = new Uri(Configuration.GetSection("RsApiUrl").Value);
    });
}
```

### 3. Define a model
(Almost) as usual, you'll have to define a model. One thing: add the following attribute as seen below.
```c#
[Links(Link.None)]    // <---- this needs to be added for serialization to work, ie when sending JA data to your api.
public  class  Client  :  Identifiable<Guid>  
{    
    [Attr("number")]  
    public  int  Number  {  get;  set;  }  
    [Attr("name")]  
    public  string  Name  {  get;  set;  }  
}
```


### 4. Implement deriving classes of `JsonApiClient<TEntity, TId>`

Then, implement the `YourModelService`  api client service/consumer you require for your app. It has to extend `JsonApiClient<TEntity, TId>`.  The idea is that this class will be the base class for a client for any model, just like `DefaultResourceService` is in JADNC.  Right now, this class is still pretty empty, so you will need to make a "custom" implementation to fill in the logic. Later, when we have seen all the ins and outs of what we need, we should  generalize that and put in the `JsonApiClient` base class.

As an example: check the implementation of `Client` in the `Nbmrs` microservice.

#### 4a. Example without auth
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

#### 4b. Example using auth
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

#### 4c. Example sending data
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


### 5. Register ApiClient implementations

```c#
// Registering actual clients, ie the classes that will do the communication with the API.
void ConfigureApiClients()
{
    // register the Client Services that will consume your API
    _services.AddScoped<IJsonApiClient<Client, Guid>, ClientService>();
    _services.AddScoped<IJsonApiClient<InfineTask, Guid>, InfineTaskService>();
    _services.AddScoped<IJsonApiClient<Run, int>, RunService>();
}
```



