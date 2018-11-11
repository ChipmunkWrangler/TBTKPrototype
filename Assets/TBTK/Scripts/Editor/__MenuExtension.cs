using UnityEngine;
using UnityEditor;

#if UNITY_5_3_OR_NEWER
using UnityEditor.SceneManagement;
#endif

using System.Collections;

using TBTK;

namespace TBTK {

	public class MenuExtension : EditorWindow {
		
		[MenuItem ("Tools/TBTK/New Scene - Square Grid", false, -100)]
		private static void NewSceneSquareGrid(){
			CreateEmptyScene();
			
			GameObject obj=(GameObject)Instantiate(Resources.Load("ScenePrefab/TBTK_SquareGrid", typeof(GameObject)));
			obj.name="TBTK_SquareGrid";
			
			GameObject uiObj=(GameObject)Instantiate(Resources.Load("ScenePrefab/UIObject", typeof(GameObject)));
			uiObj.name="UIObject";
			
			uiObj.transform.parent=obj.transform;
		}
		
		[MenuItem ("Tools/TBTK/New Scene - Hex Grid", false, -100)]
		private static void NewSceneHexGrid() {
			CreateEmptyScene();
			
			GameObject obj=(GameObject)Instantiate(Resources.Load("ScenePrefab/TBTK_HexGrid", typeof(GameObject)));
			obj.name="TBTK_HexGrid";
			
			GameObject uiObj=(GameObject)Instantiate(Resources.Load("ScenePrefab/UIObject", typeof(GameObject)));
			uiObj.name="UIObject";
			
			uiObj.transform.parent=obj.transform;
		}

		private static void CreateEmptyScene(){
			#if UNITY_5_3_OR_NEWER
				EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
			#else
				EditorApplication.NewScene();
				GameObject camObj=Camera.main.gameObject; 	DestroyImmediate(camObj);
				Light light=GameObject.FindObjectOfType<Light>();
				if(light!=null) DestroyImmediate(light.gameObject);
			#endif
			
			RenderSettings.skybox=null;
		}
		
		
		
		
		
		
		
		
		[MenuItem ("Tools/TBTK/FactionManagerEditor", false, 10)]
		static void OpenFactionManagerEditorWindow () {
			//FactionManagerEditorWindow.Init();
			NewFactionManagerEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TBTK/UnitEditor", false, 10)]
		static void OpenUnitEditor () {
			//UnitEditorWindow.Init();
			NewUnitEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TBTK/CollectibleEditor", false, 10)]
		static void OpenCollectibleEditor () {
			//UnitEditorWindow.Init();
			NewCollectibleEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TBTK/UnitAbilityEditor", false, 10)]
		public static void OpenUnitAbilityEditor () {
			//UnitAbilityEditorWindow.Init();
			NewUnitAbilityEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TBTK/FactionAbilityEditor", false, 10)]
		public static void OpenFactionAbilityEditor () {
			//FactionAbilityEditorWindow.Init();
			NewFactionAbilityEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TBTK/PerkEditor", false, 10)]
		public static void OpenPerkEditor () {
			//PerkEditorWindow.Init();
			NewPerkEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TBTK/DamageArmorTable", false, 10)]
		public static void OpenDamageTable () {
			//DamageArmorDBEditor.Init();
			DamageTableEditorWindow.Init();
		}
		
		[MenuItem ("Tools/TBTK/Contact and Support Info", false, 100)]
		static void OpenForumLink () {
			SupportContactWindow.Init();
		}
		
	}


}