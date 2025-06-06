using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Limekuma.Utils;

public class Optional<TA, TB>
{
    private readonly TA? _valueA;
    private readonly TB? _valueB;

    public Optional(TA? value) => _valueA = value;
    public Optional(TB? value) => _valueB = value;

    public object? Value
    {
        get
        {
            if (_valueA is not null)
            {
                return _valueA;
            }

            if (_valueB is not null)
            {
                return _valueB;
            }

            return null;
        }
    }

    public static implicit operator TA?(Optional<TA, TB> o) => o._valueA;

    public static implicit operator TB?(Optional<TA, TB> o) => o._valueB;

    public static implicit operator Optional<TA, TB>(TA a) => new(a);

    public static implicit operator Optional<TA, TB>(TB b) => new(b);

    public new Type? GetType() => Value?.GetType();

    public new string? ToString() => Value?.ToString();

    public new bool? Equals(object? obj) => Value?.Equals(obj);

    public new int? GetHashCode() => Value?.GetHashCode();
}

public class OptionalConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<,>);

    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        Type[] typeArguments = type.GetGenericArguments();
        Type valueAType = typeArguments[0];
        Type valueBType = typeArguments[1];

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(OptionalConverterInner<,>).MakeGenericType(valueAType, valueBType),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            [options],
            null)!;

        return converter;
    }

    private class OptionalConverterInner<TA, TB>(JsonSerializerOptions options) : JsonConverter<Optional<TA, TB>>
    {
        private readonly JsonConverter<TA> _valueAConverter = (JsonConverter<TA>)options.GetConverter(typeof(TA));
        private readonly Type _valueAType = typeof(TA);
        private readonly JsonConverter<TB> _valueBConverter = (JsonConverter<TB>)options.GetConverter(typeof(TB));
        private readonly Type _valueBType = typeof(TB);

        public override Optional<TA, TB> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            try
            {
                TA? valueA = JsonSerializer.Deserialize<TA>(ref reader, options);
                return new(valueA);
            }
            catch (JsonException)
            {
            }

            try
            {
                TB? valueB = JsonSerializer.Deserialize<TB>(ref reader, options);
                return new(valueB);
            }
            catch (JsonException)
            {
            }

            throw new JsonException($"Unable to deserialize value as either {_valueAType} or {_valueBType}");
        }

        public override void Write(Utf8JsonWriter writer, Optional<TA, TB> options1, JsonSerializerOptions options)
        {
            object? value = options1.Value;
            if (value is TA valueA)
            {
                _valueAConverter.Write(writer, valueA, options);
                return;
            }

            if (value is TB valueB)
            {
                _valueBConverter.Write(writer, valueB, options);
                return;
            }

            throw new JsonException($"Value is neither {_valueAType} nor {_valueBType}");
        }
    }
}