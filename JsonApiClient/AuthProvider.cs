using System.Threading.Tasks;

namespace JsonApiClient
{
    public interface IJsonApiClientAuthProvider
    {
        Task<string> GetAccessToken();
    }
}
