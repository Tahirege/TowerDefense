using SkiaSharp;
using TowerDefense.Audio;
using TowerDefense.Core;
using TowerDefense.Enemies;
using TowerDefense.Exceptions;
using TowerDefense.Maps;
using TowerDefense.Particles;
using TowerDefense.Projectiles;
using TowerDefense.Towers;

namespace TowerDefense.Managers
{
    public enum GameState { Playing, Paused, GameOver, Victory }

    public class GameManager : IWaveSpawner
    {
        // ── Sub-managers ──────────────────────────────────────
        public MapManager        Map          { get; }
        public ScoreManager      Score        { get; }
        public AudioManager      Audio        { get; }
        public ParticleSystem    Particles    { get; } = new();

        // ── Game objects ──────────────────────────────────────
        public List<Tower>      Towers      { get; } = new();
        public List<Enemy>      Enemies     { get; } = new();
        public List<Projectile> Projectiles { get; } = new();

        // ── Hero & Control ────────────────────────────────────
        public Hero             PlayerHero  { get; private set; } = null!;

        // ── Explosions ────────────────────────────────────────
        private List<(float x, float y, float r, float t)> _explosions = new();

        // ── State ─────────────────────────────────────────────
        public GameState State      { get; private set; } = GameState.Playing;
        public int       Lives      { get; private set; }
        public int       Gold       { get; private set; }
        public int       CurrentWave { get; private set; }
        public int       TotalKills  { get; private set; }
        public int       TotalGoldEarned { get; private set; }
        public float     WaveTimer   { get; private set; }
        public int       MaxWaves    { get; private set; }
        public bool      WaveInProgress { get; private set; }

        private bool  _spawning;
        private int   _toSpawn, _spawned;
        private float _spawnTimer;
        private int   _livesAtWaveStart;
        private readonly HashSet<string> _placedTowerTypes = new();

        // ── Events ────────────────────────────────────────────
        public event Action<string>?      OnMessage;
        public event Action?              OnGameOver;
        public event Action?              OnVictory;

        public bool IsWaveInProgress => _spawning || Enemies.Any(e => e.IsAlive);


        public GameManager(MapManager map, ScoreManager score, AudioManager? audio = null)
        {
            Map          = map;
            Score        = score;
            Audio        = audio        ?? new AudioManager();

            Cannonball.OnExplosion += HandleCannonExplosion;
            
            var def = MapLibrary.All.FirstOrDefault(m => m.Id == Map.CurrentMapId) ?? MapLibrary.All[0];
            MaxWaves = def.MaxWaves;

            ResetGame();
        }

        public void ResetGame()
        {
            Towers.Clear(); Enemies.Clear(); Projectiles.Clear();
            _explosions.Clear(); Particles.Update(999f); // temizle
            Lives = 20; Gold = 200; CurrentWave = 0;
            TotalKills = 0; TotalGoldEarned = 0; WaveTimer = 0f;
            State = GameState.Playing; _spawning = false;
            _placedTowerTypes.Clear();
            Score.Reset();
            PlayerHero = new Hero(Map.MapWidth / 2f, Map.MapHeight / 2f);
        }

        // ── Update ────────────────────────────────────────────
        public void Update(float dt)
        {
            if (State == GameState.Paused) return;

            // Allow hero movement in Victory state too
            if (State == GameState.Playing || State == GameState.Victory)
            {
                PlayerHero.Update(dt, Map);
            }

            if (State != GameState.Playing) return;

            if (WaveInProgress) WaveTimer += dt;

            UpdateSpawn(dt);
            UpdateEnemies(dt);
            UpdateTowers(dt);
            UpdateProjectiles(dt);
            UpdateExplosions(dt);
            Particles.Update(dt);
            CheckWave();

            if (PlayerHero.Health <= 0) DoGameOver();

            // Melee hits check while swinging
            if (PlayerHero.IsSwinging)
            {
                foreach (var e in Enemies.ToList())
                {
                    if (e.IsAlive && PlayerHero.TryHitEnemy(e))
                        e.TakeDamage(PlayerHero.AttackDamage);
                }
            }
        }

