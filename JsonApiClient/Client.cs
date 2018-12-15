using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using QueryParams = System.Collections.Generic.Dictionary<string, string>;


namespace JsonApiClient
{
    public interface IJsonApiClient<TModel, TId>
    {

        Task<IEnumerable<TModel>> CreateAsync(IEnumerable<TModel> entity, QueryParams queryParams = null);
        Task<TModel> CreateAsync(TModel entity, QueryParams queryParams = null);
        Task<bool> DeleteAsync(TId id, QueryParams queryParams = null);
        Task<IEnumerable<TModel>> GetAsync(QueryParams queryParams = null);
        Task<TModel> GetAsync(TId id, QueryParams queryParams = null);
        Task<object> GetRelationshipAsync(TId id, string relationshipName, QueryParams queryParams = null);
        Task<object> GetRelationshipsAsync(TId id, string relationshipName, QueryParams queryParams = null);
        Task<TModel> UpdateAsync(TId id, TModel entity, QueryParams queryParams = null);
        Task UpdateRelationshipsAsync(TId id, string relationshipName, List<ResourceObject> relationships, QueryParams queryParams = null);
    }

    /// <summary>
    /// The client class that should be responsible for making the calls to the RS api (or any other JADNC API).
    /// The class is basically a stripped version of the default JADNC EntityResourceService with some extra methods
    /// and the posibility of allowing for queryparams.
    /// 
    /// As a reference see https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/src/JsonApiDotNetCore/Services/EntityResourceService.cs
    /// 
    /// For now, all these methods have no implementation, this needs to be done manually. I think it is a bit premature to do that already.
    ///  ater on, when we have a better overview of the different implemen
    /// 
    /// </summary>
    public abstract class JsonApiClient<TModel, TId> : IJsonApiClient<TModel, TId>
    {
  
        protected readonly HttpClient _httpClient;
        protected readonly IJsonApiDeSerializer _deserializer;
        protected readonly IJsonApiClientSerializer<TModel> _serializer;
        protected readonly IJsonApiClientAuthProvider _authProvider;
        protected JsonApiClient(
            HttpClient httpClient,
            IJsonApiDeSerializer deserializer,
            IJsonApiClientSerializer<TModel> serializer,
            IJsonApiClientAuthProvider authProvider = null
        )
        {
            _deserializer = deserializer;
            _httpClient = httpClient;
            _serializer = serializer;
            _authProvider = authProvider;
        }

        public virtual async Task<IEnumerable<TModel>> GetAsync(QueryParams qp = null)
        {
            throw new NotImplementedException();
        }
        public virtual async Task<TModel> GetAsync(TId id, QueryParams qp = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task<object> GetRelationshipAsync(TId id, string relationshipName, QueryParams qp = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task<object> GetRelationshipsAsync(TId id, string relationshipName, QueryParams qp = null)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IEnumerable<TModel>> CreateAsync(IEnumerable<TModel> entities, QueryParams qp = null)
        {
            return await Task.WhenAll(entities.Select(e => CreateAsync(e, qp)));
        }

        public virtual async Task<TModel> CreateAsync(TModel entity, QueryParams qp = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> DeleteAsync(TId id, QueryParams qp = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task<TModel> UpdateAsync(TId id, TModel entity, QueryParams qp = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task UpdateRelationshipsAsync(TId id, string relationshipName, List<ResourceObject> relationships, QueryParams qp = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Creates a URL compatible queryparams strings, to be (manually) prepended to the request endpoint URL.
        /// </summary>
        /// <returns>The query parameters.</returns>
        /// <param name="qp">Qp.</param>
        protected string CreateQueryParams(QueryParams qp)
        {
        return "?" + string.Join("&",
            qp.Select(kvp =>
                string.Format("{0}={1}", kvp.Key, kvp.Value)));
        }
        /// <summary>
        /// Gets a token from the injected AuthProvider and adds it to the headers.
        /// </summary>
        /// <returns>A boolean to indicate whether auth was added or not</returns>
        protected async Task<bool> AddAuthIfAvailable()
        {
            bool authIsAvailable = false;
            if (_authProvider != null)
            {
                string token = await _authProvider.GetAccessToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                authIsAvailable = true;
            }
            return authIsAvailable;
        }
        protected void SetContentType(HttpRequestMessage request)
        {
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.api+json");
        }
    }







}
