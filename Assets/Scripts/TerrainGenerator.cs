using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : BaseGenerator
{
    private Mesh _landMesh;
    private Texture2D _terrainTexture;
    private MeshRenderer _terrainRenderer;
    [SerializeField] private WaterChanger _water;

    public int Density;
    public float HeightScale;
    public float WaterHeight;
    
    public Gradient LandColor;

    public float BottomY => -HeightScale;

    void Start()
    {
        //создаем меш
        _landMesh = new Mesh();
        _landMesh.name = "Landscape";

        _terrainRenderer = _targetTransform.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = _terrainRenderer.GetComponent<MeshFilter>();
        meshFilter.mesh = _landMesh; //вставляем меш в меш рендерер

        GenerateDefault();
    }

    public override void GenerateDefault() {}

    public override void Generate()
    {
        _landMesh.triangles = new int[0];
        _landMesh.uv = new Vector2[0];

        List<Vector3> points = new List<Vector3>(); //все точки
        List<int> indices = new List<int>(); //индексы (треугольники)
        List<Vector2> uv = new List<Vector2>(); //юв
        List<int> borders = new List<int>(); //границы
        
        // i по ширине, j по высоте
        // points and uv. Для начала заполняются верхние точки. 
        for (int i = 0; i <= Density; i++) //Денсити - кол-во полосок. денс 3, а линий 4. Поэтому <=
        {
            for (int j = 0; j <= Density; j++)
            {
                //добавляем точку
                float height = heights.GetHeightAtPixel(PointToTexture(i, j)); //берется высота на текстуре. тут высота . i j от 0 до 3
                points.Add(new Vector3(
                    Mathf.Lerp(-1, 1, (float) i / Density), //заполняет точки от 0 до 1
                    (height - 0.5f) * 2 * HeightScale, //выставляет высоту от 0 до 1 (или от -1 до 1 ?)
                    Mathf.Lerp(-1, 1, (float) j / Density)
                ));
                uv.Add(new Vector2((float) i / Density, (float) j / Density)); //добавляем юв равертку для нее 
            }
        }

        // top triangles
        int pc = Density + 1; // actual points count in a row
        float a,b,c,d;
        int ai, bi, ci, di;
        for (int i = 0; i < Density; i++)
        {
            for (int j = 0; j < Density; j++)
            {
                a = heights.GetHeightAtPixel(PointToTexture(i, j));
                b = heights.GetHeightAtPixel(PointToTexture(i, j + 1));
                c = heights.GetHeightAtPixel(PointToTexture(i + 1, j));
                d = heights.GetHeightAtPixel(PointToTexture(i + 1, j + 1));
                
                //индексы точек . высчитываем координаты точек в массиве
                //видеокарта работает с одномерными массивами . вертисес траянглс юв принимает только одномерный массив
                ai = i * pc + j;
                bi = i * pc + j + 1;
                ci = (i + 1) * pc + j;
                di = (i + 1) * pc + j + 1;

                //линии поведения. в высотах возвращается - 1, если точки нет
                //если а < 0, b < 0 в сумме полчится 2, учл не выплняется - пропускаем if
                if ((a < 0 ? 1 : 0) + (b < 0 ? 1 : 0) + (c < 0 ? 1 : 0) + (d < 0 ? 1 : 0) <= 1) //если не больше 1 точки не существует (2, 3, 4 не сущ - пропускаем условие). 
                {
                    //для каждого случая заполняем indices
                    if (a < 0)  //если точки а нет
                    {
                        indices.AddRange(new int[] { bi, di, ci}); 
                        borders.AddRange(new int[] { bi, ci });
                    }
                    else if (b < 0)
                    {
                        indices.AddRange(new int[] { ai, di, ci});
                        borders.AddRange(new int[] { di, ai });
                    }
                    else if (c < 0)
                    {
                        indices.AddRange(new int[] { ai, bi, di});
                        borders.AddRange(new int[] { ai, di });
                    }
                    else if (d < 0)
                    {
                        indices.AddRange(new int[] { ai, bi, ci});
                        borders.AddRange(new int[] { ci, bi });
                    }
                    else
                    {
                        indices.AddRange(new int[] { ai, bi, ci, bi, di, ci});
                    }
                }

                //добавляем в массив границ
                if (a >= 0 && b >= 0 && i == 0) //границы
                    borders.AddRange(new int[] { bi, ai });
                if (a >= 0 && b >= 0 && c < 0 && d < 0) //границы внутри дырки
                    borders.AddRange(new int[] { ai, bi });

                if (a >= 0 && c >= 0 && j == 0)
                    borders.AddRange(new int[] { ai, ci });
                if (a >= 0 && c >= 0 && b < 0 && d < 0)
                    borders.AddRange(new int[] { ci, ai });
                
                if (b >= 0 && d >= 0 && j == Density - 1)
                    borders.AddRange(new int[] { di, bi });
                if (b >= 0 && d >= 0 && a < 0 && c < 0)
                    borders.AddRange(new int[] { bi, di });
                
                if (c >= 0 && d >= 0 && i == Density - 1)
                    borders.AddRange(new int[] { ci, di });   
                if (c >= 0 && d >= 0 && a < 0 && b < 0)
                    borders.AddRange(new int[] { di, ci });  
            }
        }

        // side triangles . здесь уже боковушки
        Dictionary<int, (int, int)> bottomSideIndexes = new Dictionary<int, (int, int)>();
        for (int i = 0; i < borders.Count; i += 2)
        {
            if (Mathf.Abs(points[borders[i]].y - BottomY) < 0.01f && Mathf.Abs(points[borders[i + 1]].y - BottomY) < 0.01f) //если высота точек близка к нулю, то ничего не делаем (не рисуем)
                continue;
            int j = points.Count;
            indices.AddRange(new int[]{ j, j + 1, j + 2, j + 1, j + 3, j + 2 }); //заполняет индайсес
            points.Add(points[borders[i]]); //и добавляет нужные точки
            points.Add(points[borders[i + 1]]); //дублируем эти точки и закидываем заново
            points.Add(ChangeY(points[borders[i]], BottomY)); //закидываем такие же точки, но с высотой ниже.
            points.Add(ChangeY(points[borders[i + 1]], BottomY));
            
            for (int k = 0; k < 4; k++)
                uv.Add(uv[borders[i]]);
        }

        _landMesh.vertices = points.ToArray();
        _landMesh.triangles = indices.ToArray();
        _landMesh.uv = uv.ToArray();
        _landMesh.RecalculateBounds();
        _landMesh.RecalculateNormals();
        _landMesh.RecalculateTangents();

        // water
        _water.UpdateMesh(-HeightScale, Mathf.Lerp(-1f, 1f, WaterHeight) * HeightScale);

        // landscape texture
        if (_terrainTexture == null || _terrainTexture.width != heights.Texture.width)
        {
            _terrainTexture = new Texture2D(heights.Texture.width, heights.Texture.height, TextureFormat.RGB24, 3, false);
            _terrainTexture.name = "Terrain";
        }

        for (int i = 0; i < _terrainTexture.width; i++)
            for (int j = 0; j < _terrainTexture.height; j++)
                _terrainTexture.SetPixel(i, j, LandColor.Evaluate(heights.GetHeightAtPixel(i, j)));

        _terrainTexture.Apply();
        _terrainRenderer.material.SetTexture("_MainTex", _terrainTexture);
        _terrainRenderer.material.SetTexture("_BaseMap", _terrainTexture);
    }

    public Vector2Int PointToTexture(float i, float j) => new Vector2Int(
    
        Mathf.FloorToInt(i / Density * heights.Texture.width), // делим на денсити (3), умножаем на ширину текстуры. т.е. берем i-тую часть из N (денсити) частей и * на ширину = участок на карте, где нужно разместить точку
        Mathf.FloorToInt(j / Density * heights.Texture.height)
    );

    public Vector3 TileToCoords(float i, float j) => new Vector3(
    
        i / Density * 2 - 1,
        (heights.GetHeightAtPixel(PointToTexture(i, j)) * 2 - 1) * HeightScale,
        j / Density * 2 - 1
    );
    
    private Vector3 ChangeY(Vector3 a, float h){
        a.y = h;
        return a;
    }
}
