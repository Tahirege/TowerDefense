using SkiaSharp;
using TowerDefense.Audio;
using TowerDefense.Core;
using TowerDefense.Enemies;
using TowerDefense.Exceptions;
using TowerDefense.Maps;
using TowerDefense.Effects;
using TowerDefense.Shots;
using TowerDefense.Towers;

namespace TowerDefense.Managers
{
    public enum GameState { Playing, Paused, GameOver, Victory }

    public class GameManager : IWaveSpawner
    {
        public MapManager        Map          { get; }
        public ScoreManager      Score        { get; }
        public AudioManager      Audio        { get; }
        public Effects.Effects    Effects    { get; } = new();

        public List<Tower>      Towers      { get; } = new();
        public List<Enemy>      Enemies     { get; } = new();
        public List<Shot>       Shots       { get; } = new();

        public Hero             PlayerHero  { get; set; } = null!;
        public List<(float x, float y, float r, float t)> Explosions = new();

        public GameState State      { get; set; } = GameState.Playing;
        public int       Lives      { get; set; }
        public int       Gold       { get; set; }
        public int       CurrentWave { get; set; }
        public int       TotalKills  { get; set; }
        public int       TotalGoldEarned { get; set; }
        public float     WaveTimer   { get; set; }
        public int       MaxWaves    { get; set; }
        public bool      WaveInProgress { get; set; }

        public bool  Spawning;
        public int   ToSpawn, Spawned;
        public float SpawnTimer;
        public int   LivesAtWaveStart;
        public HashSet<string> PlacedTowerTypes = new();

        public event Action<string>?      OnMessage;
        public event Action?              OnGameOver;
        public event Action?              OnVictory;

        public bool IsWaveInProgress => Spawning || Enemies.Any(e => e.IsAlive);

        public GameManager(MapManager map, ScoreManager score, AudioManager? audio = null)
        {
            Map          = map;
            Score        = score;
            Audio        = audio        ?? new AudioManager();

            Ball.OnExplosion += HandleCannonExplosion;
            
            var def = MapLibrary.All.FirstOrDefault(m => m.Id == Map.CurrentMapId) ?? MapLibrary.All[0];
            MaxWaves = def.MaxWaves;

            ResetGame();
        }

        public void ResetGame()
        {
            Towers.Clear(); Enemies.Clear(); Shots.Clear();
            Explosions.Clear(); Effects.Update(999f);
            Lives = 20; Gold = 200; CurrentWave = 0;
            TotalKills = 0; TotalGoldEarned = 0; WaveTimer = 0f;
            State = GameState.Playing; Spawning = false;
            PlacedTowerTypes.Clear();
            Score.Reset();
            PlayerHero = new Hero(Map.MapWidth / 2f, Map.MapHeight / 2f);
            PlayerHero.Map = Map;
        }

        public void Update(float dt)
        {
            if (State == GameState.Paused) return;

            if (State == GameState.Playing || State == GameState.Victory)
                PlayerHero.Update(dt);

            if (State != GameState.Playing) return;

            if (WaveInProgress) WaveTimer += dt;

            UpdateSpawn(dt);
            UpdateEnemies(dt);
            UpdateTowers(dt);
            UpdateShots(dt);
            UpdateExplosions(dt);
            Effects.Update(dt);
            CheckWave();

            if (PlayerHero.Health <= 0) DoGameOver();

            if (PlayerHero.IsSwinging)
            {
                foreach (var e in Enemies.ToList())
                {
                    if (e.IsAlive && PlayerHero.TryHitEnemy(e))
                        e.TakeDamage(PlayerHero.AttackDamage);
                }
            }
        }

        public void UpdateSpawn(float dt)
        {
            if (!Spawning) return;
            SpawnTimer -= dt;
            if (SpawnTimer > 0 || Spawned >= ToSpawn) return;

            var mapDef = MapLibrary.All.FirstOrDefault(m => m.Id == Map.CurrentMapId) ?? MapLibrary.All[0];

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

            Enemy e;
            int roll = Random.Shared.Next(100);
            if      (CurrentWave >= 9 && roll < 15)  e = new BossEnemy(sx, sy);
            else if (CurrentWave >= 6 && roll < 12)  e = new HealerEnemy(sx, sy);
            else if (CurrentWave >= 4 && roll < 20)  e = new ArmoredEnemy(sx, sy);
            else if (CurrentWave >= 3 && roll < 25)  e = new BomberEnemy(sx, sy);
            else if (CurrentWave >= 2 && roll < 30)  e = new AggroEnemy(sx, sy);
            else if (CurrentWave >= 5 && roll < 40)  e = new FlyingEnemy(sx, sy);
            else                                      e = new BasicEnemy(sx, sy);

            if (e is BomberEnemy bomber)
                bomber.OnBomberExplode += HandleBomberExplosion;

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
            Spawned++;
            SpawnTimer = Math.Max(0.25f, 1.0f - CurrentWave * 0.055f);
            if (Spawned >= ToSpawn) Spawning = false;
        }

