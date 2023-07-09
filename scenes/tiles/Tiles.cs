using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Tiles : TileMap
{
  [Export]
  private float mountainFrequency = 0.02f;

  [Export]
  private float mountainAmplitude = 1f;

  [Export]
  private float mountainThreshold = 0.3f;

  [Export]
  private float forestFrequency = 0.02f;

  [Export]
  private float forestAmplitude = 1f;

  [Export]
  private float forestThreshold = 0.2f;

  [Export]
  private float waterFrequency = 0.02f;

  [Export]
  private float waterAmplitude = 1f;

  [Export]
  private float waterThreshold = 0.35f;

  [Export]
  private float fieldFrequency = 0.1f;

  [Export]
  private float fieldAmplitude = 1f;

  [Export]
  private float fieldThreshold = 0.2f;

  [Export]
  private float farmhouseFrequency = 0.01f;

  [Export]
  private float farmhouseAmplitude = 1f;

  [Export]
  private float farmhouseThreshold = 0.42f;

  [Export]
  private int mapWidth = 160;

  [Export]
  private int mapHeight = 90;

  [Export]
  public Vector2I Empty = new Vector2I(0, 0);

  [Export]
  public Vector2I Mountains = new Vector2I(2, 1);

  [Export]
  public Vector2I Forest = new Vector2I(0, 1);

  [Export]
  public Vector2I Water = new Vector2I(1, 1);

  [Export]
  public int EmptyTerrain = 4;

  [Export]
  public int ForestTerrain = 2;

  [Export]
  public int WaterTerrain = 0;

  [Export]
  public int FieldTerrain = 1;

  [Export]
  public int FarmhouseTerrain = 3;

  [Export]
  public Vector2I Fields = new Vector2I(1, 0);

  [Export]
  public Vector2I Farmhouse = new Vector2I(2, 0);

  public List<TileMapPattern> MountainPatterns;

  public TileArray TilesArray { get; set; }
  public override void _EnterTree()
  {
    base._EnterTree();

    TilesArray = new TileArray(mapWidth, mapHeight);
    MountainPatterns = Enumerable.Range(0, this.TileSet.GetPatternsCount()).Select(i => this.TileSet.GetPattern(i)).ToList();

    AddMountains();
    AddForest();
    AddWater();
    AddFields();
    TilesArray.InstantiateTiles();
  }

  private FastNoiseLite GetNoise(float frequency, float amplitude)
  {
    var noise = new FastNoiseLite();
    noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
    noise.Frequency = frequency;
    noise.DomainWarpAmplitude = amplitude;
    noise.Seed = new Random().Next();
    return noise;
  }

  private void AddMountains()
  {
    var noise = GetNoise(mountainFrequency, mountainAmplitude);

    var toPaintEmpty = new Godot.Collections.Array<Vector2I>();

    for (int x = 0; x < mapWidth; x++)
    {
      for (int y = 0; y < mapHeight; y++)
      {
        var noiseVal = noise.GetNoise2D(x, y);
        var currentCell = TilesArray.Tiles[x, y];
        if (currentCell.Type != TileTypes.Mountains)
        {
          if (noiseVal > mountainThreshold && x < mapWidth - 3 && y < mapHeight - 3)
          {
            try
            {

              this.SetPattern(0, new Vector2I(x, y), MountainPatterns[new Random().Next(0, MountainPatterns.Count - 1)]);
              TilesArray.Tiles[x, y].Type = TileTypes.Mountains;
              TilesArray.Tiles[x, y].Coords = new Vector2I(x, y);

              TilesArray.Tiles[x + 1, y].Type = TileTypes.Mountains;
              TilesArray.Tiles[x + 1, y].Coords = new Vector2I(x + 1, y);

              TilesArray.Tiles[x, y + 1].Type = TileTypes.Mountains;
              TilesArray.Tiles[x, y + 1].Coords = new Vector2I(x, y + 1);

              TilesArray.Tiles[x + 1, y + 1].Type = TileTypes.Mountains;
              TilesArray.Tiles[x + 1, y + 1].Coords = new Vector2I(x + 1, y + 1);
            }
            catch (Exception ex)
            {
              GD.PrintErr(ex.Message);
            }
          }
          else
          {
            toPaintEmpty.Add(new Vector2I(x, y));
            TilesArray.Tiles[x, y].Type = TileTypes.Empty;
            TilesArray.Tiles[x, y].Coords = new Vector2I(x, y);
          }
        }
      }
    }

    this.SetCellsTerrainConnect(0, toPaintEmpty, 0, EmptyTerrain);
  }

  private void AddForest()
  {
    var noise = GetNoise(forestFrequency, forestAmplitude);

    var toPaintForest = new Godot.Collections.Array<Vector2I>();
    for (int x = 0; x < mapWidth; x++)
    {
      for (int y = 0; y < mapHeight; y++)
      {
        var noiseVal = noise.GetNoise2D(x, y);
        var currentCell = TilesArray.Tiles[x, y];
        if (noiseVal > forestThreshold && currentCell.Type != TileTypes.Mountains)
        {
          toPaintForest.Add(new Vector2I(x, y));
          TilesArray.Tiles[x, y].Type = TileTypes.Forest;
          TilesArray.Tiles[x, y].Coords = new Vector2I(x, y);
        }
      }
    }

    this.SetCellsTerrainConnect(0, toPaintForest, 0, ForestTerrain);
  }

  private void AddWater()
  {
    var noise = GetNoise(waterFrequency, waterAmplitude);
    var toPaintWater = new Godot.Collections.Array<Vector2I>();
    for (int x = 0; x < mapWidth; x++)
    {
      for (int y = 0; y < mapHeight; y++)
      {
        var noiseVal = noise.GetNoise2D(x, y);
        var currentCell = TilesArray.Tiles[x, y];
        if (noiseVal > waterThreshold && currentCell.Type != TileTypes.Mountains)
        {
          toPaintWater.Add(new Vector2I(x, y));
          TilesArray.Tiles[x, y].Type = TileTypes.Water;
          TilesArray.Tiles[x, y].Coords = new Vector2I(x, y);
        }
      }
    }
    this.SetCellsTerrainConnect(0, toPaintWater, 0, WaterTerrain);
  }

  private void AddFields()
  {
    var noise = GetNoise(fieldFrequency, fieldAmplitude);
    var toPaintFields = new Godot.Collections.Array<Vector2I>();
    var toPaintFarmhouse = new Godot.Collections.Array<Vector2I>();
    for (int x = 0; x < mapWidth; x++)
    {
      for (int y = 0; y < mapHeight; y++)
      {
        var noiseVal = noise.GetNoise2D(x, y);
        var currentCell = TilesArray.Tiles[x, y];
        if (noiseVal > fieldThreshold && currentCell.Type != TileTypes.Mountains && currentCell.Type != TileTypes.Water && currentCell.Type != TileTypes.Forest)
        {
          if (noiseVal > farmhouseThreshold && !currentCell.SurroundingTiles.Any(t => t.Type == TileTypes.Farmhouse))
          {
            toPaintFarmhouse.Add(new Vector2I(x, y));
            TilesArray.Tiles[x, y].Type = TileTypes.Farmhouse;
            TilesArray.Tiles[x, y].Coords = new Vector2I(x, y);
          }
          else
          {
            toPaintFields.Add(new Vector2I(x, y));
            TilesArray.Tiles[x, y].Type = TileTypes.Fields;
            TilesArray.Tiles[x, y].Coords = new Vector2I(x, y);
          }
        }
      }
    }
    this.SetCellsTerrainConnect(0, toPaintFarmhouse, 0, FarmhouseTerrain);
    this.SetCellsTerrainConnect(0, toPaintFields, 0, FieldTerrain);
  }

  public bool runSim { get; set; } = false;

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    if (!runSim) return;
    var growableCells = TilesArray.GrowableCells;

    for (int i = 8; i > 0; i--)
    {
      var priorityCells = growableCells.Where(t => t.SurroundingTiles.Where(m => m.Type == TileTypes.Fields).Count() == i).ToList();
      if (priorityCells.Count > 0)
      {
        var random = new Random().Next(0, priorityCells.Count);
        var randomCell = priorityCells[random];
        this.SetCell(0, randomCell.Coords, sourceId: 0, atlasCoords: Fields);
        TilesArray.Tiles[randomCell.Coords.X, randomCell.Coords.Y].Type = TileTypes.Fields;
        TilesArray.Tiles[randomCell.Coords.X, randomCell.Coords.Y].InstantiateTile();
        TilesArray.RemoveFromGrowableCells(randomCell);
        TilesArray.AddToGrowableCells(randomCell.SurroundingTiles.Where(t => t.Type == TileTypes.Empty));
        i = 0;

      }
    }
  }

  public TileItem GetTile(Vector2 global)
  {
    Vector2I coords = LocalToMap(ToLocal(global));
    return TilesArray.Tiles[coords.X, coords.Y];
  }

  // @mika work on this thing too
  public void SetTiles(Vector2I topLeft, Vector2I botRight, TileTypes type)
  {
    List<Vector2I> tileList = new List<Vector2I>();

    if (type == TileTypes.Mountains)
    {
      // @mika the mountains are patterns :(
      this.SetPattern(0, new Vector2I(x, y), MountainPatterns[new Random().Next(0, MountainPatterns.Count - 1)]);
      TilesArray.Tiles[x, y].Type = TileTypes.Mountains;
      TilesArray.Tiles[x, y].Coords = new Vector2I(x, y);

      TilesArray.Tiles[x + 1, y].Type = TileTypes.Mountains;
      TilesArray.Tiles[x + 1, y].Coords = new Vector2I(x + 1, y);

      TilesArray.Tiles[x, y + 1].Type = TileTypes.Mountains;
      TilesArray.Tiles[x, y + 1].Coords = new Vector2I(x, y + 1);

      TilesArray.Tiles[x + 1, y + 1].Type = TileTypes.Mountains;
      TilesArray.Tiles[x + 1, y + 1].Coords = new Vector2I(x + 1, y + 1);
      return;
    }
    else
    {
      // @mika terrains are an int that can be set as below
      int terrian;
      switch (type)
      {
        case TileTypes.Empty:
          terrian = EmptyTerrain;
          break;
        case TileTypes.Forest:
          terrian = ForestTerrain;
          break;
        case TileTypes.Water:
          terrian = WaterTerrain;
          break;
        case TileTypes.Fields:
          terrian = FieldTerrain;
          break;
        case TileTypes.Farmhouse:
          terrian = FarmhouseTerrain;
          break;
      }
      toPaintEmpty.Add(new Vector2I(x, y));
      TilesArray.Tiles[x, y].Type = TileTypes.Empty;
      TilesArray.Tiles[x, y].Coords = new Vector2I(x, y);
      return;
    }
    SetCellsTerrainConnect(0, toPaintEmpty, 0, EmptyTerrain);
  }
}


