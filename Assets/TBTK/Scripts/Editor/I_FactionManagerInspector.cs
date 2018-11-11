using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TBTK;

namespace TBTK{
	
	[CustomEditor(typeof(FactionManager))]
	public class I_FactionManagerInspector : TBEditorInspector {
		
		private static FactionManager instance;
		void Awake(){
			instance = (FactionManager)target;
			LoadDB();
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			EditorGUILayout.Space();
			
			
			if(!Application.isPlaying){
				if(GUILayout.Button("Generate Unit")) instance._GenerateUnit();
			}
			
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			
			EditorGUILayout.HelpBox("Editing of Faction Information via Inspector is not recommended, please use FactionManager-EditorWindow instead", MessageType.Info);
			//GUIStyle style=new GUIStyle();
			//style.wordWrap=true;
			//EditorGUILayout.LabelField("Editing of Faction Information via Inspector is not recommended, please use FactionManager-EditorWindow instead", style);
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.Space();
			if(GUILayout.Button("Open FactionManager-EditorWindow")){
				NewFactionManagerEditorWindow.Init();
			}
			EditorGUILayout.Space();
			
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
	}

}