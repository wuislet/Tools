/*************************************************************
   Copyright(C) 2017 by WuIslet
   All rights reserved.
   
   FindInPrefab.cs
   Utils
   
   Created by WuIslet on 2018-02-01.
   
*************************************************************/
#if !USE_ILRUNTIME

using UnityEditor;
using UnityEngine;

public partial class FindInPrefab
{
    private bool isShowOtherTool = false;

    private void DrawOtherTool()
    {
        //isShowOtherTool = EditorGUILayout.Foldout(isShowOtherTool, "看看其他小公举", true);
        //if (isShowOtherTool)
        //{
        //    GUILayout.BeginVertical("Box");
        //    FindPathWithGUID();
        //    GUILayout.Space(20);
        //    GUILayout.EndVertical();
        //}
        //GUILayout.Space(50);
    }

    private string findguid = "";
    private string result = "";
    private void FindPathWithGUID()
    {
        GUILayout.Label("GUID：");
        findguid = EditorGUILayout.TextField(findguid);
        if (GUILayout.Button("查找GUID"))
        {
            result = "";
            if (!string.IsNullOrEmpty(findguid))
            {
                result = AssetDatabase.GUIDToAssetPath(findguid);
            }
        }
        //显示查找结果
        if (string.IsNullOrEmpty(result))
        {
            EditorGUILayout.SelectableLabel("无数据");
        }
        else
        {
            EditorGUILayout.SelectableLabel(result);
        }
    }
}
#endif