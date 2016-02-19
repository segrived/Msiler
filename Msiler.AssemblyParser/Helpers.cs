using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Msiler.AssemblyParser
{
    public class OpCodeInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public static class Helpers
    {
        private static Dictionary<string, OpCodeInfo> ReadOpCodeInfoResource() {
            var reader = new StringReader(Resources.Instructions);
            var serializer = new XmlSerializer(typeof(List<OpCodeInfo>));
            var list = (List<OpCodeInfo>)serializer.Deserialize(reader);
            return list.ToDictionary(i => i.Name, i => i);
        }

        private static readonly Dictionary<string, OpCodeInfo> OpCodesInfoCache
            = ReadOpCodeInfoResource();

        public static List<string> GetOpCodesList() {
            return OpCodesInfoCache.Select(i => i.Key).ToList();
        }

        public static OpCodeInfo GetInstructionInformation(string s) {
            var instruction = s.ToLower();
            if (OpCodesInfoCache.ContainsKey(instruction)) {
                return OpCodesInfoCache[instruction];
            }
            return null;
        }

        public static string ReplaceNewLineCharacters(this string str) {
            return str.Replace("\n", @"\n").Replace("\r", @"\r");
        }

        public static string ReplaceLastOccurrence(this string input, string substr, string replacement) {
            var lastSlash = input.LastIndexOf(substr, StringComparison.Ordinal);
            if (lastSlash != -1) {
                var sb = new StringBuilder();
                sb.Append(input.Substring(0, lastSlash));
                sb.Append(replacement);
                sb.Append(input.Substring(lastSlash + 1));
                return sb.ToString();
            }
            return input;
        }
    }
}
