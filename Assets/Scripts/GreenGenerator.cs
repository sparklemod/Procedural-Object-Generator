using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenGenerator : BaseGenerator
{
    public bool EnableGrass;
    [SerializeField] private GameObject _grassPrefab;
    [SerializeField] private GameObject _bushPrefab;
    [SerializeField] private GameObject _treePrefab;
    [SerializeField] private TerrainGenerator _terrainGenerator;
    [SerializeField] private BuildingGroupGenerator _buildingGenerator;

    public int Density, GrassInsideTile, ObjectsInsideTile;
    public float DeformationStrength = 1f;

    private Mesh grassFieldMesh;

    public override void GenerateDefault()
    {
        Clear();
    }

    public override void Generate()
    {
        Clear();
        
        float defaultScale = 1.0f / Density;

        // grass
        if (EnableGrass)
        {
            Mesh singleGrass = _grassPrefab.GetComponent<MeshFilter>().sharedMesh;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> triangles = new List<int>();

            Vector3[] sourceVertices = singleGrass.vertices;
            Vector2[] sourceUv = singleGrass.uv;
            int[] sourceTriangles = singleGrass.triangles;

            Quaternion rotation;

            int grassFieldIndex = 1;
            CreateNewGrass(grassFieldIndex++);

            for (int i = 0, start; i < Density; i++)
                for (int j = 0; j < Density; j++)
                {
                    for (int k = 0; k < GrassInsideTile; k++)
                    {
                        Vector2 coord = new Vector2(i + Random.value, j + Random.value);
                        float pixelHeight = heights.GetHeightAtPixel(PointToTexture(coord.x, coord.y));
                        if (pixelHeight <= _terrainGenerator.WaterHeight) continue;

                        Vector2 tileOnTerrain = new Vector2(
                            (float) coord.x / Density * _terrainGenerator.Density,
                            (float) coord.y / Density * _terrainGenerator.Density
                        );
                        if (_terrainGenerator.heights.GetHeightAtPixel(_terrainGenerator.PointToTexture(tileOnTerrain.x, tileOnTerrain.y)) < 0)
                            continue;
                        if (_buildingGenerator.TileBlocked((int) tileOnTerrain.x, (int) tileOnTerrain.y))
                            continue;
                        Vector3 grassPos = _terrainGenerator.TileToCoords(tileOnTerrain.x, tileOnTerrain.y);

                        rotation = Quaternion.Euler(0, 360 * Random.value, 0);
                        start = vertices.Count;
                        for (int v = 0; v < sourceVertices.Length; v++)
                        {
                            vertices.Add(rotation * (sourceVertices[v] * defaultScale) + grassPos);
                            uv.Add(sourceUv[v]);
                        }
                        for (int t = 0, l = sourceTriangles.Length; t < l; t++)
                            triangles.Add(sourceTriangles[t] + start);

                        if (triangles.Count > 60000)
                        {
                            ApplyChangesToGrass(vertices, uv, triangles);
                            vertices = new List<Vector3>();
                            uv = new List<Vector2>();
                            triangles = new List<int>();
                            CreateNewGrass(grassFieldIndex++);
                        }
                    }
                }
            
            ApplyChangesToGrass(vertices, uv, triangles);
        }

        // trees and bushes
        int treeCounter = 0, bushCounter = 0;
        for (int i = 0; i < Density; i++)
            for (int j = 0; j < Density; j++)
            {
                for (int k = 0; k < ObjectsInsideTile; k++)
                {
                    Vector2 coord = new Vector2(i + Random.value, j + Random.value);
                    float pixelHeight = heights.GetHeightAtPixel(PointToTexture(coord.x, coord.y));
                    if (pixelHeight <= _terrainGenerator.WaterHeight) continue;

                    Vector2 tileOnTerrain = new Vector2(
                        (float) coord.x / Density * _terrainGenerator.Density,
                        (float) coord.y / Density * _terrainGenerator.Density
                    );
                    if (_terrainGenerator.heights.GetHeightAtPixel(_terrainGenerator.PointToTexture(tileOnTerrain.x, tileOnTerrain.y)) < 0)
                        continue;
                    if (_buildingGenerator.TileBlocked((int) tileOnTerrain.x, (int) tileOnTerrain.y))
                        continue;

                    bool isTree = pixelHeight > 0.4f;
                    GameObject obj = Object.Instantiate(isTree ? _treePrefab : _bushPrefab, _targetTransform);
                    obj.name = isTree ? ("Tree " + treeCounter++) : ("Bush " + bushCounter++);
                    obj.transform.position = _terrainGenerator.TileToCoords(tileOnTerrain.x, tileOnTerrain.y);

                    MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                    meshFilter.mesh = CreateDeformedMesh(meshFilter.mesh, Quaternion.Euler(0, 360 * Random.value, 0), defaultScale * 10 * pixelHeight);
                    meshFilter.mesh.name = obj.name;
                }
            }
    }

    private void CreateNewGrass(int index)
    {
        grassFieldMesh = new Mesh();
        grassFieldMesh.name = "Grass " + index;

        MeshFilter grassField = Object.Instantiate(_grassPrefab, _targetTransform).GetComponent<MeshFilter>();
        grassField.transform.position = Vector3.zero;
        grassField.mesh = grassFieldMesh;
    }

    private void ApplyChangesToGrass(List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
    {
        grassFieldMesh.vertices = vertices.ToArray();
        grassFieldMesh.uv = uv.ToArray();
        grassFieldMesh.triangles = triangles.ToArray();
        grassFieldMesh.RecalculateBounds();
        grassFieldMesh.RecalculateNormals();
        grassFieldMesh.RecalculateTangents();
    }

    private const int deformSlices = 5;
    private Mesh CreateDeformedMesh(Mesh origin, Quaternion rotation, float scale)
    {
        Mesh newMesh = MeshUnion.CloneMesh(origin);

        Vector3 displacement = Vector3.zero;
        Vector3[] vertices = newMesh.vertices;
        float minY = origin.bounds.min.y - 0.01f, maxY = origin.bounds.max.y + 0.01f, height = maxY - minY;
        for (int i = 0; i < deformSlices; i++)
        {
            for (int v = 0; v < vertices.Length; v++)
            {
                int sliceIndex = Mathf.FloorToInt((vertices[v].y - minY) / height * deformSlices);
                if (sliceIndex != i) continue;
                vertices[v] = rotation * ((vertices[v] + displacement) * scale);
            }
            displacement += new Vector3(Random.value, 0, Random.value) * DeformationStrength;
        }
        newMesh.vertices = vertices;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();
        return newMesh;
    }

    public Vector2Int PointToTexture(float i, float j) => new Vector2Int(
    
        Mathf.FloorToInt(i / Density * heights.Texture.width),
        Mathf.FloorToInt(j / Density * heights.Texture.height)
    );

}
