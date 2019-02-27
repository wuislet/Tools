/*************************************************************
   Copyright(C) 2017 by dayugame
   All rights reserved.
   
   PrintAllPrefabTree.cs
   Utils
   
   Created by WuIslet on 2018-02-01.
   
*************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

public class PrintAllPrefabTree : EditorWindow
{
    private string root = "Assets/LuaFramework/MyResources/UI";
    private string resourceFile = "outPrefabTree.txt";

    List<string> results = new List<string>();

    [MenuItem("Tools/PrintAllPrefabTree")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(PrintAllPrefabTree));
    }

    void OnGUI()
    {
        GUILayout.Label("根路径：");
        root = EditorGUILayout.TextField(root);
        if (GUILayout.Button("PrintAllPrefabTree"))
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
        DoFindFullPath();
        WriteFile();
    }

    // 配置文件的格式：
    // 1. 以End Compare为结尾
    // 1. 每行以path:后面紧接需要查找的资源路径 e.g. path:Asset/Resources/a.png
    void WriteFile()
    {
        FileStream fs = new FileStream(resourceFile, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);

        foreach (var result in results)
        {
            sw.WriteLine(result);
        }

        sw.Close(); fs.Close();
    }

    void DoFindFullPath()
    {
        string[] lookFor = { root };
        string[] guids2 = AssetDatabase.FindAssets("t:gameObject", lookFor);
        int i = 0;
        foreach (string guid in guids2)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log("Search one.. " + path + "   ->   ");
            results.Add("================== " + path + " ==================");
            GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            DFSImgInPrefab(go.transform, new StringBuilder("  \\-> "));
            i++;
            //if(i > 2)
            //    break;
        }
    }

    void DFSImgInPrefab(Transform parent, StringBuilder path)
    {
        path.Append("/" + parent.name);
        results.Add(path.ToString());

        for (int i = 0; i < parent.childCount; i++)
        {
            DFSImgInPrefab(parent.GetChild(i), path);
        }

        var l = parent.name.Length + 1;
        path.Remove(path.Length - l, l);
    }
}
