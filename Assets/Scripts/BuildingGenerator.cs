using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _buildingBase;
    [SerializeField] private BuildingConfig _config;

    private bool[,] currentMap;
    private int width, length;
    private Mesh finalMesh;

    public GameObject ConstructBuilding(bool[,] map, int floors, float foundationHeight)
    {
        currentMap = map;
        width = map.GetLength(0);
        length = map.GetLength(1);

        finalMesh = new Mesh();
        finalMesh.name = "building";

        // foundation and walls
        Mesh mesh;
        float y;
        Vector3 scale;
        for (int floor = -1; floor < floors; floor++)
        {
            if (floor < 0)
            {
                y = 0; scale = new Vector3(1, foundationHeight, 1);
            }
            else
            {
                y = floor; scale = new Vector3(1, 1, 1);
            }
            for (int i = 0; i < width; i++)
                for (int j = 0; j < length; j++)
                {
                    if (!Filled(i, j)) continue;
                    mesh = floor < 0 ? _config.foundation : _config.RandomWall;
                    if (!Filled(i - 1, j))
                        MeshUnion.AddMesh(
                            mesh, finalMesh, 0,
                            new Vector3(-width / 2.0f + i, y, -length / 2.0f + j + 0.5f),
                            Quaternion.Euler(0, -90, 0),
                            scale
                        );
                    if (!Filled(i + 1, j))
                        MeshUnion.AddMesh(
                            mesh, finalMesh, 0,
                            new Vector3(-width / 2.0f + i + 1, y, -length / 2.0f + j + 0.5f),
                            Quaternion.Euler(0, 90, 0),
                            scale
                        );
                    if (!Filled(i, j - 1))
                        MeshUnion.AddMesh(
                            mesh, finalMesh, 0,
                            new Vector3(-width / 2.0f + i + 0.5f, y, -length / 2.0f + j),
                            Quaternion.Euler(0, 180, 0),
                            scale
                        );
                    if (!Filled(i, j + 1))
                        MeshUnion.AddMesh(
                            mesh, finalMesh, 0,
                            new Vector3(-width / 2.0f + i + 0.5f, y, -length / 2.0f + j + 1),
                            Quaternion.Euler(0, 0, 0),
                            scale
                        );
                }
        }
        
        // roof
        bool[,] roofMap = new bool[3,3];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < length; j++)
            {
                if (!Filled(i, j)) continue;

                for (int i1 = 0; i1 < 3; i1++)
                    for (int j1 = 0; j1 < 3; j1++)
                        roofMap[i1, j1] = Filled(i + i1 - 1, j + j1 - 1);
                        
                (Mesh, Quaternion, Vector3) tileConfig = _config.GetRoofTile(roofMap);
                MeshUnion.AddMesh(
                    tileConfig.Item1,
                    finalMesh,
                    0,
                    new Vector3(-width / 2.0f + i + 0.5f, floors, -length / 2.0f + j + 0.5f),
                    tileConfig.Item2,
                    Vector3.Scale(tileConfig.Item3, new Vector3(1f, 0.2f, 1f))
                );
            }

        finalMesh.RecalculateBounds();
        finalMesh.RecalculateNormals();
        finalMesh.RecalculateTangents();

        GameObject newBuilding = Object.Instantiate(_buildingBase);
        newBuilding.GetComponent<MeshFilter>().mesh = finalMesh;
        newBuilding.transform.parent = transform;

        return newBuilding;
    }

    private bool Filled(int i, int j)
    {
        if (i < 0 || i >= width || j < 0 || j >= length) return false;
        return currentMap[i, j];
    }
}
