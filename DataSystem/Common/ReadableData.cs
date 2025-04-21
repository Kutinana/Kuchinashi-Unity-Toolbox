using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

# if SUPPORT_QFRAMEWORK

using QFramework;

# endif

namespace Kuchinashi.DataSystem
{
    public abstract partial class ReadableData : IReadableData
    {
        public abstract string Path { get; }

        public virtual IReadableData DeSerialization()
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path)) return null;

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            return JsonConvert.DeserializeObject<ReadableData>(File.ReadAllText(Path), settings);
        }

        public virtual T DeSerialization<T>() where T : IReadableData, new()
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path)) return new T();

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(Path), settings);
        }

        public virtual bool Validation<T>(out T value) where T : IReadableData, new()
        {
            value = new T();
            try
            {
                // Ability of reading
                value = DeSerialization<T>();
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
    }

    public partial class ReadableData
    {
        public static T DeSerialization<T>(string _path) where T : new()
        {
            if (string.IsNullOrEmpty(_path)) return new T();

            string json = "";
            try
            {
                if (!File.Exists(_path)) json = Resources.Load<TextAsset>(_path).text;
                else json = File.ReadAllText(_path);
            } catch {}

            if (string.IsNullOrEmpty(json)) return new T();

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static bool Validation<T>(string _path, out T value) where T : new()
        {
            value = new T();
            try
            {
                // Ability of reading
                value = DeSerialization<T>(_path);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

# if SUPPORT_QFRAMEWORK

        public static T DeSerialization<T>(string _bundle, string _id) where T : IReadableData, new()
        {
            try
            {
                var resLoader = ResLoader.Allocate();
                string config = resLoader.LoadSync<TextAsset>(_bundle, _id).text;

                resLoader.Dispose();

                var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
                return JsonConvert.DeserializeObject<T>(config, settings);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return new T();
            }
        }

        public static bool Validation<T>(string _bundle, string _id, out T value) where T : IReadableData, new()
        {
            value = new T();
            try
            {
                // Ability of reading
                value = DeSerialization<T>(_bundle, _id);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
# endif

    }
}