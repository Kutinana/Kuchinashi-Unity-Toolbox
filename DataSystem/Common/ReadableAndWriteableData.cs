using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Kuchinashi.DataSystem
{
    public abstract partial class ReadableAndWriteableData : IReadableData , IWriteableData
    {
        public abstract string Path { get; }

        public void Serialization()
        {
            if (File.Exists(Path)) File.Delete(Path);
            File.Create(Path).Dispose();

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            File.WriteAllText(Path, JsonConvert.SerializeObject(this, Formatting.Indented, settings));
        }
        
        public virtual IReadableData DeSerialization()
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path)) return null;

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            return JsonConvert.DeserializeObject<ReadableAndWriteableData>(File.ReadAllText(Path), settings);
        }

        public virtual T DeSerialization<T>() where T : IReadableData, new()
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path)) return new T();

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(Path), settings) ?? new T();
        }

        public virtual bool Validation<T>(out T value) where T : IReadableData, new()
        {
            value = new T();
            try
            {
                // Ability of reading
                value = DeSerialization<T>() ?? new T();
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
    }
}