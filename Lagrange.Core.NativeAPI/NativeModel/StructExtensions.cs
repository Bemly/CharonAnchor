using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.NativeAPI.NativeModel.Common;
using Lagrange.Core.NativeAPI.NativeModel.Context;
using Lagrange.Core.NativeAPI.NativeModel.Event;

namespace Lagrange.Core.NativeAPI.NativeModel
{
    public static class StructExtensions
    {
        #region Context

        #region Dictionary

        public static KeyValuePairNative<T1, T2>[] ToDictionaryStruct<T1, T2>(
            this Dictionary<T1, T2> dict
        )
            where T1 : notnull
        {
            var result = new KeyValuePairNative<T1, T2>[dict.Count];
            int i = 0;
            foreach (var kvp in dict)
            {
                result[i++] = new KeyValuePairNative<T1, T2>() { Key = kvp.Key, Value = kvp.Value };
            }

            return result;
        }

        public static Dictionary<T1, T2> ToDictionary<T1, T2>(
            this KeyValuePairNative<T1, T2>[] dict
        )
            where T1 : notnull
        {
            var result = new Dictionary<T1, T2>(dict.Length);
            foreach (var kvp in dict)
            {
                result[kvp.Key] = kvp.Value;
            }

            return result;
        }

        #endregion

        #region BotConfig

        public static BotConfigStruct ToStruct(this BotConfig config)
        {
            return new BotConfigStruct()
            {
                Protocol = (byte)config.Protocol,
                AutoReconnect = config.AutoReconnect,
                UseIPv6Network = config.UseIPv6Network,
                GetOptimumServer = config.GetOptimumServer,
                HighwayChunkSize = config.HighwayChunkSize,
                HighwayConcurrent = config.HighwayConcurrent,
                AutoReLogin = config.AutoReLogin
            };
        }

        public static BotConfig ToConfig(this BotConfigStruct config)
        {
            return new BotConfig()
            {
                Protocol = (Protocols)config.Protocol,
                AutoReconnect = config.AutoReconnect,
                UseIPv6Network = config.UseIPv6Network,
                GetOptimumServer = config.GetOptimumServer,
                HighwayChunkSize = config.HighwayChunkSize,
                HighwayConcurrent = config.HighwayConcurrent,
                AutoReLogin = config.AutoReLogin
            };
        }

        #endregion

        #region BotKeystore

        public static BotKeystoreStruct ToStruct(this BotKeystore keystore)
        {
            var bytePsKey = new KeyValuePairNative<byte[], byte[]>[keystore.WLoginSigs.PsKey.Count];
            int i = 0;
            foreach (var kvp in keystore.WLoginSigs.PsKey)
            {
                bytePsKey[i++] = new KeyValuePairNative<byte[], byte[]>()
                {
                    Key = Encoding.UTF8.GetBytes(kvp.Key),
                    Value = Encoding.UTF8.GetBytes(kvp.Value)
                };
            }

            return new BotKeystoreStruct()
            {
                Uin = keystore.Uin,
                Uid = Encoding.UTF8.GetBytes(keystore.Uid),
                Guid = keystore.Guid,
                AndroidId = Encoding.UTF8.GetBytes(keystore.AndroidId),
                Qimei = Encoding.UTF8.GetBytes(keystore.Qimei),
                DeviceName = Encoding.UTF8.GetBytes(keystore.DeviceName),
                A2 = keystore.WLoginSigs.A2,
                A2Key = keystore.WLoginSigs.A2Key,
                D2 = keystore.WLoginSigs.D2,
                D2Key = keystore.WLoginSigs.D2Key,
                A1 = keystore.WLoginSigs.A1,
                A1Key = keystore.WLoginSigs.A1Key,
                NoPicSig = keystore.WLoginSigs.NoPicSig,
                TgtgtKey = keystore.WLoginSigs.TgtgtKey,
                Ksid = keystore.WLoginSigs.Ksid,
                SuperKey = keystore.WLoginSigs.SuperKey,
                StKey = keystore.WLoginSigs.StKey,
                StWeb = keystore.WLoginSigs.StWeb,
                St = keystore.WLoginSigs.St,
                WtSessionTicket = keystore.WLoginSigs.WtSessionTicket,
                WtSessionTicketKey = keystore.WLoginSigs.WtSessionTicketKey,
                RandomKey = keystore.WLoginSigs.RandomKey,
                SKey = keystore.WLoginSigs.SKey,
                PsKey = bytePsKey
            };
        }

