using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection.Emit;
using LuaFramework;

[CustomEditor(typeof(BattleField))]
public class BattleFieldEditor : Editor
{
#if UNITY_EDITOR
    #region 作弊模式
    private bool isFOWActive = true;
    private bool isAIActive = true;
    private bool isInvincible = true;

    private int addMagicPos = 1;
    private int addMagicID = 0;
    private int addCardPos = 1;
    private int addCardID = 0;

    private List<int> unitIds = new List<int>();
    private List<string> unitNames = new List<string>();
    private List<int> skillIds = new List<int>();
    private List<string> skillNames = new List<string>();

    public void OnEnable()
    {
        if (unitIds.Count > 0)
        {
            unitIds.Clear();
            unitNames.Clear();
            skillIds.Clear();
            skillNames.Clear();
        }

        var unitdata = BattleManager.BuildingDataConfig.Instance.GetAllData();
        foreach (var pair in unitdata)
        {
            unitIds.Add(pair.Key);
            unitNames.Add(pair.Value.Name_desc);
        }

        var skilldata = BattleManager.SkillConfig.Instance.GetAllData();
        foreach (var pair in skilldata)
        {
            if (string.IsNullOrEmpty(pair.Value.SkillImg) || pair.Value.SkillImg == "0")
            {
                continue;
            }
            skillIds.Add(pair.Key);
            skillNames.Add(pair.Value.Name_desc);
        }
    }
    #endregion
#endif

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BattleField battleField = target as BattleField;

        if (GUILayout.Button("测试掉线"))
        {
            battleField.DisConnect();
        }

        if (GUILayout.Button("测试重连"))
        {
            battleField.TestReconnect();
        }

        if (GUILayout.Button("DrawPlacement"))
        {
            DrawPlacementArea();
        }

        if (GUILayout.Button("PlaceBuilding"))
        {
            battleField.TestBattleMgr.AddStructureWithID(27, 3, 611110);
        }

