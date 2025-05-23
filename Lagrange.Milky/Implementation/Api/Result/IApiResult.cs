using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Api.Result.Data;

namespace Lagrange.Milky.Implementation.Api.Result;

// get_login_info
[JsonDerivedType(typeof(ApiOkResult<GetLoginInfoData>))]
// get_friend_list
[JsonDerivedType(typeof(ApiOkResult<IEnumerable<Entity.Friend>>))]

[JsonDerivedType(typeof(ApiFailedResult))]
public interface IApiResult
{
    public string Status { get; }

    public long Retcode { get; }

    public static IApiResult Ok<TData>(TData data) => new ApiOkResult<TData>()
    {
        Data = data,
    };

    public static IApiResult Failed(long retcode, string message) => new ApiFailedResult()
    {
        Retcode = retcode,
        Message = message,
    };
}