using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.NativeAPI.NativeModel.Common;

namespace Lagrange.Core.NativeAPI.NativeModel.Message
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotGroupMemberStruct
    {
        public BotGroupMemberStruct() { }

        public BotGroupStruct BotGroup = new();
        
        public long Uin = 0;

        public ByteArrayNative Uid = new();

        public ByteArrayNative Nickname = new();

        public int Age = 0;

        public int Gender = 0;

        public int Permission = 0;

        public int GroupLevel = 0;

        public ByteArrayNative MemberCard = new();

        public ByteArrayNative SpecialTitle = new();

        public ByteArrayNative JoinTime = new();

        public ByteArrayNative LastMsgTime = new();

        public ByteArrayNative ShutUpTimestamp = new();


        public static implicit operator BotGroupMember(BotGroupMemberStruct member)
        {
            return new BotGroupMember(
                member.BotGroup,
                member.Uin,
                Encoding.UTF8.GetString(member.Uid),
                Encoding.UTF8.GetString(member.Nickname),
                (GroupMemberPermission)member.Permission,
                member.GroupLevel,
                Encoding.UTF8.GetString(member.MemberCard),
                Encoding.UTF8.GetString(member.SpecialTitle),
                DateTime.ParseExact(
                    Encoding.UTF8.GetString(member.JoinTime),
                    "O",
                    CultureInfo.InvariantCulture
                ),
                DateTime.ParseExact(
                    Encoding.UTF8.GetString(member.LastMsgTime),
                    "O",
                    CultureInfo.InvariantCulture
                ),
                DateTime.ParseExact(
                    Encoding.UTF8.GetString(member.ShutUpTimestamp),
                    "O",
                    CultureInfo.InvariantCulture
                )
            );
        }

        public static implicit operator BotGroupMemberStruct(BotGroupMember member)
        {
            return new BotGroupMemberStruct()
            {
                BotGroup = member.Group,
                Uin = member.Uin,
                Uid = Encoding.UTF8.GetBytes(member.Uid),
                Nickname = Encoding.UTF8.GetBytes(member.Nickname),
                Age = member.Age,
                Gender = (int)member.Gender,
                Permission = (int)member.Permission,
                GroupLevel = member.GroupLevel,
                MemberCard = Encoding.UTF8.GetBytes(member.MemberCard ?? string.Empty),
                SpecialTitle = Encoding.UTF8.GetBytes(member.SpecialTitle ?? string.Empty),
                JoinTime = Encoding.UTF8.GetBytes(member.JoinTime.ToString("O")),
                LastMsgTime = Encoding.UTF8.GetBytes(member.LastMsgTime.ToString("O")),
                ShutUpTimestamp = Encoding.UTF8.GetBytes(member.ShutUpTimestamp.ToString("O"))
            };
        }
    }
}
