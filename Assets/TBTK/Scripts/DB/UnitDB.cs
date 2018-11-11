using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UnitDB : MonoBehaviour {

		public List<Unit> unitList=new List<Unit>();
	
		public static UnitDB LoadDB(){
			GameObject obj=Resources.Load("DB_TBTK/UnitDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			if(obj==null) Debug.Log("no object");
			
			return obj.GetComponent<UnitDB>();
		}
		
		public static List<Unit> Load(){
			GameObject obj=Resources.Load("DB_TBTK/UnitDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			UnitDB instance=obj.GetComponent<UnitDB>();
			return instance.unitList;
		}
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<UnitDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/DB_TBTK/UnitDB.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
		
		public bool ClearEmptyElement(){
			bool reset=false;
			for(int i=0; i<unitList.Count; i++){
				if(unitList[i]==null){
					unitList.RemoveAt(i);
					i-=1;
					reset=true;
				}
			}
			return reset;
			//if(reset) TBEditor.UpdateLabel_Unit();
		}
	}
	
}
