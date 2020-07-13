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
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

public partial class FindInPrefab
{
    // 1. 查找范围 SearchRangeClass
    private enum SearchRange
    {
        浏览文件夹,
        浏览物体,
        指定路径,
        //打包log,
        选中物体,
        拖入物体,
        当前场景,
        指定GUID使用yaml查找,
    }
    private SearchRange mSearchRange;
    private readonly Dictionary<SearchRange, Phase<Func<FindResult, bool>, List<FindResult>>> SearchRangeClass = new Dictionary<SearchRange, Phase<Func<FindResult, bool>, List<FindResult>>>
    {
        {SearchRange.浏览文件夹, new SearchExplorerFolder()},
        {SearchRange.浏览物体, new SearchExplorerFile()},
        {SearchRange.指定路径, new SearchInPath()},
        {SearchRange.选中物体, new SearchSelection()},
        {SearchRange.拖入物体, new SearchDrag()},
        {SearchRange.当前场景, new SearchScene()},
        {SearchRange.指定GUID使用yaml查找, new SearchWithGuid()},
    };

    #region class 浏览文件夹
    class SearchExplorerFolder : Phase<Func<FindResult, bool>, List<FindResult>>
    {
        string findPath = ROOT_PATH;
        public List<FindResult> FuncReturn(Func<FindResult, bool> input)
        {
            var list = new List<FindResult>();
            string[] searchGuids = AssetDatabase.FindAssets("t:gameObject", new[] { findPath });
            foreach (string guid in searchGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (go == null)
                    continue;

                var find = new FindResult
                {
                    go = go,
                    name = go.name,
                    path = path,
                };
                if (input == null || input(find))
                {
                    list.Add(find);
                }
            }
            return list;
        }

        public void OnDraw()
        {
            GUILayout.BeginHorizontal();
            findPath = EditorGUILayout.TextField("    文件夹：", findPath);
            if (GUILayout.Button("浏览", GUILayout.MaxWidth(100)))
            {
                findPath = EditorUtility.OpenFolderPanel("打开文件夹", findPath, "UI");
            }

            if (findPath.Contains(":/"))
            {
                findPath = findPath.Remove(0, Application.dataPath.Length - 6);
            }
            GUILayout.EndHorizontal();
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            return "";
        }
    }
    #endregion
    
    #region class 浏览物体
    class SearchExplorerFile : Phase<Func<FindResult, bool>, List<FindResult>>
    {
        string findPath = ROOT_PATH;
        public List<FindResult> FuncReturn(Func<FindResult, bool> input)
        {
            var list = new List<FindResult>();
            GameObject go = AssetDatabase.LoadAssetAtPath(findPath, typeof(GameObject)) as GameObject;
            if (go == null)
                return list;

            var find = new FindResult
            {
                go = go,
                name = go.name,
                path = findPath,
            };
            if (input == null || input(find))
            {
                list.Add(find);
            }
            return list;
        }

        public void OnDraw()
        {
            GUILayout.BeginHorizontal();
            findPath = EditorGUILayout.TextField("    文件：", findPath);
            if (GUILayout.Button("浏览", GUILayout.MaxWidth(100)))
            {
                findPath = EditorUtility.OpenFilePanel("打开文件", findPath, "prefab");
            }

            if (findPath.Contains(":/"))
            {
                findPath = findPath.Remove(0, Application.dataPath.Length - 6);
            }
            GUILayout.EndHorizontal();
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            return "";
        }
    }
    #endregion

    #region class 指定路径
    class SearchInPath : Phase<Func<FindResult, bool>, List<FindResult>>
    {
        private string findPath = ROOT_PATH;
        public List<FindResult> FuncReturn(Func<FindResult, bool> input)
        {
            var list = new List<FindResult>();
            string[] searchGuids = AssetDatabase.FindAssets("t:gameObject", new[] { findPath });
            foreach (string guid in searchGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (go == null)
                    continue;
                
                var find = new FindResult
                {
                    go = go,
                    name = go.name,
                    path = path,
                };
                if (input == null || input(find))
                {
                    list.Add(find);
                }
            }
            return list;
        }

