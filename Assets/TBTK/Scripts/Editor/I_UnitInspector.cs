using UnityEngine;
using UnityEditor;

using System;
using System.Collections;

using TBTK;

namespace TBTK{
	
	[CustomEditor(typeof(Unit))]
	[CanEditMultipleObjects]
	public class I_UnitInspector : TBEditorInspector {
		
		private static Unit instance;
		void Awake(){
			instance = (Unit)target;
			LoadDB();
		}
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			if(instance==null) Awake();
			
			GUI.changed = false;
			
			EditorGUILayout.Space();
			
			
			PrefabType type=PrefabUtility.GetPrefabType(instance);
			
			if(type==PrefabType.Prefab || type==PrefabType.PrefabInstance){
				
				bool existInDB=false;
				if(type==PrefabType.PrefabInstance) existInDB=TBEditor.ExistInDB((Unit)PrefabUtility.GetCorrespondingObjectFromSource(instance));
				else if(type==PrefabType.Prefab) existInDB=TBEditor.ExistInDB(instance);
				
				if(!existInDB){
					EditorGUILayout.Space();
					
					EditorGUILayout.HelpBox("This prefab hasn't been added to database hence it won't be accessible to the game.", MessageType.Warning);
					GUI.color=new Color(1f, 0.7f, .2f, 1f);
					if(GUILayout.Button("Add Prefab to Database")){
						NewUnitEditorWindow.Init();
						NewUnitEditorWindow.NewItem(instance);
						NewUnitEditorWindow.Init();		//call again to select the instance in editor window
					}
					GUI.color=Color.white;
				}
				else{
					EditorGUILayout.HelpBox("Editing unit using Inspector is not recommended.\nPlease use the editor window instead.", MessageType.Info);
					if(GUILayout.Button("Unit Editor Window")) NewUnitEditorWindow.Init(instance.prefabID);
				}
				
				EditorGUILayout.Space();
			}
			else{
				string text="Unit object won't be available for game deployment, or accessible in TBTK editor until it's made a prefab and added to TBTK database.";
				text+="\n\nYou can still edit the unit using default inspector. However it's not recommended";
				EditorGUILayout.HelpBox(text, MessageType.Warning);
				
				EditorGUILayout.Space();
				if(GUILayout.Button("Unit Editor Window")) NewUnitEditorWindow.Init(instance.prefabID);
			}
			
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
	}

}