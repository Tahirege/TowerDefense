using SkiaSharp;

namespace TowerDefense.Core
{
    /// <summary>
    /// Oyundaki tüm nesnelerin temel soyut sınıfı.
    /// Encapsulation: X, Y, IsAlive property'leri korumalı.
    /// </summary>
    public abstract class GameObject
    {
        private float _x, _y;
        private bool _isAlive = true;

        public float X { get => _x; protected set => _x = value; }
        public float Y { get => _y; protected set => _y = value; }
        public bool IsAlive { get => _isAlive; protected set => _isAlive = value; }
        public string Id { get; } = Guid.NewGuid().ToString();

        protected GameObject(float x, float y) { _x = x; _y = y; }

        public abstract void Update(float dt);
        public abstract void Draw(SKCanvas canvas);

        public virtual void Destroy() => _isAlive = false;

        public override string ToString() => $"{GetType().Name}[{Id[..6]}] ({_x:F0},{_y:F0})";
    }
}
