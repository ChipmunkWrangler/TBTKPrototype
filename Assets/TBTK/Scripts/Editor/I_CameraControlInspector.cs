using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{

	[CustomEditor(typeof(CameraControl))]
	public class CameraControlEditor : TBEditorInspector {

		private static CameraControl instance;
		
		private float width=116;
		
		
		void Awake(){
			instance = (CameraControl)target;
		}
		
		
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			
			Undo.RecordObject(instance, "CameraControl");
			
			EditorGUILayout.Space();
			
			cont=new GUIContent("Pan Speed:", "The speed at which the camera pans on the horizontal axis");
			instance.panSpeed=EditorGUILayout.FloatField(cont, instance.panSpeed);
			
			cont=new GUIContent("Zoom Speed:", "The speed at witch the camera zooms");
			instance.zoomSpeed=EditorGUILayout.FloatField(cont, instance.zoomSpeed);
			
			cont=new GUIContent("Rotate Speed:", "The speed at witch the camera rotate around the pivot");
			instance.rotateSpeed=EditorGUILayout.FloatField(cont, instance.rotateSpeed);
			
			EditorGUILayout.Space();
			
			width=150;
			
			#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("EnableTouchPan:", "Check to enable finger drag on screen to pan the camera");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.enableTouchPan=EditorGUILayout.Toggle(instance.enableTouchPan);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("EnableTouchRotate:", "Check to enable two fingers drag on screen to rotate the camera angle");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.enableTouchRotate=EditorGUILayout.Toggle(instance.enableTouchRotate);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("enableTouchZoom:", "Check to enable two fingers pinching on screen to rotate the camera angle");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.enableTouchZoom=EditorGUILayout.Toggle(instance.enableTouchZoom);
				EditorGUILayout.EndHorizontal();
				
				//EditorGUILayout.BeginHorizontal();
				//	cont=new GUIContent("RotateSensitivity:", "The input sensitivity to the rotate input (two fingers drag)");
				//	EditorGUILayout.LabelField(cont, GUILayout.Width(width));
				//	instance.rotationSpeed=EditorGUILayout.FloatField(instance.rotationSpeed);
				//EditorGUILayout.EndHorizontal();
				
			#else
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("EnableKeyPanning:", "Check to enable camera panning when the mouse cursor is moved to the edge of the screen");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.enableKeyPanning=EditorGUILayout.Toggle(instance.enableKeyPanning);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("EnableMousePanning:", "Check to enable camera panning using 'wasd' key");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.enableMousePanning=EditorGUILayout.Toggle(instance.enableMousePanning);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("EnableMouseRotate:", "Check to enable right-mouse-click drag to rotate the camera angle");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.enableMouseRotate=EditorGUILayout.Toggle(instance.enableMouseRotate);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("EnableMouseZoom:", "Check to enable using mouse wheel to zoom the camera");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.enableMouseZoom=EditorGUILayout.Toggle(instance.enableMouseZoom);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("MousePanningZoneWidth:", "The clearing from the edge of the screen where the mouse panning will start");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.mousePanningZoneWidth=EditorGUILayout.IntField(instance.mousePanningZoneWidth);
				EditorGUILayout.EndHorizontal();
				
			#endif
			
			EditorGUILayout.Space();
			
			width=116;
			
			EditorGUILayout.BeginHorizontal();
				cont=new GUIContent("X-Axis Limit:", "The min/max X-axis position limit of the camera pivot");
				EditorGUILayout.LabelField(cont, GUILayout.Width(width));
				instance.minPosX=EditorGUILayout.FloatField(instance.minPosX);
				instance.maxPosX=EditorGUILayout.FloatField(instance.maxPosX);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				cont=new GUIContent("Z-Axis Limit:", "The min/max Z-axis position limit of the camera pivot");
				EditorGUILayout.LabelField(cont, GUILayout.Width(width));
				instance.minPosZ=EditorGUILayout.FloatField(instance.minPosZ);
				instance.maxPosZ=EditorGUILayout.FloatField(instance.maxPosZ);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				cont=new GUIContent("Zoom Limit:", "The limit of the camera zoom. This is effectively the local Z-axis position limit of the camera transform as a child of the camera pivot");
				EditorGUILayout.LabelField(cont, GUILayout.Width(width));
				instance.minZoomDistance=EditorGUILayout.FloatField(instance.minZoomDistance);
				instance.maxZoomDistance=EditorGUILayout.FloatField(instance.maxZoomDistance);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				cont=new GUIContent("Elevation Limit:", "The limit of the elevation of the camera pivot, effectively the X-axis rotation. Recommend to keep the value between 10 to 89");
				EditorGUILayout.LabelField(cont, GUILayout.Width(width));
				instance.minRotateAngle=EditorGUILayout.FloatField(instance.minRotateAngle);
				instance.maxRotateAngle=EditorGUILayout.FloatField(instance.maxRotateAngle);
			EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.Space();
			
			
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("CenterOnSelected:", "Check to have the camera looking at the selected unit");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					instance.centerOnSelectedUnit=EditorGUILayout.Toggle(instance.centerOnSelectedUnit);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("Smooth Transition:", "Check to have the camera smoothly move to the selected unit. Otherwise a jump cut will be used");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					if(!instance.centerOnSelectedUnit) EditorGUILayout.LabelField("-");
					else instance.smoothLerping=EditorGUILayout.Toggle(instance.smoothLerping);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
					cont=new GUIContent("Lerp Duration:", "The duration of the transition\nNote that this is not the exact duration but rather a reference\nThe actual duration of each transition will be adjusted according to the distance");
					EditorGUILayout.LabelField(cont, GUILayout.Width(width));
					if(!instance.centerOnSelectedUnit || !instance.smoothLerping) EditorGUILayout.LabelField("-");
					else instance.lerpDuration=EditorGUILayout.FloatField(instance.lerpDuration);
				EditorGUILayout.EndHorizontal();
			
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
			
		}

	}
	
}
