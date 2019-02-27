using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AutoAddTagSuffix : EditorWindow
{
    private string root = "Assets/Resources/Texture";
    private string suffix = "_en_";

    List<string> results = new List<string>();

    [MenuItem("Tools/AddTagSuffix")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(AutoAddTagSuffix));
    }

    void OnGUI()
    {
        GUILayout.Label("根路径：");
        root = EditorGUILayout.TextField(root);
        suffix = EditorGUILayout.TextField(suffix);
        if (GUILayout.Button("AutoAddTagSuffix"))
        {
            StartFind();
        }

        //显示查找结果
        if (results.Count > 0)
        {
            int i = 0;
            foreach (string t in results)
            {
                i++;
                if (i > 10)
                {
                    GUILayout.Label(" More print in Code/outPrefabTree.txt");
                    break;
                }
                GUILayout.Label(t);
            }
        }
        else
        {
            GUILayout.Label("无数据");
        }
    }

    void StartFind()
    {
        results.Clear();
        Debug.Log("开始查找.");
        DoAddTagSuffix(root);
    }

    void DoAddTagSuffix(string path)
    {
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            if (!(file.EndsWith(".png") || file.EndsWith(".jpg")))
            {
                continue;
            }

            TextureImporter ti = AssetImporter.GetAtPath(file) as TextureImporter;
            if (string.IsNullOrEmpty(ti.spritePackingTag) || ti.textureType != TextureImporterType.Sprite || ti.spritePackingTag.EndsWith(suffix))
            {
                continue;
            }
            UnityEngine.Debug.Log("    check  file  " + file + "   <change> " + ti.spritePackingTag + "  --->  " + ti.spritePackingTag + suffix);
            ti.spritePackingTag = ti.spritePackingTag + suffix;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
        }

        var directories = Directory.GetDirectories(path);
        foreach (var dir in directories)
        {
            DoAddTagSuffix(dir);
        }
    }
}