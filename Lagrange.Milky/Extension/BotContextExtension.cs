using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Lagrange.Core;

namespace Lagrange.Milky.Extension;

public static class BotContextExtension
{
    private delegate ValueTask<(int RetCode, string Extra, ReadOnlyMemory<byte> Data)> SendPacketDelegate(BotContext bot, string cmd, int sequence, ReadOnlyMemory<byte> data);

    private const string SsoPacketFullName = "Lagrange.Core.Internal.Packets.Struct.SsoPacket";
    private const string RequestTypeFullName = "Lagrange.Core.Internal.Packets.Struct.RequestType";
    private const string EncryptTypeFullName = "Lagrange.Core.Internal.Packets.Struct.EncryptType";
    private const string ServiceAttributeFullName = "Lagrange.Core.Internal.Services.ServiceAttribute";

    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    [UnconditionalSuppressMessage("Trimming", "IL2075")]
    private static readonly Lazy<SendPacketDelegate> _sendPacket = new(() =>
    {
        var coreAssembly = Assembly.GetAssembly(typeof(BotContext));
        if (coreAssembly == null) throw new Exception("Assembly(Lagrange.Core) not found");
        Type ssoPacketType;
        {
            var type = coreAssembly.GetType(SsoPacketFullName);
            if (type == null) throw new Exception($"Type({SsoPacketFullName}) not found");
            ssoPacketType = type;
        }
        Type serviceAttributeType;
        {
            var type = coreAssembly.GetType(ServiceAttributeFullName);
            if (type == null) throw new Exception($"Type({ServiceAttributeFullName}) not found");
            serviceAttributeType = type;
        }

        // 
        {
            var bot = Expression.Parameter(typeof(BotContext));
            var cmd = Expression.Parameter(typeof(string));
            var sequence = Expression.Parameter(typeof(int));
            var dataInput = Expression.Parameter(typeof(ReadOnlyMemory<byte>));

            // Get PacketContext
            var context = Expression.Property(bot, "PacketContext");

            // Create SsoPacket
            NewExpression packet;
            {
                var ctor = ssoPacketType.GetConstructor(
                    [typeof(string), typeof(ReadOnlyMemory<byte>), typeof(int)]
                );
                if (ctor == null)
                {
                    throw new Exception($"Ctor({SsoPacketFullName}(string, ReadOnlyMemory<byte>, int)) not found");
                }
                packet = Expression.New(ctor, [cmd, dataInput, sequence]);
            }

            // Create ServiceAttribute
            NewExpression options;
            {
                var requestTypeType = coreAssembly.GetType(RequestTypeFullName);
                if (requestTypeType == null) throw new Exception($"Type({RequestTypeFullName}) not found");
                var requestType = Expression.Constant(Enum.Parse(requestTypeType, "D2Auth"));

                var encryptTypeType = coreAssembly.GetType(EncryptTypeFullName);
                if (encryptTypeType == null) throw new Exception($"Type({EncryptTypeFullName}) not found");
                var encryptType = Expression.Constant(Enum.Parse(encryptTypeType, "EncryptD2Key"));

                var ctor = serviceAttributeType.GetConstructor(
                    [typeof(string), requestTypeType, encryptTypeType]
                );
                if (ctor == null)
                {
                    throw new Exception($"Ctor({ServiceAttributeFullName}(string, RequestType, EncryptType)) not found");
                }
                options = Expression.New(ctor, [cmd, requestType, encryptType]);
            }

            // Call SendPacket
            var vt = Expression.Call(context, "SendPacket", [], [packet, options]);

            var taskResult = Expression.Property(vt, "Result");

            var retcode = Expression.Property(taskResult, "RetCode");
            var extra = Expression.Property(taskResult, "Extra");
            var dataResult = Expression.Property(taskResult, "Data");

            MethodCallExpression result;
            {
                var ctor = typeof(Task<(int, string, ReadOnlyMemory<byte>)>).GetConstructor([typeof(int), typeof(string), typeof(ReadOnlyMemory<byte>)]);
                if (ctor == null) throw new Exception("Ctor((int, string, ReadOnlyMemory<byte>)) not found");
                var newResult = Expression.New(ctor, [retcode, extra, dataInput]);

                var method = typeof(ValueTask).GetMethod("FromResult", BindingFlags.Static);
                if (method == null) throw new Exception("Method(ValueTask.FromResult<>) not found");

                result = Expression.Call(method.MakeGenericMethod(typeof((int, string, ReadOnlyMemory<byte>))), newResult);
            }

            return Expression.Lambda<SendPacketDelegate>(result, [bot, cmd, sequence, dataInput]).Compile();
        }
    });

    public static ValueTask<(int RetCode, string Extra, ReadOnlyMemory<byte> Data)> SendPacket(this BotContext bot, string cmd, int sequence, ReadOnlyMemory<byte> data)
    {
        return _sendPacket.Value(bot, cmd, sequence, data);
    }
}