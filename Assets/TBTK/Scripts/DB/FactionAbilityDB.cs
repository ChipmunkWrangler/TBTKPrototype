using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class FactionAbilityDB : MonoBehaviour {

		public List<FactionAbility> abilityList=new List<FactionAbility>();
		public List<FactionAbility> factionAbilityList=new List<FactionAbility>();
	
		public static FactionAbilityDB LoadDB(){
			GameObject obj=Resources.Load("DB_TBTK/FactionAbilityDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			if(obj==null) Debug.Log("no object");
			
			return obj.GetComponent<FactionAbilityDB>();
		}
		
		public static List<FactionAbility> Load(){
			GameObject obj=Resources.Load("DB_TBTK/FactionAbilityDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			FactionAbilityDB instance=obj.GetComponent<FactionAbilityDB>();
			return instance.abilityList;
		}
		
		public static List<FactionAbility> LoadClone(){
			GameObject obj=Resources.Load("DB_TBTK/FactionAbilityDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			FactionAbilityDB instance=obj.GetComponent<FactionAbilityDB>();
			
			List<FactionAbility> list=new List<FactionAbility>();
			for(int i=0; i<instance.abilityList.Count; i++){
				list.Add(instance.abilityList[i].Clone());
			}
			
			return list;
		}
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<FactionAbilityDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/DB_TBTK/FactionAbilityDB.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
		
	}
	
}
