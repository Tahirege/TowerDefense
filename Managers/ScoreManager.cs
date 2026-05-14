using System.Text.Json;
using TowerDefense.Core;
using TowerDefense.Exceptions;

namespace TowerDefense.Managers
{
    public record ScoreRecord(string PlayerName, int Score, int Wave, DateTime Date);

    public class ScoreManager : ISaveable
    {
        public int CurrentScore;
        public List<ScoreRecord> LeaderboardList = new();
        public string PlayerName;
        public const string DefaultPath = "Data/scores.json";

        public int Score => CurrentScore;
        public List<ScoreRecord> Leaderboard => LeaderboardList;

        public ScoreManager(string playerName = "Oyuncu")
        { PlayerName = playerName; TryLoad(); }

        public void Add(int pts) => CurrentScore += pts;
        public void Reset() => CurrentScore = 0;

        public void SaveHighScore(int wave)
        {
            LeaderboardList.Add(new ScoreRecord(PlayerName, CurrentScore, wave, DateTime.Now));
            LeaderboardList = LeaderboardList.OrderByDescending(r => r.Score).Take(10).ToList();
            Save(DefaultPath);
        }

        public void Save(string path)
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path) ?? "Data");
                File.WriteAllText(path, JsonSerializer.Serialize(LeaderboardList, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) { throw new SaveLoadException("Skor kaydedilemedi", ex); }
        }

        public void Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return;
                LeaderboardList = JsonSerializer.Deserialize<List<ScoreRecord>>(File.ReadAllText(path)) ?? new();
            }
            catch (Exception ex) { throw new SaveLoadException("Skor yüklenemedi", ex); }
        }

        public void TryLoad() { try { Load(DefaultPath); } catch { } }
    }
}
