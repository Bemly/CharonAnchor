using System.Data.Common;
using Dapper;
using Lagrange.Core;
using Lagrange.Core.Message;
using Microsoft.Extensions.Logging;

namespace Lagrange.OneBot.Database;

[DapperAot]
public partial class StorageService
{
    private readonly ILogger<StorageService> _logger;
    
    private readonly BotContext _context;
    
    private readonly DbConnection _database;

    public StorageService(ILogger<StorageService> logger, BotContext context, DbConnection database)
    {
        _logger = logger;
        _context = context;
        _database = database;
        
        _database.Execute(MessageRecord.CreateStatement);
    }

    public async Task SaveMessage(BotMessage message)
    {
        var record = new MessageRecord
        {
            SelfUin = _context.BotUin,
            MessageId = CalcMessageHash(message.MessageId, message.Sequence),
            Sequence = message.Sequence,
            ClientSequence = message.ClientSequence,
            Random = message.Random,
            ContactUin = message.Contact.Uin,
            GroupUin = message.Group?.GroupUin ?? 0,
            Data = []
        };
        
        const string sql = "INSERT INTO MessageRecord VALUES (@SelfUin, @MessageId, @Sequence, @ClientSequence, @ContactUin, @GroupUin, @Random, @Data)";
        await _database.ExecuteAsync(sql, record);
        Logger.StorageServiceInfo(_logger, record.MessageId, record.ContactUin, record.GroupUin);
    }
    
    public async Task<BotMessage?> GetMessage(ulong messageId, int seq)
    {
        const string sql = "SELECT * FROM MessageRecord WHERE SelfUin = @SelfUin AND MessageId = @MessageId";
        var record = await _database.QuerySingleOrDefaultAsync<MessageRecord>(sql, new { SelfUin = _context.BotUin, MessageId = CalcMessageHash(messageId, seq) });
        if (record == null) return null;

        return _context.MessagePacker.Parse(record.Data);
    }
    
    public async Task<BotMessage?> GetMessageBySequence(long groupUin, int seq)
    {
        const string sql = "SELECT * FROM MessageRecord WHERE SelfUin = @SelfUin AND GroupUin = @GroupUin AND Sequence = @Sequence";
        var record = await _database.QuerySingleOrDefaultAsync<MessageRecord>(sql, new { SelfUin = _context.BotUin, GroupUin = groupUin, Sequence = seq });
        if (record == null) return null;

        return _context.MessagePacker.Parse(record.Data);
    }
    
    public static int CalcMessageHash(ulong msgId, int seq) => ((ushort)seq << 16) | (ushort)msgId;
    
    private static partial class Logger 
    {
        [LoggerMessage(0, LogLevel.Debug, "Message {MessageId} from {ContactUin} to {GroupUin} saved", EventName = "MessageSaved")]
        public static partial void StorageServiceInfo(ILogger logger, int messageId, long contactUin, long groupUin);
    }
}