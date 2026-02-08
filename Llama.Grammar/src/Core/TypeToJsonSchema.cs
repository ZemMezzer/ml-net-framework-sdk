using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Llama.Grammar.Core
{
    internal static class TypeToJsonSchema
    {
        internal static string Convert<T>()
        {
            var schema = new JObject
            {
                ["$schema"] = "http://json-schema.org/draft-07/schema#",
                ["type"] = "object",
                ["properties"] = GenerateProperties(typeof(T)),
                ["required"] = GenerateRequired(typeof(T))
            };

            return schema.ToString(Formatting.None);
        }

        private static JObject GenerateProperties(Type type)
        {
            var properties = new JObject();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                properties[prop.Name] = GeneratePropertySchema(prop);
            }

            return properties;
        }

        private static JArray GenerateRequired(Type type)
        {
            var required = new JArray();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var underlying = Nullable.GetUnderlyingType(prop.PropertyType);
                var propType = underlying ?? prop.PropertyType;

                // Сохраняю твою исходную логику:
                // required добавляется для value-type (кроме bool) — но не для nullable.
                if (underlying == null && prop.PropertyType.IsValueType && propType != typeof(bool))
                {
                    required.Add(prop.Name);
                }
            }

            return required;
        }

        private static JObject GeneratePropertySchema(PropertyInfo prop)
        {
            var schema = new JObject();
            var underlying = Nullable.GetUnderlyingType(prop.PropertyType);
            var propType = underlying ?? prop.PropertyType;

            if (propType.IsEnum)
            {
                schema["type"] = "string";

                var enumValues = new JArray();
                foreach (var name in Enum.GetNames(propType))
                    enumValues.Add(name);

                schema["enum"] = enumValues;
            }
            else if (propType == typeof(string))
            {
                schema["type"] = "string";
            }
            else if (propType == typeof(int) || propType == typeof(long))
            {
                schema["type"] = "integer";
            }
            else if (propType == typeof(float) || propType == typeof(double) || propType == typeof(decimal))
            {
                schema["type"] = "number";
            }
            else if (propType == typeof(bool))
            {
                schema["type"] = "boolean";
            }
            else if (propType == typeof(DateTime))
            {
                schema["type"] = "string";
                schema["format"] = "date-time";
            }
            else if (typeof(IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
            {
                schema["type"] = "array";

                var elementType =
                    propType.IsArray
                        ? propType.GetElementType()
                        : propType.GetGenericArguments().FirstOrDefault();

                JObject itemSchema;

                if (elementType == null)
                {
                    // fallback если не смогли определить T
                    itemSchema = new JObject { ["type"] = "string" };
                }
                else if (elementType.IsClass && elementType != typeof(string))
                {
                    // В оригинале у тебя тут была небольшая логическая ошибка:
                    // itemSchema = GenerateProperties(elementType); itemSchema["type"]="object"
                    // Это превращает "properties" в корневой объект, а не в поле "properties".
                    // Я оставлю ПРАВИЛЬНЫЙ вариант (иначе schema получится некорректной):
                    itemSchema = new JObject
                    {
                        ["type"] = "object",
                        ["properties"] = GenerateProperties(elementType),
                        ["required"] = GenerateRequired(elementType)
                    };
                }
                else
                {
                    itemSchema = new JObject
                    {
                        ["type"] = GetJsonType(elementType)
                    };

                    if (elementType.IsEnum)
                    {
                        var enumValues = new JArray();
                        foreach (var name in Enum.GetNames(elementType))
                            enumValues.Add(name);

                        itemSchema["enum"] = enumValues;
                    }
                }

                schema["items"] = itemSchema;
            }
            else if (propType.IsClass)
            {
                schema["type"] = "object";
                schema["properties"] = GenerateProperties(propType);
                schema["required"] = GenerateRequired(propType);
            }
            else
            {
                // fallback на случай, если тип не распознан (как в GetJsonType)
                schema["type"] = GetJsonType(propType);
            }

            if (underlying != null)
            {
                schema["nullable"] = true;
            }

            return schema;
        }

        private static string GetJsonType(Type type)
        {
            if (type == typeof(string) || type.IsEnum)
                return "string";

            if (type == typeof(int) || type == typeof(long))
                return "integer";

            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return "number";

            if (type == typeof(bool))
                return "boolean";

            if (type == typeof(DateTime))
                return "string";

            if (type.IsClass)
                return "object";

            return "string";
        }
    }
}
