using System.Runtime.Versioning;

namespace TowerDefense.Audio
{
    /// <summary>
    /// Procedurel ses üreteci — harici dosya gerektirmez.
    /// System.Media (Windows) veya basit beep benzeri cross-platform çözüm.
    /// Gerçek ses için NAudio/OpenAL entegrasyonu yapılabilir.
    /// </summary>
    public class AudioManager
    {
        private bool _enabled = true;
        private float _volume = 0.7f;

        public bool Enabled { get => _enabled; set => _enabled = value; }
        public float Volume  { get => _volume; set => _volume = Math.Clamp(value, 0f, 1f); }

        // Ses çalma — platform bağımsız (gelecekte NAudio ile genişletilebilir)
        public void Play(SoundEffect effect)
        {
            if (!_enabled) return;
            Task.Run(() => PlayInternal(effect));
        }

        private void PlayInternal(SoundEffect effect)
        {
            try
            {
                if (OperatingSystem.IsMacOS())
                    PlayMacOS(effect);
                else if (OperatingSystem.IsWindows())
                    PlayWindows(effect);
                else if (OperatingSystem.IsLinux())
                    PlayLinux(effect);
            }
            catch { /* Ses hatası oyunu durdurmamalı */ }
        }

        [SupportedOSPlatform("windows")]
        private void PlayWindows(SoundEffect effect)
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

        private void PlayLinux(SoundEffect effect)
        {
            // Linux'ta Console.Beep genellikle sistem zilini (beep) çalar.
            // Bazen çalışmayabilir, fallback olarak \a (bell) gönderilebilir.
            Console.Write("\a");
        }

        private void PlayMacOS(SoundEffect effect)
        {
            // macOS say komutu ile sistem sesi
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
