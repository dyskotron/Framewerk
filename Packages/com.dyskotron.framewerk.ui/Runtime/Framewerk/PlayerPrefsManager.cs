using System;
using UnityEngine;

namespace Framewerk.Managers
{
    public class PlayerPrefsManager
    {
        public string Prefix { get; set; } = "";

		public bool HasKey(string key)
		{
			return PlayerPrefs.HasKey(Prefix + key);
		}

		public bool HasKey(Enum key)
		{
			return HasKey(key.ToString());
		}
        
        #region bool
        
        public void SetUserData(string key, bool value)
        {
            PlayerPrefs.SetInt(Prefix + key, value ? 1 : 0);
			PlayerPrefs.Save();
        }

        public void SetUserData(Enum key, bool value)
        {
            SetUserData(key.ToString(), value);
        }
        
        public bool GetUserBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(Prefix + key, defaultValue ? 1 : 0) > 0;
        }

        public bool GetUserBool(Enum key, bool defaultValue = false)
        {
            return GetUserBool(key.ToString(), defaultValue);
        }

        #endregion

        #region int

        public void SetUserData(string key, int value)
        {
            PlayerPrefs.SetInt(Prefix + key, value);
            PlayerPrefs.Save();
        }

        public void SetUserData(Enum key, int value)
        {
            SetUserData(key.ToString(), value);
        }
        
        public int GetUserInt(string key, int defaultValue = 0)
        {
            return  PlayerPrefs.GetInt(Prefix + key, defaultValue);
        }
        
        public int GetUserInt(Enum key, int defaultValue = 0)
        {
            return  GetUserInt(key.ToString(), defaultValue);
        }
        
        #endregion

        #region float
        
        public void SetUserData(string key, float value)
        {
            PlayerPrefs.SetFloat(Prefix + key, value);
            PlayerPrefs.Save();
        }

        public void SetUserData(Enum key, float value)
        {
            SetUserData(key.ToString(), value);
        }
        
        public float GetUserFloat(string key, float defaultValue = 0)
        {
            return  PlayerPrefs.GetFloat(Prefix + key, defaultValue);
        }
        
        public float GetUserFloat(Enum key, float defaultValue = 0)
        {
            return  GetUserFloat(key.ToString(), defaultValue);
        }
        
        #endregion
        
        #region string

        public void SetUserData(string key, string value)
        {
            PlayerPrefs.SetString(Prefix + key, value);
            PlayerPrefs.Save();
        }

        public void SetUserData(Enum key, string value)
        {
            SetUserData(key.ToString(), value);
        }

        public string GetUserString(string key, string defaultValue = "")
        {
            return  PlayerPrefs.GetString(Prefix + key, defaultValue);
        }
        
        public string GetUserString(Enum key, string defaultValue = "")
        {
            return  GetUserString(key.ToString(), defaultValue);
        }
        
        #endregion
        
    }
}