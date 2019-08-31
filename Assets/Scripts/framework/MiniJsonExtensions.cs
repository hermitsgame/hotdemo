
using System;
using System.Collections;
using System.Collections.Generic;

namespace BoBao.Framework
{
    public static class MiniJsonExtensions
    {
        public static string toJson(this Hashtable obj)
        {
            return MiniJSON.jsonEncode(obj);
        }

        public static string toJson(this Dictionary<string, string> obj)
        {
            return MiniJSON.jsonEncode(obj);
        }

        public static ArrayList arrayListFromJson(this string json)
        {
            return MiniJSON.jsonDecode(json) as ArrayList;
        }

        public static Hashtable ToHashtable(this string json)
        {
            return MiniJSON.jsonDecode(json) as Hashtable;
        }
    }
}

