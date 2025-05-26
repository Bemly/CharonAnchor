using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Api;
using Lagrange.Milky.Implementation.Api.Handler.File;
using Lagrange.Milky.Implementation.Api.Handler.Friend;
using Lagrange.Milky.Implementation.Api.Handler.Group;
using Lagrange.Milky.Implementation.Api.Handler.Message;
using Lagrange.Milky.Implementation.Api.Handler.System;
using Lagrange.Milky.Implementation.Entity.Message.Incoming;

namespace Lagrange.Milky.Implementation.Utility;

public static partial class MilkyJsonUtility
{
    // === api ===
    [JsonSerializable(typeof(ApiOkResult))]
    [JsonSerializable(typeof(ApiFailedResult))]
    // == system ==
    // get_friend_list
    [JsonSerializable(typeof(GetFriendListParameter))]
    [JsonSerializable(typeof(GetFriendListResult))]
    // get_friend_info
    [JsonSerializable(typeof(GetFriendInfoParameter))]
    [JsonSerializable(typeof(GetFriendInfoResult))]
    // get_group_list
    [JsonSerializable(typeof(GetGroupListParameter))]
    [JsonSerializable(typeof(GetGroupListResult))]
    // get_group_info
    [JsonSerializable(typeof(GetGroupInfoParameter))]
    [JsonSerializable(typeof(GetGroupInfoResult))]
    // get_group_member_list
    [JsonSerializable(typeof(GetGroupMemberListParameter))]
    [JsonSerializable(typeof(GetGroupMemberListResult))]
    // get_group_member_info
    [JsonSerializable(typeof(GetGroupMemberInfoParameter))]
    [JsonSerializable(typeof(GetGroupMemberInfoResult))]
    // == message ==
    // send_private_message
    [JsonSerializable(typeof(SendPrivateMessageParameter))]
    [JsonSerializable(typeof(SendPrivateMessageResult))]
    // send_group_message
    [JsonSerializable(typeof(SendGroupMessageParameter))]
    [JsonSerializable(typeof(SendGroupMessageResult))]
    // == friend ==
    // send_friend_nudge
    [JsonSerializable(typeof(SendFriendNudgeParameter))]
    // == group ==
    // send_group_nudge
    [JsonSerializable(typeof(SendGroupNudgeParameter))]
    // == file ==
    // get_group_file_download_url
    [JsonSerializable(typeof(GetGroupFileDownloadUrlParameter))]
    [JsonSerializable(typeof(GetGroupFileDownloadUrlResult))]
    // delete_group_file
    [JsonSerializable(typeof(DeleteGroupFileParameter))]

    // === event ===
    [JsonSerializable(typeof(Event.Event))]
    // message_receive
    [JsonSerializable(typeof(FriendIncomingMessage))]
    [JsonSerializable(typeof(GroupIncomingMessage))]
    [JsonSerializable(typeof(TempIncomingMessage))]
    private partial class MilkyJsonContext : JsonSerializerContext;

    public static byte[] SerializeToUtf8Bytes(Type type, object? value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, type, MilkyJsonContext.Default);
    }

    public static ValueTask<object?> DeserializeAsync(Type type, Stream json, CancellationToken token)
    {
        return JsonSerializer.DeserializeAsync(json, type, MilkyJsonContext.Default, token);
    }
}
