using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UnitAbilityDB : MonoBehaviour {

		public List<UnitAbility> abilityList=new List<UnitAbility>();
		public List<UnitAbility> unitAbilityList=new List<UnitAbility>();
	
		public static UnitAbilityDB LoadDB(){
			GameObject obj=Resources.Load("DB_TBTK/UnitAbilityDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			if(obj==null) Debug.Log("no object");
			
			return obj.GetComponent<UnitAbilityDB>();
		}
		
		public static List<UnitAbility> Load(){
			GameObject obj=Resources.Load("DB_TBTK/UnitAbilityDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			UnitAbilityDB instance=obj.GetComponent<UnitAbilityDB>();
			return instance.abilityList;
		}
		
		public static List<UnitAbility> LoadClone(){
			GameObject obj=Resources.Load("DB_TBTK/UnitAbilityDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			UnitAbilityDB instance=obj.GetComponent<UnitAbilityDB>();
			
			List<UnitAbility> list=new List<UnitAbility>();
			for(int i=0; i<instance.abilityList.Count; i++){
				list.Add(instance.abilityList[i].Clone());
			}
			
			return list;
		}
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<UnitAbilityDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/DB_TBTK/UnitAbilityDB.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
		
	}
	
}
