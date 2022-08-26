using System;
using Signals;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameManager: IInitializable, IDisposable 
    {
        private readonly SaveSystem _saveSystem;
        private readonly SignalBus _signalBus;
        private readonly BoardController _boardController;

        private const int SCORE_FOR_ELEMENT = 10;
        private int _score = -1;
        
        private int Score
        {
            get => _score;
            set
            {
                if (value == _score)
                    return;
                
                _score = value;
                _signalBus.Fire(new ScoreChangedSignal(_score));
            }
        }

        public GameManager(SaveSystem saveSystem, SignalBus signalBus, BoardController boardController)
        {
            _saveSystem = saveSystem;
            _signalBus = signalBus;
            _boardController = boardController;
        }

        public void Initialize()
        {
            _saveSystem.Initialize();
            SubscribeSignals();
            Score = _saveSystem.Data.Score;
            CreateGame();
        }
        
        public void Dispose()
        {
            UnSubscribeSignals();
        }

        private void SubscribeSignals()
        {
            _signalBus.Subscribe<CreateGameSignal>(CreateGame);
            _signalBus.Subscribe<RestartGameSignal>(OnRestart);
            _signalBus.Subscribe<OnBoardMatchSignal>(OnAddScore);
        }
        
        private void UnSubscribeSignals()
        {
            _signalBus.Unsubscribe<CreateGameSignal>(CreateGame);
            _signalBus.Unsubscribe<RestartGameSignal>(OnRestart);
            _signalBus.Unsubscribe<OnBoardMatchSignal>(OnAddScore);
        }

        private void CreateGame()
        {
            if (_saveSystem.Data.BoardState== null||_saveSystem.Data.BoardState.Length == 0)
            {
                _boardController.Initialize();
            }
            else
            {
                _boardController.Initialize(_saveSystem.Data.BoardState);
            }
        }

        private void OnAddScore(OnBoardMatchSignal signal)
        {
            Score += SCORE_FOR_ELEMENT * signal.Value;
        }


        private void OnRestart() 
        {
            _boardController.Restart();
            Debug.Log("Restart Game Invoke");
            if (Score > 0)
            {
                Score = 0;
            }
        }
        

    }
}