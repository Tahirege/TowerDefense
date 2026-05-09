using TowerDefense.Maps;
using SkiaSharp;

namespace TowerDefense.Managers
{
    public enum CellType { Grass, Path, Start, End, Occupied, Water, Tree, House }

    public class MapManager
    {
        public const int CellSize = 48;
        public const int Cols = 18;
        public const int Rows = 12;

        private CellType[,] _grid = new CellType[Rows, Cols];
        private List<(float x, float y)> _path = new();
        private float _animTimer = 0f;

        public List<(float x, float y)> PathPoints => _path;
        public int MapWidth  => Cols * CellSize;
        public int MapHeight => Rows * CellSize;

        public string CurrentMapId { get; private set; } = "classic";

        public MapManager(string mapId = "classic") => LoadMap(mapId);

        public void LoadMap(string mapId)
        {
            CurrentMapId = mapId;
            var def = MapLibrary.All.FirstOrDefault(m => m.Id == mapId) ?? MapLibrary.All[0];
            BuildMap(def);
        }

        private void BuildMap(MapDefinition def)
        {
            var cells = def.PathCells;
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    _grid[r, c] = CellType.Grass;

            if (def.WaterCells != null)
                foreach (var w in def.WaterCells)
                    if (w[0] < Rows && w[1] < Cols) _grid[w[0], w[1]] = CellType.Water;
                    
            if (def.TreeCells != null)
                foreach (var t in def.TreeCells)
                    if (t[0] < Rows && t[1] < Cols) _grid[t[0], t[1]] = CellType.Tree;

            if (def.HouseCells != null)
                foreach (var h in def.HouseCells)
                    if (h[0] < Rows && h[1] < Cols) _grid[h[0], h[1]] = CellType.House;

            _path.Clear();
            foreach (var c in cells)
            {
                _grid[c[0], c[1]] = CellType.Path;
                _path.Add((c[1] * CellSize + CellSize/2f, c[0] * CellSize + CellSize/2f));
            }
            _grid[cells[0][0],   cells[0][1]]   = CellType.Start;
            _grid[cells[^1][0],  cells[^1][1]]  = CellType.End;
        }

        public bool IsBuildable(float wx, float wy)
        {
            int c = (int)(wx / CellSize), r = (int)(wy / CellSize);
            if (r < 0 || r >= Rows || c < 0 || c >= Cols) return false;
            return _grid[r, c] == CellType.Grass;
        }

        public bool IsWalkable(float wx, float wy, float radius = 0f, bool allowOccupied = false)
        {
            if (radius <= 0)
            {
                int c = (int)(wx / CellSize), r = (int)(wy / CellSize);
                if (r < 0 || r >= Rows || c < 0 || c >= Cols) return false;
                var cell = _grid[r, c];
                bool walkable = cell == CellType.Grass || cell == CellType.Path || 
                                cell == CellType.Start || cell == CellType.End;
                if (allowOccupied && cell == CellType.Occupied) walkable = true;
                return walkable;
            }

            // Check 4 corners with radius
            return IsWalkable(wx - radius, wy - radius, 0, allowOccupied) &&
                   IsWalkable(wx + radius, wy - radius, 0, allowOccupied) &&
                   IsWalkable(wx - radius, wy + radius, 0, allowOccupied) &&
                   IsWalkable(wx + radius, wy + radius, 0, allowOccupied);
        }

        public (float x, float y) Snap(float wx, float wy)
        {
            int c = Math.Clamp((int)(wx / CellSize), 0, Cols-1);
            int r = Math.Clamp((int)(wy / CellSize), 0, Rows-1);
            return (c * CellSize + CellSize/2f, r * CellSize + CellSize/2f);
        }

        public void MarkOccupied(float wx, float wy)
        {
            int c = (int)(wx / CellSize), r = (int)(wy / CellSize);
            if (r >= 0 && r < Rows && c >= 0 && c < Cols) _grid[r, c] = CellType.Occupied;
        }

        public void UnmarkOccupied(float wx, float wy)
        {
            int c = (int)(wx / CellSize), r = (int)(wy / CellSize);
            if (r >= 0 && r < Rows && c >= 0 && c < Cols && _grid[r, c] == CellType.Occupied)
                _grid[r, c] = CellType.Grass;
        }