        private void UpdateSpawn(float dt)
        {
            if (!_spawning) return;
            _spawnTimer -= dt;
            if (_spawnTimer > 0 || _spawned >= _toSpawn) return;

            var mapDef = MapLibrary.All.FirstOrDefault(m => m.Id == Map.CurrentMapId)
                         ?? MapLibrary.All[0];
            float speedMult = mapDef.EnemySpeedMultiplier;

            float sx, sy;
            if (Map.CurrentMapId == "linedefense")
            {
                int col = Random.Shared.Next(0, MapManager.Cols);
                sx = col * MapManager.CellSize + MapManager.CellSize / 2f;
                sy = -20f;
            }
            else
            {
                sx = Map.PathPoints[0].x;
                sy = Map.PathPoints[0].y;
            }

            // HP scales with wave number
            float hpMult = 1f + (CurrentWave - 1) * 0.20f;  // +20% HP per wave

            Enemy e;
            int roll = Random.Shared.Next(100);
            if      (CurrentWave >= 9 && roll < 15)  e = new BossEnemy(sx, sy);
            else if (CurrentWave >= 6 && roll < 12)  e = new HealerEnemy(sx, sy);
            else if (CurrentWave >= 4 && roll < 20)  e = new ArmoredEnemy(sx, sy);
            else if (CurrentWave >= 3 && roll < 25)  e = new BomberEnemy(sx, sy);
            else if (CurrentWave >= 2 && roll < 30)  e = new AggroEnemy(sx, sy);
            else if (CurrentWave >= 5 && roll < 40)  e = new FlyingEnemy(sx, sy);
            else                                      e = new BasicEnemy(sx, sy);

            // Wire bomber explosion to tower damage
            if (e is BomberEnemy bomber)
                bomber.OnBomberExplode += HandleBomberExplosion;

            // Wire up references
            e.TargetHero = PlayerHero;
            if (e is HealerEnemy healer) healer.NearbyEnemies = Enemies;
            if (e is AggroEnemy aggro)
            {
                aggro.Map = Map;
                aggro.OnAggroExplode += HandleAggroExplosion;
            }

            if (Map.CurrentMapId == "linedefense")
            {
                var fakePath = new List<(float x, float y)> { (sx, sy), (sx, MapManager.Rows * MapManager.CellSize + 20f) };
                e.SetPath(fakePath);
            }
            else
            {
                e.SetPath(Map.PathPoints);
            }

            Enemies.Add(e);
            _spawned++;
            // Faster spawn rate as waves progress
            _spawnTimer = Math.Max(0.25f, 1.0f - CurrentWave * 0.055f);
            if (_spawned >= _toSpawn) _spawning = false;
        }

        private void UpdateEnemies(float dt)
        {
            foreach (var e in Enemies.ToList())
            {
                // Update specific enemy targets
                if (e is BomberEnemy bomber)
                {
                    // Bombers no longer target invincible towers
                    bomber.CurrentTowerTarget = null;
                }

                e.Update(dt);
                if (e.IsAlive) continue;

                if (e.ReachedEnd)
                {
                    Lives--;
                    Particles.LifeLostEffect(Map.PathPoints[^1].x, Map.PathPoints[^1].y);
                    Audio.Play(SoundEffect.LifeLost);
                    OnMessage?.Invoke($"❤️ Düşman geçti! Can: {Lives}");
                    if (Lives <= 0) DoGameOver();
                }
                else
                {
                    // Öldürüldü
                    TotalKills++;
                    int reward = e.Reward;
                    Score.Add(reward * 10);
                    Gold += reward;
                    TotalGoldEarned += reward;
                    Particles.EnemyDeath(e.X, e.Y, e.EnemyColor);
                    Audio.Play(SoundEffect.EnemyDeath);

                    TotalGoldEarned += reward;
                    Particles.EnemyDeath(e.X, e.Y, e.EnemyColor);
                }
                Enemies.Remove(e);
            }
        }

        private void UpdateTowers(float dt)
        {
            foreach (var t in Towers.ToList())
            {
                if (!t.IsAlive)
                {
                    Towers.Remove(t);
                    Map.UnmarkOccupied(t.X, t.Y); // Ensure we free up the space
                    continue;
                }
                t.Update(dt);
                var p = t.TryShoot(Enemies);
                if (p != null) Projectiles.Add(p);
            }
        }

