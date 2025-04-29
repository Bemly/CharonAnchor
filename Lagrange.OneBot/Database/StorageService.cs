using System.Data.Common;
using Dapper;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Message;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
        _database.Execute(FileRecord.CreateStatement);
    }

    public async Task SaveMessage(BotMessageEvent @event)
    {
        var message = @event.Message;
        var record = new MessageRecord
        {
            SelfUin = _context.BotUin,
            MessageId = CalcMessageHash(message.MessageId, message.Sequence),
            Sequence = message.Sequence,
            ClientSequence = message.ClientSequence,
            ContactUin = 0,
            GroupUin = (message.Contact as BotGroupMember)?.Group.GroupUin ?? 0,
            Random = message.Random,
            Data = @event.RawMessage.ToArray()
        };
        
        const string sql = "INSERT INTO MessageRecord VALUES (@SelfUin, @MessageId, @ContactUin, @GroupUin, @Sequence, @ClientSequence, @Random, @Data)";
        await _database.ExecuteAsync(sql, record);
        Logger.StorageServiceInfo(_logger, record.MessageId, record.ContactUin, record.GroupUin);
    }
    
    public async Task<BotMessage?> GetMessage(ulong messageId, int seq)
    {
        const string sql = "SELECT * FROM MessageRecord WHERE SelfUin = @SelfUin AND MessageId = @MessageId";
        var record = await _database.QuerySingleOrDefaultAsync<MessageRecord>(sql, new { SelfUin = _context.BotUin, MessageId = CalcMessageHash(messageId, seq) });
        if (record == null) return null;

        return await _context.MessagePacker.Parse(record.Data);
    }
    
    public async Task<BotMessage?> GetMessageBySequence(long groupUin, int seq)
    {
        const string sql = "SELECT * FROM MessageRecord WHERE SelfUin = @SelfUin AND GroupUin = @GroupUin AND Sequence = @Sequence";
        var record = await _database.QuerySingleOrDefaultAsync<MessageRecord>(sql, new { SelfUin = _context.BotUin, GroupUin = groupUin, Sequence = seq });
        if (record == null) return null;

        return await _context.MessagePacker.Parse(record.Data);
    }
    
    public async Task<bool> ClearMessage()
    {
        const string sql = "TRUNCATE TABLE MessageRecord";
        await _database.ExecuteAsync(sql);
        return true;
    }
    
    public async Task SaveFile(string file, byte[] data)
    {
        var record = new FileRecord
        {
            SelfUin = _context.BotUin,
            File = file,
            Data = data
        };
        
        const string sql = "INSERT INTO FileRecord VALUES (@SelfUin, @File, @Data)";
        await _database.ExecuteAsync(sql, record);
    }
    
    public async Task<byte[]?> GetFile(string file)
    {
        const string sql = "SELECT * FROM FileRecord WHERE SelfUin = @SelfUin AND File = @File";
        var record = await _database.QuerySingleOrDefaultAsync<FileRecord>(sql, new { SelfUin = _context.BotUin, File = file });
        return record?.Data;
    }
    
    public async Task<bool> ClearFile()
    {
        const string sql = "TRUNCATE TABLE FileRecord";
        await _database.ExecuteAsync(sql);
        return true;
    }
    
    public static int CalcMessageHash(ulong msgId, int seq) => ((ushort)seq << 16) | (ushort)msgId;
    
    private static partial class Logger 
    {
        [LoggerMessage(0, LogLevel.Debug, "Message {MessageId} from {ContactUin} to {GroupUin} saved", EventName = "MessageSaved")]
        public static partial void StorageServiceInfo(ILogger logger, int messageId, long contactUin, long groupUin);
    }
}