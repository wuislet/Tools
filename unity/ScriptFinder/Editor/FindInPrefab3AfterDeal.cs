/*************************************************************
   Copyright(C) 2017 by WuIslet
   All rights reserved.
   
   FindInPrefab.cs
   Utils
   
   Created by WuIslet on 2018-02-01.
   
*************************************************************/
#if !USE_ILRUNTIME

using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public partial class FindInPrefab
{
    static readonly int ShaderProperty_MainTex = Shader.PropertyToID("_MainTex");
    private const System.Reflection.BindingFlags BIND_TYPE =
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;

    // 3. 后续处理 AfterDealClass
    private enum AfterDeal
    {
        // 通用方法
        打印详细,
        打印树结构,
        参数查找替换,
        调用指定函数,
        比较尺寸,
        删除节点,
        查找Prefab里的资源依赖,

        UI规范化, //Transform清除小数点，RayCast删除，Navigation关闭，z值设置为0，空的CanvasRender, TODO，替换miss/none资源为白底，替换图集

        // 特殊Component对应方法
        文字射线,
        默认字体,
        检查TMP的边距,
        //查找miss,
        检查UI材质,
        查找未使用图集的Image,
        //分析prefab成分

        //临时
        blur修复,
    }
    private AfterDeal mAfterDeal;
    private readonly Dictionary<AfterDeal, Phase<FindResult, string>> AfterDealClass = new Dictionary<AfterDeal, Phase<FindResult, string>>
    {
        // 通用方法
        {AfterDeal.打印详细, new AfterDealPrintDetail() },
        {AfterDeal.打印树结构, new AfterDealPrintTree() },
        {AfterDeal.参数查找替换, new AfterDealPrintParam() },
        {AfterDeal.调用指定函数, new AfterDealCallFunction() },
        {AfterDeal.比较尺寸, new AfterDealCheckSize() },
        {AfterDeal.删除节点, new AfterDealDeleteGameobject() },
        {AfterDeal.查找Prefab里的资源依赖, new AfterDealGetDependencies() },

        {AfterDeal.UI规范化, new AfterDealFormatPrefab() },

        // 特殊Component对应方法
        {AfterDeal.文字射线, new AfterDealTextRaycast() },
        {AfterDeal.默认字体, new AfterDealDefaultFont() },
        {AfterDeal.检查TMP的边距, new AfterDealTMPMargins() },
        {AfterDeal.检查UI材质, new AfterDealNoneImage() },
        {AfterDeal.查找未使用图集的Image, new AfterDealUnTGAImage() },
        //{AfterDeal.分析prefab成分, new AfterDealAnalyzePrefab() },
        
        //临时
        {AfterDeal.blur修复, new AfterDealBlurPrefab() },
    };

    #region class 打印详细
    class AfterDealPrintDetail : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";
            if (input != null)
            {
                str = input.path;
            }
            return str;
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

    #region class 打印树结构
    class AfterDealPrintTree : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";
            if (input != null)
            {
                var result = BFSPrefab(input, null);
                for (int i = 0, len = result.Count; i < len; ++i)
                {
                    var findResult = result[i];
                    str += findResult.path;
                    str += "\r\n";
                }
            }
            return str;
        }

        public void OnDraw()
        {
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (isSave)
            {
                return "不需要需要启用保存prefab功能";
            }

            if (isDS)
            {
                return "不可以启用深度搜索";
            }

            return "";
        }
    }
    #endregion

    #region class 参数查找替换
    class AfterDealPrintParam : Phase<FindResult, string>
    {
        private enum ArgType
        {
            整数,
            字符串,
        }

        private string component;
        private string propertyName;
        private bool isEqual;
        private bool isReplace;
        private ArgType argType;
        private int intValue;
        private int intNewValue;
        private string strValue;
        private string strNewValue;
        private List<string> propertyNameList = new List<string>();
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var com = input.go.GetComponent(component);
            if (com != null)
            {
                var value = ReflectionHelper.GetValue(com, propertyName);
                if (value != null)
                {
                    if (isEqual)
                    {
                        if (argType == ArgType.整数)
                        {
                            if ((int)value == intValue)
                            {
                                if (isReplace)
                                {
                                    ReflectionHelper.SetValue(com, propertyName, intNewValue);
                                }
                                str = input.path;
                            }
                        }
                        else if (argType == ArgType.字符串)
                        {
                            if ((string)value == strValue)
                            {
                                if (isReplace)
                                {
                                    ReflectionHelper.SetValue(com, propertyName, strNewValue);
                                }
                                str = input.path;
                            }
                        }
                    }
                    else
                    {
                        if (value != null)
                        {
                            str = input.path + " : " + value;
                        }
                    }
                }
                else
                {
                    //报错处理
                }
            }
            else
            {
                //报错处理
            }
            return str;
        }

        public void OnDraw()
        {
            GUILayout.BeginHorizontal();
            component = EditorGUILayout.TextField("    控件名：", component);
            //if (GUILayout.Button("检查参数", GUILayout.MaxWidth(100)))
            //{
            //    propertyNameList.Clear();
            //    if (!string.IsNullOrEmpty(component))
            //    {
            //        var comType = Type.GetType(component);
            //        if (comType != null)
            //        {
            //            var properties = comType.GetProperties(BIND_TYPE);
            //            foreach (var pro in properties)
            //            {
            //                var name = pro.Name;
            //                if (!propertyNameList.Contains(name))
            //                {
            //                    propertyNameList.Add(name);
            //                }
            //            }

            //            var fields = comType.GetFields(BIND_TYPE);
            //            foreach (var field in fields)
            //            {
            //                var name = field.Name;
            //                if (!propertyNameList.Contains(name))
            //                {
            //                    propertyNameList.Add(name);
            //                }
            //            }
            //        }
            //    }
            //}
            GUILayout.EndHorizontal();

            propertyName = EditorGUILayout.TextField("    参数名：", propertyName);
            if (propertyNameList != null && propertyNameList.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                foreach (var key in propertyNameList)
                {
                    if (GUILayout.Button(key, GUILayout.MaxWidth(100)))
                    {
                        propertyName = key;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            isEqual = GUILayout.Toggle(isEqual, "是否等于一个值？");
            if (isEqual)
            {
                argType = (ArgType)EditorGUILayout.EnumPopup("参数类型：", argType, GUILayout.MinWidth(100));
                switch (argType)
                {
                    case ArgType.整数:
                        intValue = EditorGUILayout.IntField(intValue);
                        break;
                    case ArgType.字符串:
                        strValue = EditorGUILayout.TextField(strValue);
                        break;
                }

                isReplace = GUILayout.Toggle(isReplace, "是否替换？");
                if (isReplace)
                {
                    switch (argType)
                    {
                        case ArgType.整数:
                            intNewValue = EditorGUILayout.IntField(intNewValue);
                            break;
                        case ArgType.字符串:
                            strNewValue = EditorGUILayout.TextField(strNewValue);
                            break;
                    }
                }
            }
            else
            {
                isReplace = false;
            }
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (isSave && !isReplace)
            {
                return "不需要启用保存prefab功能";
            }
            if (!isSave && isReplace)
            {
                return "需要启用保存prefab功能";
            }

            return "";
        }
    }
    #endregion

    #region class 调用指定函数
    class AfterDealCallFunction : Phase<FindResult, string>
    {
        private string component;
        private string functionName;
        private List<string> functionNameList = new List<string>(){ "SetNativeSize", "DoDuplicate" };
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var com = input.go.GetComponent(component);
            if (com != null)
            {
                ReflectionHelper.InvokMethod(com, functionName);
            }
            else
            {
                //报错处理
            }
            return str;
        }

        public void OnDraw()
        {
            GUILayout.BeginHorizontal();
            component = EditorGUILayout.TextField("    控件名：", component);
            GUILayout.EndHorizontal();

            functionName = EditorGUILayout.TextField("    函数名：", functionName);
            if (functionNameList != null && functionNameList.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                foreach (var key in functionNameList)
                {
                    if (GUILayout.Button(key, GUILayout.MaxWidth(100)))
                    {
                        functionName = key;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (!isSave)
            {
                return "需要启用保存prefab功能";
            }

            return "";
        }
    }
    #endregion

    #region class 比较尺寸
    class AfterDealCheckSize : Phase<FindResult, string>
    {
        enum CompareWay
        {
            大于,
            等于,
            小于,
        }

        enum CalculateWay
        {
            或,
            且,
            加,
            乘,
        }
        private CompareWay mCompareWay;
        private CalculateWay mCalculateWay;
        private Vector2 mMinSize;
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var rect = input.go.transform as RectTransform;
            if (rect != null)
            {
                var result = true;
                switch (mCalculateWay)
                {
                    case CalculateWay.加:
                        result = Compare(rect.rect.width + rect.rect.height, mMinSize.x + mMinSize.y);
                        break;
                    case CalculateWay.乘:
                        result = Compare(rect.rect.width * rect.rect.height, mMinSize.x * mMinSize.y);
                        break;
                    case CalculateWay.且:
                        result = Compare(rect.rect.width, mMinSize.x) && Compare(rect.rect.height, mMinSize.y);
                        break;
                    case CalculateWay.或:
                        result = Compare(rect.rect.width, mMinSize.x) || Compare(rect.rect.height, mMinSize.y);
                        break;
                }

                if (result)
                {
                    str = string.Format("{0} size:({1}, {2})", input.path, rect.rect.width, rect.rect.height);
                }
            }
            return str;
        }

        private bool Compare(float x1, float x2)
        {
            switch (mCompareWay)
            {
                case CompareWay.大于:
                    return x1 > x2;
                case CompareWay.等于:
                    return Mathf.Abs(x1 - x2) < float.Epsilon;
                case CompareWay.小于:
                    return x1 < x2;
            }

            return false;
        }

        public void OnDraw()
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            mCompareWay = (CompareWay)EditorGUILayout.EnumPopup("比较方法：", mCompareWay, GUILayout.MinWidth(100));
            mCalculateWay = (CalculateWay)EditorGUILayout.EnumPopup("计算方法：", mCalculateWay, GUILayout.MinWidth(100));
            EditorGUILayout.EndHorizontal();
            mMinSize = EditorGUILayout.Vector2Field("比较尺寸", mMinSize);
            GUILayout.EndVertical();
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (!isDS)
            {
                return "需要启用深度查找功能";
            }
            if (isHE == false)
            {
                return "需要启用命中判断";
            }
            return "";
        }
    }
    #endregion

    #region class 删除节点
    class AfterDealDeleteGameobject : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";
            str = input.path;
            GameObject.DestroyImmediate(input.go, true);
            return str;
        }

        public void OnDraw()
        {
            EditorGUILayout.HelpBox("删除节点操作不可逆，请谨慎使用", MessageType.Warning);
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (!isSave)
            {
                return "需要启用保存prefab功能";
            }
            if (!isDS)
            {
                return "需要启用深度查找功能";
            }
            if (isHE == false)
            {
                return "需要启用命中判断";
            }
            return "";
        }
    }
    #endregion

    #region class 查找Prefab里的资源依赖
    class AfterDealGetDependencies : Phase<FindResult, string>
    {
        private string extString = ".tga|.png|.jpg";
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var list = extString.Split('|');
            var path = AssetDatabase.GetAssetPath(input.go);
            foreach (var resourcesName in AssetDatabase.GetDependencies(path))
            {
                for (int i = 0; i < list.Length; i++)
                {
                    var ext = list[i];
                    if (resourcesName.EndsWith(ext))
                    {
                        str += resourcesName;
                        str += "\r\n";
                        break;
                    }
                }
            }
            return str;
        }

        public void OnDraw()
        {
            extString = EditorGUILayout.TextField(extString);
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (isDS)
            {
                return "查找Prefab里的资源依赖暂时不支持深度查找";
            }
            return "";
        }
    }
    #endregion

    #region class UI规范化
    class AfterDealFormatPrefab : Phase<FindResult, string>
    {
        private int roundNum = 0;
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var rect = input.go.GetComponent<RectTransform>();
            if (rect != null)
            {
                var pos = rect.localPosition;
                if (roundNum == 0)
                {
                    rect.localPosition = new Vector3((int)System.Math.Round(pos.x, roundNum), (int)System.Math.Round(pos.y, roundNum), 0);
                }
                else
                {
                    rect.localPosition = new Vector3((float)System.Math.Round(pos.x, roundNum), (float)System.Math.Round(pos.y, roundNum), 0);
                }
                var size = rect.sizeDelta;
                if (roundNum == 0)
                {
                    rect.sizeDelta = new Vector2((int)System.Math.Round(size.x, roundNum), (int)System.Math.Round(size.y, roundNum));
                }
                else
                {
                    rect.sizeDelta = new Vector2((float)System.Math.Round(size.x, roundNum), (float)System.Math.Round(size.y, roundNum));
                }
                var scale = rect.localScale;
                var scaleRound = roundNum < 1 ? 1 : roundNum;
                rect.localScale = new Vector3((float)System.Math.Round(scale.x, scaleRound), (float)System.Math.Round(scale.y, scaleRound), (float)System.Math.Round(scale.z, scaleRound));
            }

            var text = input.go.GetComponent<Text>();
            if (text != null && text.raycastTarget)
            {
                text.raycastTarget = false;
                str = "[text raycast]" + input.path;
            }

            var tmp = input.go.GetComponent<TextMeshProUGUI>();
            if (tmp != null && tmp.raycastTarget)
            {
                tmp.raycastTarget = false;
                str = "[text raycast]" + input.path;
            }

            var btn = input.go.GetComponent<Selectable>();
            if (btn != null)
            {
                var navigation = new Navigation { mode = Navigation.Mode.None };
                if (btn.navigation.mode != Navigation.Mode.None)
                {
                    btn.navigation = navigation;
                    str = "[navigation]" + input.path;
                }
            }

            var render = input.go.GetComponent<CanvasRenderer>();
            if (render != null)
            {
                if (input.go.GetComponents<Graphic>().Length == 0)
                {
                    UnityEngine.Object.DestroyImmediate(render, true);
                    str = "[empty renderer]" + input.path;
                }
            }

            return str;
        }

        public void OnDraw()
        {
            roundNum = EditorGUILayout.IntField("保留位数： ", roundNum);
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (!isSave)
            {
                return "需要启用保存prefab功能";
            }
            if (!isDS)
            {
                return "需要启用深度查找功能";
            }
            return "";
        }
    }
    #endregion
    
    #region class 文字射线
    class AfterDealTextRaycast : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var text = input.go.GetComponent<Text>();
            if (text != null)
            {
                if (text.raycastTarget)
                {
                    text.raycastTarget = false;
                    str = input.path;
                }
            }

            var tmp = input.go.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                if (tmp.raycastTarget)
                {
                    tmp.raycastTarget = false;
                    str = input.path;
                }
            }

            var inputField = input.go.GetComponent<InputField>();
            if (inputField != null)
            {
                inputField.textComponent.raycastTarget = true;
                inputField.targetGraphic.raycastTarget = true;
                str = input.path;
            }

            return str;
        }

        public void OnDraw()
        {
            EditorGUILayout.LabelField("如果查找的是Text或者TMP组件，则删除raycast。如果查找的是InputField组件，则添加回对应text的raycast");
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (!isSave)
            {
                return "需要启用保存prefab功能";
            }
            if (!isDS)
            {
                return "需要启用深度查找功能";
            }
            if (hr != HitRule.含有控件 || isHE == false)
            {
                return "需要启用命中Text组件或TextMeshProUGUI组件的判断";
            }
            return "";
        }
    }
    #endregion

    #region class 默认字体
    class AfterDealDefaultFont : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var com = input.go.GetComponent<Text>();
            if (com != null)
            {
                if (com.font.name == "Arial")
                {
                    str = input.path;
                }
            }
            return str;
        }

        public void OnDraw()
        {
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (!isDS)
            {
                return "需要启用深度查找功能";
            }
            if (hr != HitRule.含有控件 || isHE == false)
            {
                return "需要启用命中Text组件的判断";
            }
            return "";
        }
    }
    #endregion

    #region class 检查TMP的边距
    class AfterDealTMPMargins : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var com = input.go.GetComponent<TextMeshProUGUI>();
            if (com != null)
            {
                if (com.margin.x > float.Epsilon)
                {
                    str = input.path + " Margins: " + com.margin;
                }
                else if (com.margin.y > float.Epsilon)
                {
                    str = input.path + " Margins: " + com.margin;
                }
                else if (com.margin.z > float.Epsilon)
                {
                    str = input.path + " Margins: " + com.margin;
                }
                else if (com.margin.w > float.Epsilon)
                {
                    str = input.path + " Margins: " + com.margin;
                }
            }
            return str;
        }

        public void OnDraw()
        {
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (!isDS)
            {
                return "需要启用深度查找功能";
            }
            if (hr != HitRule.含有控件 || isHE == false)
            {
                return "需要启用命中TMP组件的判断";
            }
            return "";
        }
    }
    #endregion

    #region class 检查UI材质
    class AfterDealNoneImage : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var com = input.go.GetComponent<Graphic>();
            if (com != null)
            {
                if (com.material != null && !com.material.HasProperty(ShaderProperty_MainTex))
                {
                    str = input.path + " , " + com.material.name;
                }
            }

            return str;
        }

        public void OnDraw()
        {
        }

        public string CheckNeed(bool isSave, SearchRange sr, bool isDS, HitRule hr, bool isHE, AfterDeal ad)
        {
            if (!isDS)
            {
                return "需要启用深度查找功能";
            }
            if (isHE == false)
            {
                return "需要启用命中判断";
            }
            return "";
        }
    }
    #endregion

    #region class 查找未使用图集的Image
    class AfterDealUnTGAImage : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var com = input.go.GetComponent<Image>();
            str = AssetDatabase.GetAssetPath(com.sprite);
            if (!str.EndsWith(".tga"))
            {
                return input.path;
            }
            return "";
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

    #region class 分析prefab成分
    class AfterDealAnalyzePrefab : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";

            return str;
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

    //临时
    #region class blur修复
    class AfterDealBlurPrefab : Phase<FindResult, string>
    {
        public string FuncReturn(FindResult input)
        {
            var str = "";
            var com = input.go.GetComponent<Transform>();
            if (com.childCount == 1)
            {
                var childRect = com.GetChild(0) as RectTransform;
                if (childRect != null)
                {
                    childRect.anchorMin = Vector2.zero;
                    childRect.anchorMax = Vector2.one;
                    childRect.sizeDelta = Vector2.zero;
                    return input.path + childRect.sizeDelta;
                }
            }
            return "";
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

    private bool DrawAfterDeal()
    {
        bool isOK = true;
        GUILayout.BeginVertical("Box");

        mAfterDeal = (AfterDeal)EditorGUILayout.EnumPopup("后续处理：", mAfterDeal, GUILayout.MinWidth(100));
        if (AfterDealClass.ContainsKey(mAfterDeal))
        {
            var cls = AfterDealClass[mAfterDeal];
            var warning = cls.CheckNeed(isSavePrefab, mSearchRange, mIsDeepSearch, mHitRule, mIsHitEnable, mAfterDeal);
            if (!string.IsNullOrEmpty(warning))
            {
                isOK = false;
                EditorGUILayout.HelpBox(warning, MessageType.Error);
            }
            cls.OnDraw();
        }

        GUILayout.EndVertical();
        return isOK;
    }
}
#endif