public enum TileTypes
{
  Empty,
  Mountains,
  Forest,
  Water,
  Fields,
  Farmhouse
}

public class TileArray
{
  public TileItem[,] Tiles { get; set; }

  public TileArray(int width, int height)
  {
    Tiles = new TileItem[width, height];
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        Tiles[x, y] = new TileItem(this);
      }
    }
  }

  private List<TileItem> growableCells;
  public List<TileItem> GrowableCells
  {
    get
    {
      return growableCells;
    }
    set
    {
      growableCells = value;
    }
  }

  public void AddToGrowableCells(IEnumerable<TileItem> tiles)
  {
    growableCells.AddRange(tiles);
  }

  public void AddToGrowableCells(TileItem tile)
  {
    growableCells.Add(tile);
  }

  public void RemoveFromGrowableCells(IEnumerable<TileItem> tiles)
  {
    growableCells.RemoveAll(t => tiles.Contains(t));
  }

  public void RemoveFromGrowableCells(TileItem tile)
  {
    growableCells.Remove(tile);
  }

  public void InstantiateTiles()
  {
    foreach (var tile in Tiles)
    {
      tile.InstantiateTile();
    }

    GrowableCells = Tiles.Cast<TileItem>().Where(t => t.CanGrow).ToList();
  }
}
public class TileItem
{
  public TileItem(TileArray parent, Vector2I coords, Vector2I[] stateArray, int maxHealth, bool canBeAttacked, bool canGrow, bool canSpread, int regenRate)
  {
    this.parent = parent;
    this.Coords = coords;
    this.MaxHealth = maxHealth;
    this.Health = maxHealth;
    this.CanBeAttacked = canBeAttacked;
    this.canSpread = canSpread;
    this.CanGrow = canGrow;
  }

