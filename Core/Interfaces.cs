using System.Collections.Generic;
using TowerDefense.Enemies;

namespace TowerDefense.Core
{
    public interface IUpgradeable
    {
        int Level { get; }
        int UpgradeCost { get; }
        int SellValue { get; }
        void Upgrade();
        void Sell();
    }

    public interface IWaveSpawner
    {
        int CurrentWave { get; }
        void SpawnWave();
        bool IsWaveComplete();
    }

    public interface ISaveable
    {
        void Save(string filePath);
        void Load(string filePath);
    }
}
