/*************************************************************
   Copyright(C) 2017 by WuIslet
   All rights reserved.
   
   FindInPrefab.cs
   Utils
   
   Created by WuIslet on 2018-02-01.
   
*************************************************************/
#if !USE_ILRUNTIME

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public partial class FindInPrefab : EditorWindow
{
    interface Phase<TI, TO>
    {
        TO FuncReturn(TI input);
        void OnDraw();  // UI界面的绘制
        string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad); //确认前提条件
    }

    class FindResult
    {
        public GameObject go;
        public string name;
        public string path;
    }
    
    private const string ROOT_PATH = "Assets/Resources/UI";
    private string printFileName = "FindResult";
    private const string subSeparatorChar = "/";

    private bool init = false;
    private const float MIN_WINDOW_WIDTH = 500;
    private const float MIN_WINDOW_HEIGHT = 300f;
    Vector2 _scrollPos;
    Texture2D buttonTexture;

    private bool isSavePrefab = false;
    private bool _logFoldout = true;
    private Dictionary<FindResult, List<string>> mResultLog = new Dictionary<FindResult, List<string>>();

    [MenuItem("X2Tools/局外工具/超级查找prefab")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(FindInPrefab));
    }
    
    private void Initialize()
    {
        minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
    }

    void OnGUI()
    {
        // Style initialization
        if (!init)
        {
            Initialize();
            init = true;
        }

        DrawOtherTool();

        isSavePrefab = GUILayout.Toggle(isSavePrefab, "是否要保存prefab的修改？");
        var isOK = true;
        isOK &= DrawSearchRange();
        isOK &= DrawHitRule();
        isOK &= DrawAfterDeal();

        GUILayout.Space(20);

        if (GUILayout.Button("开始"))
        {
            if (!isOK)
            {
                EditorUtility.DisplayDialog("提示", "条件有错误", "OK");
            }
            else
            {
                StartFind();
                OpenShowResultWindow();
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("输出文件", GUILayout.MaxWidth(50));
        printFileName = EditorGUILayout.TextField(printFileName, GUILayout.MaxWidth(200));
        if (GUILayout.Button("打印查找结果到文件"))
        {
            PrintToFile();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        //显示查找结果
        EditorGUILayout.BeginHorizontal();
        _logFoldout = EditorGUILayout.Foldout(_logFoldout, _logFoldout ? "收起" : "展开", true);
        if (mResultLog.Count > 0)
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(string.Format("命中Prefab个数：{0}", mResultLog.Count), GUILayout.MaxWidth(200));
            var cnt = 0;
            foreach (var kv in mResultLog)
            {
                cnt += kv.Value.Count;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(string.Format("共命中{0}行", cnt), GUILayout.MaxWidth(100));
        }
        EditorGUILayout.EndHorizontal();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        if (_logFoldout)
        {
            if (mResultLog.Count <= 0)
            {
                EditorGUILayout.TextArea("没有结果。");
            }
            else
            {
                foreach (var kv in mResultLog)
                {
                    StringBuilder sb = new StringBuilder();
                    var list = kv.Value;
                    for (int i = 0, len = list.Count; i < len; ++i)
                    {
                        var line = list[i];
                        sb.AppendLine((i + 1) + ": " + line);
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    var prefabName = kv.Key.name;
                    EditorGUILayout.LabelField(prefabName, GUILayout.MaxWidth(200));
                    EditorGUILayout.LabelField(string.Format("共{0}行", list.Count), GUILayout.MaxWidth(100));
                    if (GUILayout.Button("复制prefab名", GUILayout.MaxWidth(100)))
                    {
                        EditorGUIUtility.systemCopyBuffer = prefabName;
                        UnityEngine.Debug.Log(" 复制prefab名>> " + prefabName);
                    }
                    
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    //var prefab = kv.Key.go;
                    //if (AssetPreview.GetAssetPreview(prefab) == null)
                    //{
                    //    buttonTexture = AssetPreview.GetMiniThumbnail(prefab);
                    //}
                    //else
                    //{
                    //    buttonTexture = AssetPreview.GetAssetPreview(prefab);
                    //}
                    //if (GUILayout.Button(buttonTexture, GUILayout.Width(80), GUILayout.Height(80)))
                    //{
                    //    Selection.activeObject = prefab;
                    //    SceneView.RepaintAll();
                    //}
                    EditorGUILayout.TextArea(sb.ToString());
                    GUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void StartFind()
    {
        mResultLog.Clear();
        Debug.Log("开始查找.");
        Func<FindResult, bool> filter = null;
        if (mIsHitEnable)
        {
            filter = HitRuleClass[mHitRule].FuncReturn;
        }
        
        var searchResult = SearchRangeClass[mSearchRange].FuncReturn(null);
        if (!mIsDeepSearch) //prefab查找
        {
            foreach (var go in searchResult)
            {
                if (filter == null || filter(go))
                {
                    var log = AfterDealClass[mAfterDeal].FuncReturn(go);
                    if (!string.IsNullOrEmpty(log))
                    {
                        mResultLog.Add(go, new List<string>{log});
                    }
                }

                if (isSavePrefab)
                {
                    EditorUtility.SetDirty(go.go);
                }
            }
        }
        else //深入查找
        {
            foreach (var go in searchResult)
            {
                var isFind = false;
                List<string> onePrefabResult = new List<string>();
                var compList = BFSPrefab(go, filter);
                foreach (var fr in compList)
                {
                    var log = AfterDealClass[mAfterDeal].FuncReturn(fr);
                    if (!string.IsNullOrEmpty(log))
                    {
                        onePrefabResult.Add(log);
                        isFind = true;
                    }
                }

                if (isFind)
                {
                    mResultLog.Add(go, onePrefabResult);
                }

                if (isSavePrefab)
                {
                    EditorUtility.SetDirty(go.go);
                }
            }
        }

        if (isSavePrefab)
        {
            AssetDatabase.SaveAssets();
        }
    }

    static List<FindResult> BFSPrefab(FindResult go, Func<FindResult, bool> filter = null)
    {
        List<FindResult> result = new List<FindResult>();
        result.Add(go);
        int head = 0;
        int tail = 1;
        while (head != tail)
        {
            var curGo = result[head];
            head++;
            for (int i = 0; i < curGo.go.transform.childCount; ++i)
            {
                var childTr = curGo.go.transform.GetChild(i);
                var child = new FindResult();
                child.go = childTr.gameObject;
                child.name = childTr.name;
                child.path = curGo.path + subSeparatorChar + childTr.name;

                result.Add(child);
                tail++;
            }
        }

        if (filter != null)
        {
            for (int i = result.Count - 1; i >= 0; --i)
            {
                if (!filter(result[i]))
                {
                    result.RemoveAt(i);
                }
            }
        }

        return result;
    }

    private void PrintToFile()
    {
        if (mResultLog.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有查找结果", "OK");
            return;
        }

        if (!Directory.Exists(printFileName))
        {
            Directory.CreateDirectory(printFileName);
        }

        using (FileStream fs = new FileStream(printFileName + Path.DirectorySeparatorChar + printFileName + ".txt", FileMode.Create))
        {
            var sw = new StreamWriter(fs);
            foreach (var kv in mResultLog)
            {
                sw.WriteLine(kv.Key.name);
                var list = kv.Value;
                for (int i = 0, len = list.Count; i < len; ++i)
                {
                    var line = list[i];
                    sw.WriteLine("    |-> " + line);
                }
            }
            sw.Close();
        }
        string resultDir = Path.GetFullPath(printFileName) + Path.DirectorySeparatorChar;
        System.Diagnostics.Process.Start(resultDir);
    }

    private void OpenShowResultWindow()
    {
        if (mResultLog.Count > 0)
        {
            int[] findIds = new int[mResultLog.Count];
            int index = -1;
            foreach (var result in mResultLog.Keys)
            {
                var find = result.go;
                findIds[++index] = find.GetInstanceID();
            }
            FilterShowAssets(findIds);
        }
    }

    #region Tools
    /// <summary>
    /// Project中筛选显示指定的资源
    /// TwoColumns模式下，会将资源集中到一起；
    /// OneColumn模式下，仅选中资源。
    /// </summary>
    /// <param name="instanceIds">要显示的资源id，通过ObjectGetInstanceID()获取</param>
    public static void FilterShowAssets(int[] instanceIds)
    {
        try
        {
            var t = typeof(ProjectWindowUtil);
            var obj = t.ReflectionPrivateStaticCall<object>("GetProjectBrowserIfExists");
            
            if (null != obj)
            {

                if (obj.ReflectionPrivateGetField<int>("m_ViewMode") == 1)
                {
                    obj.ReflectionPrivateCall<object, int[]>("ShowObjectsInList", instanceIds);
                    return;
                }
            }

            Selection.instanceIDs = instanceIds;
            //        _staticBrowser.CallPrivateStaticMethod("ShowSelectedObjectsInLastInteractedProjectBrowser");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    #endregion
}
#endif