        if (GUILayout.Button("ChangePlacement"))
        {
            var gos = GameObject.FindGameObjectsWithTag("placement");
            for (int i = 0; i < gos.Length; ++i)
            {
                var go = gos[i];
                var name = go.name;
                var index = System.Int32.Parse(name);
                if (index >= 27 && index <= 29)
                {
                    {
                        var greenTrans = go.transform.Find("Green/Quad");
                        var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Effect/skill/texture/tx/Materials/Untitled1 (2).mat");
                        if (mat != null)
                        {
                            var meshRenderer = greenTrans.GetComponent<MeshRenderer>();
                            meshRenderer.material = mat;
                            greenTrans.localScale = new Vector3(1.63f, 1.63f, 1.63f);
                        }
                        else
                        {
                            Debug.LogError("load wrong!!!");
                        }
                    }

                    {
                        var redTrans = go.transform.Find("Red/Quad");
                        var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Effect/skill/texture/tx/Materials/Untitled11.mat");
                        if (mat != null)
                        {
                            var meshRenderer = redTrans.GetComponent<MeshRenderer>();
                            meshRenderer.material = mat;
                            redTrans.localScale = new Vector3(1.63f, 1.63f, 1.63f);
                        }
                        else
                        {
                            Debug.LogError("load wrong!!!");
                        }
                    }
                }
            }
        }
#if UNITY_EDITOR
        #region 作弊模式
        var titleStyle = new GUIStyle();
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.richText = true;
        titleStyle.fontSize = 20;
        GUILayout.Label("\n<color=#FF0000>↓↓↓ 作弊指令 ↓↓↓</color>", titleStyle);
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("战斗速度：", GUILayout.MaxWidth(70));
        if (GUILayout.Button("慢放 ◀", GUILayout.MaxWidth(100)))
        {
            Time.timeScale = 0.1f;
        }
        if (GUILayout.Button("正常 ▶", GUILayout.MaxWidth(100)))
        {
            Time.timeScale = 1;
        }
        if (GUILayout.Button("快进 ▶▶", GUILayout.MaxWidth(100)))
        {
            Time.timeScale = 10;
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("属性设置：", GUILayout.MaxWidth(70));
        var fow = GUILayout.Toggle(isFOWActive, "战争迷雾", GUILayout.MaxWidth(100));
        if (isFOWActive != fow)
        {
            FogOfWarManager.Instance.SetActiveFOW(fow);
            isFOWActive = fow;
        }
        var ai = GUILayout.Toggle(isAIActive, "开启AI摆兵 TODO", GUILayout.MaxWidth(100));
        if (isAIActive != ai)
        {
            isAIActive = ai;
        }
        var invincible = GUILayout.Toggle(isInvincible, "开启战神无敌 TODO", GUILayout.MaxWidth(100));
        if (isInvincible != invincible)
        {
            isInvincible = invincible;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (GUILayout.Button("给我加钱 + 1000"))
        {
            CustomHelper.SetPlayerGold(0, 1000);
        }

        if (GUILayout.Button("给我加战斗力 + 1000"))
        {
            battleField.AddPlayerProductivity(0, 1000);
        }

        if (GUILayout.Button("☢☢☢ Big Bang ☢☢☢ TODO"))
        {
        }


        GUILayout.Space(10);

        GUILayout.Label("设置兵种卡：");
        GUILayout.BeginVertical("Box");
        addCardPos = EditorGUILayout.IntSlider("添加手卡的位置：", addCardPos, 1, 10);

        int unitIdx = unitIds.FindIndex(x => x == addCardID);

        GUILayout.BeginHorizontal();
        unitIdx = EditorGUILayout.Popup("选择你要添加的英雄：", unitIdx, unitNames.ToArray());
        addCardID = EditorGUILayout.IntField(addCardID, GUILayout.MaxWidth(50));
        GUILayout.EndHorizontal();

        Texture unitPic = null;
        if (unitIdx >= 0)
        {
            addCardID = unitIds[unitIdx];

            int unitID = addCardID / 10 % 10000;
            var unitcfg = BattleManager.UnitConfig.Instance.GetData(unitID);
            string unitPath = "Assets/LuaFramework/MyResources/Textures/" + unitcfg.IconFolder + "/" + unitcfg.UnitIcon + ".png";
            unitPic = EditorGUIUtility.Load(unitPath) as Texture;
        }
        EditorGUILayout.ObjectField("当前选中的英雄：", unitPic, typeof(Texture), true);

        if (GUILayout.Button("加手卡"))
        {
            if (unitIdx >= 0)
            {
                battleField.TestBattleMgr.SetHandCard(0, addCardPos - 1, addCardID);
            }
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);

        GUILayout.Label("设置法术卡：");
        GUILayout.BeginVertical("Box");
        addMagicPos = EditorGUILayout.IntSlider("添加法术的位置：", addMagicPos, 1, 5);
        
        int skillIdx = skillIds.FindIndex(x => x == addMagicID);

        GUILayout.BeginHorizontal();
        skillIdx = EditorGUILayout.Popup("选择你要添加的法术：", skillIdx, skillNames.ToArray());
        addMagicID = EditorGUILayout.IntField(addMagicID, GUILayout.MaxWidth(50));
        GUILayout.EndHorizontal();

        Texture skillPic = null;
        if (skillIdx >= 0)
        {
            addMagicID = skillIds[skillIdx];

            var skillcfg = BattleManager.SkillConfig.Instance.GetData(addMagicID);
            string skillPath = "Assets/LuaFramework/MyResources/Textures/IconSpell/" + skillcfg.SkillImg + ".png";
            skillPic = EditorGUIUtility.Load(skillPath) as Texture;
        }
        EditorGUILayout.ObjectField("当前选中的法术：", skillPic, typeof(Texture), true);

        if (GUILayout.Button("学法术"))
        {
            if (skillIdx >= 0)
            {
                CustomHelper.GuideBuySkill(0, addMagicID, 3, 3 + addMagicPos);
            }
        }
        GUILayout.EndVertical();

        if (GUILayout.Button("给AI摆兵 TODO"))
        {
        }
        GUILayout.Label("<color=#FF0000>↑↑↑ 作弊指令 ↑↑↑</color>", titleStyle);

        #endregion
#endif
    }

    void DrawPlacementArea()
    {
        {
            int start = (int)BattleManager.GroupDef.Left;
            int end = (int)BattleManager.GroupDef.Right;

            for (int l = 0; l <= 1; ++l)
            {
                var line = GameObject.Find(string.Format("Battle/Line{0}", l));
                if (line != null)
                {
                    for (int i = start; i <= end; ++i)
                    {
                        BattleManager.GroupDef groupDef = (BattleManager.GroupDef)i;

                        var parent = line.transform.Find(groupDef.ToString());
                        if (parent)
                        {
                            int index = 1;
                            var placementArea = new List<BattleManager.Rectangle>();
                            BattleManager.Battle.GetPlacementAreas(groupDef, placementArea);
                            foreach (var rect in placementArea)
                            {
                                createPlacement(parent, rect, index, l);
                                index++;
                            }

                            Scene scene = SceneManager.GetActiveScene();
                            //2v2
                            if (scene.name.Contains("BattleField2"))
                            {
                                var reinforcePlacementArea = new List<BattleManager.Rectangle>();
                                BattleManager.Battle.GetReinforcementAreas(groupDef, reinforcePlacementArea);
                                foreach (var rect in reinforcePlacementArea)
                                {
                                    int mapIndex = (l + 1) % 2;
                                    createPlacement(parent, rect, index, mapIndex);
                                    index++;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void createPlacement(Transform parent, BattleManager.Rectangle rect, int index, int mapIndex)
    {
        var center = rect.Center;
        var realPos = GetGridPosition(center.X, center.Y, mapIndex);
        var placementTrans = parent.Find(index.ToString());
        if (placementTrans == null)
        {
            var placement = new GameObject(index.ToString());
            placement.tag = "placement";
            placement.AddComponent<PlacementInput>();
            var boxCollider = placement.AddComponent<BoxCollider>();
            boxCollider.center = new Vector3(0, 0, 0);
            boxCollider.size = new Vector3(1.28f, 0.2f, 1.28f);
            var assetRed = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MyResources/Effect/Scene/Red.prefab");
            var red = GameObject.Instantiate<GameObject>(assetRed);
            red.transform.SetParent(placementTrans);
            red.transform.localPosition = Vector3.zero;
            red.transform.localRotation = Quaternion.identity;
            red.transform.localScale = Vector3.one;
            red.name = "Red";

            var assetGreen = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MyResources/Effect/Scene/Green.prefab");
            var green = GameObject.Instantiate<GameObject>(assetGreen);
            green.transform.SetParent(placementTrans);
            green.transform.localPosition = Vector3.zero;
            green.transform.localRotation = Quaternion.identity;
            green.transform.localScale = Vector3.one;
            green.name = "Green";

            placementTrans = placement.transform;
        }

        placementTrans.position = new Vector3(realPos.X, realPos.Y, realPos.Z);
    }

    BattleManager.Vector3 GetGridPosition(int x, int y, int mapIndex)
    {
        var gridSize = BattleManager.Battle.mapGridSize;
        var battleData = BattleManager.BattleDataConfig.Instance.GetData(1);
        var mapOffset = battleData.SpaceLength;
        var gridPosOffset = BattleManager.Battle.mapStartPos + new BattleManager.Vector3(gridSize / 2f, 0, gridSize / 2f)
            + new BattleManager.Vector3(0, 0, mapOffset);
        var pos = gridPosOffset + new BattleManager.Vector3(x * gridSize, 0, y * gridSize);

        return pos;
    }
}
