using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using TowerDefense.Enemies;
using TowerDefense.Shots;

namespace TowerDefense.Towers
{
    // ── Arrow Tower: fast, single target ─────────────────────────
    public class ArrowTower : Tower
    {
        public ArrowTower(float x, float y) : base(x, y, 100, 110f, 15, 1.5f)
        { TowerName = "Arrow Tower"; TowerColor = new SKColor(70, 140, 220); }

        public override Shot CreateShot(Enemy t) => new Arrow(X, Y, t, Damage);
    }

    // ── Cannon Tower: area damage ──────────────────────────────────
    public class CannonTower : Tower
    {
        public CannonTower(float x, float y) : base(x, y, 200, 90f, 50, 0.5f)
        { TowerName = "Cannon Tower"; TowerColor = new SKColor(200, 90, 50); }

        public override Shot CreateShot(Enemy t) => new Ball(X, Y, t, Damage, 40f);
    }

    // ── Ice Tower: slows enemies ───────────────────────────────────
    public class IceTower : Tower
    {
        public float SlowFactor { get; private set; } = 0.4f;
        public IceTower(float x, float y) : base(x, y, 150, 100f, 8, 0.8f)
        { TowerName = "Ice Tower"; TowerColor = new SKColor(100, 210, 255); }

        public override Shot CreateShot(Enemy t) => new Ice(X, Y, t, Damage, SlowFactor);
        public override void OnUpgrade() => SlowFactor = Math.Max(0.15f, SlowFactor - 0.08f);
    }

    // ── Laser Tower: continuous damage ────────────────────────────
    public class LaserTower : Tower
    {
        private float _beamPulse;
        public  float BeamPulse => _beamPulse;
        public Enemy? CurrentTarget { get; private set; }

        public LaserTower(float x, float y) : base(x, y, 250, 130f, 5, 10f)
        { TowerName = "Laser Tower"; TowerColor = new SKColor(255, 60, 200); }

        public override Shot CreateShot(Enemy t) { CurrentTarget = t; return new Laser(X, Y, t, Damage); }

        public override void Update(float dt)
        {
            base.Update(dt);
            _beamPulse += 0.1f;
            if (CurrentTarget != null && (!CurrentTarget.IsAlive || Dist(CurrentTarget) > Range))
                CurrentTarget = null;
        }

        private float Dist(Enemy e) { float dx = e.X-X, dy = e.Y-Y; return MathF.Sqrt(dx*dx+dy*dy); }
    }

    // ── Bomb Tower: huge area, slow ────────────────────────────────
    public class BombTower : Tower
    {
        private float _fuseTimer;
        public  float FuseTimer => _fuseTimer;

        public BombTower(float x, float y) : base(x, y, 300, 80f, 80, 0.3f)
        { TowerName = "Bomb Tower"; TowerColor = new SKColor(80, 80, 100); }

        public override void Update(float dt) { base.Update(dt); _fuseTimer += dt; }

        public override Shot CreateShot(Enemy t) => new Ball(X, Y, t, Damage, 70f);
    }

    // ── Sniper Tower: very long range, high damage ─────────────────
    public class SniperTower : Tower
    {
        public SniperTower(float x, float y) : base(x, y, 350, 220f, 120, 0.4f)
        { TowerName = "Sniper Tower"; TowerColor = new SKColor(60, 140, 60); }

        public override Shot CreateShot(Enemy t) => new Sniper(X, Y, t, Damage);
    }
}
