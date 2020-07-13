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
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public partial class FindInPrefab
{
    // 2. 命中判断 HitRuleClass
    private enum HitRule
    {
        名字含有,
        tag等于,
        含有控件,
        控件为空,
        含有指定图片资源,
        指定GUID使用rg查找,
        选中资源使用rg查找,
    }
    private HitRule mHitRule;
    private readonly Dictionary<HitRule, Phase<FindResult, bool>> HitRuleClass = new Dictionary<HitRule, Phase<FindResult, bool>>
    {
        {HitRule.名字含有, new HitWithName()},
        {HitRule.tag等于, new HitWithTag()},
        {HitRule.含有控件, new HitWithComponent()},
        {HitRule.控件为空, new HitWithNullComponent()},
        {HitRule.含有指定图片资源, new HitWithTexture()},
        {HitRule.指定GUID使用rg查找, new HitWithGuidByRG()},
        {HitRule.选中资源使用rg查找, new HitWithSelectByRG()},
    };

    #region class 名字含有
    class HitWithName : Phase<FindResult, bool>
    {
        private string[] example = new[] { "效果图", "BGBlur" };
        private string keyword;
        public bool FuncReturn(FindResult input)
        {
            if (string.IsNullOrEmpty(keyword))
                return true;

            return input.name.Contains(keyword);
        }

        public void OnDraw()
        {
            keyword = EditorGUILayout.TextField("    关键字：", keyword);
            EditorGUILayout.BeginHorizontal();
            foreach (var key in example)
            {
                if (GUILayout.Button(key, GUILayout.MaxWidth(100)))
                {
                    keyword = key;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            return "";
        }
    }
    #endregion

    #region class tag等于
    class HitWithTag : Phase<FindResult, bool>
    {
        private string[] example = new[] {"EditorOnly", "UI_ControlData"};
        private string keyword;
        public bool FuncReturn(FindResult input)
        {
            if (string.IsNullOrEmpty(keyword))
                return true;

            return input.go.CompareTag(keyword);
        }

        public void OnDraw()
        {
            keyword = EditorGUILayout.TextField("    关键字：", keyword);
            EditorGUILayout.BeginHorizontal();
            foreach (var key in example)
            {
                if (GUILayout.Button(key, GUILayout.MaxWidth(100)))
                {
                    keyword = key;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            return "";
        }
    }
    #endregion

    #region class 含有控件
    class HitWithComponent : Phase<FindResult, bool>
    {
        private bool findWithString = true;
        private string[] example = new[] { "Graphic", "Selectable", "Text", "TextMeshProUGUI", "X2UIWindowAsset" };
        private string component;
        private MonoScript scriptObj;
        public bool FuncReturn(FindResult input)
        {
            if (findWithString)
            {
                if (string.IsNullOrEmpty(component))
                    return true;

                return input.go.GetComponent(component) != null;
            }
            else
            {
                if (scriptObj == null)
                    return true;
                return input.go.GetComponent(scriptObj.GetClass()) != null;
            }
        }

        public void OnDraw()
        {
            findWithString = GUILayout.Toggle(findWithString, "使用名字查找？");
            if (findWithString)
            {
                component = EditorGUILayout.TextField("    控件名：", component);
                EditorGUILayout.BeginHorizontal();
                foreach (var key in example)
                {
                    if (GUILayout.Button(key, GUILayout.MaxWidth(100)))
                    {
                        component = key;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                scriptObj = (MonoScript)EditorGUILayout.ObjectField(scriptObj, typeof(MonoScript), true);
            }
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            return "";
        }
    }
    #endregion

    #region class 控件为空
    class HitWithNullComponent : Phase<FindResult, bool>
    {
        public bool FuncReturn(FindResult input)
        {
            var list = input.go.GetComponents<Component>();
            for (int i = 0, len = list.Length; i < len; ++i)
            {
                var component = list[i];
                if (component == null)
                    return true;
            }

            return false;
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

    #region class 含有指定图片资源
    class HitWithTexture : Phase<FindResult, bool>
    {
        private bool isLock = false;
        private Object target;
        private string targetGUID;
        public bool FuncReturn(FindResult input)
        {
            if (target == null)
                return false;

            var graphic = input.go.GetComponent<Graphic>();
            if (graphic != null)
            {
                return targetGUID == AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(graphic.mainTexture));
            }

            var selectable = input.go.GetComponent<Selectable>();
            if (selectable != null)
            {
                if (targetGUID == AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectable.spriteState.highlightedSprite)))
                    return true;
                if (targetGUID == AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectable.spriteState.pressedSprite)))
                    return true;
                if (targetGUID == AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectable.spriteState.disabledSprite)))
                    return true;
            }
            return false;
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

            string selectedAssetPath = AssetDatabase.GetAssetPath(target);
            targetGUID = AssetDatabase.AssetPathToGUID(selectedAssetPath);
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

    #region class 指定GUID使用rg查找
    class HitWithGuidByRG : Phase<FindResult, bool>
    {
        List<string> references = new List<string>();

        private string targetGUID;
        private bool isFileID;
        private string targetFileID;
        public bool FuncReturn(FindResult input)
        {
            references.Clear();
            var name = Path.GetFileName(input.path);
            string appDataPath = Application.dataPath + Path.GetDirectoryName(input.path).Substring(6);
            int cpuCount = Environment.ProcessorCount;
            var keyword = isFileID ? string.Format("\"fileID: {0}, guid: {1}\"", targetFileID, targetGUID) : string.Format("\"guid: {0}\"", targetGUID);
            var psi = new ProcessStartInfo();

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                psi.FileName = "/usr/bin/mdfind";
                psi.Arguments = string.Format("-onlyin {0} {1}", appDataPath, keyword);
            }
            else
            {
                psi.FileName = Path.Combine(Environment.CurrentDirectory, @"Tool\rg.exe");
                psi.Arguments = string.Format("--case-sensitive --follow --files-with-matches --no-text --fixed-strings " +
                                              "--ignore-file Assets/Editor/ignore.txt -g {3} " +
                                              "--threads {0} --regexp {1} -- {2}",
                    cpuCount, keyword, appDataPath, name);
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
            
            while (!process.HasExited)
            {
                Thread.Sleep(100);
            }

            var isFind = references.Count > 0;
            return isFind;
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

    #region class 选中资源使用rg查找
    class HitWithSelectByRG : Phase<FindResult, bool>
    {
        List<string> references = new List<string>();

        private bool isLock = false;
        private Object target;
        private string targetGUID;
        private bool isSprite;
        private string targetFileID;
        public bool FuncReturn(FindResult input)
        {
            references.Clear();
            var name = Path.GetFileName(input.path);
            string appDataPath = Application.dataPath + Path.GetDirectoryName(input.path).Substring(6);
            int cpuCount = Environment.ProcessorCount;
            var keyword = isSprite ? string.Format("\"fileID: {0}, guid: {1}\"", targetFileID, targetGUID) : string.Format("\"guid: {0}\"", targetGUID);
            var psi = new ProcessStartInfo();

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                psi.FileName = "/usr/bin/mdfind";
                psi.Arguments = string.Format("-onlyin {0} {1}", appDataPath, keyword);
            }
            else
            {
                psi.FileName = Path.Combine(Environment.CurrentDirectory, @"Tool\rg.exe");
                psi.Arguments = string.Format("--case-sensitive --follow --files-with-matches --no-text --fixed-strings " +
                                              "--ignore-file Assets/Editor/ignore.txt -g {3} " +
                                              "--threads {0} --regexp {1} -- {2}",
                    cpuCount, keyword, appDataPath, name);
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

            while (!process.HasExited)
            {
                Thread.Sleep(100);
            }

            var isFind = references.Count > 0;
            return isFind;
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

            string selectedAssetPath = AssetDatabase.GetAssetPath(target);
            targetGUID = AssetDatabase.AssetPathToGUID(selectedAssetPath);
            GUILayout.BeginHorizontal();
            GUILayout.Label("    GUID：", GUILayout.MaxWidth(50));
            EditorGUILayout.TextArea(targetGUID);
            GUILayout.EndHorizontal();

            isSprite = (target as Sprite) != null;
            if (isSprite)
            {
                #region 获取FileID
                PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

                SerializedObject serializedObject = new SerializedObject(target);
                inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

                SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");
                #endregion

                targetFileID = localIdProp.intValue.ToString();
                GUILayout.BeginHorizontal();
                GUILayout.Label("    FileID：", GUILayout.MaxWidth(50));
                EditorGUILayout.TextArea(targetFileID);
                GUILayout.EndHorizontal();
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

    private bool mIsHitEnable = false;

    private bool DrawHitRule()
    {
        bool isOK = true;
        GUILayout.BeginVertical("Box");

        mIsHitEnable = GUILayout.Toggle(mIsHitEnable, "是否要使用命中判断(不选则无条件全命中)？");
        if (mIsHitEnable)
        {
            mHitRule = (HitRule)EditorGUILayout.EnumPopup("命中判断：", mHitRule, GUILayout.MinWidth(100));
            if (HitRuleClass.ContainsKey(mHitRule))
            {
                var cls = HitRuleClass[mHitRule];
                var warning = cls.CheckNeed(isSavePrefab, mSearchRange, mIsDeepSearch, mHitRule, mIsHitEnable, mAfterDeal);
                if (!string.IsNullOrEmpty(warning))
                {
                    isOK = false;
                    EditorGUILayout.HelpBox(warning, MessageType.Error);
                }
                cls.OnDraw();
            }
        }

        GUILayout.EndVertical();
        return isOK;
    }
}
#endif