  public void InstantiateTile()
  {
    SurroundingTiles = GetSurroundingTiles();
    CanGrow = GetCanGrow();
  }
  public TileArray parent { get; private set; }
  public Vector2I Coords { get; private set; }
  public Vector2I[] StateArray { get; private set; }
  public int Health { get; private set; } = 1;
  public int MaxHealth { get; private set; }
  public bool CanBeAttacked { get; private set; }
  public bool CanGrow
  {
    get; private set;
  }

  public bool canSpread;
  public bool CanSpread
  {
    get
    {
      return canSpread && Health > 0;
    }
  }

  private List<TileItem> surroundingTiles;

  public List<TileItem> SurroundingTiles
  {
    get
    {
      if (surroundingTiles == null)
      {
        surroundingTiles = GetSurroundingTiles();
      }
      return surroundingTiles;
    }
    set
    {
      surroundingTiles = value;
    }
  }

  // @mika check this and see if it works
  public void Clone(TileItem target)
  {
    // if we're a spreadable, don't allow clone
    if (CanSpread) return;
    // clone only if target can clone and we can accept the growth
    if (!CanGrow || !target.canSpread) return;

    this.MaxHealth = target.MaxHealth;
    this.Health = target.MaxHealth;
    this.CanBeAttacked = target.CanBeAttacked;
    this.canSpread = target.canSpread;
    this.CanGrow = target.CanGrow;
  }

  public void Attack(int dmg)
  {
    // @mika put attack logic here and change the spirite
    // when damaged to stages, show by going down the health tree
    // potentially add a regen function?
  }

  public List<TileItem> GetSurroundingTiles()
  {
    return (from x in Enumerable.Range(Coords.X - 1, 3)
            from y in Enumerable.Range(Coords.Y - 1, 3)
            where x >= 0 && y >= 0 && x < parent.Tiles.GetLength(0) && y < parent.Tiles.GetLength(1) && (x != Coords.X || y != Coords.Y)
            select parent.Tiles[x, y]).ToList();
  }

  private bool GetCanGrow()
  {
    return SurroundingTiles.Any(t => t.CanSpread) && CanGrow;
  }
}

