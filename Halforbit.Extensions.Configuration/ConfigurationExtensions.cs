using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Halforbit.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static TConfig ToObject<TConfig>(
            this IConfigurationSection configuration)
        {
            var root = new JObject();

            foreach (var kv in configuration.AsEnumerable())
            {
                if (kv.Value == null) continue;

                var segments = kv.Key
                    .Substring(configuration.Path.Length)
                    .Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                if (!segments.Any()) continue;

                var scope = root as JToken;

                for (var i = 0; i < segments.Count - 1; i++)
                {
                    var segment = segments[i];

                    if (int.TryParse(segment, out var segmentIndex))
                    {
                        var scopeJArray = scope as JArray;

                        var subJObject = scopeJArray[segmentIndex];

                        if (subJObject is JValue subJValue && subJValue.Value == null)
                        {
                            if (int.TryParse(segments[i + 1], out var topIndex))
                            {
                                subJObject = new JArray(Enumerable.Repeat(default(object), topIndex + 1));
                            }
                            else
                            {
                                subJObject = new JObject();
                            }

                            scopeJArray[segmentIndex] = subJObject;
                        }

                        scope = subJObject;
                    }
                    else
                    {
                        var scopeJObject = scope as JObject;

                        var subJObject = scopeJObject[segment];

                        if (subJObject == null)
                        {
                            if (int.TryParse(segments[i + 1], out var topIndex))
                            {
                                subJObject = new JArray(Enumerable.Repeat(default(object), topIndex + 1));
                            }
                            else
                            {
                                subJObject = new JObject();
                            }

                            scopeJObject[segment] = subJObject;
                        }

                        scope = subJObject;
                    }
                }

                var leaf = segments.Last();

                if (int.TryParse(leaf, out var leafIndex))
                {
                    var scopeJArray = scope as JArray;

                    scopeJArray[leafIndex] = kv.Value;
                }
                else
                {
                    var scopeJObject = scope as JObject;

                    scopeJObject[leaf] = kv.Value;
                }
            }

            return root.ToObject<TConfig>();
        }
    }
}
