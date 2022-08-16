using System.Linq;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "ElementConfig", menuName = "Config/ElementConfig", order = 0)]
    public class ElementsConfig : ScriptableObject
    {
        [SerializeField] private ElementConfigItem[] configItem;

        public ElementConfigItem[] ConfigItem => configItem;

        public ElementConfigItem GetByKey(string key)
        {
            return configItem.FirstOrDefault(item => item.Key == key);
        }
    }

    [System.Serializable]
    public class ElementConfigItem
    {
        [SerializeField] private string key;
        [SerializeField] private Sprite sprite;

        public string Key => key;
        public Sprite Sprite => sprite;
    }
}