using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using NLog;

namespace FacebookSoapService
{
    static class MemoryCacheUtil
    {
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        private static readonly ObjectCache _Cache = MemoryCache.Default;

        public static readonly Dictionary<KeyEnums, string> KeyDictionary = new Dictionary<KeyEnums, string>
        {
            {KeyEnums.GetBranchAndAtmList, "GetBranchAndAtmList"},
            {KeyEnums.GetLocalBranchAndAtmList, "GetBranchAndAtmList_{0}_{1}"},
            {KeyEnums.GetCashClientInfo, "GetCashClientInfo_{0}_{1}"},
            {KeyEnums.GetBobSiteCardAndBranchList, "GetBobSiteCardAndBranchList"},
            {KeyEnums.CurrencyRate, "CurrencyRate"},
            {KeyEnums.FeedbackCategories, "FeedbackCategories"}
        };

        public enum KeyEnums
        {
            GetBranchAndAtmList = 0,
            GetLocalBranchAndAtmList = 1,
            GetCashClientInfo = 2,
            GetBobSiteCardAndBranchList = 3,
            CurrencyRate = 4,
            FeedbackCategories
        }

        public static bool Contains(string key)
        {
            if (_Cache.Contains(key))
            {
                return true;
            }

            return false;
        }

        public static T Get<T>(string key) where T : class
        {
            try
            {
                return (T)_Cache[key];
            }
            catch
            {
                return null;
            }
        }

        public static List<string> GetAll()
        {
            return _Cache.Select(keyValuePair => keyValuePair.Key).ToList();
        }

        public static void Add(object objectToCache, string key, int timeValue, bool isMinute = true)
        {
            Log.Info("Cache created by key:{0} and active for {1}, isMinute: {2}", key, timeValue, isMinute);
            if (Contains(key))
            {
                Clear(key);
            }
            _Cache.Add(
                       key,
                       objectToCache,
                       isMinute ? DateTime.Now.AddMinutes(timeValue) : DateTime.Now.AddSeconds(timeValue));
        }

        public static void Clear(string key)
        {
            _Cache.Remove(key);
        }
    }
}
