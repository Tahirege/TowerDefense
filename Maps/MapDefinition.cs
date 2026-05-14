namespace TowerDefense.Maps
{
    public record MapDefinition(
        string Id, string Name, string Description,
        int[][] PathCells, int MaxWaves, float EnemySpeedMultiplier,
        int[][]? WaterCells = null, int[][]? TreeCells = null, 
        int[][]? HouseCells = null, string? BackgroundImagePath = null);

    public static class MapLibrary
    {
        public static readonly MapDefinition[] All =
        {
            new MapDefinition(
                "classic", "Classic", "Classic S-shaped path - Easy",
                new int[][] {
                    new[]{5,0},new[]{5,1},new[]{5,2},new[]{5,3},new[]{5,4},
                    new[]{5,5},new[]{4,5},new[]{3,5},new[]{2,5},new[]{1,5},
                    new[]{1,6},new[]{1,7},new[]{1,8},new[]{1,9},new[]{2,9},
                    new[]{3,9},new[]{4,9},new[]{5,9},new[]{6,9},new[]{7,9},
                    new[]{8,9},new[]{8,10},new[]{8,11},new[]{8,12},new[]{7,12},
                    new[]{6,12},new[]{5,12},new[]{4,12},new[]{3,12},new[]{2,12},
                    new[]{2,13},new[]{2,14},new[]{2,15},new[]{2,16},new[]{2,17},
                    new[]{3,17},new[]{4,17},new[]{5,17},new[]{6,17},new[]{7,17},
                }, 10, 1.0f,
                WaterCells: new int[][] {
                    new[]{9,0},new[]{10,0},new[]{11,0},new[]{10,1},new[]{11,1},new[]{11,2},
                    new[]{0,14},new[]{0,15},new[]{0,16},new[]{0,17},new[]{1,16},new[]{1,17}
                },
                TreeCells: new int[][] {
                    new[]{2,2},new[]{2,3},new[]{3,2},
                    new[]{8,2},new[]{8,3},new[]{9,2},
                    new[]{3,15},new[]{4,15},
                    new[]{9,16},new[]{10,15},new[]{10,16}
                },
                HouseCells: new int[][] {
                    new[]{0,0},new[]{1,1},new[]{8,15},new[]{9,1}
                }),

            new MapDefinition(
                "spiral", "Labyrinth Forest", "A complex path through dense trees and narrow passages - Medium",
                new int[][] {
                    new[]{0,1},new[]{1,1},new[]{2,1},new[]{3,1},new[]{4,1},
                    new[]{4,2},new[]{4,3},new[]{4,4},new[]{4,5},
                    new[]{3,5},new[]{2,5},new[]{1,5},new[]{0,5},
                    new[]{0,6},new[]{0,7},new[]{0,8},new[]{0,9},
                    new[]{1,9},new[]{2,9},new[]{3,9},new[]{4,9},new[]{5,9},new[]{6,9},
                    new[]{6,8},new[]{6,7},new[]{6,6},new[]{6,5},new[]{6,4},new[]{6,3},new[]{6,2},
                    new[]{7,2},new[]{8,2},new[]{9,2},new[]{10,2},new[]{11,2},
                    new[]{11,3},new[]{11,4},new[]{11,5},new[]{11,6},new[]{11,7},new[]{11,8},new[]{11,9},new[]{11,10},
                    new[]{10,10},new[]{9,10},new[]{8,10},new[]{7,10},
                    new[]{7,11},new[]{7,12},new[]{7,13},new[]{7,14},
                    new[]{8,14},new[]{9,14},new[]{10,14},new[]{11,14},new[]{11,15},new[]{11,16},new[]{11,17}
                }, 12, 1.1f,
                WaterCells: new int[][] {
                    new[]{2,2},new[]{2,3},new[]{3,2},new[]{3,3},
                    new[]{8,4},new[]{8,5},new[]{9,4},new[]{9,5},
                    new[]{2,12},new[]{2,13},new[]{3,12},new[]{3,13},new[]{4,12},new[]{4,13},
                    new[]{0,14},new[]{0,15},new[]{1,14},new[]{1,15}
                },
                TreeCells: new int[][] {
                    new[]{1,3},new[]{1,4},new[]{2,4},new[]{5,1},new[]{6,1},new[]{7,1},
                    new[]{5,4},new[]{5,5},new[]{5,6},new[]{4,6},new[]{3,7},new[]{3,8},
                    new[]{5,10},new[]{5,11},new[]{6,11},new[]{8,8},new[]{9,8},
                    new[]{10,12},new[]{10,13},new[]{9,12},new[]{9,13},
                    new[]{5,15},new[]{5,16},new[]{6,15},new[]{6,16},new[]{7,16},new[]{8,16}
                },
                HouseCells: new int[][] {
                    new[]{0,3},new[]{0,11},new[]{3,11},new[]{9,0},new[]{10,4}
                }),

            new MapDefinition(
                "hell", "Hell", "Fast enemies - Expert",
                new int[][] {
                    new[]{1,0},new[]{1,1},new[]{1,2},new[]{1,3},new[]{1,4},new[]{1,5},
                    new[]{1,6},new[]{1,7},new[]{1,8},new[]{1,9},new[]{1,10},new[]{1,11},
                    new[]{2,11},new[]{3,11},new[]{4,11},new[]{5,11},new[]{6,11},
                    new[]{6,12},new[]{6,13},new[]{6,14},new[]{6,15},new[]{6,16},new[]{6,17},
                    new[]{7,17},new[]{8,17},new[]{9,17},new[]{10,17},new[]{11,17},
                }, 15, 1.3f,
                WaterCells: new int[][] {
                    new[]{3,0},new[]{4,0},new[]{5,0},new[]{3,1},new[]{4,1},new[]{5,1},new[]{3,2},
                    new[]{8,8},new[]{8,9},new[]{9,8},new[]{9,9},new[]{10,8},new[]{10,9},
                    new[]{11,12},new[]{11,13},new[]{11,14},new[]{10,12},new[]{10,13}
                },
                TreeCells: new int[][] {
                    new[]{0,4},new[]{0,5},new[]{0,6},new[]{2,4},new[]{2,5},
                    new[]{4,5},new[]{4,6},new[]{5,6},
                    new[]{3,13},new[]{3,14},new[]{4,14},new[]{4,15},
                    new[]{8,11},new[]{8,12},new[]{9,11},new[]{11,15},new[]{11,16}
                },
                HouseCells: new int[][] {
                    new[]{0,0},new[]{0,17},new[]{11,0},new[]{5,3},new[]{9,15}
                }),
                
            new MapDefinition(
                "linedefense", "Chess (Line Defense)", "Open field defense. Enemies comes at random spots.",
                new int[][] {
                    new[]{0,8}, new[]{11,8}
                }, 10, 1.0f)
        };
    }
}
