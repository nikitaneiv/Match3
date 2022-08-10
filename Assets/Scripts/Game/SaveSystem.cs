using UnityEngine;

namespace Game
{
    public class SaveSystem
    {
        private GameData _gameData = null;
            private const string DATA_KEY = "GameData";

            public GameData Data => _gameData;

            public void Initialize()
            {
                if (PlayerPrefs.HasKey(DATA_KEY))
                {
                    LoadData();
                }
                else
                {
                    _gameData = new GameData();
                }
            }

            public void LoadData()
            {
                string jsonData = PlayerPrefs.GetString(DATA_KEY);
                _gameData = JsonUtility.FromJson<GameData>(jsonData);
            }

            public void SaveData()
            {
                string jsonData = JsonUtility.ToJson(_gameData);
                PlayerPrefs.SetString(DATA_KEY, jsonData);
            }
    }
}