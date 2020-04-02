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
    private GameObject beShotSource = null;
    // effect data
    private static EffectData effectData;
    

    [MenuItem("Tools/Effect Tool")]
    static void Init()
    {
        effectData = ScriptableObject.CreateInstance<EffectData>();
        effectData.LoadData();

        EffectTool window = (EffectTool)EditorWindow.GetWindow<EffectTool>(false, "Effect Tool");
        window.Show();
    }

    private void OnGUI()
    {
        if (EffectTool.effectData == null)
        {
            return;
        }

		this.EffectClip_ShowTab();
        
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
                    EffectTool.effectData.AddEffect("NewEffect");
                    this.selection = EffectTool.effectData.GetDataCount() - 1;
                    this.effectSource = null;
                    GUI.FocusControl("ID");
                }
                GUI.SetNextControlName("Copy");
                if (GUILayout.Button("Copy", GUILayout.Width(this.uiWidth200)))
                {
                    GUI.FocusControl("Copy");
                    EffectTool.effectData.Copy(this.selection);
                    this.effectSource = null;
                    this.selection = EffectTool.effectData.GetDataCount() - 1;
                }
                if (EffectTool.effectData.GetDataCount() > 1)
                {
                    GUI.SetNextControlName("Remove");
                    if (GUILayout.Button("Remove", GUILayout.Width(this.uiWidth200)))
                    {
                        GUI.FocusControl("Remove");
                        this.effectSource = null;
                        EffectTool.effectData.RemoveData(this.selection);
                    }

                }

                if (this.selection > EffectTool.effectData.GetDataCount() - 1)
                {
                    this.selection = EffectTool.effectData.GetDataCount() - 1;
                }

            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                //+ 목록 리스트를 보여준다.
                EditorGUILayout.BeginVertical(GUILayout.Width(this.uiWidth300));
                {
                    EditorGUILayout.Separator();
                    EditorGUILayout.BeginVertical("box");
                    {
                        this.SP1 = EditorGUILayout.BeginScrollView(this.SP1);
                        {
                            if (EffectTool.effectData.GetDataCount() > 0)
                            {
                                var __prev = this.selection;
                                this.selection = GUILayout.SelectionGrid(this.selection, EffectTool.effectData.GetNameList(true), 1);
                                if (__prev != this.selection)
                                {
                                    this.effectSource = null;
                                    this.beShotSource = null;
                                    
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
                    this.SP2 = EditorGUILayout.BeginScrollView(this.SP2);
                    {
                        if (EffectTool.effectData.GetDataCount() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();
                                GUI.SetNextControlName("ID");
                                EditorGUILayout.LabelField("Position ID", this.selection.ToString(), GUILayout.Width(this.uiWidth300));
                                EffectTool.effectData.names[this.selection] = EditorGUILayout.TextField("이름.", EffectTool.effectData.names[this.selection], GUILayout.Width(this.uiWidth300 * 1.5f));
								EffectTool.effectData.effectClips[this.selection].effectType = (EffectType)EditorGUILayout.EnumPopup("이펙트타입.", EffectTool.effectData.effectClips[this.selection].effectType, GUILayout.Width(this.uiWidth300));
                                //special property
								EffectType eType = EffectTool.effectData.effectClips[this.selection].effectType;
                                EditorGUILayout.Separator();
								if (this.effectSource == null && EffectTool.effectData.effectClips[selection].effectName != string.Empty)
                                {
                                    EffectTool.effectData.effectClips[selection].PreLoad();
									this.effectSource = Resources.Load(EffectTool.effectData.effectClips[selection].effectPath + EffectTool.effectData.effectClips[selection].effectName) as GameObject;
                                }
                                this.effectSource = (GameObject)EditorGUILayout.ObjectField("이펙트", this.effectSource, typeof(GameObject), false, GUILayout.Width(this.uiWidth300 * 1.5f));
                                if (this.effectSource != null)
                                {
									EffectTool.effectData.effectClips[selection].effectPath = EditorHelper.GetPath(this.effectSource);
									EffectTool.effectData.effectClips[selection].effectName = this.effectSource.name;
                                    //EffectTool.effectData.effectClips[selection].effect_sound = (SoundList)EditorGUILayout.EnumPopup("사운드.", EffectTool.effectData.effectClips[selection].effect_sound, GUILayout.Width(this.uiWidth300 * 1.5f));
                                    
									if (eType != EffectType.NORMAL)
                                    {
                                                                      
										if (this.beShotSource == null && EffectTool.effectData.effectClips[selection].beHitEffect_Path != string.Empty)
                                        {
											this.beShotSource = EffectTool.effectData.effectClips[selection].beHitEffect_Prefab;//Resources.Load(EffectTool.effectData.effectClips[selection].beShot_effect) as GameObject;
                                        }
                                        this.beShotSource = (GameObject)EditorGUILayout.ObjectField("피격이펙트.", this.beShotSource, typeof(GameObject), false, GUILayout.Width(this.uiWidth300 * 1.5f));
                                        if (beShotSource != null)
                                        {
											EffectTool.effectData.effectClips[selection].beHitEffect_Path = EditorHelper.GetPath(this.beShotSource) + this.beShotSource.name;
											if (this.beShotSource != EffectTool.effectData.effectClips[selection].beHitEffect_Prefab)
                                            {
                                                EffectTool.effectData.effectClips[selection].PreLoad();
                                            }
                                            //EffectTool.effectData.effectClips[selection].beshot_sound = (SoundList)EditorGUILayout.EnumPopup("피격사운드.", EffectTool.effectData.effectClips[selection].beshot_sound, GUILayout.Width(this.uiWidth300));

                                        }
                                        else
                                        {
											EffectTool.effectData.effectClips[selection].beHitEffect_Path = string.Empty;
											EffectTool.effectData.effectClips[selection].beHitEffect_Prefab = null;
                                        }
                                    }


                                    
                                }
                                else
                                {
									EffectTool.effectData.effectClips[selection].effectName = string.Empty;
									EffectTool.effectData.effectClips[selection].effectPath = string.Empty;
                                    //EffectTool.effectData.effectClips[selection].effect_sound = SoundList.None;
                                    //EffectTool.effectData.effectClips[selection].beshot_sound = SoundList.None;
                                    this.effectSource = null;
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
                this.beShotSource = null;
                
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
		for (int i = 0; i < EffectTool.effectData.names.Length; i++)
        {
			if (!EffectTool.effectData.names[i].ToLower().Contains("none"))
            {
				builder.AppendLine("    " + EffectTool.effectData.names[i] + " = " + i.ToString() + ",");
            }
        }

		EditorHelper.CreateEnumStructure(enumName, builder);
    }




}
