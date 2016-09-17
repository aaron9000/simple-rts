using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


public static class F
{
    public class Tuple<T1, T2>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }

        public Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }

    private class KeyInfo
    {
        private readonly FieldInfo _fieldInfo;
        private readonly PropertyInfo _propertyInfo;

        public KeyInfo(PropertyInfo p)
        {
            _fieldInfo = null;
            _propertyInfo = p;
        }

        public KeyInfo(FieldInfo f)
        {
            _fieldInfo = f;
            _propertyInfo = null;
        }

        public object GetValue(object obj)
        {
            if (obj == null)
                return null;
            return _fieldInfo != null ? _fieldInfo.GetValue(obj) : _propertyInfo.GetValue(obj, null);
        }

        public void SetValue(object obj, object value)
        {
            if (obj == null)
                return;

            if (_fieldInfo != null)
            {
                _fieldInfo.SetValue(obj, value);
            }
            else
            {
                _propertyInfo.SetValue(obj, value, null);
            }
        }
    }

    private class TypeReflectionInfo
    {
        public readonly Dictionary<string, KeyInfo> Info = new Dictionary<string, KeyInfo>();
        public readonly HashSet<string> KeySet;

        public TypeReflectionInfo(object obj)
        {
            var t = obj.GetType();
            var props = t.GetProperties();
            foreach (var prop in props)
            {
                if (prop.CanRead)
                {
                    Info.Add(prop.Name, new KeyInfo(prop));
                }
            }
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                Info.Add(field.Name, new KeyInfo(field));
            }
            KeySet = new HashSet<string>(Info.Keys.ToArray());
        }
    }

    #region Misc

    public static Dictionary<string, object> EmptyDictionary()
    {
        return new Dictionary<string, object>();
    }

    public static List<T> EmptyList<T>()
    {
        return new List<T>();
    }

    public static int[] Range(int startInclusive, int endExclusive)
    {
        var l = endExclusive - startInclusive;
        if (l < 0)
        {
            return new int[0];
        }
        return Enumerable.Range(startInclusive, l).ToArray();
    }

    #endregion

    #region Reflection & Type Helpers

    private static readonly Dictionary<Type, TypeReflectionInfo> TypeReflectionInfoByType =
        new Dictionary<Type, TypeReflectionInfo>();

    private static TypeReflectionInfo GetReflectionInfo(object obj)
    {
        if (obj == null)
            return null;
        var type = obj.GetType();
        if (TypeReflectionInfoByType.ContainsKey(type) == false)
        {
            TypeReflectionInfoByType.Add(type, new TypeReflectionInfo(obj));
        }
        return TypeReflectionInfoByType[type];
    }

    private static T GetObjectValueFast<T>(string key, object subject, TypeReflectionInfo info)
    {
        if (subject == null)
            return default(T);
        if (info.Info.ContainsKey(key))
        {
            try
            {
                return (T) info.Info[key].GetValue(subject);
            }
            catch (Exception e)
            {
                Debug.LogWarning(String.Format("F: GetObjectValue: key = {0} exception: {1}", key, e));
            }
        }
        return default(T);
    }

    private static void SetObjectValueFast(string key, object value, object subject, TypeReflectionInfo info)
    {
        if (subject == null)
            return;
        if (info.Info.ContainsKey(key))
        {
            try
            {
                info.Info[key].SetValue(subject, value);
            }
            catch (Exception e)
            {
                Debug.LogWarning(String.Format("F: SetObjectValue: key = {0} exception: {1}", key, e));
            }
        }
        else
        {
            Debug.LogWarning("F: SetObjectValue: nonexistent key: " + key);
        }
    }

    #endregion

    #region Key / Value

    public static T GetValue<T>(string key, IDictionary<string, object> dictionary)
    {
        if (dictionary == null || !dictionary.ContainsKey(key))
            return default(T);
        return (T) dictionary[key];
    }

    public static T GetValue<T>(string key, object subject)
    {
        if (subject == null)
            return default(T);
        var info = GetReflectionInfo(subject);
        return GetObjectValueFast<T>(key, subject, info);
    }

    public static void SetValue(string key, object value, object subject)
    {
        if (subject == null)
            return;
        var info = GetReflectionInfo(subject);
        SetObjectValueFast(key, value, subject, info);
    }

    public static void SetValue(string key, object value, IDictionary<string, object> subject)
    {
        if (subject == null)
            return;
        subject.Add(key, value);
    }

    public static string[] GetKeys(IDictionary<string, object> dict)
    {
        if (dict == null)
            return new string[0];
        return dict.Keys.ToArray();
    }

    public static string[] GetKeys(object obj)
    {
        if (obj == null)
            return new string[0];
        return GetReflectionInfo(obj).KeySet.ToArray();
    }

    public static object[] GetValues(IDictionary<string, object> dict)
    {
        if (dict == null)
            return new object[0];
        return dict.Values.ToArray();
    }

    public static object[] GetValues(object obj)
    {
        if (obj == null)
            return new object[0];
        var info = GetReflectionInfo(obj);
        var values = new object[info.KeySet.Count];
        var index = 0;
        foreach (var key in info.KeySet)
        {
            values[index] = GetObjectValueFast<object>(key, obj, info);
            index++;
        }
        return values;
    }

    #endregion

    #region Cloning & Converting Collections and Objects

    public static T ShallowObjectFromDictionary<T>(IDictionary<string, object> dict) where T : new()
    {
        if (dict == null)
            return default(T);
        var obj = new T();
        var info = GetReflectionInfo(obj);
        foreach (var key in info.KeySet)
        {
            if (dict.ContainsKey(key))
            {
                SetObjectValueFast(key, dict[key], obj, info);
            }
        }
        return obj;
    }

    public static Dictionary<string, object> ShallowDictionaryFromObject(object obj)
    {
        if (obj == null)
            return null;
        var info = GetReflectionInfo(obj);
        var dict = new Dictionary<string, object>();
        foreach (var key in info.KeySet)
        {
            dict.Add(key, GetObjectValueFast<object>(key, obj, info));
        }
        return dict;
    }

    public static T ShallowCloneObject<T>(T source) where T : new()
    {
        if (source == null)
            return default(T);
        var clone = new T();
        var info = GetReflectionInfo(source);
        foreach (var key in info.KeySet)
        {
            var value = GetObjectValueFast<object>(key, source, info);
            SetObjectValueFast(key, value, clone, info);
        }
        return clone;
    }

    public static TDictionary ShallowCloneDictionary<TKey, TValue, TDictionary>(
        IDictionary<TKey, TValue> source)
        where TDictionary : IDictionary<TKey, TValue>, new()
    {
        if (source == null)
            return default(TDictionary);
        var clone = new TDictionary();
        foreach (var pair in source)
        {
            clone.Add(pair.Key, pair.Value);
        }
        return clone;
    }

    public static TCollection ShallowCloneCollection<TElement, TCollection>(TCollection source)
        where TCollection : ICollection<TElement>, new()
    {
        if (source == null)
            return default(TCollection);
        var clone = new TCollection();
        foreach (var value in source)
        {
            clone.Add(value);
        }
        return clone;
    }

    public static TCollection DeepCloneObjectCollection<TElement, TCollection>(TCollection source)
        where TCollection : ICollection<TElement>, new() where TElement : new()
    {
        if (source == null)
            return default(TCollection);
        var clone = new TCollection();
        foreach (var value in source)
        {
            clone.Add(ShallowCloneObject(value));
        }
        return clone;
    }

    #endregion

    #region Map

    public static void Loop<T>(Action<T> loopAction, IEnumerable<T> collection)
    {
        foreach (var v in collection)
        {
            loopAction(v);
        }
    }

    public static TOutputElement[] MapObject<TOutputElement>(Func<string, object, TOutputElement> mappingFunction,
        object obj)
    {
        if (obj == null)
            return default(TOutputElement[]);
        var info = GetReflectionInfo(obj);
        var newArray = new TOutputElement[info.KeySet.Count];
        var index = 0;
        foreach (var key in info.KeySet)
        {
            newArray[index] = mappingFunction(key, info.Info[key].GetValue(obj));
            index++;
        }
        return newArray;
    }

    public static TOutputElement[] MapDictionary<TOutputElement>(Func<string, object, TOutputElement> mappingFunction,
        IDictionary<string, object> dictionary)
    {
        if (dictionary == null)
            return default(TOutputElement[]);
        var newArray = new TOutputElement[dictionary.Keys.Count];
        var index = 0;
        foreach (var entry in dictionary)
        {
            newArray[index] = mappingFunction(entry.Key, entry.Value);
            index++;
        }
        return newArray;
    }

    public static TOutputElement[] Map<TInputElement, TOutputElement>(
        Func<TInputElement, TOutputElement> mappingFunction,
        IEnumerable<TInputElement> values)
    {
        if (values == null)
            return default(TOutputElement[]);
        var newList = new List<TOutputElement>();
        foreach (var value in values)
        {
            newList.Add(mappingFunction(value));
        }
        return newList.ToArray();
    }

    public static TOutputElement[] MapRectangularArray<TInputElement, TOutputElement>(
        Func<TInputElement[], TOutputElement> mappingFunction, TInputElement[,] rectArray)
    {
        if (rectArray == null)
            return default(TOutputElement[]);
        var newArray = new TOutputElement[rectArray.GetLength(0)];
        var innerLength = rectArray.GetLength(1);
        var row = new TInputElement[innerLength];
        for (var i = 0; i < rectArray.GetLength(0); i++)
        {
            for (var j = 0; j < innerLength; j++)
            {
                row[j] = rectArray[i, j];
            }
            newArray[i] = mappingFunction(row);
        }
        return newArray;
    }

    #endregion

    #region FromPairs

    public static Dictionary<string, TValue> CoerceDictionary<TValue>(IDictionary<string, object> dict)
    {
        var clone = new Dictionary<string, TValue>();
        foreach (var pair in dict)
        {
            clone.Add(pair.Key, (TValue) pair.Value);
        }
        return clone;
    }

    public static Dictionary<string, object> FromPairs(object[][] pairs)
    {
        if (pairs == null)
            return null;
        var newDict = new Dictionary<string, object>();
        foreach (var pair in pairs)
        {
            newDict.Add((string) pair[0], pair[1]);
        }
        return newDict;
    }

    public static Dictionary<string, object> FromPairs(ICollection<List<object>> pairs)
    {
        if (pairs == null)
            return null;
        var newDict = new Dictionary<string, object>();
        foreach (var pair in pairs)
        {
            newDict.Add((string) pair[0], pair[1]);
        }
        return newDict;
    }

    public static Dictionary<string, object> FromPairs(object[,] pairs)
    {
        if (pairs == null)
            return null;
        var newDict = new Dictionary<string, object>();
        for (var i = 0; i < pairs.GetLength(0); i++)
        {
            newDict.Add((string) pairs[i, 0], pairs[i, 1]);
        }
        return newDict;
    }

    #endregion

    #region ToPairs

    public static object[][] ToPairs(object obj)
    {
        var info = GetReflectionInfo(obj);
        var pairs = new object[info.KeySet.Count][];
        var index = 0;
        foreach (var key in info.KeySet)
        {
            pairs[index] = new[] {key, GetObjectValueFast<object>(key, obj, info)};
            index++;
        }
        return pairs;
    }

    public static object[][] ToPairs<TKey, TValue>(IDictionary<TKey, TValue> dict)
    {
        var pairs = new object[dict.Keys.Count][];
        var index = 0;
        foreach (var key in dict.Keys)
        {
            pairs[index] = new[] {key, (object) dict[key]};
            index++;
        }
        return pairs;
    }

    #endregion

    #region Reduce

    public static TAccum Reduce<TAccum, TElement>(Func<TAccum, TElement, TAccum> reducingFunction, TAccum startValue,
        IEnumerable<TElement> list)
    {
        // TODO: shallowClone if not a value Type?
        var accum = startValue;
        foreach (var value in list)
        {
            accum = reducingFunction(accum, value);
        }
        return accum;
    }

    #endregion

    #region Merge & Zip

    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(IDictionary<TKey, TValue> a, IDictionary<TKey, TValue> b)
    {
        var setA = new HashSet<TKey>(a.Keys);
        var setB = new HashSet<TKey>(b.Keys);
        setB.UnionWith(setA);
        var merge = new Dictionary<TKey, TValue>();
        foreach (var key in setB)
        {
            merge.Add(key, b.ContainsKey(key) ? b[key] : a[key]);
        }
        return merge;
    }

    public static Dictionary<string, object> Zip(IEnumerable<string> keys, IEnumerable<object> values)
    {
        var dict = new Dictionary<string, object>();
        var k = keys.GetEnumerator();
        var v = values.GetEnumerator();
        while (k.MoveNext() && v.MoveNext())
        {
            dict[k.Current] = v.Current;
        }
        return dict;
    }

    public static Dictionary<string, object> Zip(object[,] pairs)
    {
        var dict = new Dictionary<string, object>();
        var length = pairs.GetLength(0);
        for (var i = 0; i < length; i++)
        {
            dict[(string) pairs[i, 0]] = pairs[i, 1];
        }
        return dict;
    }

    #endregion

    #region Pick and Pluck

    public static TPluckedValue[] PluckFromDictionaries<TPluckedValue>(string key,
        IEnumerable<Dictionary<string, object>> values)
    {
        var plucked = new List<TPluckedValue>();
        foreach (var dict in values)
        {
            plucked.Add((TPluckedValue) dict[key]);
        }
        return plucked.ToArray();
    }

    public static TPluckedValue[] PluckFromObjects<TPluckedValue>(string key, IEnumerable<object> values)
    {
        if (values == null)
            return default(TPluckedValue[]);

        var pluckedValues = new List<TPluckedValue>();
        TypeReflectionInfo info;
        foreach (var value in values)
        {
            info = GetReflectionInfo(value);
            pluckedValues.Add(GetObjectValueFast<TPluckedValue>(key, value, info));
        }
        return pluckedValues.ToArray();
    }

    public static Dictionary<string, object> PickAll(IEnumerable<string> keys, object subject)
    {
        var dict = new Dictionary<string, object>();
        var info = GetReflectionInfo(subject);
        foreach (var key in keys)
        {
            dict.Add(key, GetObjectValueFast<object>(key, subject, info));
        }
        return dict;
    }

    public static Dictionary<string, object> PickAll(IEnumerable<string> keys, IDictionary<string, object> subject)
    {
        var dict = new Dictionary<string, object>();
        foreach (var key in keys)
        {
            if (subject.ContainsKey(key))
            {
                dict.Add(key, subject[key]);
            }
            else
            {
                dict.Add(key, null);
            }
        }
        return dict;
    }

    #endregion

    #region Shuffle

    public static TList Shuffle<TElement, TList>(TList source)
        where TList : IList<TElement>, new()
    {
        var copy = ShallowCloneCollection<TElement, TList>(source);
        for (int i = 0; i < copy.Count; i++)
        {
            TElement temp = copy[i];
            var randomIndex = UnityEngine.Random.Range(i, copy.Count);
            copy[i] = copy[randomIndex];
            copy[randomIndex] = temp;
        }
        return copy;
    }

    public static List<TElement> ShuffleList<TElement>(List<TElement> source)
    {
        var copy = ShallowCloneCollection<TElement, List<TElement>>(source);
        for (int i = 0; i < copy.Count; i++)
        {
            TElement temp = copy[i];
            var randomIndex = UnityEngine.Random.Range(i, copy.Count);
            copy[i] = copy[randomIndex];
            copy[randomIndex] = temp;
        }
        return copy;
    }

    #endregion

    #region ShallowFlatten

    public static TElement[] ShallowFlatten<TElement>(TElement[,] array)
    {
        var newList = new TElement[array.Length];
        var index = 0;
        foreach (var element in array)
        {
            newList[index] = element;
            index++;
        }
        return newList;
    }

    public static TElement[] ShallowFlatten<TElement>(TElement[][] lists)
    {
        return lists.SelectMany(s => s).ToArray();
    }

    public static TElement[] ShallowFlatten<TElement>(IEnumerable<List<TElement>> lists)
    {
        return lists.SelectMany(s => s).ToArray();
    }

    #endregion
}