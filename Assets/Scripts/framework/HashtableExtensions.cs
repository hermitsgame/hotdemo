
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoBao.Framework
{

    public static class HashtableExtensions
    {
        public static Hashtable GetHash(params object[] value)
        {
            Hashtable hashtable = new Hashtable();
            if (value.Length % 2 != 0)
            {
                UnityEngine.Debug.LogError("参数必须为2的倍数!");
                return hashtable;
            }
            int num = value.Length / 2;
            for (int i = 0; i < num; i++)
            {
                int num2 = i * 2;
                hashtable.Add(value[num2], value[num2 + 1]);
            }
            return hashtable;
        }

        public static int HashToInt(this Hashtable table, string key)
        {
            if (!table.ContainsKey(key))
            {
                return 0;
            }
            int result = 0;
            int.TryParse(table[key].ToString(), out result);
            return result;
        }

        public static float HashToFloat(this Hashtable table, object key)
        {
            float result = 0f;
            if (!table.ContainsKey(key))
            {
                return result;
            }
            float.TryParse(table[key].ToString(), out result);
            return result;
        }

        public static string HashToString(this Hashtable table, object key)
        {
            if (!table.ContainsKey(key))
            {
                return string.Empty;
            }
            if (table[key] == null)
            {
                return string.Empty;
            }
            return table[key].ToString();
        }

        public static Hashtable HashToHash(this Hashtable table, object key)
        {
            if (!table.ContainsKey(key))
            {
                return new Hashtable();
            }
            Hashtable hashtable = table[key] as Hashtable;
            if (hashtable == null)
            {
                return new Hashtable();
            }
            return hashtable;
        }

        public static int[] HashArrayListToIntArr(this Hashtable table, object key)
        {
            if (!table.ContainsKey(key))
            {
                return new int[0];
            }
            ArrayList arrayList = table[key] as ArrayList;
            if (arrayList == null)
            {
                return new int[0];
            }
            int[] array = new int[arrayList.Count];
            for (int i = 0; i < array.Length; i++)
            {
                int num = 0;
                int.TryParse(arrayList[i].ToString(), out num);
                array[i] = num;
            }
            return array;
        }

        public static bool[] HashArrayListToBoolArr(this Hashtable table, object key)
        {
            if (!table.ContainsKey(key))
                return new bool[0];

            ArrayList arrayList = table[key] as ArrayList;
            if (arrayList == null)
                return new bool[0];

            bool[] array = new bool[arrayList.Count];
            for (int i = 0; i < array.Length; i++)
            {
                var text = arrayList[i].ToString();
                array[i] = text.Equals("1") || text.Equals("true") || text.Equals("True");
            }

            return array;
        }

        public static string[] HashArrayListToStringArr(this Hashtable table, object key)
        {
            if (!table.ContainsKey(key))
            {
                return new string[0];
            }
            ArrayList arrayList = table[key] as ArrayList;
            if (arrayList == null)
            {
                return new string[0];
            }
            string[] array = new string[arrayList.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = arrayList[i].ToString();
            }
            return array;
        }

        public static int[,] HashToTowIntArr(this Hashtable table, object key)
        {
            if (!table.ContainsKey(key))
            {
                return new int[0, 0];
            }
            string text = table[key].ToString();
            string[] array = text.Split(new char[]
            {
            ';'
            }, StringSplitOptions.RemoveEmptyEntries);
            int num = array.Length;
            int[,] array2 = new int[num, 2];
            for (int i = 0; i < num; i++)
            {
                string[] array3 = array[i].Split(new char[]
                {
                ','
                }, StringSplitOptions.RemoveEmptyEntries);
                if (array3.Length != 2)
                {
                    array2[i, 0] = 0;
                    array2[i, 1] = 0;
                }
                else
                {
                    int num2 = 0;
                    int num3;
                    int.TryParse(array3[0], out num3);
                    int.TryParse(array3[1], out num2);
                    array2[i, 0] = num3;
                    array2[i, 1] = num2;
                }
            }
            return array2;
        }

        public static List<string[]> HashToTowString(this Hashtable table, object key)
        {
            if (!table.ContainsKey(key))
            {
                return new List<string[]>();
            }
            string text = table[key].ToString();
            string[] array = text.Split(new char[]
            {
            ';'
            }, StringSplitOptions.RemoveEmptyEntries);
            List<string[]> list = new List<string[]>();
            int num = array.Length;
            for (int i = 0; i < num; i++)
            {
                string[] item = array[i].Split(new char[]
                {
                ','
                }, StringSplitOptions.RemoveEmptyEntries);
                list.Add(item);
            }
            return list;
        }

        public static List<string[]> HashToTowStringByHash(this Hashtable table, object key)
        {
            List<string[]> list = new List<string[]>();
            UnityEngine.Debug.LogError(table.ContainsKey(key));
            if (!table.ContainsKey(key))
            {
                return list;
            }
            Hashtable hashtable = table.HashToHash(key);
            IDictionaryEnumerator enumerator = hashtable.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
                    list.Add(new string[]
                    {
                    dictionaryEntry.Key.ToString(),
                    dictionaryEntry.Value.ToString()
                    });
                }
            }
            finally
            {
                IDisposable disposable;
                if ((disposable = (enumerator as IDisposable)) != null)
                {
                    disposable.Dispose();
                }
            }
            return list;
        }

        public static List<int[]> HashToTowIntByHash(this Hashtable table, object key)
        {
            List<int[]> result = new List<int[]>();
            if (!table.ContainsKey(key))
            {
                return result;
            }
            return result;
        }

        public static string[] HashToStringArr(this Hashtable table, object key, char c = ',')
        {
            if (!table.ContainsKey(key))
            {
                return new string[0];
            }
            return table.HashToString(key).Split(new char[]
            {
            c
            }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static int[] HashToIntArr(this Hashtable table, object key, char c = ',')
        {
            if (!table.ContainsKey(key))
            {
                return new int[0];
            }
            string[] array = table.HashToString(key).Split(new char[]
            {
            c
            }, StringSplitOptions.RemoveEmptyEntries);
            int[] array2 = new int[array.Length];
            for (int i = 0; i < array2.Length; i++)
            {
                int num = 0;
                int.TryParse(array[i], out num);
                array2[i] = num;
            }
            return array2;
        }

        public static bool[] HashToBoolArr(this Hashtable table, object key, char c = ',')
        {
            if (!table.ContainsKey(key))
                return new bool[0];

            string[] array = table.HashToString(key).Split(new char[]
                {
                c
                }, StringSplitOptions.RemoveEmptyEntries);

            bool[] array2 = new bool[array.Length];
            for (int i = 0; i < array2.Length; i++)
            {
                var text = array[i];
                array2[i] = text.Equals("1") || text.Equals("true") || text.Equals("True");
            }

            return array2;
        }

        public static Hashtable[] HashToHashArr(this Hashtable table, object key)
        {
            if (!table.ContainsKey(key))
            {
                return new Hashtable[0];
            }
            ArrayList arrayList = table[key] as ArrayList;
            if (arrayList == null)
            {
                return new Hashtable[0];
            }
            Hashtable[] array = new Hashtable[arrayList.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (arrayList[i] as Hashtable);
            }
            return array;
        }

        public static bool HashToBool(this Hashtable table, object key)
        {
            if (!table.ContainsKey(key))
            {
                return false;
            }
            if (table[key] == null)
            {
                return false;
            }
            string text = table[key].ToString();
            return text.Equals("1") || text.Equals("true") || text.Equals("True");
        }

        public static void AddRange(this Hashtable table, Hashtable hash, bool replace = false)
        {
            IDictionaryEnumerator enumerator = hash.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
                    if (table.ContainsKey(dictionaryEntry.Key))
                    {
                        if (replace)
                        {
                            table[dictionaryEntry.Key] = dictionaryEntry.Value;
                        }
                    }
                    else
                    {
                        table.Add(dictionaryEntry.Key, dictionaryEntry.Value);
                    }
                }
            }
            finally
            {
                IDisposable disposable;
                if ((disposable = (enumerator as IDisposable)) != null)
                {
                    disposable.Dispose();
                }
            }
        }

        public static T HashToComponent<T>(this Hashtable table, object key) where T : Component
        {
            if (!table.ContainsKey(key))
            {
                return (T)((object)null);
            }
            return table[key] as T;
        }

        public static T HashToObject<T>(this Hashtable table, object key) where T : class
        {
            if (!table.ContainsKey(key))
            {
                return (T)((object)null);
            }
            return table[key] as T;
        }
    }
}
