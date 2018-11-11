using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{

	[CustomEditor(typeof(AbilityManagerFaction))]
	public class AbilityManagerFactionEditor : TBEditorInspector {

		private static AbilityManagerFaction instance;
		
		void Awake(){
			instance = (AbilityManagerFaction)target;
		}
		
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "FacAbilityManager");
			
			EditorGUILayout.Space();
			
			GUIContent cont=new GUIContent("StartWithFullEnergy:", "Check to have the faction(s) starts with full energy");
			instance.startWithFullEnergy=EditorGUILayout.Toggle(cont, instance.startWithFullEnergy);
			
			//EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
			
		}
		
	}
	
}