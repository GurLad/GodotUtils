using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public partial class LevelGenerator : Node
{
    public static readonly int PHYSICAL_SIZE = 2;
    // General data
    [ExportGroup("Paths")]
    [Export(PropertyHint.Dir)]
    public string LevelsPath;
    [Export(PropertyHint.File, "*.csv")]
    public string WallsCSVPath;
    [Export(PropertyHint.File, "*.json")]
    public string EntitiesJSONPath;
    [ExportGroup("Data")]
    [Export]
    public int NumberOfLevels = 1;
    [Export]
    public int TileSize = 16;
    // Terrain
    [ExportGroup("Terrain")]
    [Export]
    public PackedScene SampleTerrain0;
    // Units
    [ExportGroup("Entities")]
    [Export]
    public PackedScene SampleEntityNamedBob;
    [ExportGroup("Objects")]
    public Node3D ObjectsHolder;

    private LevelData levelData;
    private int[,] walls;
    private int currentLevel;

    public override void _Ready()
    {
        base._Ready();
        // Generate first level
        GenerateLevel(0);
    }

    public void Win()
    {
        SceneController.Current.Transition(() => GenerateLevel((currentLevel + 1) % NumberOfLevels), BeginLevel);
    }

    public void Lose()
    {
        SceneController.Current.Transition(() => GenerateLevel(currentLevel), BeginLevel);
    }

    private void GenerateLevel(int number)
    {
        currentLevel = number;
        // Clear previous level
        foreach (Node child in ObjectsHolder.GetChildren())
        {
            if (!child.IsQueuedForDeletion())
            {
                child.QueueFree();
            }
        }
        // Read CSV
        var file = FileAccess.Open(LevelsPath + number + WallsCSVPath, FileAccess.ModeFlags.Read);
        string wallsCSV = file.GetAsText();
        file.Close();
        // Read JSON
        file = FileAccess.Open(LevelsPath + number + EntitiesJSONPath, FileAccess.ModeFlags.Read);
        string entitiesJSON = file.GetAsText();
        file.Close();
        levelData = LevelData.Interpret(entitiesJSON, TileSize);
        // Generate walls
        walls = ImportWalls(wallsCSV, levelData.Width, levelData.Height);
        for (int x = 0; x < levelData.Width; x++)
        {
            for (int y = 0; y < levelData.Height; y++)
            {
                switch (walls[x, y])
                {
                    case 0: // SampleTerrain0
                        Node3D newFloor = SampleTerrain0.Instantiate<Node3D>();
                        newFloor.Translate(new Vector2I(x, y).To3D());
                        ObjectsHolder.AddChild(newFloor);
                        break;
                    default:
                        break;
                }
            }
        }
        // Init pathfinder
        Pathfinder.SetMap(walls, new Vector2I(levelData.Width, levelData.Height));
        // Generate objects
        foreach (List<Entity> entities in levelData.entities.Values)
        {
            foreach (Entity entity in entities)
            {
                Node3D entityObject = null;
                Vector2I pos = new Vector2I(entity.x / TileSize, entity.y / TileSize);
                switch (entity.id)
                {
                    case "Bob":
                        entityObject = SampleEntityNamedBob.Instantiate<Node3D>();
                        break;
                    default:
                        throw new System.Exception("No matching entity type! (" + entity.id + ")");
                }
                entityObject.Translate(pos.To3D());
                ObjectsHolder.AddChild(entityObject);
            }
        }
    }

    private void BeginLevel()
    {
        // Do nothing
    }

    private int SafeGetWall(int x, int y)
    {
        if (x < 0 || y < 0 || x >= levelData.Width || y >= levelData.Height)
        {
            return 0;
        }
        return walls[x, y];
    }

    private int[,] ImportWalls(string csv, int width, int height)
    {
        int[,] result = new int[width, height];
        string[] rows = csv.Replace("\r", "").Split('\n');
        for (int y = 0; y < rows.Length - 1; y++) // Ends with newline
        {
            string row = rows[y][rows[y].Length - 1] == ',' ? rows[y].Substring(0, rows[y].Length - 1) : rows[y];
            string[] columns = row.Split(',');
            for (int x = 0; x < columns.Length; x++)
            {
                //Debug.Log("(" + x + ", " + y + "): " + columns[x]);
                result[x, y] = int.Parse(columns[x]) - 1;
            }
        }
        return result;
    }

    [System.Serializable]
    private class LevelData
    {
        public Dictionary<string, List<Entity>> entities;
        [Newtonsoft.Json.JsonProperty]
        private int width;
        public int Width => width / tileSize;
        [Newtonsoft.Json.JsonProperty]
        private int height;
        public int Height => height / tileSize;
        private int tileSize;

        public static LevelData Interpret(string json, int tileSize)
        {
            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json);
            levelData.tileSize = tileSize;
            //JsonUtility.FromJsonOverwrite(json, this);
            //Debug.Log(JsonConvert.SerializeObject(levelData));
            return levelData;
        }
    }

    [System.Serializable]
    private class Entity
    {
        public string id;
        public int x;
        public int y;
        public int width;
        public int height;
        public Dictionary<string, EntityField> customFields;
    }

    [System.Serializable]
    private class EntityField
    {
        public int cx;
        public int cy;
        public System.Int64 intData;
        public bool boolData;

        public EntityField() { }
        public EntityField(System.Int64 data) { intData = data; }
        public EntityField(bool data) { boolData = data; }

        public Vector2I ToVector2Int()
        {
            return new Vector2I(cx, cy);
        }

        public static implicit operator EntityField(System.Int64 i) =>
            new EntityField(i);
        public static implicit operator System.Int64(EntityField ef) =>
            ef.intData;
        public static implicit operator EntityField(bool b) =>
            new EntityField(b);
        public static implicit operator bool(EntityField ef) =>
            ef.boolData;
    }
}
