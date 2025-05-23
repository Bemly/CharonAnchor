using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Parameter;


[JsonDerivedType(typeof(EmptyApiParameter))]

// === system ===
// get_friend_list
[JsonDerivedType(typeof(GetFriendListApiParameter))]
// get_friend_info
[JsonDerivedType(typeof(GetFriendInfoApiParameter))]

// === message ===
// send_private_message
[JsonDerivedType(typeof(SendPrivateMessageApiParameter))]
// send_group_message
[JsonDerivedType(typeof(SendGroupMessageApiParameter))]
public class IApiParameter { }