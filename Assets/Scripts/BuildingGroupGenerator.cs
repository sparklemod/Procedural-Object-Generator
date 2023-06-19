using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BuildingGroupGenerator : BaseGenerator
{
    public BuildingGenerator buildingGenerator;
    public TerrainGenerator terrainGenerator;
    public int buildingWidth, roadWidth, maxFloors;

    public int[,] _cityMap = null;

    void Start()
    {
        GenerateDefault();
    }

    public override void Generate()
    {
        Clear();

        int width = 0;
        while (width + buildingWidth + roadWidth <= terrainGenerator.Density)
        {
            width += buildingWidth + roadWidth;
        }

        int[,] cityMap = new int[width, width];
        float offset = (terrainGenerator.Density - width) / 2.0f;
        // 0 - empty, [1..] - building, -1 - road

        // road rows
        bool isRoad = false;
        for (int i = 0, accum = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
                cityMap[i, j] = isRoad ? -1 : 0;
            accum ++;
            if (isRoad)
            {
                if (accum >= roadWidth)
                {
                    isRoad = false;
                    accum = 0;
                }
            }
            else
            {
                if (accum >= buildingWidth)
                {
                    isRoad = true;
                    accum = 0;
                }
            }
        }

        // buildings rects
        for (int i = 0, j, i2, k; i < width; i++)
        {
            if (cityMap[i, 0] == -1) continue;
            j = 0;
            while (j < width)
            {
                int curBuildingLength = buildingWidth * Random.Range(1, 5); //ширина здания * на сл величину от 1 до 5

                int[,] buildingMap = GenerateBuildingMap(
                    curBuildingLength,
                    buildingWidth,
                    GetBuildingAreaHeight(i, j, Mathf.Min(i + buildingWidth + 1, width), Mathf.Min(j + curBuildingLength + 1, width))
                );
                for (k = 0; k < curBuildingLength && j < width; k++, j++)
                    for (i2 = 0; i2 < buildingWidth; i2++)
                        cityMap[i + i2, j] = buildingMap[k, i2];
                for (k = 0; k < roadWidth && j < width; k++, j++)
                    for (i2 = 0; i2 < buildingWidth; i2++)
                        cityMap[i + i2, j] = -1;
                k--;
            }
            i += buildingWidth - 1;
        }
        
        // remove all tiles on terrain holes
        for (int i = 0; i < width; i++)
            for (int j = 0; j < width; j++)
            {
                if (cityMap[i, j] == 0) continue;
                float tileHeight = terrainGenerator.heights.GetHeightAtPixel(terrainGenerator.PointToTexture(i + 0.5f, j + 0.5f));
                if (tileHeight <= terrainGenerator.WaterHeight || heights.GetHeightAtPixel(terrainGenerator.PointToTexture(i + 0.5f, j + 0.5f)) < 0)
                    cityMap[i, j] = 0;
            }
        
        _cityMap = CloneMatrix(cityMap);

        // construct buildings from map
        for (int i = 0, j, i2, j2; i < width; i++)
        {
            for (j = 0; j < width; j++)
            {
                if (cityMap[i, j] < 1) continue;
                int floorsCount = cityMap[i, j];
                RectInt rect = new RectInt(i, j, 1, 1);
                var cloneMap = CloneMatrix(cityMap);
                SearchBuilding(ref cloneMap, i, j, ref rect);
                
                bool[,] buildingMap = new bool[rect.width, rect.height];
                for (i2 = 0; i2 < rect.width; i2++)
                    for (j2 = 0; j2 < rect.height; j2++)
                        buildingMap[i2, j2] = cityMap[rect.min.x + i2, rect.min.y + j2] > 0;

                var buildingPos = terrainGenerator.TileToCoords(rect.center.x, rect.center.y);
                var scale = 2.0f / terrainGenerator.Density * 0.99f;
                buildingPos.y = GetLandscapeAreaHeight(rect.min.x, rect.min.y, rect.max.x, rect.max.y) + 0.005f;
                var building = buildingGenerator.ConstructBuilding(buildingMap, floorsCount, (buildingPos.y - terrainGenerator.BottomY) / scale);
                building.transform.parent = _targetTransform;
                building.transform.localPosition = buildingPos;
                building.transform.localScale *= scale;

                cityMap = cloneMap;
            }
        }
    }

    public override void GenerateDefault()
    {
        Clear();
        _cityMap = null;
    }

    private int[,] GenerateBuildingMap(int width, int length, int height)
    {
        int[,] map = new int[width, length]; //генерируем массив одного здания
        int i, j;
        //заполняем нулями
        for (i = 0; i < width; i++)
            for (j = 0; j < length; j++)
                map[i, j] = 0;
        i = Random.Range(0, width); //берем случайную точку и заполняем переданной высотой
        j = Random.Range(0, length);
        map[i, j] = height;
        int counter = width * length / 2; //половина всех точек. (стреляем в сл направлении)
        //берем случайную точку рядом с той, с которой мы начали. берет либо 1, -1, либо j (лево право верх низ)
        //если это 0, то он ее заполняет (0 - значит данных еще нет)
        while (counter > 0)
        {
            if (Random.value > 0.5f)
                i = Mathf.Clamp(i + (Random.value > 0.5 ? 1 : -1), 0, width - 1);
            else
                j = Mathf.Clamp(j + (Random.value > 0.5 ? 1 : -1), 0, length - 1);

            if (map[i, j] == 0)
            {
                map[i, j] = height;
                counter--;
            }
        }
        return map;
    }
    
    private int GetBuildingAreaHeight(int x1, int y1, int x2, int y2)
    {
        float a = heights.GetHeightAtPixel(terrainGenerator.PointToTexture(x1, y1));
        float b = heights.GetHeightAtPixel(terrainGenerator.PointToTexture(x1, y2));
        float c = heights.GetHeightAtPixel(terrainGenerator.PointToTexture(x2, y1));
        float d = heights.GetHeightAtPixel(terrainGenerator.PointToTexture(x2, y2));
        float o = heights.GetHeightAtPixel(terrainGenerator.PointToTexture((x1 + x2) / 2, (y1 + y2) / 2));
        float sum = 0;
        int counter = 0;
        if (a >= 0) { sum += a; counter++; }
        if (b >= 0) { sum += b; counter++; }
        if (c >= 0) { sum += c; counter++; }
        if (d >= 0) { sum += d; counter++; }
        if (o >= 0) { sum += o; counter++; }
        return Mathf.Max(1, Mathf.RoundToInt(sum / counter * maxFloors));
    }

    private float GetLandscapeAreaHeight(int x1, int y1, int x2, int y2)
    {
        float max = -1;
        for (int i = x1; i <= x2; i++)
            for (int j = y1; j <= y2; j++)
                max = Mathf.Max(max, terrainGenerator.TileToCoords(i, j).y);
        return max;
    }
    

    private void SearchBuilding(ref int[,] map, int i, int j, ref RectInt rect)
    {
        if (i < 0 || i >= map.GetLength(0)) return;
        if (j < 0 || j >= map.GetLength(1)) return;
        if (map[i, j] < 1) return;
        map[i, j] = 0;
        rect.min = new Vector2Int(
            Mathf.Min(rect.min.x, i),
            Mathf.Min(rect.min.y, j)
        );
        rect.max = new Vector2Int(
            Mathf.Max(rect.max.x, i + 1),
            Mathf.Max(rect.max.y, j + 1)
        );
        SearchBuilding(ref map, i - 1, j, ref rect);
        SearchBuilding(ref map, i + 1, j, ref rect);
        SearchBuilding(ref map, i, j - 1, ref rect);
        SearchBuilding(ref map, i, j + 1, ref rect);
    }

    private int[,] CloneMatrix(int[,] origin)
    {
        int[,] copy = new int[origin.GetLength(0), origin.GetLength(1)];
        for (int i = 0; i < copy.GetLength(0); i++)
            for (int j = 0; j < copy.GetLength(1); j++)
                copy[i, j] = origin[i, j];
        return copy;
    }

    private void PrintMatrix(int[,] map)
    {
        StringBuilder sb = new StringBuilder(); 
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                sb.Append(map[i, j].ToString());
                sb.Append(" ");
            }
            sb.Append("\n");
        }
        Debug.Log(sb.ToString());
    }

    public bool TileBlocked(int i, int j) => 
        _cityMap != null && 
        i < _cityMap.GetLength(0) &&
        j < _cityMap.GetLength(1) &&
        _cityMap[i, j] != 0;
}
