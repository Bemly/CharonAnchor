using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Internal.Packets.Struct;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<LoginEventReq>(Protocols.PC)]
[Service("wtlogin.login", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class LoginService : BaseService<LoginEventReq, LoginEventResp>
{
    private Lazy<WtLogin> _packet = new();

    protected override ValueTask<ReadOnlyMemory<byte>> Build(LoginEventReq input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));
        return new ValueTask<ReadOnlyMemory<byte>>(_packet.Value.BuildOicq09());
    }

    protected override ValueTask<LoginEventResp?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));

        return base.Parse(input, context);
    }
}

[EventSubscribe<LoginEventReq>(Protocols.Android)]
[Service("wtlogin.login", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class LoginServiceAndroid : BaseService<LoginEventReq, LoginEventResp>
{
    private Lazy<WtLogin> _packet = new();

    protected override ValueTask<ReadOnlyMemory<byte>> Build(LoginEventReq input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));

        return base.Build(input, context);
    }

    protected override ValueTask<LoginEventResp?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));

        return base.Parse(input, context);
    }
}