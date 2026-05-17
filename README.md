# 🛡️ Tower Defense — OOP Projesi

## Proje Özeti
Bu proje, C# programlama dili ve Avalonia UI kütüphanesi kullanılarak geliştirilmiş bir Kule Savunma (Tower Defense) oyunudur. İstanbul Medeniyet Üniversitesi Bilgisayar Mühendisliği 1. sınıf Nesne Yönelimli Programlama dersi dönem sonu projesi olarak Tahir Ege Baybür (25120205076) ve Yasin Söylemez (25120205088) tarafından hazırlanmıştır.

Oyunun temel amacı, dalgalar halinde gelen düşman birimlerine karşı stratejik noktalara farklı kuleler yerleştirmek, bu kuleleri geliştirmek ve haritayı savunmaktır. Proje, teorik Nesne Yönelimli Programlama (NYP) prensiplerini pratik bir oyun mimarisi üzerinde uygulamayı hedefler. Görev dağılımına son bölümünde yer verilmiştir.

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


---

## Oynanış

| Kontrol | Açıklama |
|---------|----------|
| Sol tık (panel) | Kule yerleştir / kule seç |
| Sağ tık | Seçimi iptal et |
| ⬆ Yükselt | Seçili kuleyi yükselt (max Sev.3) |
| 💰 Sat | Kuleyi sat, altın geri al |
| ▶ Dalga Başlat | Sonraki düşman dalgasını başlat |

---

## Proje Yapısı
```
TowerDefense/
├── Core/
│   ├── GameObject.cs      # Abstract base
│   └── Interfaces/
│       └── Interfaces.cs  # ITargetingStrategy, IUpgradeable, IWaveSpawner, ISaveable
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
└── Program.cs             # Giriş noktası
```

---

## Kurulum & Çalıştırma
```bash
git clone <repo>
cd TowerDefense
dotnet run
```


## İş Bölümü

### 1. Tahir Ege Baybür
Oyunun arka plan mekanikleri, veri yönetimi ve temel nesne yapılarının kurulması:
* **`GameManager.cs`:** Ana oyun döngüsü ve düşman dalga yönetimi.
* **`Enemy.cs`, `Tower.cs`, `Hero.cs`:** Karakter/kule taban sınıfları ve yetenek mantığı.
* **`MapManager.cs`:** Harita matrisi ve engel yerleşimi.
* **`ScoreManager.cs`:** Skor ve kaynak sistemi.
* **`Shot.cs`, `Bullet.cs`:** Mermi hareket fiziği ve hasar tespiti.

### 2. Yasin Söylemez
Oyuncu etkileşimi, görsel pencerelerin tasarımı ve seslerin entegre edilmesi:
* **`MainMenuWindow.axaml/.cs`:** Ana menü ve kullanıcı etkileşim arayüzü.
* **`GameWindow.axaml/.cs`:** Oyun içi kaynak gösterimleri (can, altın vb.).
* **`GameCanvas.cs`:** Giriş (input) kontrolleri ve çizim alanı yönetimi.
* **`AudioManager.cs`:** Arka plan müzikleri ve ses efektleri.
* **`Effects.cs`:** Patlama, can kaybı ve kule yükseltme gibi görsel efektler.

### 3. Yapay Zeka (AI) Destekli Geliştirme
Kod kalitesini artırmak, karakter/düşman çizimlerini oluşturmak ve matematiksel hesaplamalar (örneğin kulenin en yakın düşmanı seçerek saldırması) için yapay zeka araçlarından destek alınmıştır:
* **`HeroRenderer.cs`, `EnemyRenderer.cs`, `TowerRenderer.cs`:** Karakterlerin ekrana çizilmesini sağlayan render kodları.
* **Matematiksel Hesaplamalar:** Menzil kontrolü ve mermi açısı gibi geometrik hesaplamalar.
* **Optimizasyon ve Refactoring:** Kod iyileştirmeleri ve temiz kod düzenlemeleri.
* **Şablon Kod Üretimi:** Yeni nesne türleri için altyapı üretimi.
* **Hata Analizi:** Çalışma zamanı hatalarının tespiti ve çözümü.