        public void Draw(SKCanvas canvas)
        {
            _animTimer += 0.016f; // ~60fps increment

            // ── Draw all cells ────────────────────────────────────
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    float rx = c * CellSize, ry = r * CellSize;
                    var cell = _grid[r, c];

                    switch (cell)
                    {
                        case CellType.Grass:
                        case CellType.Occupied:
                            DrawGrassCell(canvas, rx, ry, r, c, cell == CellType.Occupied);
                            break;
                        case CellType.Path:
                            DrawPathCell(canvas, rx, ry);
                            break;
                        case CellType.Start:
                            DrawPathCell(canvas, rx, ry);
                            DrawStartCell(canvas, rx, ry);
                            break;
                        case CellType.End:
                            DrawPathCell(canvas, rx, ry);
                            DrawEndCell(canvas, rx, ry);
                            break;
                        case CellType.Water:
                            DrawWaterCell(canvas, rx, ry);
                            break;
                        case CellType.Tree:
                            DrawGrassCell(canvas, rx, ry, r, c, false); // Base grass
                            DrawTreeCell(canvas, rx, ry, r, c);
                            break;
                        case CellType.House:
                            DrawGrassCell(canvas, rx, ry, r, c, false); // Base grass
                            DrawHouseCell(canvas, rx, ry, r, c);
                            break;
                    }
                }
            }

            // ── Draw path arrows ──────────────────────────────────
            DrawPathArrows(canvas);

            // ── Subtle outer border ───────────────────────────────
            using var border = new SKPaint { Color = new SKColor(0, 0, 0, 80),
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            canvas.DrawRect(0, 0, MapWidth, MapHeight, border);
        }

