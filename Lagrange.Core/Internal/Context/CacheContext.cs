using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Internal.Context;

internal class CacheContext(BotContext context)
{
    private List<BotFriend>? _friends;

    private readonly Dictionary<int, BotFriendCategory> _categories = new();
    
    public async Task<List<BotFriend>> GetFriendList(bool refresh = false)
    {
        if (refresh || _friends == null) Interlocked.Exchange(ref _friends, await FetchFriends());
        
        return _friends;
    }

    public async Task<List<BotFriendCategory>> GetCategories(bool refresh = false)
    {
        if (refresh || _categories.Count == 0) Interlocked.Exchange(ref _friends, await FetchFriends());

        return _categories.Values.ToList();
    }

    public async Task<BotFriend?> ResolveFriend(long uin)
    {
        if (_friends == null) Interlocked.Exchange(ref _friends, await FetchFriends());
        var friend = _friends?.FirstOrDefault(f => f.Uin == uin);
        
        if (friend == null)
        {
            _friends = Interlocked.Exchange(ref _friends, await FetchFriends());
            friend = _friends?.FirstOrDefault(f => f.Uin == uin);
        }
        
        return friend;
    }
    
    /// <summary>
    /// Fetches the friends list from the server.
    /// </summary>
    private async Task<List<BotFriend>> FetchFriends()
    {
        var friends = new List<BotFriend>();

        byte[]? cookie = null;
        do
        {
            var result = await context.EventContext.SendEvent<FetchFriendsEventResp>(new FetchFriendsEventReq(cookie));
            if (result == null)
            {
                // TODO: Log
                break;
            }

            cookie = result.Cookie;
            
            friends.AddRange(result.Friends);
            foreach (var category in result.Category) _categories[category.Id] = category;
        } while (cookie != null);
        
        return friends;
    }
}