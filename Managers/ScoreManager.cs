using System.Text.Json;
using TowerDefense.Core;
using TowerDefense.Exceptions;

namespace TowerDefense.Managers
{
    public record ScoreRecord(string PlayerName, int Score, int Wave, DateTime Date);

    public class ScoreManager : ISaveable
    {
        private int _score;
        private List<ScoreRecord> _board = new();
        private string _playerName;
        private const string DefaultPath = "Data/scores.json";

        public int Score => _score;
        public IReadOnlyList<ScoreRecord> Leaderboard => _board.AsReadOnly();

        public ScoreManager(string playerName = "Oyuncu")
        { _playerName = playerName; TryLoad(); }

        public void Add(int pts) => _score += pts;
        public void Reset() => _score = 0;

        public void SaveHighScore(int wave)
        {
            _board.Add(new ScoreRecord(_playerName, _score, wave, DateTime.Now));
            _board = _board.OrderByDescending(r => r.Score).Take(10).ToList();
            Save(DefaultPath);
        }

        public void Save(string path)
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path) ?? "Data");
                File.WriteAllText(path, JsonSerializer.Serialize(_board, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) { throw new SaveLoadException("Skor kaydedilemedi", ex); }
        }

        public void Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return;
                _board = JsonSerializer.Deserialize<List<ScoreRecord>>(File.ReadAllText(path)) ?? new();
            }
            catch (Exception ex) { throw new SaveLoadException("Skor yüklenemedi", ex); }
        }

        private void TryLoad() { try { Load(DefaultPath); } catch { } }
    }
}