        public void OnDraw()
        {
            findPath = EditorGUILayout.TextField("    指定路径：", findPath);
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            return "";
        }
    }
    #endregion

    #region class 选中物体
    class SearchSelection : Phase<Func<FindResult, bool>, List<FindResult>>
    {
        private bool isLock = false;
        private UnityEngine.Object target;
        private string targetPath;
        private string targetGUID;
        public List<FindResult> FuncReturn(Func<FindResult, bool> input)
        {
            var list = new List<FindResult>();
            var go = target as GameObject;
            if (go == null)
                return list;

            var find = new FindResult
            {
                go = go,
                name = go.name,
                path = targetPath,
            };
            if (input == null || input(find))
            {
                list.Add(find);
            }
            return list;
        }

        public void OnDraw()
        {
            if (target == null)
            {
                EditorGUILayout.HelpBox("没有选中物体", MessageType.Error);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("    名字：", GUILayout.MaxWidth(50));
            GUILayout.TextArea(target == null ? "无选中" : target.name);
            isLock = GUILayout.Toggle(isLock, "锁定选中", GUILayout.MaxWidth(100));
            if (!isLock)
            {
                target = Selection.activeObject;
            }

            GUILayout.EndHorizontal();

            targetPath = AssetDatabase.GetAssetPath(target);
            targetGUID = AssetDatabase.AssetPathToGUID(targetPath);
            GUILayout.BeginHorizontal();
            GUILayout.Label("    GUID：", GUILayout.MaxWidth(50));
            EditorGUILayout.TextArea(targetGUID);
            GUILayout.EndHorizontal();
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            return "";
        }
    }
    #endregion

    #region class 拖入物体
    class SearchDrag : Phase<Func<FindResult, bool>, List<FindResult>>
    {
        private GameObject mTarget;
        public List<FindResult> FuncReturn(Func<FindResult, bool> input)
        {
            var list = new List<FindResult>();
            if (mTarget != null)
            {
                var find = new FindResult
                {
                    go = mTarget,
                    name = mTarget.name,
                    path = AssetDatabase.GetAssetPath(mTarget),
                };
                if (input == null || input(find))
                {
                    list.Add(find);
                }
            }
            return list;
        }

        public void OnDraw()
        {
            mTarget = (GameObject)EditorGUILayout.ObjectField(mTarget, typeof(GameObject), true);
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            return "";
        }
    }
    #endregion

    #region class 当前场景
    class SearchScene : Phase<Func<FindResult, bool>, List<FindResult>>
    {
        private bool isLock = false;
        private UnityEngine.Object target;
        private string targetPath;
        private string targetGUID;
        public List<FindResult> FuncReturn(Func<FindResult, bool> input)
        {
            var list = new List<FindResult>();
            GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
            
            foreach (GameObject pObject in pAllObjects)
            {
                if (pObject.transform.parent != null)
                {
                    continue;
                }

                if (pObject.hideFlags == HideFlags.NotEditable || pObject.hideFlags == HideFlags.HideAndDontSave)
                {
                    continue;
                }

                if (Application.isEditor)
                {
                    string sAssetPath = AssetDatabase.GetAssetPath(pObject.transform.root.gameObject);
                    if (!string.IsNullOrEmpty(sAssetPath))
                    {
                        continue;
                    }
                }

                var find = new FindResult
                {
                    go = pObject,
                    name = pObject.name,
                    path = pObject.name,
                };
                if (input == null || input(find))
                {
                    list.Add(find);
                }
            }
            return list;
        }

        public void OnDraw()
        {
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            return "";
        }
    }
    #endregion

    #region class 指定GUID使用yaml查找
    class SearchWithGuid : Phase<Func<FindResult, bool>, List<FindResult>>
    {
        List<string> references = new List<string>();
        int totalWaitMilliseconds = 300 * 1000;

