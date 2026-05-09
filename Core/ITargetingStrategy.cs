using System.Collections.Generic;
using TowerDefense.Enemies;

namespace TowerDefense.Core
{
    public interface ITargetingStrategy
    {
        string Name { get; }
        Enemy? SelectTarget(List<Enemy> enemies, float towerX, float towerY, float range);
    }
}
