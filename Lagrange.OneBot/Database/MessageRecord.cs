namespace Lagrange.OneBot.Database;

[Serializable]
public class MessageRecord
{
    public const string CreateStatement =  """
                                             CREATE TABLE IF NOT EXISTS `MessageRecord` (
                                                        `SelfUin` INTEGER NOT NULL,
                                                        `MessageId` INTEGER NOT NULL,
                                                        `ContactUin` INTEGER NOT NULL,
                                                        `GroupUin` INTEGER NOT NULL,
                                                        `Sequence` INTEGER NOT NULL,
                                                        `ClientSequence` INTEGER NOT NULL,
                                                        `Random` INTEGER NOT NULL,
                                                        `Data` BLOB NOT NULL,
                                                        PRIMARY KEY (`SelfUin`, `MessageId`)
                                                 );
                                             CREATE INDEX IF NOT EXISTS `MessageRecordIndex` ON `MessageRecord` (
                                                 `SelfUin`,
                                                 `ContactUin`,
                                                 `GroupUin`,
                                                 `Sequence`
                                             );
                                             """;
    
    public long SelfUin { get; set; }
    
    public int MessageId { get; set; }
    
    public long ContactUin { get; set; }
    
    public long GroupUin { get; set; }
    
    public int Sequence { get; set; }
    
    public int ClientSequence { get; set; }
    
    public int Random { get; set; }
    
    public byte[] Data { get; set; } = [];
}