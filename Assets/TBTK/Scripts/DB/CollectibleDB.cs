using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class CollectibleDB : MonoBehaviour {

		public List<Collectible> collectibleList=new List<Collectible>();
	
		public static CollectibleDB LoadDB(){
			GameObject obj=Resources.Load("DB_TBTK/CollectibleDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			if(obj==null) Debug.Log("no object");
			
			return obj.GetComponent<CollectibleDB>();
		}
		
		public static List<Collectible> Load(){
			GameObject obj=Resources.Load("DB_TBTK/CollectibleDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			CollectibleDB instance=obj.GetComponent<CollectibleDB>();
			return instance.collectibleList;
		}
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<CollectibleDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/DB_TBTK/CollectibleDB.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
		
		
		public bool ClearEmptyElement(){
			bool reset=false;
			for(int i=0; i<collectibleList.Count; i++){
				if(collectibleList[i]==null){
					collectibleList.RemoveAt(i);
					i-=1;
					reset=true;
				}
			}
			return reset;
			//if(reset) TBEditor.UpdateLabel_Collectible();
		}	
	}
	
}
