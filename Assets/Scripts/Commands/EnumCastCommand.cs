using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands
{
    public class EnumCastCommand
    {
        public TEnum EnumToEnum<TEnum, TValue>(TValue changeType) where TEnum : struct, Enum
        {
            if (Enum.TryParse(typeof(TEnum), changeType.ToString(), out var result))
            {
                return (TEnum)result;
            }
            else
            {
                return default;
            }
        }

        public TEnum StringToEnum<TEnum>(string value) where TEnum : struct, Enum
        {
            if (Enum.TryParse(value, out TEnum result))
            {
                return result;
            }
            else
            {
                return default;
            }
        }
    }
}
