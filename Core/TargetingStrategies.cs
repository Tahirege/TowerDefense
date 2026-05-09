using System;
using System.Collections.Generic;
using System.Linq;
using TowerDefense.Enemies;

namespace TowerDefense.Core
{
    public class FirstTargeting : ITargetingStrategy
    {
        public string Name => "First";
        public Enemy? SelectTarget(List<Enemy> enemies, float towerX, float towerY, float range) =>
            enemies.Where(e => e.IsAlive && Dist(e, towerX, towerY) <= range)
                   .OrderByDescending(e => e.PathProgress).FirstOrDefault();

        private float Dist(Enemy e, float x, float y) { float dx = e.X-x, dy = e.Y-y; return MathF.Sqrt(dx*dx+dy*dy); }
    }

    public class LastTargeting : ITargetingStrategy
    {
        public string Name => "Last";
        public Enemy? SelectTarget(List<Enemy> enemies, float towerX, float towerY, float range) =>
            enemies.Where(e => e.IsAlive && Dist(e, towerX, towerY) <= range)
                   .OrderBy(e => e.PathProgress).FirstOrDefault();

        private float Dist(Enemy e, float x, float y) { float dx = e.X-x, dy = e.Y-y; return MathF.Sqrt(dx*dx+dy*dy); }
    }

    public class StrongestTargeting : ITargetingStrategy
    {
        public string Name => "Strongest";
        public Enemy? SelectTarget(List<Enemy> enemies, float towerX, float towerY, float range) =>
            enemies.Where(e => e.IsAlive && Dist(e, towerX, towerY) <= range)
                   .OrderByDescending(e => e.Hp).FirstOrDefault();

        private float Dist(Enemy e, float x, float y) { float dx = e.X-x, dy = e.Y-y; return MathF.Sqrt(dx*dx+dy*dy); }
    }

    public class WeakestTargeting : ITargetingStrategy
    {
        public string Name => "Weakest";
        public Enemy? SelectTarget(List<Enemy> enemies, float towerX, float towerY, float range) =>
            enemies.Where(e => e.IsAlive && Dist(e, towerX, towerY) <= range)
                   .OrderBy(e => e.Hp).FirstOrDefault();

        private float Dist(Enemy e, float x, float y) { float dx = e.X-x, dy = e.Y-y; return MathF.Sqrt(dx*dx+dy*dy); }
    }

    public class ClosestTargeting : ITargetingStrategy
    {
        public string Name => "Closest";
        public Enemy? SelectTarget(List<Enemy> enemies, float towerX, float towerY, float range)
        {
            Enemy? best = null;
            float minDist = float.MaxValue;
            foreach (var e in enemies)
            {
                if (!e.IsAlive) continue;
                float d = Dist(e, towerX, towerY);
                if (d <= range && d < minDist)
                {
                    minDist = d;
                    best = e;
                }
            }
            return best;
        }

        private float Dist(Enemy e, float x, float y) { float dx = e.X-x, dy = e.Y-y; return MathF.Sqrt(dx*dx+dy*dy); }
    }
}
