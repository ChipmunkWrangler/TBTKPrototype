using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	[CustomEditor(typeof(AIManager))]
	public class AIManagerInspector : TBEditorInspector{

		private static AIManager instance;
		
		private static string[] AIModeLabel=new string[0];
		private static string[] AIModeTooltip=new string[0];
		
		void Awake(){
			instance = (AIManager)target;
			LoadDB();
			
			InitLabel();
		}
		
		void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_AIMode)).Length;
			AIModeLabel=new string[enumLength];
			AIModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				AIModeLabel[i]=((_AIMode)i).ToString();
				if((_AIMode)i==_AIMode.Passive) AIModeTooltip[i]="the unit wont move unless the there are hostile within the faction's sight (using unit sight value even when Fog-Of-War is not used)";
				else if((_AIMode)i==_AIMode.Trigger) AIModeTooltip[i]="the unit wont move unless it's being triggered, when it spotted any hostile or attacked";
				else if((_AIMode)i==_AIMode.Aggressive) AIModeTooltip[i]="the unit will be on move all the time, looking for potential target";
			}
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "AIManager");
			
			EditorGUILayout.Space();
			
				int aiMode=(int)instance.mode;
				cont=new GUIContent("AI Mode:", "The default AI mode to be used by all faction, if not assigned to other mode");
				contL=new GUIContent[AIModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(AIModeLabel[i], AIModeTooltip[i]);
				aiMode = EditorGUILayout.Popup(cont, aiMode, contL);
				instance.mode=(_AIMode)aiMode;
				
			EditorGUIUtility.labelWidth=150;
				cont=new GUIContent("Move Untriggered Unit:", "Check to enable untriggered unit to move randomly (without actively pursuing any hostile)");
				instance.untriggeredUnitMove=EditorGUILayout.Toggle(cont, instance.untriggeredUnitMove);
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
	}
}
