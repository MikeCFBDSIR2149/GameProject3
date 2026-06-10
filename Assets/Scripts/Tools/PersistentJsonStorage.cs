using System;
using System.IO;
using UnityEngine;

namespace Tools
{
    public static class PersistentJsonStorage
    {
        public static string GetPersistentFilePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static bool TryLoad<T>(string fileName, out T data) where T : class
        {
            data = null;

            try
            {
                string path = GetPersistentFilePath(fileName);
                if (!File.Exists(path))
                {
                    return false;
                }

                string jsonString = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return false;
                }

                data = JsonUtility.FromJson<T>(jsonString);
                return data != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PersistentJsonStorage] Failed to load '{fileName}': {ex}");
                data = null;
                return false;
            }
        }

        public static void Save<T>(string fileName, T data, bool prettyPrint = true) where T : class
        {
            try
            {
                string path = GetPersistentFilePath(fileName);
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string jsonString = JsonUtility.ToJson(data, prettyPrint);
                File.WriteAllText(path, jsonString);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PersistentJsonStorage] Failed to save '{fileName}': {ex}");
            }
        }
    }
}