        private void UpdateProjectiles(float dt)
        {
            foreach (var p in Projectiles.ToList())
            {
                p.Update(dt);
                
                if (p is DirectionalProjectile dp)
                {
                    foreach (var e in Enemies.Where(en => en.IsAlive))
                    {
                        float dx = p.X - e.X;
                        float dy = p.Y - e.Y;
                        float dist = MathF.Sqrt(dx*dx + dy*dy);
                        if (dist < e.Size/2 + 5)
                        {
                            e.TakeDamage(p.Damage);
                            p.Destroy();
                            break;
                        }
                    }
                }
                
                if (!p.IsAlive) Projectiles.Remove(p);
            }
        }

        private void UpdateExplosions(float dt)
        {
            for (int i = _explosions.Count - 1; i >= 0; i--)
            {
                var (x, y, r, t) = _explosions[i];
                if (t <= 0) { _explosions.RemoveAt(i); continue; }
                _explosions[i] = (x, y, r, t - dt);
            }
        }

        private void HandleCannonExplosion(float cx, float cy, float radius)
        {
            _explosions.Add((cx, cy, radius, 0.35f));
            Particles.Explode(cx, cy, new SKColor(255, 120, 0), 15);
            foreach (var e in Enemies.Where(e => e.IsAlive))
            {
                float dx = e.X - cx, dy = e.Y - cy;
                if (MathF.Sqrt(dx*dx + dy*dy) <= radius) e.TakeDamage(45);
            }
        }

        private void HandleAggroExplosion(float cx, float cy, float radius, int damage)
        {
            _explosions.Add((cx, cy, radius, 0.5f));
            Particles.Explode(cx, cy, SKColors.DarkRed, 18);
            ApplyExplosionDamage(cx, cy, radius, damage, "Aggro Warrior");
        }

        private void HandleBomberExplosion(float cx, float cy, float radius, int damage)
        {
            _explosions.Add((cx, cy, radius, 0.5f));
            Particles.Explode(cx, cy, new SKColor(100, 220, 60), 18);
            ApplyExplosionDamage(cx, cy, radius, damage, "Bomber");
        }

        private void ApplyExplosionDamage(float cx, float cy, float radius, int damage, string source)
        {
            // Damage nearby enemies
            foreach (var e in Enemies.Where(e => e.IsAlive))
            {
                float dx = e.X - cx, dy = e.Y - cy;
                if (MathF.Sqrt(dx*dx + dy*dy) <= radius)
                    e.TakeDamage(damage);
            }

            // Damage nearby towers
            foreach (var t in Towers.ToList())
            {
                float dx = t.X - cx, dy = t.Y - cy;
                if (MathF.Sqrt(dx*dx + dy*dy) <= radius)
                {
                    t.TakeDamage(damage);
                    if (!t.IsAlive)
                    {
                        OnMessage?.Invoke($"🔥 {t.TowerName} destroyed by {source}!");
                    }
                }
            }
        }

        private void CheckWave()
        {
            if (!_spawning && !Enemies.Any(e => e.IsAlive) && CurrentWave > 0 && WaveInProgress)
            {
                WaveInProgress = false;
                // Speed achievement (30 saniyede bitirme)
                if (WaveTimer < 30f) { } // Achievement removed

                if (CurrentWave >= MaxWaves)
                {
                    State = GameState.Victory;
                    Score.SaveHighScore(CurrentWave);
                    Audio.Play(SoundEffect.Victory);
                    OnVictory?.Invoke();
                }
                else
                {
                    Audio.Play(SoundEffect.WaveStart);
                    OnMessage?.Invoke($"✅ Dalga {CurrentWave} tamamlandı! +50 Bonus altın");
                    Gold += 50;
                }
            }
        }

        // ── IWaveSpawner ──────────────────────────────────────
        public void SpawnWave()
        {
            if (State != GameState.Playing) return;
            if (IsWaveInProgress) throw new GameException("Wave in progress!");
            if (CurrentWave >= MaxWaves) return;

            CurrentWave++;
            // Slightly easier start: 8 base + 3 per wave
            _toSpawn = 8 + CurrentWave * 3;
            _spawned = 0; _spawning = true; _spawnTimer = 0.5f;
            WaveInProgress = true; WaveTimer = 0f;
            _livesAtWaveStart = Lives;

            Audio.Play(SoundEffect.WaveStart);
            OnMessage?.Invoke($"🌊 Wave {CurrentWave} incoming! ({_toSpawn} enemies)");
        }

