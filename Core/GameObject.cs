using SkiaSharp;

namespace TowerDefense.Core
{
    public abstract class GameObject
    {
        public float X { get; set; }
        public float Y { get; set; }
        public bool IsAlive { get; set; } = true;
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public GameObject(float x, float y) { X = x; Y = y; }

        public abstract void Update(float dt);
        public abstract void Draw(SKCanvas canvas);

        public virtual void Destroy() => IsAlive = false;

        public override string ToString() => $"{GetType().Name}[{Id[..6]}] ({X:F0},{Y:F0})";
    }
}
