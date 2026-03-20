using System;
using System.Collections.Generic;

namespace UI
{
    public class UIEventManager
    {
        private static Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();
    
        public static void AddListener(string eventName, Action<object> listener)
        {
            if (!eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = null;
            }
            eventDictionary[eventName] += listener;
        }
    
        public static void RemoveListener(string eventName, Action<object> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] -= listener;
            }
        }
    
        public static void TriggerEvent(string eventName, object data = null)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName]?.Invoke(data);
            }
        }
    }
}

// // 预定义事件类型
// public static class UIEventType
// {
//     public const string PLAYER_INFO_UPDATE = "PlayerInfoUpdate";
//     public const string INVENTORY_UPDATE = "InventoryUpdate";
//     public const string QUEST_UPDATE = "QuestUpdate";
//     public const string DIALOG_SHOW = "DialogShow";
// }