        public bool IsWaveComplete() => !IsWaveInProgress;

        // ── Tower CRUD ────────────────────────────────────────
        public void PlaceTower(Tower tower)
        {
            if (Gold < tower.Cost)
                throw new InsufficientGoldException(tower.Cost, Gold);
            if (!Map.IsBuildable(tower.X, tower.Y))
                throw new TowerPlacementException("Bu alana kule inşa edilemez!");

            // Prevent building on top of the hero
            float distToHero = MathF.Sqrt(MathF.Pow(PlayerHero.X - tower.X, 2) + MathF.Pow(PlayerHero.Y - tower.Y, 2));
            if (distToHero < 30f)
                throw new TowerPlacementException("Hero bu alanda duruyor!");

            Gold -= tower.Cost;
            Map.MarkOccupied(tower.X, tower.Y);
            Towers.Add(tower);
            Score.Add(5);
            Particles.UpgradeEffect(tower.X, tower.Y);
            Audio.Play(SoundEffect.TowerPlace);

            _placedTowerTypes.Add(tower.GetType().Name);
        }

        public void UpgradeTower(Tower tower)
        {
            if (Gold < tower.UpgradeCost)
                throw new InsufficientGoldException(tower.UpgradeCost, Gold);
            Gold -= tower.UpgradeCost;
            tower.Upgrade();
            Score.Add(20);
            Particles.UpgradeEffect(tower.X, tower.Y);
            Audio.Play(SoundEffect.Upgrade);
        }

        public void SellTower(Tower tower)
        {
            Gold += tower.SellValue;
            tower.Sell();
            Map.UnmarkOccupied(tower.X, tower.Y); // Free up the cell
            Towers.Remove(tower);
        }


        // ── Hero & Hero Control ──────────────────────────────

        public void TogglePause()
        {
            State = State == GameState.Paused ? GameState.Playing : GameState.Paused;
        }

        public void HeroMeleeAttack(float targetX, float targetY)
        {
            if (!PlayerHero.TrySwing(targetX, targetY)) return;
            Audio.Play(SoundEffect.TowerShoot);
        }

        // ── Draw ──────────────────────────────────────────────
        public void Draw(SKCanvas canvas)
        {
            Map.Draw(canvas);
            foreach (var t in Towers)
            {
                t.Draw(canvas);
            }
            foreach (var e in Enemies)     e.Draw(canvas);
            PlayerHero.Draw(canvas);
            foreach (var p in Projectiles)
            {
                if (p is DirectionalProjectile dp) dp.DrawHeroProj(canvas);
                else p.Draw(canvas);
            }
            Particles.Draw(canvas);
            DrawExplosions(canvas);
            if (State == GameState.Paused) DrawPauseOverlay(canvas);
        }

        private void DrawExplosions(SKCanvas canvas)
        {
            foreach (var (x, y, r, t) in _explosions)
            {
                float a = t / 0.35f;
                using var p = new SKPaint { Color = new SKColor(255, 120, 0, (byte)(180*a)), IsAntialias = true };
                canvas.DrawCircle(x, y, r, p);
            }
        }

        private void DrawPauseOverlay(SKCanvas canvas)
        {
            using var bg = new SKPaint { Color = new SKColor(0, 0, 0, 120) };
            canvas.DrawRect(0, 0, Map.MapWidth, Map.MapHeight, bg);
            using var tp = new SKPaint { Color = SKColors.White, IsAntialias = true,
                TextSize = 36f, TextAlign = SKTextAlign.Center };
            canvas.DrawText("⏸ PAUSED", Map.MapWidth / 2f, Map.MapHeight / 2f, tp);
        }

        private void DoGameOver()
        {
            State = GameState.GameOver;
            Score.SaveHighScore(CurrentWave);
            Audio.Play(SoundEffect.GameOver);
            OnGameOver?.Invoke();
        }
    }
}
