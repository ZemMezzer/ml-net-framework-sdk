using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Llama.Grammar.Core
{
    internal static class JsonSchemaToGbnf
    {
        private static readonly string EbmpBase =
            """
            value  ::= (object | array | string | number | boolean | null) ws

            object ::=
              "{" ws (
                string ":" ws value
                ("," ws string ":" ws value)*
              )? "}"

            array  ::=
              "[" ws01 (
                        value
                ("," ws01 value)*
              )? "]"

            string ::=
              "\"" (string-char)* "\""

            string-char ::= [^"\\] | "\\" (["\\/bfnrt] | "u" [0-9a-fA-F] [0-9a-fA-F] [0-9a-fA-F] [0-9a-fA-F]) # escapes

            number ::= integer ("." [0-9]+)? ([eE] [-+]? [0-9]+)?
            integer ::= "-"? ([0-9] | [1-9] [0-9]*)
            boolean ::= "true" | "false"
            null ::= "null"

            # Optional space: by convention, applied in this grammar after literal chars when allowed
            ws ::= ([ \t\n] ws)?
            ws01 ::= ([ \t\n])?
            """;

        internal static string Convert(string jsonSchema)
        {
            var root = JToken.Parse(jsonSchema);

            var rules = new Dictionary<string, string>();
            var sb = new StringBuilder();

            void Traverse(JToken schema, string pointer, JToken? parent = null, string? parentKey = null)
            {
                var name = JsonPointerToName(pointer);

                string FormatPropertyName(string? key) =>
                    key is null ? "" : $"\"\\\"{key}\\\"\" ws01 \":\" ws01 ";

                bool IsNullable(JToken s) =>
                    s["nullable"]?.Type == JTokenType.Boolean &&
                    s["nullable"]!.Value<bool>();

                string WrapNullable(string g) =>
                    IsNullable(schema) ? $"({g} | null)" : g;

                bool IsRequired(string prop) =>
                    schema["required"] is JArray req &&
                    req.Any(e => e.Type == JTokenType.String && e.Value<string>() == prop);

                string Alt(IEnumerable<string> vs) =>
                    "(" + string.Join(" | ", vs) + ")";

                string Lit(JToken v)
                {
                    return v.Type switch
                    {
                        JTokenType.String => $"\"\\\"{v.Value<string>()}\\\"\"",
                        JTokenType.Integer or JTokenType.Float =>
                            $"\"{v.ToString(Formatting.None)}\"",
                        JTokenType.Boolean =>
                            $"\"{v.ToString(Formatting.None).ToLowerInvariant()}\"",
                        _ => throw new InvalidOperationException()
                    };
                }

                string? TryFormat(JToken s, string ptr)
                {
                    var typeTok = s["type"];

                    if (typeTok != null && typeTok.Type == JTokenType.String)
                    {
                        var type = typeTok.Value<string>();

                        // -------- object --------
                        if (type == "object")
                        {
                            var props = s["properties"] as JObject;
                            if (props == null)
                                return "object";

                            var parts = props.Properties()
                                .Select((p, i) =>
                                {
                                    var subPtr = ptr + "/properties/" + p.Name;
                                    var ruleName = JsonPointerToName(subPtr);

                                    Traverse(p.Value, subPtr, s, p.Name);

                                    var core = ruleName;
                                    if (!IsRequired(p.Name))
                                        core = $"({core})?";

                                    return (i == 0 ? "" : "\" ,\" ws01 ") + core;
                                });

                            return $"\"{{\" ws01 {string.Concat(parts)} \"}}\"";
                        }

                        // -------- array --------
                        if (type == "array")
                        {
                            var items = s["items"];
                            if (items == null)
                                throw new InvalidOperationException("Array schema without items");

                            var itemPtr = ptr + "/items";
                            var itemName = JsonPointerToName(itemPtr);

                            Traverse(items, itemPtr);

                            int? min = s["minItems"]?.Value<int>();
                            int? max = s["maxItems"]?.Value<int>();

                            string Repeat()
                            {
                                if (min.HasValue && max.HasValue)
                                {
                                    var first = string.Join(" ,\" ws01 ",
                                        Enumerable.Repeat(itemName, min.Value));

                                    var opt = string.Join(" ",
                                        Enumerable.Repeat($"(\",\" ws01 {itemName})?",
                                            max.Value - min.Value));

                                    return $"{first} {opt}";
                                }

                                if (min.HasValue)
                                {
                                    return string.Join(" ,\" ws01 ",
                                               Enumerable.Repeat(itemName, min.Value))
                                           + " (\",\" ws01 " + itemName + ")*";
                                }

                                if (max.HasValue)
                                {
                                    return $"({itemName})? "
                                           + string.Join(" ",
                                               Enumerable.Repeat($"(\",\" ws01 {itemName})?",
                                                   max.Value - 1));
                                }

                                return $"{itemName} ( ws01 \",\" ws01 {itemName})*";
                            }

                            return $"\"[\" ws01 {Repeat()} ws01 \"]\"";
                        }

                        // -------- primitives --------
                        if (type is "string" or "number" or "integer" or "boolean" or "null")
                        {
                            var enm = s["enum"] as JArray;
                            if (enm != null)
                            {
                                var lits = enm.Select(Lit);
                                return Alt(lits);
                            }

                            var pat = s["pattern"];
                            if (pat != null && pat.Type == JTokenType.String)
                            {
                                return $"\"\\\"\" {ConvertRegexpToGbnf(pat.Value<string>())} \"\\\"\"";
                            }

                            if (type == "string" &&
                                (s["minLength"] != null || s["maxLength"] != null))
                            {
                                return $"\"\\\"\" /* length logic here */ \"\\\"\"";
                            }

                            return type;
                        }
                    }

                    // -------- anyOf --------
                    var anyOf = s["anyOf"] as JArray;
                    if (anyOf != null)
                    {
                        var opts = anyOf
                            .Select(x => x["type"]?.Value<string>())
                            .Where(x => x != null)!;

                        return Alt(opts!);
                    }

                    // -------- const --------
                    var cst = s["const"];
                    if (cst != null)
                    {
                        return Lit(cst);
                    }

                    return null;
                }

                if (!rules.ContainsKey(name))
                {
                    var formatted = TryFormat(schema, pointer);
                    if (formatted != null)
                        rules[name] = FormatPropertyName(parentKey) + WrapNullable(formatted);
                }

                // ---- traverse children ----

                if (schema["properties"] is JObject propsObj)
                {
                    foreach (var p in propsObj.Properties())
                        Traverse(p.Value, pointer + "/properties/" + p.Name, schema, p.Name);
                }

                if (schema["items"] is JToken itemsTok)
                {
                    Traverse(itemsTok, pointer + "/items", schema);
                }

                if (schema["anyOf"] is JArray anyOfArr)
                {
                    for (int i = 0; i < anyOfArr.Count; i++)
                        Traverse(anyOfArr[i], pointer + $"/anyOf/{i}", schema);
                }
            }

            Traverse(root, "");

            rules["root"] += " ws01";

            var outSb = new StringBuilder();
            foreach (var kv in rules)
                outSb.AppendLine($"{kv.Key} ::= {kv.Value}");

            outSb.AppendLine();
            outSb.Append(EbmpBase.TrimStart());

            return outSb.ToString();
        }

        private static string JsonPointerToName(string ptr)
        {
            if (string.IsNullOrEmpty(ptr))
                return "root";

            return "root" +
                   Regex.Replace(
                       ptr.Replace("/properties", ""),
                       @"[^a-zA-Z0-9-]+", "-");
        }

        private static string ConvertRegexpToGbnf(string pattern)
            => RegexToGbnf.Convert(pattern);
    }
}