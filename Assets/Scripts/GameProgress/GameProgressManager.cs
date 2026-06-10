using System;
using Tools;

namespace GameProgress
{
    public class GameProgressManager : MonoSingleton<GameProgressManager>
    {
        public string fileName = "game_progress.json";
        private GameProgressData _gameProgressData;

        public event Action OnGameProgressChanged;

        protected override void Awake()
        {
            base.Awake();
            LoadGameProgress();
        }

        public void LoadGameProgress()
        {
            if (!PersistentJsonStorage.TryLoad(fileName, out _gameProgressData) || _gameProgressData == null)
            {
                _gameProgressData = new GameProgressData();
                PersistentJsonStorage.Save(fileName, _gameProgressData);
            }
            NormalizeData();
            OnGameProgressChanged?.Invoke();
        }

        public void SaveGameProgress()
        {
            if (_gameProgressData == null)
            {
                _gameProgressData = new GameProgressData();
            }

            NormalizeData();
            PersistentJsonStorage.Save(fileName, _gameProgressData);
            OnGameProgressChanged?.Invoke();
        }

        public void ResetToDefault()
        {
            _gameProgressData = new GameProgressData();
            SaveGameProgress();
        }

        public GameProgressData GetGameProgress()
        {
            _gameProgressData ??= new GameProgressData();

            return _gameProgressData;
        }

        public bool IsNewPlayer()
        {
            return GetGameProgress().isNewPlayer;
        }

        public void SetIsNewPlayer(bool isNewPlayer)
        {
            EnsureData();
            _gameProgressData.isNewPlayer = isNewPlayer;
            SaveGameProgress();
        }

        public System.Collections.Generic.List<int> GetClearedLevels()
        {
            return GetGameProgress().clearedLevelIndices;
        }

        public bool IsLevelCleared(int levelIndex)
        {
            if (levelIndex < 0) return false;
            var list = GetGameProgress().clearedLevelIndices;
            return list != null && list.Contains(levelIndex);
        }

        public void SetLevelCleared(int levelIndex, bool cleared = true)
        {
            if (levelIndex < 0) return;
            EnsureData();

            if (_gameProgressData.clearedLevelIndices == null)
                _gameProgressData.clearedLevelIndices = new System.Collections.Generic.List<int>();

            if (cleared)
            {
                if (!_gameProgressData.clearedLevelIndices.Contains(levelIndex))
                    _gameProgressData.clearedLevelIndices.Add(levelIndex);

                _gameProgressData.isNewPlayer = false;
            }
            else
            {
                _gameProgressData.clearedLevelIndices.Remove(levelIndex);
            }

            SaveGameProgress();
        }

        private void EnsureData()
        {
            if (_gameProgressData == null)
            {
                _gameProgressData = new GameProgressData();
            }
        }

        private void NormalizeData()
        {
            EnsureData();

            if (_gameProgressData.clearedLevelIndices == null)
            {
                _gameProgressData.clearedLevelIndices = new System.Collections.Generic.List<int>();
                return;
            }

            // remove negatives and duplicates
            var cleaned = new System.Collections.Generic.List<int>();
            foreach (var idx in _gameProgressData.clearedLevelIndices)
            {
                if (idx >= 0 && !cleaned.Contains(idx)) cleaned.Add(idx);
            }

            _gameProgressData.clearedLevelIndices = cleaned;
        }
    }
}

