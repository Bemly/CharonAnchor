using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Struct;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<NewDeviceLoginEventReq>(Protocols.PC)]
[Service("trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginNewDevice", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class NewDeviceLoginService : BaseService<NewDeviceLoginEventReq, NewDeviceLoginEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(NewDeviceLoginEventReq input, BotContext context)
    {
        return new ValueTask<ReadOnlyMemory<byte>>(NTLoginCommon.Encode(context, input.Sig, null));
    }

    protected override ValueTask<NewDeviceLoginEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var state = NTLoginCommon.Decode(context, input, out var info, out _);
    
        return new ValueTask<NewDeviceLoginEventResp>(state switch
        {
            NTLoginCommon.State.LOGIN_ERROR_SUCCESS => new NewDeviceLoginEventResp(state, null),
            _ when info is not null => new NewDeviceLoginEventResp(state, (info.TipsTitle, info.TipsContent)),
            _ => new NewDeviceLoginEventResp(state, null)
        });
    }
}