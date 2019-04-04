/*************************************************************
   Copyright(C) 2017 by DH
   All rights reserved.
   
   SuperFinder.cs
   解神者
   
   Created by WuIslet on 2019-03-13.
   
*************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 
/// </summary>
public class SuperFinder : EditorWindow
{
    private const string ROOT_PATH = "Assets/Resources/UI";

    [MenuItem("Tools/超级prefab查找")] //TODO 分歧 目前只做prefab查找器，文件查找器待整合， #分歧
    static void Init()
    {
        EditorWindow.GetWindow(typeof(SuperFinder));
    }

    private static string resourcePathInput;
    #region 比对对象  CompareTypeFuns
    private bool mIsCompareEnable = true;
    private enum CompareType
    {
        选中物体,
        指定路径,
    }
    private CompareType mCompareType;
    private Dictionary<CompareType, Func<Object>> CompareTypeFuns = new Dictionary<CompareType, Func<Object>>
    {
        {CompareType.选中物体, CompareFromSelect },
        {CompareType.指定路径, CompareFromPathObject },
    };

    static Object CompareFromSelect()
    {
        return Selection.activeObject;
    }

    static Object CompareFromPathObject()
    {
        return AssetDatabase.LoadAssetAtPath(resourcePathInput, typeof(Object));
    }
    #endregion
    #region 对象解释  CompareWayFuns
    private enum CompareWay
    {
        GUID,
        名字,
        路径,
    }
    private CompareWay mCompareWay;
    private Dictionary<CompareWay, Func<Object, string>> CompareWayFuns = new Dictionary<CompareWay, Func<Object, string>>
    {
        {CompareWay.GUID, CompareGetGuid },
        {CompareWay.名字, CompareGetName },
        {CompareWay.路径, CompareGetPath },
    };

    static string CompareGetGuid(Object resource)
    {
        string selectedAssetPath = AssetDatabase.GetAssetPath(resource);
        return AssetDatabase.AssetPathToGUID(selectedAssetPath);
    }

    static string CompareGetName(Object resource)
    {
        return resource.name;
    }

    static string CompareGetPath(Object resource)
    {
        return AssetDatabase.GetAssetPath(resource);
    }
    #endregion

    private enum OverAllType //TODO 分歧
    {
        预制体查找器,
        文件查找器,
    }
    private static OverAllType mOverAllType; //TODO 分歧

    #region 查找范围
    private enum SearchRange
    {
        指定路径,
        全文件夹,
        树查找,
    }
    private SearchRange mSearchRange;
    private Dictionary<SearchRange, Func<Object>> SearchRangeFuns = new Dictionary<SearchRange, Func<Object>>//TODO next 输出类型确定
    {
        {SearchRange.指定路径, SearchInPath },
        {SearchRange.全文件夹, SearchInAll },
        {SearchRange.树查找, SearchInTree },
    };

    static Object SearchInPath()
    {
        return null; //TODO next 加具体作用
    }

    static Object SearchInAll()
    {
        return null; //TODO next 加具体作用
    }

    static Object SearchInTree()
    {
        return null; //TODO next 加具体作用
    }
    #endregion

    void OnGUI()
    {
        //mOverAllType = (OverAllType)EditorGUILayout.EnumPopup("选择你要用的查找器：", mOverAllType);  //TODO 分歧

        GUILayout.BeginVertical("Box");
        #region 1.比对对象： 1.当前选中; 2.指定路径;  1.1.比对对象数据： 1.guid; 2.名字;
        mIsCompareEnable = GUILayout.Toggle(mIsCompareEnable, "是否要跟一个标准文件进行比对？");
        if (mIsCompareEnable)
        {
            mCompareType = (CompareType)EditorGUILayout.EnumPopup("获取方法：", mCompareType, GUILayout.MinWidth(100));
            switch (mCompareType)
            {
                case CompareType.选中物体:
                    break;
                case CompareType.指定路径:
                    resourcePathInput = EditorGUILayout.TextField("    指定路径：", resourcePathInput);
                    break;
            }
            //var tmp = CompareTypeFuns[mCompareType]();

            mCompareWay = (CompareWay)EditorGUILayout.EnumPopup("对象解释：", mCompareWay, GUILayout.MinWidth(100));
            //var tmp2 = CompareWayFuns[mCompareWay](tmp);
        }
        #endregion
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");
        #region 2.查找范围： 1.指定路径; 2.全文件夹查找; 3.对单个prefab进行树查找; //TODO next 确定prefab是怎么获取，选中，路径还是托引用？
        mSearchRange = (SearchRange)EditorGUILayout.EnumPopup("查找范围：", mSearchRange, GUILayout.MinWidth(100));
        switch (mSearchRange) //TODO next 加具体作用
        {
        }
        //var tmp = CompareTypeFuns[mCompareType]();

        #endregion
        GUILayout.EndVertical();

        //2. 查找范围， 1. 默认路径-指定路径 2. 全文件夹查找 3. 对单个prefab进行树查找
        //3. 过滤规则， 1. 系统文件-后缀名过滤  2. prefab-含有引用 3. 不过滤
        //4. 找到判断， 1. 系统文件-含有文字 2. prefab树结构-含有控件 3. 系统文件/prefab-名字含有 4. 无条件全命中
        //5. 找到后操作， 1. 获取指定控件  2. 替换文字 3. 打印 4. 找到对应的prefabGuid进行替换。
    }
}
