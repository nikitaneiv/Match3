using UnityEngine;
using Zenject;

namespace Game
{
    public class GameManager: IInitializable
    {
        private readonly SaveSystem _saveSystem;

        public GameManager(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
        }

        public void Initialize()
        {
            _saveSystem.Initialize();
            Debug.Log(_saveSystem.Data);
        }
    }
}