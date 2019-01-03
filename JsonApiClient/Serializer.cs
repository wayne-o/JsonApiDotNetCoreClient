using System.Collections.Generic;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace JsonApiClient
{

    public interface IJsonApiClientSerializer<T>
    {
        string Serialize(T entity);
        string Serialize(IEnumerable<T> entity);
    }

    /// <summary>
    ///  The default JADNC serializer adds links in the relation data. We need to get rid of these, which we do in this custom implementation of IJsonApiSerializer 
    /// 
    ///  For now, I have made a generic version of this class so that it is easier to make a custom implementation for a specific entity, to tweak little things in serialization. 
    ///  Later on, if we know all of the caveats, this should be generalized such that we no longer need a generic version.
    /// 
    ///  Since we can now use different serializers for different entities (unlike in JADNC), we need to configure which to use in the startup. In default cases it should be enough to
    ///  to use IJsonApiClientSerializer{DefaultUsageCase} => JsonApiClientSerializer. 
    /// 
    ///  TODO: if the fields/methods in JsonApiSerializer (in JADNC) were protected/virtual, we could just inherit from JANDC.JsonApiSerializer and build on top of that. Now, however, 
    ///  we need to define our own class entirely. Make an issue for this @ JADNC.
    /// </summary>
    public class JsonApiClientSerializer<T> : IJsonApiClientSerializer<T>
    {

        private readonly IClientDocumentBuilder _documentBuilder;
        private readonly ILogger<JsonApiSerializer> _logger;
        private readonly IJsonApiContext _jsonApiContext;
        private readonly IResourceGraph _contextGraph;


        public JsonApiClientSerializer(
            IJsonApiContext jsonApiContext,
            IClientDocumentBuilder documentBuilder,
            IResourceGraph contextGraph)
        {
            _jsonApiContext = jsonApiContext;
            _documentBuilder = documentBuilder;
            _contextGraph = contextGraph;
        }

        public JsonApiClientSerializer(
            IJsonApiContext jsonApiContext,
            IClientDocumentBuilder documentBuilder,
            ILoggerFactory loggerFactory,
            IResourceGraph contextGraph)
        {
            _jsonApiContext = jsonApiContext;
            _documentBuilder = documentBuilder;
            _logger = loggerFactory?.CreateLogger<JsonApiSerializer>();
            _contextGraph = contextGraph;
        }


        public string Serialize(T entity)
        {
            string abort = CheckForNullOrError(entity);
            if (abort != null) return abort;
            return SerializeDocument(entity);
        }

        public string Serialize(IEnumerable<T> entity)
        {
            string abort = CheckForNullOrError(entity);
            if (abort != null) return abort;
            return SerializeDocuments(entity);
        }


        protected string CheckForNullOrError(object entity)
        {
            if (entity == null)
                return GetNullDataResponse();

            if (entity.GetType() == typeof(ErrorCollection) )
                return GetErrorJson(entity, _logger);

            return null;
        }

        protected string GetNullDataResponse()
        {
            return JsonConvert.SerializeObject(new Document
            {
                Data = null
            });
        }

        protected string GetErrorJson(object responseObject, ILogger logger)
        {
            if (responseObject is ErrorCollection errorCollection)
            {
                return errorCollection.GetJson();
            }
            else
            {
                if (logger?.IsEnabled(LogLevel.Information) == true)
                {
                    logger.LogInformation("Response was not a JSONAPI entity. Serializing as plain JSON.");
                }

                return JsonConvert.SerializeObject(responseObject);
            }
        }

        protected string SerializeDocuments(object entity)
        {
            var entities = entity as IEnumerable<IIdentifiable>;
            var documents = _documentBuilder.Build(entities);
            return _serialize(documents);
        }

        protected string SerializeDocument(object entity)
        {
            var identifiableEntity = entity as IIdentifiable;
            var document = RemoveLinks(_documentBuilder.Build(identifiableEntity));
            return _serialize(document);
        }


        protected Document RemoveLinks(Document document)
        {
            document.Links = null;
            if (document.Data.Relationships != null)
            {
                foreach (var entry in document.Data.Relationships)
                {
                    var relation = entry.Value;
                    relation.Links = null;
                }
            }
            return document;
        }

        protected string _serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _jsonApiContext.Options.SerializerSettings);
        }
    }

}
