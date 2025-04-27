using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<QrLoginEventReq>(Protocols.Android)]
[Service("wtlogin.qrlogin")]
internal class QrLoginService : BaseService<QrLoginEventReq, QrLoginEventResp>
{
    private Lazy<WtLogin> _packet = new();
    
    protected override ValueTask<ReadOnlyMemory<byte>> Build(QrLoginEventReq input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));

        return new ValueTask<ReadOnlyMemory<byte>>(input.IsApproved is { } approved
            ? approved ? _packet.Value.BuildQrlogin20(input.K) : _packet.Value.BuildQrlogin22(input.K)
            : _packet.Value.BuildQrlogin19(input.K));
    }

    protected override ValueTask<QrLoginEventResp?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));

        var wtlogin = _packet.Value.Parse(input.Span, out ushort command);
        var code2d = _packet.Value.ParseCode2dPacket(wtlogin, out ushort code2dCmd);
        var reader = new BinaryPacket(code2d);
        
        _ = reader.Read<ushort>();
        long uin = reader.Read<long>();
        byte state =  reader.Read<byte>();
        if (state == 0)
        {
            uint timestamp = reader.Read<uint>();
            string platform = reader.ReadString(Prefix.Int16 | Prefix.LengthOnly);
            var tlvs = ProtocolHelper.TlvUnPack(ref reader);
        }
        else
        {
            string message = reader.ReadString(Prefix.Int16 | Prefix.LengthOnly);
        }

        return base.Parse(input, context);
    }
}