using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GeneratedObjExporter : MonoBehaviour
{
    [SerializeField] private Transform _targetObjectsParent;
    
    // Start is called before the first frame update
    public void Export ()
    {
        string filePath = Path.Combine(Application.dataPath, "Generated");
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        OBJExport exporter = new OBJExport();
        exporter.applyPosition = true;
        exporter.applyRotation = true;
        exporter.applyScale = true;
        exporter.generateMaterials = true;
        exporter.exportTextures = true;
        exporter.splitObjects = true;
        exporter.objNameAddIdNum = false;

        exporter.Export(_targetObjectsParent.GetComponentsInChildren<MeshFilter>(), filePath + "/result.obj");

        System.Diagnostics.Process.Start(filePath);
    }
}
