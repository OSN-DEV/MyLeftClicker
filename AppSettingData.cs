using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MyLeftClicker {
    public class AppSettingData {
        public static int ClickCountUnit = 10;

        #region Declaration
        [Serializable]
        public class Pair<TK, TV> {
            public TK Key;
            public TV Value;

            public Pair(KeyValuePair<TK, TV> pair) {
                Key = pair.Key;
                Value = pair.Value;
            }
        }

        public bool Observered { set; get; }
        public int ClickCount { set; get; }

        private static AppSettingData _settingData = null;
        private static string _settingFile = Application.StartupPath + @"setting";
        #endregion

        #region Constructor
        private AppSettingData() { }
        #endregion

        #region Public Property
        /// <summary>
        /// get instance
        /// </summary>
        /// <returns>instance</returns>
        public static AppSettingData GetInstance() {
            if (null == _settingData) {
                _settingData = (AppSettingData)LoadFromXml(_settingFile, typeof(AppSettingData));
                if (null == _settingData) {
                    _settingData = new AppSettingData();
                    _settingData.Observered = false;
                    _settingData.ClickCount = 10;
                }
            }
            return _settingData;
        }

        public void Save() {
            SaveToXml(_settingFile, this);
        }
        #endregion

        #region Public Method

        #endregion

        #region Protected Method
        /// <summary>
        /// load settings from xml
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="type">type</param>
        /// <returns>instance</returns>
        protected static object LoadFromXml(string filePath, Type type) {
            if (!File.Exists(filePath)) {
                return null;
            }
            object instance = null;
            using (var reader = new StreamReader(filePath, new UTF8Encoding(false))) {
                var serializer = new System.Xml.Serialization.XmlSerializer(type);
                instance = serializer.Deserialize(reader);
            }
            return instance;
        }

        /// <summary>
        /// load from xml
        /// </summary>
        /// <typeparam name="TK">type of key</typeparam>
        /// <typeparam name="TV">type of value</typeparam>
        /// <param name="filePath">file path</param>
        /// <returns>instance</returns>
        protected static Dictionary<TK, TV> LoadFromXml<TK, TV>(string filePath) {
            if (!File.Exists(filePath)) {
                return null;
            }

            List<Pair<TK, TV>> obj = null;
            using (var reader = new StreamReader(filePath, new UTF8Encoding(false))) {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Pair<TK, TV>>));
                obj = (List<Pair<TK, TV>>)serializer.Deserialize(reader);
            }
            return ConvertListToDictionary(obj);
        }

        /// <summary>
        /// save settings to xml
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="instance">instance</param>
        protected static void SaveToXml(string filePath, object instance) {
            using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(false))) {
                var seralizer = new System.Xml.Serialization.XmlSerializer(instance.GetType());
                seralizer.Serialize(writer, instance);
            }
        }

        /// <summary>
        /// save setting to xml
        /// </summary>
        /// <typeparam name="TK">type of key</typeparam>
        /// <typeparam name="TV">type of value</typeparam>
        /// <param name="filePath">file path</param>
        /// <param name="dictionary">save data</param>
        protected static void SaveToXml<TK, TV>(string filePath, Dictionary<TK, TV> dictionary) {
            List<Pair<TK, TV>> list = ConvertDictionaryToList(dictionary);
            using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(false))) {
                var seralizer = new System.Xml.Serialization.XmlSerializer(typeof(List<Pair<TK, TV>>));
                seralizer.Serialize(writer, list);
            }
        }


        /// <summary>
        /// convert dictionary to list
        /// </summary>
        /// <typeparam name="TK">type of key</typeparam>
        /// <typeparam name="TV">type of value</typeparam>
        /// <param name="dictionary">convert target</param>
        /// <returns>list</returns>
        public static List<Pair<TK, TV>> ConvertDictionaryToList<TK, TV>(Dictionary<TK, TV> dictionary) {
            List<Pair<TK, TV>> list = new List<Pair<TK, TV>>();
            foreach (KeyValuePair<TK, TV> pair in dictionary) {
                list.Add(new Pair<TK, TV>(pair));
            }
            return list;
        }

        /// <summary>
        /// convert list to dictionary
        /// </summary>
        /// <typeparam name="TK">type of key</typeparam>
        /// <typeparam name="TV">type of value</typeparam>
        /// <param name="list">convert target</param>
        /// <returns>dictionary</returns>
        public static Dictionary<TK, TV> ConvertListToDictionary<TK, TV>(List<Pair<TK, TV>> list) {
            Dictionary<TK, TV> dic = new Dictionary<TK, TV>();
            foreach (Pair<TK, TV> pair in list) {
                dic.Add(pair.Key, pair.Value);
            }
            return dic;
        }
        #endregion
    }
}