        private string targetGUID;
        private bool isFileID;
        private string targetFileID;
        public List<FindResult> FuncReturn(Func<FindResult, bool> input)
        {
            references.Clear();
            string appDataPath = Application.dataPath;
            int cpuCount = Environment.ProcessorCount;
            var keyword = isFileID ? string.Format("\"fileID: {0}, guid: {1}\"", targetFileID, targetGUID) : string.Format("\"guid: {0}\"", targetGUID);
            var psi = new ProcessStartInfo();

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                psi.FileName = "/usr/bin/mdfind";
                psi.Arguments = string.Format("-onlyin {0} {1}", appDataPath, keyword);
                totalWaitMilliseconds = 2 * 1000;
            }
            else
            {
                psi.FileName = Path.Combine(Environment.CurrentDirectory, @"Tool\rg.exe");
                psi.Arguments = string.Format("--case-sensitive --follow --files-with-matches --no-text --fixed-strings " +
                                              "--ignore-file Assets/Editor/ignore.txt " +
                                              "--threads {0} --regexp {1} -- {2}",
                    cpuCount, keyword, appDataPath);
            }

            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;

            Process process = new Process();
            process.StartInfo = psi;

            process.OutputDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;

                string relativePath = e.Data.Replace(appDataPath, "Assets").Replace("\\", "/");
                references.Add(relativePath);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!process.HasExited)
            {
                if (stopwatch.ElapsedMilliseconds < totalWaitMilliseconds)
                {
                    float progress = (float)((double)stopwatch.ElapsedMilliseconds / totalWaitMilliseconds);
                    string info = string.Format("Finding {0}/{1}s {2:P2}", stopwatch.ElapsedMilliseconds / 1000,
                        totalWaitMilliseconds / 1000, progress);
                    bool canceled = EditorUtility.DisplayCancelableProgressBar("Find References in Project", info, progress);

                    if (canceled)
                    {
                        process.Kill();
                        break;
                    }

                    Thread.Sleep(100);
                }
                else
                {
                    process.Kill();
                    break;
                }
            }
            var list = new List<FindResult>();
            for (int i = 0, len = references.Count; i < len; ++i)
            {
                var file = references[i];
                string assetPath = file;
                if (file.EndsWith(".meta"))
                {
                    assetPath = file.Substring(0, file.Length - ".meta".Length);
                }

                var obj = AssetDatabase.LoadMainAssetAtPath(assetPath) as GameObject;
                if (obj != null)
                {
                    var find = new FindResult
                    {
                        go = obj,
                        name = obj.name,
                        path = assetPath,
                    };
                    if (input == null || input(find))
                    {
                        list.Add(find);
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            stopwatch.Stop();
            return list;
        }

        public void OnDraw()
        {
            targetGUID = EditorGUILayout.TextField("    GUID：", targetGUID);
            isFileID = GUILayout.Toggle(isFileID, "是否查找FileID？");
            if (isFileID)
            {
                targetFileID = EditorGUILayout.TextField("    FileID：", targetFileID);
            }
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (isDS)
            {
                return "GUID查找暂时不支持深度查找";
            }
            return "";
        }
    }
    #endregion

    private bool mIsDeepSearch = true;

    private bool DrawSearchRange()
    {
        bool isOK = true;
        GUILayout.BeginVertical("Box");

        mSearchRange = (SearchRange)EditorGUILayout.EnumPopup("查找范围：", mSearchRange, GUILayout.MinWidth(100));
        if (SearchRangeClass.ContainsKey(mSearchRange))
        {
            var cls = SearchRangeClass[mSearchRange];
            var error = cls.CheckNeed(isSavePrefab, mSearchRange, mIsDeepSearch, mHitRule, mIsHitEnable, mAfterDeal);
            if (!string.IsNullOrEmpty(error))
            {
                isOK = false;
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }
            cls.OnDraw();
        }
        mIsDeepSearch = GUILayout.Toggle(mIsDeepSearch, "是否深度查找每个组件？");

        GUILayout.EndVertical();
        return isOK;
    }
}

#endif