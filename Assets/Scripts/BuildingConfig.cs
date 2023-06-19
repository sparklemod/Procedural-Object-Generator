using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Building Config")]
public class BuildingConfig : ScriptableObject
{
    public Mesh foundation;
    public Mesh[] walls;

    public Mesh roof1, roof2, roof3, roof4, roof5, roof6, roof7, roof8, roof9, roof10, roof11, roof12, roof13, roof14;

    public Mesh RandomWall => walls[Random.Range(0, walls.Length)];

    // roof 1    roof 2    roof 3    roof 4    roof 5    roof 6    roof 7    roof 8    roof 9    roof 10   roof 11   roof 12   roof 13   roof 13
    // x x x     x x ?     ? o ?     o x ?     x x x     ? o ?     o x x     o x o     o x o     x x o     ? o ?     o x o     x x o     x x o
    // x X x     x X o     x X o     x X o     x X x     x X x     x X x     x X x     x x x     x x x     o x o     x X x     x x x     x x x
    // ? o ?     ? o ?     ? o ?     ? o ?     x x x     ? o ?     ? o ?     ? o ?     o x o     o x o     ? o ?     x x x     x x x     o x x

    // o = 0, ? = 1, x = 2
    private static int[,] roof1Map = new int[3,3]
    {
        {2, 2, 2},
        {2, 2, 2},
        {1, 0, 1}
    };
    private static int[,] roof2Map = new int[3,3]
    {
        {2, 2, 1},
        {2, 2, 0},
        {1, 0, 1}
    };
    private static int[,] roof3Map = new int[3,3]
    {
        {1, 0, 1},
        {2, 2, 0},
        {1, 0, 1}
    };
    private static int[,] roof4Map = new int[3,3]
    {
        {0, 2, 1},
        {2, 2, 0},
        {1, 0, 1}
    };
    private static int[,] roof5Map = new int[3,3]
    {
        {2, 2, 2},
        {2, 2, 2},
        {2, 2, 2}
    };
    private static int[,] roof6Map = new int[3,3]
    {
        {1, 0, 1},
        {2, 2, 2},
        {1, 0, 1}
    };
    private static int[,] roof7Map1 = new int[3,3]
    {
        {0, 2, 2},
        {2, 2, 2},
        {1, 0, 1}
    };
    private static int[,] roof7Map2 = Invert(roof7Map1);
    private static int[,] roof8Map = new int[3,3]
    {
        {0, 2, 0},
        {2, 2, 2},
        {1, 0, 1}
    };
    private static int[,] roof9Map = new int[3,3]
    {
        {0, 2, 0},
        {2, 2, 2},
        {0, 2, 0}
    };
    private static int[,] roof10Map1 = new int[3,3]
    {
        {2, 2, 0},
        {2, 2, 2},
        {0, 2, 0}
    };
    private static int[,] roof10Map2 = Invert(roof10Map1);
    private static int[,] roof11Map = new int[3,3]
    {
        {1, 0, 1},
        {0, 2, 0},
        {1, 0, 1}
    };
    private static int[,] roof12Map = new int[3,3]
    {
        {0, 2, 0},
        {2, 2, 2},
        {2, 2, 2}
    };
    private static int[,] roof13Map = new int[3,3]
    {
        {2, 2, 0},
        {2, 2, 2},
        {2, 2, 2}
    };
    private static int[,] roof14Map = new int[3,3]
    {
        {2, 2, 0},
        {2, 2, 2},
        {0, 2, 2}
    };

    public (Mesh, Quaternion, Vector3) GetRoofTile(bool[,] roofMap)
    {
        var check = DefaultCheck(roof1Map, roofMap, roof1);
        if (check.Item1 != null) return check;

        check = DefaultCheck(roof2Map, roofMap, roof2);
        if (check.Item1 != null) return check;

        check = DefaultCheck(roof3Map, roofMap, roof3);
        if (check.Item1 != null) return check;

        check = DefaultCheck(roof4Map, roofMap, roof4);
        if (check.Item1 != null) return check;

        if (Match(roof5Map, roofMap, CompareStraight))
            return (roof5, Quaternion.Euler(0, 0, 0), Vector3.one);

        if (Match(roof6Map, roofMap, CompareStraight))
            return (roof6, Quaternion.Euler(0, 90, 0), Vector3.one);
        if (Match(roof6Map, roofMap, CompareRotate1))
            return (roof6, Quaternion.Euler(0, 0, 0), Vector3.one);

        check = DefaultCheck(roof7Map1, roofMap, roof7);
        if (check.Item1 != null) return check;
        check = DefaultCheck(roof7Map2, roofMap, roof7);
        if (check.Item1 != null)
        {
            check.Item3.z = -1;
            return check;
        }

        check = DefaultCheck(roof8Map, roofMap, roof8);
        if (check.Item1 != null) return check;

        if (Match(roof9Map, roofMap, CompareStraight))
            return (roof9, Quaternion.Euler(0, 0, 0), Vector3.one);

        check = DefaultCheck(roof10Map1, roofMap, roof10);
        if (check.Item1 != null) return check;
        check = DefaultCheck(roof10Map2, roofMap, roof10);
        if (check.Item1 != null)
        {
            check.Item3.z = -1;
            return check;
        }

        if (Match(roof11Map, roofMap, CompareStraight))
            return (roof11, Quaternion.Euler(0, 0, 0), Vector3.one);

        check = DefaultCheck(roof12Map, roofMap, roof12);
        if (check.Item1 != null) return check;

        check = DefaultCheck(roof13Map, roofMap, roof13);
        if (check.Item1 != null) return check;

        if (Match(roof14Map, roofMap, CompareStraight))
            return (roof14, Quaternion.Euler(0, 90, 0), Vector3.one);
        if (Match(roof14Map, roofMap, CompareRotate1))
            return (roof14, Quaternion.Euler(0, 0, 0), Vector3.one);
        
        return (roof5, Quaternion.identity, Vector3.one);
    }

    private (Mesh, Quaternion, Vector3) DefaultCheck(int[,] a, bool[,] b, Mesh mesh)
    {
        if (Match(a, b, CompareStraight))
            return (mesh, Quaternion.Euler(0, 90, 0), Vector3.one);
        if (Match(a, b, CompareRotate1))
            return (mesh, Quaternion.Euler(0, 180, 0), Vector3.one);
        if (Match(a, b, CompareRotate2))
            return (mesh, Quaternion.Euler(0, -90, 0), Vector3.one);
        if (Match(a, b, CompareRotate3))
            return (mesh, Quaternion.Euler(0, 0, 0), Vector3.one);
        return (null, Quaternion.identity, Vector3.one);
    }

    private static bool Match(int[,] a, bool[,] b, System.Func<int[,], bool[,], int, int, bool> check)
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (a[i, j] != 1 && !check(a, b, i, j)) return false;
        return true;        
    }

    private static bool CompareStraight(int[,] a, bool[,] b, int i, int j) => (a[i, j] == 2 ? true : false) == b[i, j];
    private static bool CompareRotate1 (int[,] a, bool[,] b, int i, int j) => (a[i, j] == 2 ? true : false) == b[j, 2 - i];
    private static bool CompareRotate2 (int[,] a, bool[,] b, int i, int j) => (a[i, j] == 2 ? true : false) == b[2 - i, 2 - j];
    private static bool CompareRotate3 (int[,] a, bool[,] b, int i, int j) => (a[i, j] == 2 ? true : false) == b[2 - j, i];

    private static int[,] Invert(int[,] a)
    {
        int[,] b = new int[3,3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                b[i, j] = a[2 - i, j];
        return b;
    }
}
