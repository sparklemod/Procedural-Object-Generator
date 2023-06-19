using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshUnion
{
    public static void AddMesh(Mesh addition, Mesh target, int submesh)
        => AddMesh(addition, target, submesh, Vector3.zero, Quaternion.identity, Vector3.one);

    public static void AddMesh(Mesh addition, Mesh target, int submesh, Vector3 position)
        => AddMesh(addition, target, submesh, position, Quaternion.identity, Vector3.one);

    public static void AddMesh(Mesh addition, Mesh target, int submesh, Vector3 position, Quaternion rotation)
        => AddMesh(addition, target, submesh, position, rotation, Vector3.one);


    public static void AddMesh(Mesh addition, Mesh target, int submesh, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        List<Vector3> vertices = new List<Vector3>();
        target.GetVertices(vertices);
        int start = vertices.Count;
        foreach (var vertex in addition.vertices)
            vertices.Add(rotation * Vector3.Scale(vertex, scale) + position);
        
        List<Vector2> uv = new List<Vector2>();
        target.GetUVs(0, uv);
        uv.AddRange(addition.uv);

        bool flip = Mathf.Sign(scale.x * scale.y * scale.z) < 0;
        List<int> triangles = new List<int>();
        if (target.subMeshCount > submesh)
            target.GetTriangles(triangles, submesh);
        else
            target.subMeshCount = submesh + 1;
        int[] newTriangles = addition.triangles;
        for (int i = 0, l = newTriangles.Length; i < l; i++)
            triangles.Add(newTriangles[flip ? (l - i - 1) : i] + start);

        target.SetVertices(vertices);
        target.SetUVs(0, uv);
        target.SetTriangles(triangles, submesh);
    }

    public static Mesh CloneMesh(Mesh mesh)
    {
        Mesh newmesh = new Mesh();
        newmesh.vertices = mesh.vertices;
        newmesh.normals = mesh.normals;
        newmesh.triangles = mesh.triangles;
        newmesh.uv = mesh.uv;
        newmesh.colors = mesh.colors;
        newmesh.tangents = mesh.tangents;

        return newmesh;
    }
}
