using System.Runtime.Versioning;

namespace TowerDefense.Audio
{
    public class AudioManager
    {
        public bool Enabled = true;
        public float Volume = 0.7f;

        public void Play(SoundEffect effect)
        {
            if (!Enabled) return;
            Task.Run(() => PlayInternal(effect));
        }

        public void PlayInternal(SoundEffect effect)
        {
            try
            {
                if (OperatingSystem.IsMacOS())
                    PlayMacOS(effect);
                else if (OperatingSystem.IsWindows())
                    PlayWindows(effect);
            }
            catch { }
        }

        [SupportedOSPlatform("windows")]
        public void PlayWindows(SoundEffect effect)
        {
            switch (effect)
            {
                case SoundEffect.EnemyDeath:  Console.Beep(450, 50); break;
                case SoundEffect.TowerPlace:  Console.Beep(600, 80); break;
                case SoundEffect.TowerShoot:  Console.Beep(1100, 35); break;
                case SoundEffect.WaveStart:   Console.Beep(350, 250); break;
                case SoundEffect.GameOver:    Console.Beep(250, 500); break;
                case SoundEffect.Victory:     Console.Beep(700, 200); Console.Beep(900, 300); break;
                case SoundEffect.Upgrade:     Console.Beep(850, 100); Console.Beep(1050, 100); break;
                case SoundEffect.Achievement: Console.Beep(1400, 300); break;
                case SoundEffect.LifeLost:    Console.Beep(180, 400); break;
            }
        }

        public void PlayMacOS(SoundEffect effect)
        {
            string sound = effect switch {
                SoundEffect.EnemyDeath  => "Tink",
                SoundEffect.TowerPlace  => "Pop",
                SoundEffect.WaveStart   => "Sosumi",
                SoundEffect.GameOver    => "Basso",
                SoundEffect.Victory     => "Hero",
                SoundEffect.Upgrade     => "Glass",
                SoundEffect.Achievement => "Purr",
                _ => "Tink"
            };
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "afplay",
                Arguments = $"/System/Library/Sounds/{sound}.aiff",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
        }
    }

    public enum SoundEffect
    {
        EnemyDeath, TowerPlace, TowerShoot,
        WaveStart, GameOver, Victory,
        Upgrade, Achievement, LifeLost
    }
}
