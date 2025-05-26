using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Api.Result.Data;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Api.Result;

[JsonDerivedType(typeof(ApiFailedResult))]
[JsonDerivedType(typeof(ApiOkResult<object>))]

// === system ===
// get_login_info
[JsonDerivedType(typeof(ApiOkResult<GetLoginInfoResultData>))]
// get_friend_list
[JsonDerivedType(typeof(ApiOkResult<IEnumerable<Friend>>))]
// get_friend_info
[JsonDerivedType(typeof(ApiOkResult<Friend>))]
// get_group_list
[JsonDerivedType(typeof(ApiOkResult<IEnumerable<Group>>))]
// get_group_info
[JsonDerivedType(typeof(ApiOkResult<Group>))]
// get_group_member_list
[JsonDerivedType(typeof(ApiOkResult<IEnumerable<GroupMember>>))]
// get_group_member_info
[JsonDerivedType(typeof(ApiOkResult<GroupMember>))]

// === message ===
// send_private_message
[JsonDerivedType(typeof(ApiOkResult<SendPrivateMessageResultData>))]
// send_group_message
[JsonDerivedType(typeof(ApiOkResult<SendGroupMessageResultData>))]

// === file ===
// get_group_file_download_url
[JsonDerivedType(typeof(GetGroupFileDownloadUrlResultData))]
// delete_group_file
// [JsonDerivedType(typeof(ApiOkResult<object>))]
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