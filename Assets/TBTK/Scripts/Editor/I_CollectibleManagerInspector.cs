using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	[CustomEditor(typeof(CollectibleManager))]
	public class CollectibleManagerInspector : TBEditorInspector{

		private static CollectibleManager instance;
		
		
		void Awake(){
			instance = (CollectibleManager)target;
			LoadDB();
		}
		
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "CollectibleManager");
			
			EditorGUILayout.Space();
			
				if(!Application.isPlaying){
					if(GUILayout.Button("Generate Item")) instance._GenerateCollectible();
				}
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Item Limit:", "The maximum amount of collectible available on the grid at any given time");
				instance.activeItemLimit=EditorGUILayout.IntField(cont, instance.activeItemLimit);
				
				EditorGUILayout.Space();
				
				cont=new GUIContent("Generate in Game", "Check to enable spawning of collectible item during runtime at the end of each turn");
				instance.generateInGame=EditorGUILayout.Toggle(cont, instance.generateInGame);
				
				cont=new GUIContent("Spawn Per Turn:", "The maximum amount of collectible to be spawned at each turn");
				if(instance.generateInGame) instance.spawnPerTurn=EditorGUILayout.IntField(cont, instance.spawnPerTurn);
				else EditorGUILayout.LabelField(cont, new GUIContent("-"));
				
				cont=new GUIContent("Spawn Chance:", "The success rate of a collectible to be spawned at each spawning attempt during runtime");
				if(instance.generateInGame) instance.spawnChance=EditorGUILayout.FloatField(cont, instance.spawnChance);
				else EditorGUILayout.LabelField(cont, new GUIContent("-"));
				
			
			EditorGUILayout.LabelField("_____________________________________________________________________________________");
			EditorGUILayout.Space();
			
			
				cont=new GUIContent("Spawn Effect", "The effect object to be spawned when a nwe collectible item is spawned during runtime");
				if(!instance.generateInGame) EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
				else instance.spawnEffect=(GameObject)EditorGUILayout.ObjectField(cont, instance.spawnEffect, typeof(GameObject), false);
				
				cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
				if(!instance.generateInGame || instance.spawnEffect==null) EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
				else instance.autoDestroySpawnEffect=EditorGUILayout.Toggle(cont, instance.autoDestroySpawnEffect);
				
				cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
				if(instance.generateInGame && instance.spawnEffect!=null && instance.autoDestroySpawnEffect) 
					instance.spawnEffectDuration=EditorGUILayout.FloatField(cont, instance.spawnEffectDuration);
				else EditorGUILayout.LabelField(cont, new GUIContent("-", ""));
			
				
			EditorGUILayout.LabelField("_____________________________________________________________________________________");
			EditorGUILayout.Space();
			
				DrawItemList();
				
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
			
		}
		
		
		
		//private bool showItemList=true;
		void DrawItemList(){
			//~ EditorGUILayout.BeginHorizontal();
			//~ EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
			//~ showPerkList=EditorGUILayout.Foldout(showItemList, "Show Collectible List");
			//~ EditorGUILayout.EndHorizontal();
			//~ if(showItemList){
				
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("EnableAll") && !Application.isPlaying){
					instance.unavailableIDList=new List<int>();
				}
				if(GUILayout.Button("DisableAll") && !Application.isPlaying){
					//instance.purchasedIDList=new List<int>();
					
					instance.unavailableIDList=new List<int>();
					for(int i=0; i<collectibleDB.collectibleList.Count; i++) instance.unavailableIDList.Add(collectibleDB.collectibleList[i].prefabID);
				}
				EditorGUILayout.EndHorizontal ();
				
				
				for(int i=0; i<collectibleDB.collectibleList.Count; i++){
					Collectible item=collectibleDB.collectibleList[i];
					
					GUILayout.BeginHorizontal();
						
						GUILayout.Box("", GUILayout.Width(40),  GUILayout.Height(40));
						Rect rect=GUILayoutUtility.GetLastRect();
						TBEditor.DrawSprite(rect, item.icon, item.desp, false);
						
						GUILayout.BeginVertical();
							EditorGUILayout.Space();
							GUILayout.Label(item.name, GUILayout.ExpandWidth(false));
					
							GUILayout.BeginHorizontal();
								bool flag=!instance.unavailableIDList.Contains(item.prefabID) ? true : false;
								//if(Application.isPlaying) flag=!flag;	//switch it around in runtime
								EditorGUILayout.LabelField(new GUIContent(" - enabled: ", "check to enable the item in this level"), GUILayout.Width(70));
								flag=EditorGUILayout.Toggle(flag);
					
								if(!Application.isPlaying){
									if(flag) instance.unavailableIDList.Remove(item.prefabID);
									else{
										if(!instance.unavailableIDList.Contains(item.prefabID)){
											instance.unavailableIDList.Add(item.prefabID);
										}
									}
								}
								
							GUILayout.EndHorizontal();
							
						GUILayout.EndVertical();
					
					GUILayout.EndHorizontal();
				}
			
			//}
		}
	}
}
