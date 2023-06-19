#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using System.Collections.Generic;

/*=============================================================================
 |	    Project:  Unity3D Scene OBJ Exporter
 |
 |		  Notes: Only works with meshes + meshRenderers. No terrain yet
 |
 |       Author:  aaro4130
 |
 |     DO NOT USE PARTS OF THIS CODE, OR THIS CODE AS A WHOLE AND CLAIM IT
 |     AS YOUR OWN WORK. USE OF CODE IS ALLOWED IF I (aaro4130) AM CREDITED
 |     FOR THE USED PARTS OF THE CODE.
 |
 *===========================================================================*/

public class OBJExporter : ScriptableWizard
{
    //public bool materialsUseTextureName = false;
    public bool onlySelectedObjects = false;
    public bool applyPosition = true;
    public bool applyRotation = true;
    public bool applyScale = true;
    public bool generateMaterials = true;
    public bool exportTextures = true;
    public bool splitObjects = true;
    public bool autoMarkTexReadable = false;
    public bool objNameAddIdNum = false;

    bool StaticBatchingEnabled()
    {
        PlayerSettings[] playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
        if (playerSettings == null)
        {
            return false;
        }
        SerializedObject playerSettingsSerializedObject = new SerializedObject(playerSettings);
        SerializedProperty batchingSettings = playerSettingsSerializedObject.FindProperty("m_BuildTargetBatching");
        for (int i = 0; i < batchingSettings.arraySize; i++)
        {
            SerializedProperty batchingArrayValue = batchingSettings.GetArrayElementAtIndex(i);
            if (batchingArrayValue == null)
            {
                continue;
            }
            IEnumerator batchingEnumerator = batchingArrayValue.GetEnumerator();
            if (batchingEnumerator == null)
            {
                continue;
            }
            while (batchingEnumerator.MoveNext())
            {
                SerializedProperty property = (SerializedProperty)batchingEnumerator.Current;
                if (property != null && property.name == "m_StaticBatching")
                {
                    return property.boolValue;
                }
            }
        }
        return false;
    }

    void OnWizardUpdate()
    {
        helpString = "Aaro4130's OBJ Exporter " + OBJExport.versionString;
    }

    void OnWizardCreate()
    {
        if(StaticBatchingEnabled() && Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Static batching is enabled. This will cause the export file to look like a mess, as well as be a large filesize. Disable this option, and restart the player, before continuing.", "OK");
            return;
        }
        if (autoMarkTexReadable)
        {
            int yes = EditorUtility.DisplayDialogComplex("Warning", "This will convert all textures to Advanced type with the read/write option set. This is not reversible and will permanently affect your project. Continue?", "Yes", "No", "Cancel");
            if(yes > 0)
                return;
        }
        string lastPath = EditorPrefs.GetString("a4_OBJExport_lastPath", "");
        string lastFileName = EditorPrefs.GetString("a4_OBJExport_lastFile", "unityexport.obj");
        string expFile = EditorUtility.SaveFilePanel("Export OBJ", lastPath, lastFileName, "obj");
        if (expFile.Length > 0)
        {
            var fi = new System.IO.FileInfo(expFile);
            EditorPrefs.SetString("a4_OBJExport_lastFile", fi.Name);
            EditorPrefs.SetString("a4_OBJExport_lastPath", fi.Directory.FullName);
            
            OBJExport exporter = new OBJExport();
            exporter.applyPosition = applyPosition;
            exporter.applyRotation = applyRotation;
            exporter.applyScale = applyScale;
            exporter.generateMaterials = generateMaterials;
            exporter.exportTextures = exportTextures;
            exporter.splitObjects = splitObjects;
            exporter.autoMarkTexReadable = autoMarkTexReadable;
            exporter.objNameAddIdNum = objNameAddIdNum;
            EditorUtility.DisplayProgressBar("Exporting OBJ", "Please wait.. Starting export.", 0);
            
            //get list of required export things
            MeshFilter[] sceneMeshes;
            if (onlySelectedObjects)
            {
                List<MeshFilter> tempMFList = new List<MeshFilter>();
                foreach (GameObject g in Selection.gameObjects)
                {
                    MeshFilter f = g.GetComponent<MeshFilter>();
                    if (f != null)
                    {
                        tempMFList.Add(f);
                    }
                }
                sceneMeshes = tempMFList.ToArray();
            }
            else
            {
                sceneMeshes = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
            }

            exporter.Export(sceneMeshes, expFile);
            EditorUtility.ClearProgressBar();
        }
    }

    [MenuItem("Assets/Export/Wavefront OBJ")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Export OBJ", typeof(OBJExporter), "Export");
    }
}
#endif