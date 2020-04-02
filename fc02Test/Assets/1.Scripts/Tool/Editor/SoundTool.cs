using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public class SoundTool : EditorWindow
{
	// UI 그리는데 필요한 변수들.
	public int uiWidth1 = 300;
	public int uiWidth2 = 200;
	private int selection = 0;
	private Vector2 SP1 = new Vector2(0, 0);
	private Vector2 SP2 = new Vector2(0, 0);
	//오디오 클립.
	private AudioClip soundSource;
	//사운드 데이터.
	private static SoundData soundData;

	[MenuItem("Tools/Sound Tool")]
	static void Init()
	{
		soundData = ScriptableObject.CreateInstance<SoundData>();
		soundData.LoadData();

		SoundTool window = (SoundTool)EditorWindow.GetWindow<SoundTool>(false, "Sound Tool");
		window.Show();
	}

	void OnGUI()
	{
		if (SoundTool.soundData == null)
		{
			return;
		}

		EditorGUILayout.BeginVertical();
		{   //+ 상단의 Add,Copy,Remove,Resetup 버튼을 그려준다.
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Add", GUILayout.Width(this.uiWidth2)))
				{
					SoundTool.soundData.AddSound("NewSound");
					this.selection = SoundTool.soundData.GetDataCount() - 1;
					this.soundSource = null;
					GUI.FocusControl("ID");
				}
				GUI.SetNextControlName("Copy");
				if (GUILayout.Button("Copy", GUILayout.Width(this.uiWidth2)))
				{
					GUI.FocusControl("Copy");
					SoundTool.soundData.Copy(this.selection);
					this.soundSource = null;
					this.selection = SoundTool.soundData.GetDataCount() - 1;
				}
				if (SoundTool.soundData.GetDataCount() > 1)
				{
					GUI.SetNextControlName("Remove");
					if (GUILayout.Button("Remove", GUILayout.Width(this.uiWidth2)))
					{
						GUI.FocusControl("Remove");
						this.soundSource = null;
						SoundTool.soundData.RemoveData(this.selection);
					}

				}


				if (this.selection > SoundTool.soundData.GetDataCount() - 1)
				{
					this.selection = SoundTool.soundData.GetDataCount() - 1;
				}

			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{//+ 목록 리스트를 보여준다.
				EditorGUILayout.BeginVertical(GUILayout.Width(this.uiWidth1));
				{
					EditorGUILayout.Separator();
					EditorGUILayout.BeginVertical("box");
					{
						this.SP1 = EditorGUILayout.BeginScrollView(this.SP1);
						{
							if (SoundTool.soundData.GetDataCount() > 0)
							{
								var __prev = this.selection;
								this.selection = GUILayout.SelectionGrid(this.selection, SoundTool.soundData.GetNameList(true), 1);
								if (__prev != this.selection)
								{
									this.soundSource = null;
								}
							}
						}
						EditorGUILayout.EndScrollView();
					}
					EditorGUILayout.EndVertical();

				}
				EditorGUILayout.EndVertical();
				//+ 사운드 설정 부분.
				EditorGUILayout.BeginVertical();
				{
					this.SP2 = EditorGUILayout.BeginScrollView(this.SP2);
					{
						if (SoundTool.soundData.GetDataCount() > 0)
						{
							EditorGUILayout.BeginVertical();
							{
								EditorGUILayout.Separator();
								GUI.SetNextControlName("ID");
								EditorGUILayout.LabelField("Position ID", this.selection.ToString(), GUILayout.Width(this.uiWidth1));
								SoundTool.soundData.names[this.selection] = EditorGUILayout.TextField("Name", SoundTool.soundData.names[this.selection], GUILayout.Width(this.uiWidth1 + 1.5f));
								SoundTool.soundData.soundClips[this.selection].playType = (SoundPlayType)EditorGUILayout.EnumPopup("PlayType", SoundTool.soundData.soundClips[this.selection].playType, GUILayout.Width(this.uiWidth1));
								SoundTool.soundData.soundClips[this.selection].maxVolume = EditorGUILayout.FloatField("Max Volume", SoundTool.soundData.soundClips[selection].maxVolume, GUILayout.Width(this.uiWidth1));
								SoundTool.soundData.soundClips[this.selection].isLoop = EditorGUILayout.Toggle("Loop Clip", SoundTool.soundData.soundClips[selection].isLoop, GUILayout.Width(this.uiWidth1));
								EditorGUILayout.Separator();
								if (this.soundSource == null && SoundTool.soundData.soundClips[selection].clipName != string.Empty)
								{
									this.soundSource = Resources.Load(SoundTool.soundData.soundClips[selection].clipPath + SoundTool.soundData.soundClips[selection].clipName) as AudioClip;
								}
								this.soundSource = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", this.soundSource, typeof(AudioClip), false, GUILayout.Width(this.uiWidth1 * 1.5f));
								if (this.soundSource != null)
								{
									SoundTool.soundData.soundClips[selection].clipPath = EditorHelper.GetPath(this.soundSource);
									SoundTool.soundData.soundClips[selection].clipName = this.soundSource.name;
									SoundTool.soundData.soundClips[selection].pitch = EditorGUILayout.Slider("Pitch", SoundTool.soundData.soundClips[selection].pitch, -3.0f, 3.0f, GUILayout.Width(this.uiWidth1 * 1.5f));
									SoundTool.soundData.soundClips[selection].dopplerLevel = EditorGUILayout.Slider("Doppler Level", SoundTool.soundData.soundClips[selection].dopplerLevel, 0.0f, 5.0f, GUILayout.Width(this.uiWidth1 * 1.5f));
									SoundTool.soundData.soundClips[selection].rollOffMode = (AudioRolloffMode)EditorGUILayout.EnumPopup("Volume RollOff", SoundTool.soundData.soundClips[selection].rollOffMode, GUILayout.Width(this.uiWidth1 * 1.5f));
									SoundTool.soundData.soundClips[selection].minDistance = float.Parse(EditorGUILayout.TextField("MinDistance", SoundTool.soundData.soundClips[selection].minDistance.ToString(), GUILayout.Width(this.uiWidth1 * 1.5f)));
									SoundTool.soundData.soundClips[selection].maxDistance = float.Parse(EditorGUILayout.TextField("MaxDistance", SoundTool.soundData.soundClips[selection].maxDistance.ToString(), GUILayout.Width(this.uiWidth1 * 1.5f)));
									SoundTool.soundData.soundClips[selection].spatialBlend = EditorGUILayout.Slider("PanLevel", SoundTool.soundData.soundClips[selection].spatialBlend, 0.0f, 1.0f, GUILayout.Width(this.uiWidth1 * 1.5f));

								}
								else
								{
									SoundTool.soundData.soundClips[selection].clipName = string.Empty;
									SoundTool.soundData.soundClips[selection].clipPath = string.Empty;
								}

								EditorGUILayout.Separator();
								if (GUILayout.Button("Add Loop", GUILayout.Width(this.uiWidth1)))
								{
									SoundTool.soundData.soundClips[selection].AddLoop();
								}
								for (int i = 0; i < SoundTool.soundData.soundClips[selection].checkTime.Length; i++)
								{
									EditorGUILayout.BeginVertical("box");
									{
										GUILayout.Label("Loop step " + i, EditorStyles.boldLabel);
										if (GUILayout.Button("Remove", GUILayout.Width(this.uiWidth2)))
										{
											SoundTool.soundData.soundClips[selection].RemoveLoop(i);
											return;
										}
										SoundTool.soundData.soundClips[selection].checkTime[i] = EditorGUILayout.FloatField("Check Time", SoundTool.soundData.soundClips[selection].checkTime[i], GUILayout.Width(this.uiWidth1));
										SoundTool.soundData.soundClips[selection].setTime[i] = EditorGUILayout.FloatField("Set Time", SoundTool.soundData.soundClips[selection].setTime[i], GUILayout.Width(this.uiWidth1));
									}
									EditorGUILayout.EndVertical();
								}

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
				soundData = ScriptableObject.CreateInstance<SoundData>();
				selection = 0;
				this.soundSource = null;
			}
			GUI.SetNextControlName("Save");
			if (GUILayout.Button("Save Settings"))
			{
				GUI.FocusControl("Save");
				SoundTool.soundData.SaveData();
				CreateEnumStructure();
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			}
		}
		EditorGUILayout.EndHorizontal();


	}


	/// <summary>
	/// 사운드 리스트를 enum structure로 뽑아주는 함수.
	/// </summary>
	public void CreateEnumStructure()
	{

		string enumName = "SoundList";

		StringBuilder builder = new StringBuilder();
		builder.AppendLine();
		for (int i = 0; i < SoundTool.soundData.names.Length; i++)
		{
			if (!SoundTool.soundData.names[i].ToLower().Contains("none"))
			{
				builder.AppendLine("    " + SoundTool.soundData.names[i] + " = " + i.ToString() + ",");
			}
		}

		EditorHelper.CreateEnumStructure(enumName, builder);
	}

}
