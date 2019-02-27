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
using System.Text;


public class FindUsageInPrefab : EditorWindow
{
    private string root = "Assets/Resources/UI";
    private string findPath1 = null;
    private string findPath2 = null;
    private string findPath3 = null;
    private string resourceFile = null;

    private System.Action<string> FindFunc = null;

    List<string> results = new List<string>();

    [MenuItem("Tools/查找资源的引用")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(FindUsageInPrefab));
    }

    void OnGUI()
    {
        GUILayout.Label("根路径：");
        root = EditorGUILayout.TextField(root);
        GUILayout.Label("待查找的资源路径(优先查找这个资源。从Assets/开始的路径）：");
        GUILayout.BeginHorizontal();
        findPath1 = EditorGUILayout.TextField(findPath1);
        findPath2 = EditorGUILayout.TextField(findPath2, GUILayout.MaxWidth(100));
        findPath3 = EditorGUILayout.TextField(findPath3, GUILayout.MaxWidth(50));
        GUILayout.EndHorizontal();
        GUILayout.Label("待查找的配置文件路径（上面填空才会找这个文件）：");
        resourceFile = EditorGUILayout.TextField(resourceFile);

        if (GUILayout.Button("Find"))
        {
            FindFunc = DoFind;
            StartFind();
        }
        GUILayout.BeginHorizontal();
        resourceType = (ResourceType)EditorGUILayout.EnumPopup(resourceType, GUILayout.MaxWidth(50));
        if (GUILayout.Button("FindFullPath"))
        {
            FindFunc = DoFindFullPath;
            StartFind();
        }
        GUILayout.EndHorizontal();
        if (resourceType == ResourceType.图片) //TODO 将SetNative作为额外操作的勾选项提供。
        {
            if (GUILayout.Button("SetNative"))
            {
                FindFunc = DoSetNativeSize;
                StartFind();
            }
        }

        //显示查找结果
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

    void StartFind()
    {
        // 开始查找
        if (FindFunc == null)
        {
            GUILayout.Label("Error: 没有查找方法");
            return;
        }
        results.Clear();
        Debug.Log("开始查找.");
        var findPath = findPath1 + findPath2 + findPath3;
        if (!string.IsNullOrEmpty(findPath))
        {
            FindFunc(findPath);
        }
        else
        {
            if (string.IsNullOrEmpty(resourceFile))
            {
                GUILayout.Label("请输入查找对象");
                return;
            }

            if (!File.Exists(resourceFile))
            {
                GUILayout.Label("文件路径不正确");
            }

            ReadFile(FindFunc);
        }
    }

    // 配置文件的格式：
    // 1. 以End Compare为结尾
    // 1. 每行以path:后面紧接需要查找的资源路径 e.g. path:Asset/Resources/a.png
    void ReadFile(System.Action<string> FindFunc)
    {
        int linecnt = 0;
        FileStream fs = new FileStream(resourceFile, FileMode.Open);
        StreamReader sr = new StreamReader(fs);
        string line = sr.ReadLine();
        while (line != null)
        {
            linecnt += 1;
            if (line.Contains("End Compare"))
            {
                break;
            }

            var index = line.LastIndexOf("path:");
            var path = line.Remove(0, index + 5);
            Debug.Log("  Find:  " + path);
            FindFunc(path);
            line = sr.ReadLine();
            Debug.Log("================================ find end =======================================");
        }
        sr.Close();
    }

    #region 核心遍历方法
    public struct FindResultContext
    {
        public string guid;
        public string path;

        public FindResultContext(string guid, string path)
        {
            this.guid = guid;
            this.path = path;
        }
    }

    public delegate bool FindCallback(FindResultContext callback);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="FindSameCallback">retrun True means EARLY break.</param>
    void Search(string findPath, FindCallback FindSameCallback)
    {
        string[] lookFor = { root };
        string[] searchGuids = AssetDatabase.FindAssets("t:gameObject", lookFor);
        foreach (string guid in searchGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            //Debug.Log("Finding... " + path);
            foreach (var resourcesName in AssetDatabase.GetDependencies(path))
            {
                var sourceGuid = AssetDatabase.AssetPathToGUID(findPath);
                var targetGuid = AssetDatabase.AssetPathToGUID(resourcesName);
                if (sourceGuid == targetGuid)
                {
                    Debug.Log("Search one.. " + path + "   ->   " + resourcesName);
                    results.Add(path);
                    if (FindSameCallback != null && FindSameCallback(new FindResultContext(sourceGuid, path)))
                    {
                        break;
                    }
                }
            }
        }
    }
    #endregion

    void DoFind(string findPath)
    {
        Search(findPath, null);
    }

    void DoFindFullPath(string findPath)
    {
        Search(findPath, (ctx) =>
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(ctx.path, typeof(GameObject)) as GameObject;
                DFSImgInPrefab(go.transform, ctx.guid, new StringBuilder("  \\-> "));
                return true;
            }
        );
    }

    void DFSImgInPrefab(Transform parent, string sourceGuid, StringBuilder path)
    {
        path.Append("/" + parent.name);
        var targetGuid = GetGuid(parent);
        if (targetGuid == sourceGuid)
        {
            results.Add(path.ToString());
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            DFSImgInPrefab(parent.GetChild(i), sourceGuid, path);
        }

        var l = parent.name.Length + 1;
        path.Remove(path.Length - l, l);
    }

    void DoSetNativeSize(string findPath)
    {
        Search(findPath, (ctx) =>
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(ctx.path, typeof(GameObject)) as GameObject;
                var images = go.transform.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                foreach (var image in images)
                {
                    var guid3 = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(image.sprite));
                    if (guid3 == ctx.guid)
                    {
                        Debug.Log("   SetNativeSize one  " + image.gameObject.name);
                        image.SetNativeSize();
                    }
                }
                return true;
            }
        );
        AssetDatabase.SaveAssets();
    }

    #region 资源比对方法
    enum ResourceType
    {
        图片 = 0,
        X2音频,
    }
    private ResourceType resourceType;
    private delegate string GetGuidFun(Transform transform);
    private Dictionary<ResourceType, GetGuidFun> funs = new Dictionary<ResourceType, GetGuidFun>
    {
        {ResourceType.图片, GetImageSpriteGuid },
        {ResourceType.X2音频, GetX2AudioGuid },
    };

    string GetGuid(Transform transform)
    {
        if (!funs.ContainsKey(resourceType))
            return null;

        var func = funs[resourceType];
        return func(transform);
    }

    static string GetImageSpriteGuid(Transform transform)
    {
        var com = transform.GetComponent<UnityEngine.UI.Image>();
        if (com == null || com.sprite == null)
            return null;

        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(com.sprite));
    }

    static string GetX2AudioGuid(Transform transform)
    {
        var com = transform.GetComponent<UISound>();
        if (com == null || com.clip == null)
            return null;

        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(com.clip));
    }
    #endregion
}
