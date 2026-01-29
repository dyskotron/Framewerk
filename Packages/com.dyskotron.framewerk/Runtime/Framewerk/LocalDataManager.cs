using System;
using System.IO;
using UnityEngine;

namespace Framewerk
{
    public class LocalDataManager
    {
        public enum DataLocation
        {
            LocalData,
            Resources,
            PlayerPrefs
        }
        
        public string Prefix { get; set; } = "";

        private string GetFilePath(string key, DataLocation location)
        {
            switch (location)
            {
                case DataLocation.LocalData:
                    return Path.Combine(Application.persistentDataPath, Prefix + key + ".dat");
                case DataLocation.Resources:
                    return Path.Combine(Application.dataPath, "Resources", Prefix + key + ".txt");
                default:
                    throw new InvalidOperationException("Invalid data location for file path.");
            }
        }

        public bool HasKey(string key, DataLocation location)
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    return PlayerPrefs.HasKey(Prefix + key);
                default:
                    return File.Exists(GetFilePath(key, location));
            }
        }

        public bool HasKey(Enum key, DataLocation location)
        {
            return HasKey(key.ToString(), location);
        }

        #region bool

        public void SetUserData(string key, DataLocation location, bool value)
        {
            SetUserData(key, location, value ? 1 : 0);
        }

        public void SetUserData(Enum key, DataLocation location, bool value)
        {
            SetUserData(key.ToString(), location, value ? 1 : 0);
        }

        public bool GetUserBool(string key, DataLocation location, bool defaultValue = false)
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    return PlayerPrefs.GetInt(Prefix + key, defaultValue ? 1 : 0) > 0;
                default:
                    string filePath = GetFilePath(key, location);
                    if (File.Exists(filePath))
                    {
                        if (bool.TryParse(File.ReadAllText(filePath), out bool value))
                        {
                            return value;
                        }
                    }

                    return defaultValue;
            }
        }

        public bool GetUserBool(Enum key, DataLocation location, bool defaultValue = false)
        {
            return GetUserBool(key.ToString(), location, defaultValue);
        }

        #endregion

        #region int

        public void SetUserData(string key, DataLocation location, int value)
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    PlayerPrefs.SetInt(Prefix + key, value);
                    PlayerPrefs.Save();
                    break;
                default:
                    File.WriteAllText(GetFilePath(key, location), value.ToString());
                    break;
            }
        }

        public void SetUserData(Enum key, DataLocation location, int value)
        {
            SetUserData(key.ToString(), location, value);
        }

        public int GetUserInt(string key, DataLocation location, int defaultValue = 0)
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    return PlayerPrefs.GetInt(Prefix + key, defaultValue);
                default:
                    string filePath = GetFilePath(key, location);
                    if (File.Exists(filePath))
                    {
                        if (int.TryParse(File.ReadAllText(filePath), out int value))
                        {
                            return value;
                        }
                    }

                    return defaultValue;
            }
        }

        public int GetUserInt(Enum key, DataLocation location, int defaultValue = 0)
        {
            return GetUserInt(key.ToString(), location, defaultValue);
        }

        #endregion

        #region float

        public void SetUserData(string key, DataLocation location, float value)
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    PlayerPrefs.SetFloat(Prefix + key, value);
                    PlayerPrefs.Save();
                    break;
                default:
                    File.WriteAllText(GetFilePath(key, location), value.ToString());
                    break;
            }
        }

        public void SetUserData(Enum key, DataLocation location, float value)
        {
            SetUserData(key.ToString(), location, value);
        }

        public float GetUserFloat(string key, DataLocation location, float defaultValue = 0.0f)
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    return PlayerPrefs.GetFloat(Prefix + key, defaultValue);
                default:
                    string filePath = GetFilePath(key, location);
                    if (File.Exists(filePath))
                    {
                        if (float.TryParse(File.ReadAllText(filePath), out float value))
                        {
                            return value;
                        }
                    }

                    return defaultValue;
            }
        }

        public float GetUserFloat(Enum key, DataLocation location, float defaultValue = 0.0f)
        {
            return GetUserFloat(key.ToString(), location, defaultValue);
        }

        #endregion

        #region string

        public void SetUserData(string key, DataLocation location, string value)
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    PlayerPrefs.SetString(Prefix + key, value);
                    PlayerPrefs.Save();
                    break;
                default:
                    File.WriteAllText(GetFilePath(key, location), value);
                    break;
            }
        }

        public void SetUserData(Enum key, DataLocation location, string value)
        {
            SetUserData(key.ToString(), location, value);
        }

        public string GetUserString(string key, DataLocation location, string defaultValue = "")
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    return PlayerPrefs.GetString(Prefix + key, defaultValue);
                default:
                    string filePath = GetFilePath(key, location);
                    return File.Exists(filePath) ? File.ReadAllText(filePath) : defaultValue;
            }
        }

        public string GetUserString(Enum key, DataLocation location, string defaultValue = "")
        {
            return GetUserString(key.ToString(), location, defaultValue);
        }

        #endregion

        #region byte array

        public void SetUserData(string key, DataLocation location, byte[] value)
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    string base64Value = Convert.ToBase64String(value);
                    PlayerPrefs.SetString(Prefix + key, base64Value);
                    PlayerPrefs.Save();
                    break;
                default:
                    File.WriteAllBytes(GetFilePath(key, location), value);
                    break;
            }
        }

        public void SetUserData(Enum key, DataLocation location, byte[] value)
        {
            SetUserData(key.ToString(), location, value);
        }

        public byte[] GetUserByteArray(string key, DataLocation location, byte[] defaultValue = null)
        {
            switch (location)
            {
                case DataLocation.PlayerPrefs:
                    if (PlayerPrefs.HasKey(Prefix + key))
                    {
                        string base64Value = PlayerPrefs.GetString(Prefix + key);
                        return Convert.FromBase64String(base64Value);
                    }

                    return defaultValue;
                default:
                    string filePath = GetFilePath(key, location);
                    return File.Exists(filePath) ? File.ReadAllBytes(filePath) : defaultValue;
            }
        }

        public byte[] GetUserByteArray(Enum key, DataLocation location, byte[] defaultValue = null)
        {
            return GetUserByteArray(key.ToString(), location, defaultValue);
        }

        #endregion
    }
}