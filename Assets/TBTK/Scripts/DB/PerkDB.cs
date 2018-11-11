using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class PerkDB : MonoBehaviour {

		public List<Perk> perkList=new List<Perk>();
	
		public static PerkDB LoadDB(){
			GameObject obj=Resources.Load("DB_TBTK/PerkDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			if(obj==null) Debug.Log("no object");
			
			return obj.GetComponent<PerkDB>();
		}
		
		public static List<Perk> Load(){
			GameObject obj=Resources.Load("DB_TBTK/PerkDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			PerkDB instance=obj.GetComponent<PerkDB>();
			return instance.perkList;
		}
		
		public static List<Perk> LoadClone(){
			GameObject obj=Resources.Load("DB_TBTK/PerkDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			PerkDB instance=obj.GetComponent<PerkDB>();
			
			List<Perk> list=new List<Perk>();
			for(int i=0; i<instance.perkList.Count; i++){
				list.Add(instance.perkList[i].Clone());
			}
			
			return list;
		}
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<PerkDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/DB_TBTK/PerkDB.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
		
	}
	
}
