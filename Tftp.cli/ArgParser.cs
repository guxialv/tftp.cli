using System;
using System.Collections.Generic;

namespace Tftp
{
    internal class ArgParser
    {
        internal const string KEY_GEN = "--Gen";
        internal const string KEY_RUN = "--Run";
        //internal const string KEY_VERSION = "--Version";
        //internal const string KEY_HELP = "?";

        private IReadOnlyDictionary<string, string> _ValueByKeyDictionary;
        public ArgParser(string[] args)
        {
            _ValueByKeyDictionary = BuildValueByKeyDictionary(args);
        }

        public bool HasKey(string key)
        {
            return _ValueByKeyDictionary.ContainsKey(key);
        }

        public string GetValue(string key, string defaultValue = null)
        {
            if (_ValueByKeyDictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        private Dictionary<string, string> BuildValueByKeyDictionary(string[] args)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                dict[args[i]] = (i + 1) >= args.Length ? null : args[i + 1];
                i++;
            }
            return dict;
        }
    }
}
