using System;
using System.Collections.Generic;

namespace ZilLion.Core.Unities
{
    public static class AppSettingContainer
    {
        /// <summary>
        ///     系统配置缓存
        /// </summary>
        private static readonly IDictionary<string, AppSettingData> _appSetting;

        private static bool _hasInited;

        private static Action<string, AppSettingData> _saveSetingAction;

        static AppSettingContainer()
        {
            _appSetting = new Dictionary<string, AppSettingData>();
        }


        public static void InitAppSetting(Func<IDictionary<string, AppSettingData>> initSetingFunc,
            Action<string, AppSettingData> saveSetingAction)
        {
            if (_hasInited)
                throw new Exception("AppSettingContainer只能初始化一次");

            _saveSetingAction = saveSetingAction;

            if (initSetingFunc == null) return;
            foreach (var setting in initSetingFunc())
                _appSetting.Add(setting.Key.ToLower(), setting.Value);


            _hasInited = true;
        }

        public static string GetAppSetting(string key)
        {
            return _appSetting.ContainsKey(key.ToLower()) ? _appSetting[key.ToLower()].SettingData : string.Empty;
        }


        public static void SaveAppSetting(string key, string value)
        {
            var settingData = _appSetting.ContainsKey(key.ToLower()) ? _appSetting[key.ToLower()] : null;
            if (settingData == null) return;
            if (settingData.IsReadOnly)
                throw new Exception($"配置{key}，为只读属性，不可以修改");
            settingData.SettingData = value;
            _saveSetingAction?.Invoke(key.ToLower(), settingData);
        }



        public static void InsertAppSetting(string key, AppSettingData value)
        {
            var settingData = _appSetting.ContainsKey(key.ToLower()) ? _appSetting[key.ToLower()] : null;
            if (settingData != null)
            {
                if (settingData.IsReadOnly)
                    throw new Exception($"配置{key}，为只读属性，不可以修改");
                settingData.SettingData = value.SettingData;
                _saveSetingAction?.Invoke(key.ToLower(), settingData);
            }
            else
            {
                _appSetting.Add(key.ToLower(), value);
                _saveSetingAction?.Invoke(key.ToLower(), value);
            }
        }
    }

    public class AppSettingData
    {
        public string SettingData { get; set; }
        public bool IsReadOnly { get; set; }
        public SetingSource SetingSource { get; set; }
    }


    public enum SetingSource
    {
        Sysconfig,
        Sqliteappsetting
    }
}