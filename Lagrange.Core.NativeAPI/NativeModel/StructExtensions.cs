using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.NativeAPI.NativeModel.Common;
using Lagrange.Core.NativeAPI.NativeModel.Event;
using Lagrange.Core.NativeAPI.NativeModel.Message;
using Lagrange.Core.NativeAPI.NativeModel.Message.Entity;

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

        #endregion
    }
}
