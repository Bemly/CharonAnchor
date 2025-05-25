using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Api.Handler.Message;

namespace Lagrange.Milky.Implementation.Api.Parameter;


[JsonDerivedType(typeof(EmptyApiParameter))]

// === system ===
// get_friend_list
[JsonDerivedType(typeof(GetFriendListApiParameter))]
// get_friend_info
[JsonDerivedType(typeof(GetFriendInfoApiParameter))]
// get_group_list
[JsonDerivedType(typeof(GetGroupListApiParameter))]
// get_group_info
[JsonDerivedType(typeof(GetGroupInfoApiParameter))]
// get_group_member_list
[JsonDerivedType(typeof(GetGroupMemberListApiParameter))]
// get_group_member_info
[JsonDerivedType(typeof(GetGroupMemberInfoApiParameter))]

// === message ===
// send_private_message
[JsonDerivedType(typeof(SendPrivateMessageApiParameter))]
// send_group_message
[JsonDerivedType(typeof(SendGroupMessageApiParameter))]
public interface IApiParameter { }