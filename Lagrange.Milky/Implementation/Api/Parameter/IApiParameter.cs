using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Parameter;


[JsonDerivedType(typeof(EmptyApiParameter))]
// get_friend_list
[JsonDerivedType(typeof(GetFriendListApiParameter))]
// get_friend_info
[JsonDerivedType(typeof(GetFriendInfoApiParameter))]
public class IApiParameter { }