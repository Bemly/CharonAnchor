using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Api;
using Lagrange.Milky.Implementation.Api.System;
using Lagrange.Milky.Implementation.Entity;

namespace Lagrange.Milky.Implementation.Utility;

public partial class MilkyJsonUtility
{
    [JsonSerializable(typeof(object))]
    // 
    [JsonSerializable(typeof(ApiFailedResult))]
    // get_login_list
    [JsonSerializable(typeof(ApiOkResult<GetLoginInfoResult>))]
    // get_friend_list
    [JsonSerializable(typeof(GetFriendListApiParameter))]
    [JsonSerializable(typeof(ApiOkResult<IEnumerable<Friend>>))]
    private partial class MilkyJsonContext : JsonSerializerContext;

    public static byte[] SerializeToUtf8Bytes(Type type, object? value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, type, MilkyJsonContext.Default);
    }

    public static object? Deserialize(Type type, Stream json)
    {
        return JsonSerializer.Deserialize(json, type, MilkyJsonContext.Default);
    }
}
