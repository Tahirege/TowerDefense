# 🛡️ Tower Defense — OOP Projesi

## Proje Özeti
C# Avalonia UI ile geliştirilmiş tam özellikli Tower Defense oyunu.
Nesne Yönelimli Programlama prensiplerini gerçek bir oyun üzerinde uygular.

---

## OOP Prensipleri

### 1. Encapsulation (Kapsülleme)
- `GameObject`, `Tower`, `Enemy` sınıflarındaki tüm alanlar `private` — dışarıya yalnızca `property` ile açılır
- `Tower._damage`, `Enemy._currentHp` gibi kritik alanlar doğrudan erişime kapalı
- `GameManager` iç listelerini `IReadOnly` wrapper ile sunar

### 2. Inheritance (Kalıtım)
```
GameObject (abstract)
├── Tower (abstract)
│   ├── ArrowTower
│   ├── CannonTower
│   └── IceTower
├── Enemy (abstract)
│   ├── BasicEnemy
│   ├── FastEnemy
│   └── BossEnemy
└── Projectile (abstract)
    ├── Arrow
    ├── Cannonball
    └── IceProjectile
```

### 3. Polymorphism (Çok Biçimlilik)
- `GameObject.Update()` ve `Draw()` her sınıf tarafından farklı implemente edilir
- `Tower.TryShoot()` farklı `CreateProjectile()` üretir
- `Enemy.TakeDamage()` BossEnemy'de zırh hesabı ile override edilir
- `List<Tower>` üzerinde döngü — her kule kendi davranışını sergiler

### 4. Interface'ler (Sözleşmeler)
| Interface | Açıklama | Uygulayan |
|-----------|----------|-----------|
| `IUpgradeable` | Yükselt/Sat | Tower |
| `IWaveSpawner` | Dalga oluştur | GameManager |
| `ISaveable` | Kaydet/Yükle | GameManager, ScoreManager |

### 5. Exception Handling
| Exception | Ne Zaman Fırlatılır |
|-----------|---------------------|
| `GameException` | Genel oyun hataları |
| `TowerPlacementException` | Geçersiz kule konumu |
| `InsufficientGoldException` | Yetersiz altın |
| `SaveLoadException` | Dosya I/O hataları |

---

## Fonksiyonel Özellikler

### CRUD İşlemleri
- **Create**: Kule yerleştirme (haritaya tıkla)
- **Read**: Kule bilgisi, liderlik tablosu, oyun durumu
- **Update**: Kule yükseltme (3 seviye)
- **Delete**: Kule satma

### Veri Saklama
- Skor: `Data/scores.json` (JSON formatı)
- Oyun durumu: `Data/save.txt` (metin formatı)

---

## Oynanış

| Kontrol | Açıklama |
|---------|----------|
| Sol tık (panel) | Kule yerleştir / kule seç |
| Sağ tık | Seçimi iptal et |
| ⬆ Yükselt | Seçili kuleyi yükselt (max Sev.3) |
|  Sat | Kuleyi sat, altın geri al |
| ▶ Dalga Başlat | Sonraki düşman dalgasını başlat |

---

## Proje Yapısı
```
TowerDefense/
├── Core/
│   ├── GameObject.cs      # Abstract base
│   └── Interfaces.cs      # IUpgradeable, IWaveSpawner, ISaveable
├── Towers/
│   ├── Tower.cs           # Abstract tower
│   └── ConcreteTowers.cs  # Arrow, Cannon, Ice
├── Enemies/
│   ├── Enemy.cs           # Abstract enemy
│   └── ConcreteEnemies.cs # Basic, Fast, Boss
├── Projectiles/
│   └── Projectiles.cs     # Abstract + Arrow, Cannonball, Ice
├── Managers/
│   ├── GameManager.cs     # Ana oyun mantığı
│   ├── MapManager.cs      # Harita + grid
│   └── ScoreManager.cs    # Skor + JSON kayıt
├── Exceptions/
│   └── GameException.cs   # Özel exception hiyerarşisi
├── Forms/
│   └── GameForm.cs        # Windows Forms GUI
└── Program.cs             # Giriş noktası
```

---

## Kurulum & Çalıştırma
```bash
# Gereksinim: .NET 8 SDK (Windows)
git clone <repo>
cd TowerDefense
dotnet run
```
