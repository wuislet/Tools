/*************************************************************
   Copyright(C) 2017 by dayugame
   All rights reserved.
   
   FindUsage.cs
   Utils
   
   Created by WuIslet on 2018-02-01.
   
*************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

class ReplaceStringOneLine
{
    public int id;
    public string path;
    public string origin;
    public string translation;
}

public class ReplaceStringInPrefab : EditorWindow
{
    private string resourceFile = "prefab中文字符.json";
    private bool needReverse = false;

    List<string> results = new List<string>();
    Dictionary<string, bool> IsReplaced = new Dictionary<string, bool>();

    [MenuItem("Tools/ReplaceStringInPrefab")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(ReplaceStringInPrefab));
    }

    void OnGUI()
    {
        GUILayout.Label("资源默认路径： " + Application.dataPath.Remove(Application.dataPath.Length - 6) + "TranslationJson/" + resourceFile);
        GUILayout.Label("本程序按照特定格式的Json替换字符，Json模板用Tools/CheckString生成。");
        GUILayout.Label("翻译后的Json文件路径：");
        resourceFile = EditorGUILayout.TextField(resourceFile);
        if (GUILayout.Button("StartReplace"))
        {
            results.Clear();
            if (string.IsNullOrEmpty(resourceFile))
            {
                GUILayout.Label("请输入查找对象");
                return;
            }

            resourceFile = Application.dataPath.Remove(Application.dataPath.Length - 6) + "TranslationJson/" + resourceFile;

            if (!File.Exists(resourceFile))
            {
                GUILayout.Label("文件路径不正确");
            }

            Debug.Log("开始查找.");
            ReadFile();
        }

        needReverse = EditorGUILayout.Toggle("翻译回中文", needReverse);
        if (results.Count > 0)
        {
            foreach (string t in results)
            {
                GUILayout.Label(t);
            }
        }
        else
        {
            GUILayout.Label("无数据");
        }
    }
    
    void ReadFile()
    {
        FileStream fs = new FileStream(resourceFile, FileMode.Open);
        StreamReader sr = new StreamReader(fs);
        
        JsonSerializer serializer = new JsonSerializer();
        StringReader sr2 = new StringReader(sr.ReadToEnd());
        List<ReplaceStringOneLine> data = serializer.Deserialize(new JsonTextReader(sr2), typeof(List<ReplaceStringOneLine>)) as List<ReplaceStringOneLine>;

        fs.Close();
        sr.Close();
        sr2.Close();

        foreach (var replaceStringOneLine in data)
        {
            DoReplace(replaceStringOneLine);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    void DoReplace(ReplaceStringOneLine onedata)
    {
        if (needReverse)
        {
            var tmp = onedata.origin;
            onedata.origin = onedata.translation;
            onedata.translation = tmp;
        }
        var key = onedata.path + "!@#$" + onedata.origin;
        if (IsReplaced.ContainsKey(key))
        {
            return;
        }
        else
        {
            IsReplaced.Add(key, true);
        }

        GameObject go = AssetDatabase.LoadAssetAtPath(onedata.path, typeof(GameObject)) as GameObject;
        //var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (go == null)
        {
            results.Add("不存在这个预制件: " + onedata.path);
            return;
        }

        var texts = go.transform.GetComponentsInChildren<UnityEngine.UI.Text>(true);
        bool flag = true;
        foreach (var txt in texts)
        {
            if (txt.text == onedata.origin)
            {
                txt.text = txt.text.Replace(onedata.origin, onedata.translation);
                EditorUtility.SetDirty(go);
                Debug.Log("Replace one ..." + onedata.origin + "   ->   " + onedata.translation);
                flag = false;
                //results.Add(onedata.path);
            }
        }

        if(flag && needReverse)
        {
            results.Add("  ！！ 警告:  这个文字没还原成功" + onedata.path + " -> " + onedata.origin);
        }

        //PrefabUtility.ConnectGameObjectToPrefab(go, prefab);
    }
}
