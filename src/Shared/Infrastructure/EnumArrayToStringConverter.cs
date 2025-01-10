using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shared.Infrastructure;

public class EnumArrayToStringConverter<TEnum> : ValueConverter<TEnum[], string> where TEnum : struct, Enum
{
    public EnumArrayToStringConverter() : base(
        v => string.Join(',', v.Select(x => x.ToString())), // Enum Array to String
        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => Enum.Parse<TEnum>(x)).ToArray()) // String to Enum Array
    {}
}