using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	[CustomEditor(typeof(PerkManager))]
	public class PerkManagerInspector : TBEditorInspector{

		private static PerkManager instance;
		
		
		void Awake(){
			instance = (PerkManager)target;
			LoadDB();
		}
		
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "PerkManager");
			
			EditorGUILayout.Space();
			
				//EditorGUIUtility.labelWidth=150;
			
				cont=new GUIContent("Enable Save", "Check to enable auto saving of any progress made for future game session");
				instance.enableSave=EditorGUILayout.Toggle(cont, instance.enableSave);
				
				cont=new GUIContent("Enable Load", "Check to load from existing saved progress. This will override any preset purchased setting for this level");
				instance.enableLoad=EditorGUILayout.Toggle(cont, instance.enableLoad);
				
				cont=new GUIContent("Clear Saved Progress", "Remove all the save");
				if(GUILayout.Button(cont)) PerkManager.ClearPerkProgress();
				
			
			EditorGUILayout.LabelField("_____________________________________________________________________________________");
			EditorGUILayout.Space();
			
				EditorGUIUtility.labelWidth=150;
				cont=new GUIContent("Perk Currency:", "The starting value of the currency use to purchase perk. This value will be overwitten if there's any save exist");
				instance.perkCurrency=EditorGUILayout.IntField(cont, instance.perkCurrency);
				
				cont=new GUIContent("Currency Gained On Win:", "Perk currency gained when player won the level");
				instance.currencyGainOnWin=EditorGUILayout.IntField(cont, instance.currencyGainOnWin);
				
				
			EditorGUILayout.LabelField("_____________________________________________________________________________________");
			EditorGUILayout.Space();
			
			
				DrawPerkList();
				
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
			
		}
		
		
		
		private bool showPerkList=true;
		void DrawPerkList(){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
			showPerkList=EditorGUILayout.Foldout(showPerkList, "Show Perk List");
			EditorGUILayout.EndHorizontal();
			if(showPerkList){
				
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("EnableAll") && !Application.isPlaying){
					instance.unavailableIDList=new List<int>();
				}
				if(GUILayout.Button("DisableAll") && !Application.isPlaying){
					instance.purchasedIDList=new List<int>();
					
					instance.unavailableIDList=new List<int>();
					for(int i=0; i<perkDB.perkList.Count; i++) instance.unavailableIDList.Add(perkDB.perkList[i].prefabID);
				}
				EditorGUILayout.EndHorizontal ();
				
				
				for(int i=0; i<perkDB.perkList.Count; i++){
					Perk perk=perkDB.perkList[i];
					
					GUILayout.BeginHorizontal();
						
						GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
						Rect rect=GUILayoutUtility.GetLastRect();
						TBEditor.DrawSprite(rect, perk.icon, perk.desp, false);
						
						GUILayout.BeginVertical();
							EditorGUILayout.Space();
							GUILayout.Label(perk.name, GUILayout.ExpandWidth(false));
					
							GUILayout.BeginHorizontal();
								bool flag=!instance.unavailableIDList.Contains(perk.prefabID) ? true : false;
								//if(Application.isPlaying) flag=!flag;	//switch it around in runtime
								EditorGUILayout.LabelField(new GUIContent(" - enabled: ", "check to enable the perk in this level"), GUILayout.Width(70));
								flag=EditorGUILayout.Toggle(flag);
					
								if(!Application.isPlaying){
									if(flag) instance.unavailableIDList.Remove(perk.prefabID);
									else{
										if(!instance.unavailableIDList.Contains(perk.prefabID)){
											instance.unavailableIDList.Add(perk.prefabID);
											instance.purchasedIDList.Remove(perk.prefabID);
										}
									}
								}
								
								if(!instance.unavailableIDList.Contains(perk.prefabID)){
									flag=instance.purchasedIDList.Contains(perk.prefabID);
									EditorGUILayout.LabelField(new GUIContent("- purchased:", "Check to set the perk as purchased right from the start"), GUILayout.Width(75));
									flag=EditorGUILayout.Toggle(flag);
									if(!flag) instance.purchasedIDList.Remove(perk.prefabID);
									else if(!instance.purchasedIDList.Contains(perk.prefabID)) instance.purchasedIDList.Add(perk.prefabID);
								}
								
							GUILayout.EndHorizontal();
							
						GUILayout.EndVertical();
					
					GUILayout.EndHorizontal();
				}
			
			}
		}
	}
}
