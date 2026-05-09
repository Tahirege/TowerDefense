namespace TowerDefense.Exceptions
{
    public class GameException : Exception
    {
        public GameException(string message) : base(message) { }
        public GameException(string message, Exception inner) : base(message, inner) { }
    }

    public class TowerPlacementException : GameException
    {
        public TowerPlacementException(string msg) : base($"Kule yerleştirme hatası: {msg}") { }
    }

    public class InsufficientGoldException : GameException
    {
        public int Required { get; }
        public int Available { get; }
        public InsufficientGoldException(int req, int avail)
            : base($"Yetersiz altın! Gerekli: {req}, Mevcut: {avail}")
        { Required = req; Available = avail; }
    }

    public class SaveLoadException : GameException
    {
        public SaveLoadException(string msg, Exception inner) : base($"Kayıt hatası: {msg}", inner) { }
    }
}