        public void UpdateEnemies(float dt)
        {
            foreach (var e in Enemies.ToList())
            {
                if (e is BomberEnemy bomber) bomber.CurrentTowerTarget = null;
                e.Update(dt);
                if (!e.IsAlive)
                {
                    if (e.ReachedEnd)
                    {
                        Lives--;
                        Effects.LifeLostEffect(Map.PathPoints[^1].x, Map.PathPoints[^1].y);
                        Audio.Play(SoundEffect.LifeLost);
                        OnMessage?.Invoke($"❤️ Düşman geçti! Can: {Lives}");
                        if (Lives <= 0) DoGameOver();
                    }
                    else
                    {
                        TotalKills++;
                        int reward = e.Reward;
                        Score.Add(reward * 10);
                        Gold += reward;
                        TotalGoldEarned += reward;
                        Effects.EnemyDeath(e.X, e.Y, e.EnemyColor);
                        Audio.Play(SoundEffect.EnemyDeath);
                    }
                    Enemies.Remove(e);
                }
            }
        }

        public void UpdateTowers(float dt)
        {
            foreach (var t in Towers.ToList())
            {
                if (!t.IsAlive)
                {
                    Towers.Remove(t);
                    Map.UnmarkOccupied(t.X, t.Y);
                    continue;
                }
                t.Update(dt);
                var s = t.TryShoot(Enemies);
                if (s != null) Shots.Add(s);
            }
        }

        public void UpdateShots(float dt)
        {
            foreach (var s in Shots.ToList())
            {
                s.Update(dt);
                if (s is Bullet bullet)
                {
                    foreach (var e in Enemies.Where(en => en.IsAlive))
                    {
                        float dx = s.X - e.X, dy = s.Y - e.Y;
                        if (MathF.Sqrt(dx*dx + dy*dy) < e.Size/2 + bullet.Radius)
                        {
                            e.TakeDamage(s.Damage);
                            s.Destroy();
                            break;
                        }
                    }
                }
                if (!s.IsAlive) Shots.Remove(s);
            }
        }

        public void UpdateExplosions(float dt)
        {
            for (int i = Explosions.Count - 1; i >= 0; i--)
            {
                var (x, y, r, t) = Explosions[i];
                if (t <= 0) { Explosions.RemoveAt(i); continue; }
                Explosions[i] = (x, y, r, t - dt);
            }
        }

        public void HandleCannonExplosion(float cx, float cy, float radius)
        {
            Explosions.Add((cx, cy, radius, 0.35f));
            Effects.Explode(cx, cy, new SKColor(255, 120, 0), 15);
            foreach (var e in Enemies.Where(e => e.IsAlive))
            {
                float dx = e.X - cx, dy = e.Y - cy;
                if (MathF.Sqrt(dx*dx + dy*dy) <= radius) e.TakeDamage(45);
            }
        }

        public void HandleAggroExplosion(float cx, float cy, float radius, int damage)
        {
            Explosions.Add((cx, cy, radius, 0.5f));
            Effects.Explode(cx, cy, SKColors.DarkRed, 18);
            ApplyExplosionDamage(cx, cy, radius, damage, "Aggro Warrior");
        }

        public void HandleBomberExplosion(float cx, float cy, float radius, int damage)
        {
            Explosions.Add((cx, cy, radius, 0.5f));
            Effects.Explode(cx, cy, new SKColor(100, 220, 60), 18);
            ApplyExplosionDamage(cx, cy, radius, damage, "Bomber");
        }

        public void ApplyExplosionDamage(float cx, float cy, float radius, int damage, string source)
        {
            foreach (var e in Enemies.Where(e => e.IsAlive))
            {
                float dx = e.X - cx, dy = e.Y - cy;
                if (MathF.Sqrt(dx*dx + dy*dy) <= radius) e.TakeDamage(damage);
            }
            foreach (var t in Towers.ToList())
            {
                float dx = t.X - cx, dy = t.Y - cy;
                if (MathF.Sqrt(dx*dx + dy*dy) <= radius)
                {
                    t.TakeDamage(damage);
                    if (!t.IsAlive) OnMessage?.Invoke($"🔥 {t.TowerName} destroyed by {source}!");
                }
            }
        }

