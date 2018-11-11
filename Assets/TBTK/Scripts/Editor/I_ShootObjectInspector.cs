using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	[CustomEditor(typeof(ShootObject))]
	[CanEditMultipleObjects]
	public class ShootObjectInspector : TBEditorInspector{

		private static ShootObject instance;
		
		private static string[] typeLabel=new string[0];
		private static string[] typeTooltip=new string[0];
		
		void Awake(){
			instance = (ShootObject)target;
			LoadDB();
			
			InitLabel();
		}
		
		void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_ShootObjectType)).Length;
			typeLabel=new string[enumLength];
			typeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				typeLabel[i]=((_ShootObjectType)i).ToString();
				if((_ShootObjectType)i==_ShootObjectType.Projectile) 
					typeTooltip[i]="A typical projectile, travels from turret shoot-point towards target in a 2D trajectory (no rotation in y-axis)";
				if((_ShootObjectType)i==_ShootObjectType.Missile) 
					typeTooltip[i]="Similar to projectile, however the trajectory are randomized and swerve around in multiple direction";
				if((_ShootObjectType)i==_ShootObjectType.Beam) 
					typeTooltip[i]="Used to render laser or any beam like effect. The shootObject doest move instead it requires a LineRenderer component to render the beam effect";
				if((_ShootObjectType)i==_ShootObjectType.Effect) 
					typeTooltip[i]="A shootObject primarily use to show various firing effect. There's no trajectory involved, the target is hit immediately. An Effect shootObject will remain at shootPoint so it can act as a shoot effect";
				
			}
		}
		
		
		private static bool showLineRendererList=false;
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			//Undo.RecordObject(instance, "ShootObject");
			
			serializedObject.Update();
			
			EditorGUILayout.Space();
			
				srlPpt=serializedObject.FindProperty("type");
				EditorGUI.showMixedValue=srlPpt.hasMultipleDifferentValues;
				
				EditorGUI.BeginChangeCheck();
				
				cont=new GUIContent("Type:", "Type of the shoot object");
				contL=new GUIContent[typeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(typeLabel[i], typeTooltip[i]);
				int type = EditorGUILayout.Popup(cont, srlPpt.enumValueIndex, contL);
				
				if(EditorGUI.EndChangeCheck()) srlPpt.enumValueIndex=type;
				EditorGUI.showMixedValue=false;
			
			
			EditorGUILayout.Space();
			
				if(srlPpt.hasMultipleDifferentValues){
					EditorGUILayout.HelpBox("Editing of type specify attribute is unavailble when selecting multiple shoot object of different type", MessageType.Warning);
				}
				else if(!srlPpt.hasMultipleDifferentValues){
					
					if(instance.type==_ShootObjectType.Projectile || instance.type==_ShootObjectType.Missile){
						cont=new GUIContent("Speed:", "The travel speed of the shootObject");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"), cont);
					}
					
					if(instance.type==_ShootObjectType.Projectile){
						cont=new GUIContent("Max Shoot Elevation:", "The maximum elevation at which the shootObject will be fired. The firing elevation depends on the target distance. The further the target, the higher the elevation. ");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("maxShootAngle"), cont);
						
						cont=new GUIContent("Max Shoot Range:", "The maximum range of the shootObject. This is used to govern the elevation, not the actual range limit. When a target exceed this distance, the shootObject will be fired at the maximum elevation");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("maxShootRange"), cont);
					}
					else if(instance.type==_ShootObjectType.Missile){
						
					}
					else if(instance.type==_ShootObjectType.Beam){
						cont=new GUIContent("Beam Duration:", "The active duration of the beam");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("beamDuration"), cont);
						
						cont=new GUIContent("AutoSearchForLineRenderer:", "Check to let the script automatically search for all the LineRenderer component on the prefab instead of assign it manually");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("autoSearchLineRenderer"), cont);
						
						if(!instance.autoSearchLineRenderer){
							if(serializedObject.isEditingMultipleObjects){
								EditorGUILayout.HelpBox("Assignment of line renderer component is not supported for multi-instance editing", MessageType.Info);
							}
							else{
								cont=new GUIContent("LineRenderers", "The LineRenderer component in this prefab\nOnly applicable when AutoSearchForLineRenderer is unchecked");
								
								EditorGUILayout.BeginHorizontal();
								EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
								showLineRendererList = EditorGUILayout.Foldout(showLineRendererList, cont);
								EditorGUILayout.EndHorizontal();
								
								if(showLineRendererList){
									cont=new GUIContent("LineRenderers:", "The LineRenderer component on the prefab to be controlled by the script");
									float listSize=instance.lineList.Count;
									listSize=EditorGUILayout.FloatField("    Size:", listSize);
									
									if(listSize!=instance.lineList.Count){
										while(instance.lineList.Count<listSize) instance.lineList.Add(null);
										while(instance.lineList.Count>listSize) instance.lineList.RemoveAt(instance.lineList.Count-1);
									}
									
									for(int i=0; i<instance.lineList.Count; i++){
										instance.lineList[i]=(LineRenderer)EditorGUILayout.ObjectField("    Element "+i, instance.lineList[i], typeof(LineRenderer), true);
									}
								}
							}
						}
						
					}
					else if(instance.type==_ShootObjectType.Effect){
						cont=new GUIContent("Effect Duration:", "The active duration of the effect");
						EditorGUILayout.PropertyField(serializedObject.FindProperty("effectHitDelay"), cont);
					}
					
				}
			
			EditorGUILayout.Space();
				
				srlPpt=serializedObject.FindProperty("shootEffect");
				cont=new GUIContent("Shoot Effect Object:", "The game object to spawned as the visual effect at the shoot point when the shoot object is fired");
				EditorGUILayout.PropertyField(srlPpt, cont);
				
				cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
				if(srlPpt.objectReferenceValue!=null)
					EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyShootEffect"), cont);
				else EditorGUILayout.LabelField(cont, new GUIContent("-"));
				
				cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
				if(srlPpt.objectReferenceValue!=null && serializedObject.FindProperty("destroyShootEffect").boolValue)
					EditorGUILayout.PropertyField(serializedObject.FindProperty("shootEffectDuration"), cont);
				else EditorGUILayout.LabelField(cont, new GUIContent("-"));
			
			EditorGUILayout.Space();
				
				srlPpt=serializedObject.FindProperty("hitEffect");
				cont=new GUIContent("Hit Effect:", "The gameObject (as visual effect) to be spawn at hit point when the shootObject hit it's target");
				EditorGUILayout.PropertyField(srlPpt, cont);
				
				cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
				if(srlPpt.objectReferenceValue!=null)
					EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyHitEffect"), cont);
				else EditorGUILayout.LabelField(cont, new GUIContent("-"));
				
				cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
				if(srlPpt.objectReferenceValue!=null && serializedObject.FindProperty("destroyHitEffect").boolValue)
					EditorGUILayout.PropertyField(serializedObject.FindProperty("hitEffectDuration"), cont);
				else EditorGUILayout.LabelField(cont, new GUIContent("-"));
			
			DefaultInspector();
			
			serializedObject.ApplyModifiedProperties();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
		}
		
	}
	
}