        public static BotKeystore ToKeystore(this BotKeystoreStruct keystore)
        {
            var psKey = new Dictionary<string, string>();
            foreach (var kvp in keystore.PsKey)
            {
                psKey[Encoding.UTF8.GetString(kvp.Key)] = Encoding.UTF8.GetString(kvp.Value);
            }

            return new BotKeystore()
            {
                Uin = keystore.Uin,
                Uid = Encoding.UTF8.GetString(keystore.Uid),
                Guid = keystore.Guid,
                AndroidId = Encoding.UTF8.GetString(keystore.AndroidId),
                Qimei = Encoding.UTF8.GetString(keystore.Qimei),
                DeviceName = Encoding.UTF8.GetString(keystore.DeviceName),
                WLoginSigs = new WLoginSigs()
                {
                    A2 = keystore.A2,
                    A2Key = keystore.A2Key,
                    D2 = keystore.D2,
                    D2Key = keystore.D2Key,
                    A1 = keystore.A1,
                    A1Key = keystore.A1Key,
                    NoPicSig = keystore.NoPicSig,
                    TgtgtKey = keystore.TgtgtKey,
                    Ksid = keystore.Ksid,
                    SuperKey = keystore.SuperKey,
                    StKey = keystore.StKey,
                    StWeb = keystore.StWeb,
                    St = keystore.St,
                    WtSessionTicket = keystore.WtSessionTicket,
                    WtSessionTicketKey = keystore.WtSessionTicketKey,
                    RandomKey = keystore.RandomKey,
                    SKey = keystore.SKey,
                    PsKey = psKey
                }
            };
        }

        #endregion

        #endregion

        #region Event
        
        #region BotCaptchaEvent
        
        public static BotCaptchaEvent ToEvent(this BotCaptchaEventStruct e)
        {
            return new BotCaptchaEvent(
                Encoding.UTF8.GetString(e.CaptchaUrl)
            );
        }
        
        public static BotCaptchaEventStruct ToStruct(this BotCaptchaEvent e)
        {
            return new BotCaptchaEventStruct()
            {
                CaptchaUrl = Encoding.UTF8.GetBytes(e.CaptchaUrl)
            };
        }
        
        #endregion

        #region BotLoginEvent

        public static BotLoginEvent ToEvent(this BotLoginEventStruct e)
        {
            return new BotLoginEvent(
                e.State,
                (Encoding.UTF8.GetString(e.Tag), Encoding.UTF8.GetString(e.Message))
            );
        }

        public static BotLoginEventStruct ToStruct(this BotLoginEvent e)
        {
            return new BotLoginEventStruct()
            {
                State = e.State,
                Tag = Encoding.UTF8.GetBytes(e.Error?.Tag ?? string.Empty),
                Message = Encoding.UTF8.GetBytes(e.Error?.Message ?? string.Empty)
            };
        }

        #endregion

        #region BotLogEvent
        
        public static BotLogEvent ToEvent(this BotLogEventStruct e)
        {
            return new BotLogEvent(
                Encoding.UTF8.GetString(e.Tag),
                (LogLevel)e.Level,
                Encoding.UTF8.GetString(e.Message)
            );
        }

        public static BotLogEventStruct ToStruct(this BotLogEvent e)
        {
            return new BotLogEventStruct()
            {
                Level = (int)e.Level,
                Tag = Encoding.UTF8.GetBytes(e.Tag),
                Message = Encoding.UTF8.GetBytes(e.Message)
            };
        }

        #endregion

        #endregion
    }
}
