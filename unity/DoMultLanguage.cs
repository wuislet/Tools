/*************************************************************
   Copyright(C) 2017 by dayugame
   All rights reserved.
   
   CheckString.cs
   Utils
   
   Created by WuIslet on 2018-02-01.
   
*************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

#region 复制资源

public class CopyAsset : EditorWindow
{
    private string newName = "";
    
    [MenuItem("Tools/多语言/1. 复制中文资源到英文路径下", false, 1)]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CopyAsset));
    }

    void OnGUI()
    {
        GUILayout.Label("待处理的文件夹名：");
        newName = EditorGUILayout.TextField(newName);
        if (GUILayout.Button("开始复制"))
        {
            if(string.IsNullOrEmpty(newName))
            {
                EditorUtility.DisplayDialog("错误", "请输入文件夹名字", "OK");
                return;
            }
            DoCopyAsset();
        }
    }
    
    public void DoCopyAsset()
    {
        Debug.Log("   开始复制   ");
        string oldUIPath = "Assets/LuaFramework/MyResources/UI/" + newName;
        string oldAssetPath = "Assets/LuaFramework/MyResources/Textures/" + newName;
        string newUIPath = "Assets/LuaFramework/MyResources_en_/UI/" + newName;
        string newAssetPath = "Assets/LuaFramework/MyResources_en_/Textures/" + newName;
        
        if (!Directory.Exists(oldUIPath))
        {
            EditorUtility.DisplayDialog("错误", "不存在文件路径：" + oldUIPath, "OK");
            return;
        }
        if (!Directory.Exists(oldAssetPath))
        {
            EditorUtility.DisplayDialog("错误", "不存在文件路径：" + oldAssetPath, "OK");
            return;
        }
        
        if (!Directory.Exists(newUIPath))
        {
            Directory.CreateDirectory(newUIPath);
        }
        if (!Directory.Exists(newAssetPath))
        {
            Directory.CreateDirectory(newAssetPath);
        }
        
        CopyDir(oldUIPath, newUIPath);
        CopyDir(oldAssetPath, newAssetPath);
        
        Debug.Log("   复制完成   ");
        EditorUtility.DisplayDialog("提示", "复制完成", "OK");
    }
    
    public void CopyDir(string srcPath, string aimPath)
    {
        // 检查目标目录是否以目录分割字符结束如果不是则添加
        if (aimPath[aimPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
        {
            aimPath += System.IO.Path.DirectorySeparatorChar;
        }
        // 判断目标目录是否存在如果不存在则新建
        if (!System.IO.Directory.Exists(aimPath))
        {
            System.IO.Directory.CreateDirectory(aimPath);
        }
        // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
        // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
        // string[] fileList = Directory.GetFiles（srcPath）；
        string[] fileList = System.IO.Directory.GetFileSystemEntries(srcPath);
        // 遍历所有的文件和目录
        foreach (string file in fileList)
        {
            // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
            if (System.IO.Directory.Exists(file))
            {
                CopyDir(file, aimPath + System.IO.Path.GetFileName(file));
            }
            // 否则直接Copy文件
            else
            {
                System.IO.File.Copy(file, aimPath + System.IO.Path.GetFileName(file), true);
            }
        }
    }
    
}
#endregion

#region 检查中文字符
class CheckStringOneLine
{
    private static int globalID;
    public int id;
    public string path;
    public string origin;
    public string translation;

    public CheckStringOneLine(string path, string origin)
    {
        id = globalID;
        this.path = path;
        this.origin = origin;
        translation = origin;
        globalID += 1;
        if (globalID > 999999)
        {
            globalID = 0;
        }
    }
}

public class CheckString : EditorWindow
{
    private string root = "Assets/LuaFramework/MyResources/UI";
    
    [MenuItem("Tools/多语言/2. 查找prefab中的中文", false, 2)]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CheckString));
    }

    void OnGUI()
    {
        GUILayout.Label("根路径：");
        root = EditorGUILayout.TextField(root);
        if (GUILayout.Button("查找所有中文"))
        {
            DoCheckString();
        }
    }
    
    public void DoCheckString()
    {
        string[] lookFor = { root };
        string[] guids2 = AssetDatabase.FindAssets("t:gameObject", lookFor);
        List<CheckStringOneLine> strs = new List<CheckStringOneLine>();

        Debug.Log("   开始查找中文   ");
        foreach (string guid in guids2)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            string newPath = path.Replace("MyResources", "MyResources_en_");
            var texts = go.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            if(texts != null && texts.Length > 0)
            {
                foreach(var text in texts)
                {
                    if(text.text != null && text.text.Length > 0)
                    {
                        if(System.Text.RegularExpressions.Regex.IsMatch(text.text, @"[\u4e00-\u9fa5]"))
                        {
                            strs.Add(new CheckStringOneLine(newPath, text.text));
                        }
                    }
                }
            }
        }

        Debug.Log("   查找完成   ");

        string res = Newtonsoft.Json.JsonConvert.SerializeObject(strs, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText("prefab中文字符.json", res);
        string resultDir = Path.GetFullPath(".") + Path.DirectorySeparatorChar;
        EditorUtility.DisplayDialog("提示", "查找结果在：\"" + resultDir + "prefab中文字符.json\"。\n请手动把里面的所有\"translation\"项翻译成英文", "OK");
        System.Diagnostics.Process.Start(resultDir);

        //System.IO.FileStream fs = new FileStream("text.txt", FileMode.OpenOrCreate);
        //StreamWriter sw = new StreamWriter(fs);
        //foreach(var str in strs)
        //{
        //    sw.WriteLine(str);
        //}

        //sw.Close();
    }
}
#endregion

#region 替换中文字符
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

    [MenuItem("Tools/多语言/3. 替换prefab中的中文", false, 3)]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(ReplaceStringInPrefab));
    }

    void OnGUI()
    {
        GUILayout.Label("资源默认路径： " + Application.dataPath.Remove(Application.dataPath.Length - 6) + resourceFile);
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

            resourceFile = Application.dataPath.Remove(Application.dataPath.Length - 6) + resourceFile;

            if (!File.Exists(resourceFile))
            {
                GUILayout.Label("文件路径不正确");
            }

            Debug.Log("开始查找.");
            ReadFile();
            Debug.Log("查找结束.");
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
#endregion

#region 修改英文图片的tag后缀
public class AutoAddTagSuffix : EditorWindow
{
    private string root = "Assets/LuaFramework/MyResources_en_/Textures";
    private string suffix = "_en";

    [MenuItem("Tools/多语言/4. 在英文版路径下添加图片资源的tag后缀", false, 4)]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(AutoAddTagSuffix));
    }

    void OnGUI()
    {
        GUILayout.Label("根路径：");
        root = EditorGUILayout.TextField(root);
        suffix = EditorGUILayout.TextField(suffix);
        if (GUILayout.Button("自动添加资源tag后缀"))
        {
            StartFind();
        }
        if (GUILayout.Button("检查不规范的资源tag后缀"))
        {
            StartCheck();
        }
    }

    void StartFind()
    {
        Debug.Log("开始查找.");
        DoAddTagSuffix(root);
        AssetDatabase.SaveAssets();
        Debug.Log("查找结束.");
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

            var oldguid = AssetDatabase.AssetPathToGUID(file);

            UnityEngine.Debug.LogWarning("    check  file  " + file + "   <change> " + ti.spritePackingTag + "  --->  " + ti.spritePackingTag + suffix);
            ti.spritePackingTag = ti.spritePackingTag + suffix;
            ti.mipmapEnabled = false;
            ti.SaveAndReimport();
            EditorUtility.SetDirty(ti);

            var newguid = AssetDatabase.AssetPathToGUID(file);
            if (oldguid != newguid)
            {
                UnityEngine.Debug.LogError("   ???????  WTF   guid <change> " + oldguid + "  --->  " + newguid + "  p:  " + file);
            }
        }

        var directories = Directory.GetDirectories(path);
        foreach (var dir in directories)
        {
            DoAddTagSuffix(dir);
        }
    }

    void StartCheck()
    {
        Debug.Log("开始查找.");
        DoCheckTagSuffix(root);
        Debug.Log("查找结束.");
    }

    void DoCheckTagSuffix(string path)
    {
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            if (!(file.EndsWith(".png") || file.EndsWith(".jpg")))
            {
                continue;
            }

            TextureImporter ti = AssetImporter.GetAtPath(file) as TextureImporter;
            if (string.IsNullOrEmpty(ti.spritePackingTag) || ti.textureType != TextureImporterType.Sprite)
            {
                continue;
            }

            var oldguid = AssetDatabase.AssetPathToGUID(file);
            if (!ti.spritePackingTag.EndsWith(suffix))
            {
                UnityEngine.Debug.Log("   Check Fail in  " + file + "  with tag:  " + ti.spritePackingTag);
            }
        }

        var directories = Directory.GetDirectories(path);
        foreach (var dir in directories)
        {
            DoCheckTagSuffix(dir);
        }
    }
}
#endregion

#region 修改prefab里的图片引用

public class ChangeGuidData
{
    public string path;
    public string oldGuid;
    public string newGuid;
    public string asset;
}

public class FindAssetInPrefab : EditorWindow
{
    private string root = "Assets/LuaFramework/MyResources_en_/UI";

    private System.Action FindFunc = null;

    List<string> results = new List<string>();
    
    List<ChangeGuidData> changeGuidData = new List<ChangeGuidData>();

    private bool isSpecialReplace = false;
    private string filtertxt = "LuaFramework/MyResources/";
    private string fromtxt = "/MyResources/";
    private string totxt = "/MyResources_en_/";


    [MenuItem("Tools/多语言/5. 在英文prefab中查找所有的中文资源", false, 5)]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(FindAssetInPrefab));
    }

    void OnGUI()
    {
        GUILayout.Label("根路径：");
        root = EditorGUILayout.TextField(root);
        if (GUILayout.Button("列出所有中文资源"))
        {
            StartFind(false);
        }
        if (GUILayout.Button("替换所有中文资源"))
        {
            StartFind(true);
        }
        
        GUILayout.Label("tips: 可以不用列出资源，直接替换。列出资源功能是方便检查。\n");

        isSpecialReplace = GUILayout.Toggle(isSpecialReplace, "专业替换模式");
        if (isSpecialReplace)
        {
            GUILayout.Label("专业模式");
            filtertxt = EditorGUILayout.TextField("替换过滤器：", filtertxt);
            fromtxt = EditorGUILayout.TextField("替换原字符串：", fromtxt);
            totxt = EditorGUILayout.TextField("替换后字符串：", totxt);
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

    void StartFind(bool doReplace)
    {
        // 开始查找
        results.Clear();
        Debug.Log("开始查找.");
        DoFindEnglishAsset();
        if (doReplace)
        {
            DoReplace();
        }
        else
        {
            WriteFile();
        }
        Debug.Log("查找结束.");
    }

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
    
    void DoFindEnglishAsset()
    {
        string filter = (isSpecialReplace && !string.IsNullOrEmpty(filtertxt)) ? filtertxt : "LuaFramework/MyResources/";
        string from = (isSpecialReplace && !string.IsNullOrEmpty(fromtxt)) ? fromtxt : "/MyResources/";
        string to = (isSpecialReplace && !string.IsNullOrEmpty(totxt)) ? totxt : "/MyResources_en_/";
        
        string[] lookFor = { root };
        string[] guids2 = AssetDatabase.FindAssets("t:gameObject", lookFor);
        foreach (string guid in guids2)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            // 温和的方法：
            // foreach (var resourcesName in AssetDatabase.GetDependencies(path))
            // GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof (GameObject)) as GameObject;
            // var images = go.transform.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            // foreach (var image in images)
            // {
            //     var guid_img = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(image.sprite));
            //     var path_img = AssetDatabase.GUIDToAssetPath(guid_img);
            //     if (path_img.Contains("LuaFramework/MyResources/"))
            //     {
            //         string newPath_img = path_img.Replace("MyResources", "MyResources_en_");
            //         var new_guid_img = AssetDatabase.AssetPathToGUID(newPath_img);
            //         ChangeGuidData data = new ChangeGuidData();
            //         data.path = path;
            //         data.oldGuid = guid_img;
            //         data.newGuid = new_guid_img;
            //         data.asset = newPath_img;
            //         changeGuidData.Add(data);
            //         results.Add("  findOne  in  " + data.path + "->" + data.asset + "  :  " + data.oldGuid + " -> " + data.newGuid);
            //         //UnityEngine.Debug.Log("   >>>>>>>  find  one   " + newPath_img);
            //     }
            // }
            
            // 暴力的方法：
            string contents = File.ReadAllText(path);
            Regex reg = new Regex("guid: .*,");
            MatchCollection mc = reg.Matches(contents);
            foreach(Match m in mc)
            {
                var oldGuid = m.Value.Substring(6, 32);
                var oldPath = AssetDatabase.GUIDToAssetPath(oldGuid);
                if (oldPath.Contains(filter))
                {
                    string newPath = oldPath.Replace(from, to);
                    var newGuid = AssetDatabase.AssetPathToGUID(newPath);
                    ChangeGuidData data = new ChangeGuidData();
                    data.path = path;
                    data.oldGuid = oldGuid;
                    data.newGuid = newGuid;
                    data.asset = oldPath;
                    changeGuidData.Add(data);
                    results.Add("  findOne  in  " + data.path + "->" + data.asset + "  :  " + data.oldGuid + " -> " + data.newGuid);
                    //UnityEngine.Debug.Log("   >>>>>>>  find  one   " + newPath);
                }
            }
        }
    }
    
    void WriteFile()
    {
        string res = Newtonsoft.Json.JsonConvert.SerializeObject(changeGuidData, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText("prefab中文资源GUID.json", res);
    }
    
    void DoReplace()
    {
        foreach (ChangeGuidData data in changeGuidData)
        {
            if (string.IsNullOrEmpty(data.path) || string.IsNullOrEmpty(data.oldGuid) || string.IsNullOrEmpty(data.newGuid))
                continue;
            
            string contents = File.ReadAllText(data.path);
            contents = contents.Replace("guid: " + data.oldGuid, "guid: " + data.newGuid);
            File.WriteAllText(data.path, contents);
        }
    }
}

#endregion

#region 手动替换英文资源
public class HandleEnglishResources
{
    [MenuItem("Tools/多语言/6. 手动替换英文资源", false, 6)]
    public static void Do()
    {
        EditorUtility.DisplayDialog("提示", "请手动替换英文路径下的资源为正确的资源，然后在prefab里面调整正确的大小", "OK");
    }
}
#endregion

#region 设置图片的原始尺寸SetNative
#endregion