        public void CheckWave()
        {
            if (!Spawning && !Enemies.Any(e => e.IsAlive) && CurrentWave > 0 && WaveInProgress)
            {
                WaveInProgress = false;
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

        public void SpawnWave()
        {
            if (State != GameState.Playing || IsWaveInProgress || CurrentWave >= MaxWaves) return;
            CurrentWave++;
            ToSpawn = 8 + CurrentWave * 3;
            Spawned = 0; Spawning = true; SpawnTimer = 0.5f;
            WaveInProgress = true; WaveTimer = 0f;
            LivesAtWaveStart = Lives;
            Audio.Play(SoundEffect.WaveStart);
            OnMessage?.Invoke($"🌊 Wave {CurrentWave} incoming! ({ToSpawn} enemies)");
        }

        public bool IsWaveComplete() => !WaveInProgress;

        public void PlaceTower(Tower tower)
        {
            if (Gold < tower.Cost) throw new InsufficientGoldException(tower.Cost, Gold);
            if (!Map.IsBuildable(tower.X, tower.Y)) throw new TowerPlacementException("Bu alana kule inşa edilemez!");
            float distToHero = MathF.Sqrt(MathF.Pow(PlayerHero.X - tower.X, 2) + MathF.Pow(PlayerHero.Y - tower.Y, 2));
            if (distToHero < 30f) throw new TowerPlacementException("Hero bu alanda duruyor!");
            Gold -= tower.Cost;
            Map.MarkOccupied(tower.X, tower.Y);
            Towers.Add(tower);
            Score.Add(5);
            Effects.UpgradeEffect(tower.X, tower.Y);
            Audio.Play(SoundEffect.TowerPlace);
            PlacedTowerTypes.Add(tower.GetType().Name);
        }

        public void UpgradeTower(Tower tower)
        {
            if (Gold < tower.UpgradeCost) throw new InsufficientGoldException(tower.UpgradeCost, Gold);
            Gold -= tower.UpgradeCost;
            tower.Upgrade();
            Score.Add(20);
            Effects.UpgradeEffect(tower.X, tower.Y);
            Audio.Play(SoundEffect.Upgrade);
        }

        public void SellTower(Tower tower)
        {
            Gold += tower.SellValue;
            tower.Sell();
            Map.UnmarkOccupied(tower.X, tower.Y);
            Towers.Remove(tower);
        }

        public void TogglePause() => State = State == GameState.Paused ? GameState.Playing : GameState.Paused;

        public void HeroMeleeAttack(float targetX, float targetY)
        {
            if (!PlayerHero.TrySwing(targetX, targetY)) return;
            Audio.Play(SoundEffect.TowerShoot);
        }

        public void Draw(SKCanvas canvas)
        {
            Map.Draw(canvas);
            foreach (var t in Towers) t.Draw(canvas);
            foreach (var e in Enemies) e.Draw(canvas);
            PlayerHero.Draw(canvas);
            foreach (var s in Shots) s.Draw(canvas);
            Effects.Draw(canvas);
            DrawExplosions(canvas);
            if (State == GameState.Paused) DrawPauseOverlay(canvas);
        }

        public void DrawExplosions(SKCanvas canvas)
        {
            foreach (var (x, y, r, t) in Explosions)
            {
                float a = t / 0.35f;
                using var p = new SKPaint { Color = new SKColor(255, 120, 0, (byte)(180*a)), IsAntialias = true };
                canvas.DrawCircle(x, y, r, p);
            }
        }

        public void DrawPauseOverlay(SKCanvas canvas)
        {
            using var bg = new SKPaint { Color = new SKColor(0, 0, 0, 120) };
            canvas.DrawRect(0, 0, Map.MapWidth, Map.MapHeight, bg);
            using var tp = new SKPaint { Color = SKColors.White, IsAntialias = true, TextSize = 36f, TextAlign = SKTextAlign.Center };
            canvas.DrawText("⏸ PAUSED", Map.MapWidth / 2f, Map.MapHeight / 2f, tp);
        }

        public void DoGameOver()
        {
            State = GameState.GameOver;
            Score.SaveHighScore(CurrentWave);
            Audio.Play(SoundEffect.GameOver);
            OnGameOver?.Invoke();
        }
    }
}
