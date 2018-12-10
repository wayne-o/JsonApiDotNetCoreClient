using System;
using System.Threading.Tasks;

namespace nmbrs.Extensions.JsonApiClient
{
    public interface IJsonApiClientAuthProvider
    {
        Task<string> GetAccessToken();
    }
}