        private void DrawGrassCell(SKCanvas canvas, float rx, float ry, int row, int col, bool occupied)
        {
            // Checkerboard grass variation for texture
            bool alt = (row + col) % 2 == 0;
            var baseGrass  = alt ? new SKColor(68, 125, 50) : new SKColor(74, 135, 55);
            var darkGrass  = alt ? new SKColor(55, 100, 40) : new SKColor(60, 108, 44);

            using var fillP = new SKPaint { IsAntialias = false };
            fillP.Color = occupied ? new SKColor(55, 105, 40) : baseGrass;
            canvas.DrawRect(rx, ry, CellSize, CellSize, fillP);

            // Grass blade texture (small marks)
            if (!occupied)
            {
                using var bladeP = new SKPaint { Color = darkGrass.WithAlpha(120),
                    Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
                // A few random-but-deterministic blades per cell using row/col as seed
                int seed = row * 31 + col * 17;
                for (int i = 0; i < 4; i++)
                {
                    float bx = rx + (((seed * (i + 7)) % 36) + 6);
                    float by = ry + (((seed * (i + 3)) % 32) + 8);
                    canvas.DrawLine(bx, by, bx + 2, by - 5, bladeP);
                }

                // Add occasional rock or flower
                int detailSeed = (row * 13 + col * 29) % 100;
                if (detailSeed < 15) // 15% chance for a small rock
                {
                    using var rockP = new SKPaint { Color = new SKColor(120, 120, 130), IsAntialias = true };
                    canvas.DrawCircle(rx + 15 + (detailSeed % 10), ry + 15 + (detailSeed % 15), 3f, rockP);
                    using var rockHighlightP = new SKPaint { Color = new SKColor(160, 160, 170), IsAntialias = true };
                    canvas.DrawCircle(rx + 14 + (detailSeed % 10), ry + 14 + (detailSeed % 15), 1.5f, rockHighlightP);
                }
                else if (detailSeed < 25) // 10% chance for a flower
                {
                    float fx = rx + 20 + (detailSeed % 15);
                    float fy = ry + 20 + (detailSeed % 10);
                    using var stemP = new SKPaint { Color = new SKColor(40, 160, 40), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
                    canvas.DrawLine(fx, fy, fx, fy + 4, stemP);
                    
                    var fColor = (detailSeed % 2 == 0) ? SKColors.LightPink : SKColors.LightYellow;
                    using var flowerP = new SKPaint { Color = fColor, IsAntialias = true };
                    canvas.DrawCircle(fx - 2, fy - 2, 2f, flowerP);
                    canvas.DrawCircle(fx + 2, fy - 2, 2f, flowerP);
                    canvas.DrawCircle(fx - 2, fy + 2, 2f, flowerP);
                    canvas.DrawCircle(fx + 2, fy + 2, 2f, flowerP);
                    using var centerP = new SKPaint { Color = SKColors.White, IsAntialias = true };
                    canvas.DrawCircle(fx, fy, 1.5f, centerP);
                }
            }

            // Subtle grid line
            using var gridP = new SKPaint { Color = new SKColor(0, 0, 0, 25),
                Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f };
            canvas.DrawRect(rx, ry, CellSize, CellSize, gridP);

            // If occupied, draw faint tower-base mark
            if (occupied)
            {
                using var occP = new SKPaint { Color = new SKColor(0, 0, 0, 30), IsAntialias = true };
                canvas.DrawCircle(rx + CellSize/2f, ry + CellSize/2f, 14, occP);
            }
        }

        private void DrawWaterCell(SKCanvas canvas, float rx, float ry)
        {
            // Animated water base
            float wave = MathF.Sin(_animTimer * 2f + (rx + ry) * 0.05f) * 10f;
            byte bColor = (byte)(180 + wave);
            byte gColor = (byte)(130 + wave * 0.5f);
            using var waterFill = new SKPaint { Color = new SKColor(40, gColor, bColor), IsAntialias = false };
            canvas.DrawRect(rx, ry, CellSize, CellSize, waterFill);

            // Water ripples
            using var rippleP = new SKPaint { Color = new SKColor(255, 255, 255, 60), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            float cx = rx + CellSize / 2f, cy = ry + CellSize / 2f;
            float r1 = 8f + wave * 0.2f;
            float r2 = 18f - wave * 0.2f;
            canvas.DrawLine(cx - r1, cy - 6, cx + r1, cy - 6, rippleP);
            canvas.DrawLine(cx - r2, cy + 8, cx + r2, cy + 8, rippleP);
            
            // Subtle grid line
            using var gridP = new SKPaint { Color = new SKColor(0, 0, 0, 20), Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f };
            canvas.DrawRect(rx, ry, CellSize, CellSize, gridP);
        }

        private void DrawTreeCell(SKCanvas canvas, float rx, float ry, int row, int col)
        {
            float cx = rx + CellSize / 2f, cy = ry + CellSize / 2f;
            int seed = row * 19 + col * 7;
            
            // Trunk
            using var trunkP = new SKPaint { Color = new SKColor(90, 60, 30), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 5f, StrokeCap = SKStrokeCap.Round };
            canvas.DrawLine(cx, cy + 5, cx, cy + 18, trunkP);
            
            // Leaves (three overlapping circles for a stylized pine/oak look)
            var leafColor1 = (seed % 2 == 0) ? new SKColor(35, 90, 35) : new SKColor(40, 100, 45);
            var leafColor2 = (seed % 2 == 0) ? new SKColor(45, 110, 45) : new SKColor(50, 120, 55);
            
            using var leaf1 = new SKPaint { Color = leafColor1, IsAntialias = true };
            using var leaf2 = new SKPaint { Color = leafColor2, IsAntialias = true };
            
            canvas.DrawCircle(cx - 8, cy + 2, 12, leaf1);
            canvas.DrawCircle(cx + 8, cy + 2, 12, leaf1);
            canvas.DrawCircle(cx, cy - 10, 14, leaf2);
        }

        private void DrawHouseCell(SKCanvas canvas, float rx, float ry, int row, int col)
        {
            float cx = rx + CellSize / 2f, cy = ry + CellSize / 2f;
            int seed = row * 17 + col * 13;

            // Base colors
            var roofColor = (seed % 3 == 0) ? new SKColor(180, 40, 40) : (seed % 3 == 1) ? new SKColor(40, 60, 120) : new SKColor(140, 100, 40);
            var wallColor = new SKColor(230, 210, 180);
            
            // Wall
            using var wallP = new SKPaint { Color = wallColor, IsAntialias = true };
            canvas.DrawRect(rx + 8, ry + 15, CellSize - 16, CellSize - 22, wallP);
            
            // Door
            using var doorP = new SKPaint { Color = new SKColor(80, 50, 30), IsAntialias = true };
            canvas.DrawRect(cx - 4, ry + 28, 8, 12, doorP);

            // Window
            using var winP = new SKPaint { Color = new SKColor(135, 206, 235), IsAntialias = true };
            canvas.DrawRect(rx + 12, ry + 20, 6, 6, winP);
            canvas.DrawRect(rx + CellSize - 18, ry + 20, 6, 6, winP);

            // Roof
            using var roofP = new SKPaint { Color = roofColor, IsAntialias = true };
            var roofPath = new SKPath();
            roofPath.MoveTo(rx + 4, ry + 18);
            roofPath.LineTo(cx, ry + 4);
            roofPath.LineTo(rx + CellSize - 4, ry + 18);
            roofPath.Close();
            canvas.DrawPath(roofPath, roofP);
            
            // Roof outline
            using var strokeP = new SKPaint { Color = new SKColor(0,0,0,40), Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, IsAntialias = true };
            canvas.DrawPath(roofPath, strokeP);
        }

        private void DrawPathCell(SKCanvas canvas, float rx, float ry)
        {
            // Dirt path with slight edge shading
            using var pathFill = new SKPaint { IsAntialias = false };
            pathFill.Color = new SKColor(190, 158, 100);
            canvas.DrawRect(rx, ry, CellSize, CellSize, pathFill);

            // Edge darkening (inner shadow)
            using var edgeP = new SKPaint { Color = new SKColor(100, 75, 30, 60),
                Style = SKPaintStyle.Stroke, StrokeWidth = 3f };
            canvas.DrawRect(rx + 1, ry + 1, CellSize - 2, CellSize - 2, edgeP);

            // Pebble texture dots
            using var pebP = new SKPaint { Color = new SKColor(150, 120, 70, 80), IsAntialias = true };
            canvas.DrawCircle(rx + 10, ry + 12, 3, pebP);
            canvas.DrawCircle(rx + 32, ry + 30, 2.5f, pebP);
            canvas.DrawCircle(rx + 20, ry + 38, 2f, pebP);
            canvas.DrawCircle(rx + 38, ry + 16, 3f, pebP);
        }

        private void DrawStartCell(SKCanvas canvas, float rx, float ry)
        {
            float cx = rx + CellSize / 2f, cy = ry + CellSize / 2f;

            // Green glow background
            using var glow = new SKPaint { Color = new SKColor(50, 220, 80, 50), IsAntialias = true };
            canvas.DrawCircle(cx, cy, 20, glow);

            // "S" badge
            using var badge = new SKPaint { Color = new SKColor(40, 180, 70), IsAntialias = true };
            using var badgeStroke = new SKPaint { Color = new SKColor(20, 120, 40), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            canvas.DrawCircle(cx, cy, 16, badge);
            canvas.DrawCircle(cx, cy, 16, badgeStroke);

            using var labelP = new SKPaint { Color = SKColors.White, IsAntialias = true,
                TextSize = 13f, TextAlign = SKTextAlign.Center, FakeBoldText = true };
            canvas.DrawText("S", cx, cy + 5, labelP);
        }

        private void DrawEndCell(SKCanvas canvas, float rx, float ry)
        {
            float cx = rx + CellSize / 2f, cy = ry + CellSize / 2f;

            // Pulsing red glow
            float pulse = 0.7f + 0.3f * MathF.Sin(_animTimer * 3f);
            using var glow = new SKPaint { Color = new SKColor(220, 50, 50, (byte)(60 * pulse)), IsAntialias = true };
            canvas.DrawCircle(cx, cy, 22, glow);

            using var badge = new SKPaint { Color = new SKColor(200, 40, 40), IsAntialias = true };
            using var badgeStroke = new SKPaint { Color = new SKColor(140, 20, 20), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            canvas.DrawCircle(cx, cy, 16, badge);
            canvas.DrawCircle(cx, cy, 16, badgeStroke);

            using var labelP = new SKPaint { Color = SKColors.White, IsAntialias = true,
                TextSize = 13f, TextAlign = SKTextAlign.Center, FakeBoldText = true };
            canvas.DrawText("E", cx, cy + 5, labelP);
        }

        private void DrawPathArrows(SKCanvas canvas)
        {
            if (_path.Count < 2) return;

            // Draw arrows every few path segments to indicate direction
            int step = Math.Max(1, _path.Count / 8);
            using var arrowP = new SKPaint { Color = new SKColor(160, 120, 60, 130), IsAntialias = true,
                Style = SKPaintStyle.Fill };

            for (int i = step; i < _path.Count - 1; i += step)
            {
                var (x1, y1) = _path[i - 1];
                var (x2, y2) = _path[i];

                float dx = x2 - x1, dy = y2 - y1;
                float len = MathF.Sqrt(dx * dx + dy * dy);
                if (len < 1) continue;
                dx /= len; dy /= len;

                float cx = (x1 + x2) / 2f, cy = (y1 + y2) / 2f;
                float arrowSize = 7f;

                // Perpendicular
                float px = -dy, py = dx;

                var arrow = new SKPath();
                arrow.MoveTo(cx + dx * arrowSize,        cy + dy * arrowSize);
                arrow.LineTo(cx - dx * arrowSize + px * arrowSize * 0.6f, cy - dy * arrowSize + py * arrowSize * 0.6f);
                arrow.LineTo(cx - dx * arrowSize - px * arrowSize * 0.6f, cy - dy * arrowSize - py * arrowSize * 0.6f);
                arrow.Close();
                canvas.DrawPath(arrow, arrowP);
            }
        }
    }
}
