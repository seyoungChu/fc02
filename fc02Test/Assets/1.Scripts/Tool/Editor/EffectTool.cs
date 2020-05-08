using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Text;

public class EffectTool : EditorWindow
{
    // UI 그리는데 필요한 변수들.
    public int uiWidth300 = 300;
    public int uiWidth200 = 200;
    private int selection = 0;
    private Vector2 SP1 = new Vector2(0, 0);
    private Vector2 SP2 = new Vector2(0, 0);


    // 이펙트 클립.
    private GameObject effectSource = null;

    // effect data
    private static EffectData effectData;


    [MenuItem("Tools/Effect Tool")]
    static void Init()
    {
        effectData = ScriptableObject.CreateInstance<EffectData>();
        effectData.LoadData();

        EffectTool window = GetWindow<EffectTool>(false, "Effect Tool");
        window.Show();
    }

    private void OnGUI()
    {
        if (effectData == null)
        {
            return;
        }

        EffectClip_ShowTab();
    }

    public void EffectClip_ShowTab()
    {
        EditorGUILayout.BeginVertical();
        {
            //+ 상단의 Add, Copy, Remove
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add", GUILayout.Width(this.uiWidth200)))
                {
                    effectData.AddEffect("NewEffect");
                    selection = effectData.GetDataCount() - 1;
                    effectSource = null;
                    GUI.FocusControl("ID");
                }

                GUI.SetNextControlName("Copy");
                if (GUILayout.Button("Copy", GUILayout.Width(this.uiWidth200)))
                {
                    GUI.FocusControl("Copy");
                    effectData.Copy(this.selection);
                    effectSource = null;
                    selection = effectData.GetDataCount() - 1;
                }

                if (effectData.GetDataCount() > 1)
                {
                    GUI.SetNextControlName("Remove");
                    if (GUILayout.Button("Remove", GUILayout.Width(uiWidth200)))
                    {
                        GUI.FocusControl("Remove");
                        effectSource = null;
                        effectData.RemoveData(this.selection);
                    }
                }

                if (selection > effectData.GetDataCount() - 1)
                {
                    selection = effectData.GetDataCount() - 1;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                //+ 목록 리스트를 보여준다.
                EditorGUILayout.BeginVertical(GUILayout.Width(uiWidth300));
                {
                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginVertical("box");
                    {
                        SP1 = EditorGUILayout.BeginScrollView(this.SP1);
                        {
                            if (effectData.GetDataCount() > 0)
                            {
                                int __prev = selection;
                                selection = GUILayout.SelectionGrid(this.selection,
                                    effectData.GetNameList(true), 1);
                                if (__prev != selection)
                                {
                                    effectSource = null;
                                }
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();

                //+ 설정 부분.
                EditorGUILayout.BeginVertical();
                {
                    SP2 = EditorGUILayout.BeginScrollView(this.SP2);
                    {
                        if (effectData.GetDataCount() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();
                                GUI.SetNextControlName("ID");
                                EditorGUILayout.LabelField("Position ID", selection.ToString(),
                                    GUILayout.Width(uiWidth300));
                                effectData.names[selection] = EditorGUILayout.TextField("이름.",
                                    effectData.names[this.selection],
                                    GUILayout.Width(this.uiWidth300 * 1.5f));
                                effectData.effectClips[selection].effectType =
                                    (EffectType) EditorGUILayout.EnumPopup("이펙트타입.",
                                        effectData.effectClips[selection].effectType,
                                        GUILayout.Width(uiWidth300));
                                //special property
                                EffectType eType = effectData.effectClips[selection].effectType;
                                EditorGUILayout.Separator();
                                if (effectSource == null &&
                                    effectData.effectClips[selection].effectName != string.Empty)
                                {
                                    effectData.effectClips[selection].PreLoad();
                                    effectSource =
                                        Resources.Load(effectData.effectClips[selection].effectPath +
                                                       effectData.effectClips[selection]
                                                           .effectName) as GameObject;
                                }

                                effectSource = (GameObject) EditorGUILayout.ObjectField("이펙트", this.effectSource,
                                    typeof(GameObject), false, GUILayout.Width(this.uiWidth300 * 1.5f));
                                if (effectSource != null)
                                {
                                    effectData.effectClips[selection].effectPath =
                                        EditorHelper.GetPath(this.effectSource);
                                    effectData.effectClips[selection].effectName = effectSource.name;
                                }
                                else
                                {
                                    effectData.effectClips[selection].effectName = string.Empty;
                                    effectData.effectClips[selection].effectPath = string.Empty;
                                    effectSource = null;
                                }

                                EditorGUILayout.Separator();
                            }
                            EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.Separator();
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
        //+ 하단의 Reload, Save 부분.
        EditorGUILayout.BeginHorizontal();
        {
            GUI.SetNextControlName("Reload");
            if (GUILayout.Button("Reload Settings"))
            {
                GUI.FocusControl("Reload");
                effectData = ScriptableObject.CreateInstance<EffectData>();
                selection = 0;
                this.effectSource = null;
                //this.beShotSource = null;
            }

            GUI.SetNextControlName("Save");
            if (GUILayout.Button("Save Settings"))
            {
                GUI.FocusControl("Save");
                EffectTool.effectData.SaveData();
                CreateEnumStructure();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 이펙트 리스트를 enum structure로 뽑아주는 함수.
    /// </summary>
    public void CreateEnumStructure()
    {
        string enumName = "EffectList";

        StringBuilder builder = new StringBuilder();
        builder.AppendLine();
        for (int i = 0; i < effectData.names.Length; i++)
        {
            if (effectData.names[i] != string.Empty)
            {
                builder.AppendLine("    " + effectData.names[i] + " = " + i + ",");
            }
        }

        EditorHelper.CreateEnumStructure(enumName, builder);
    }
}