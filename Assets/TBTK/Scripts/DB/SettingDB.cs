using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {
	
	public class SettingDB : MonoBehaviour {
		
		
		public bool generateGridOnStart=false;
		public bool generateUnitOnStart=false;
		public bool generateCollectibleOnStart=false;
		
		[Header("Turn rules")]
		public _TurnMode turnMode;
		public _MoveOrder moveOrder;
		
		public bool enableManualUnitDeployment=true;
		
		public bool enableActionAfterAttack=false;
		
		[Header("Counter Attack")]
		public bool enableCounter=false;
		public float counterAPMultiplier=1f;
		public float counterDamageMultiplier=1f;
		
		[Header("AP rules")]
		public bool restoreUnitAPOnTurn=true;
		public bool useAPForMove=true;
		public bool useAPForAttack=true;
		
		[Header("Misc")]
		public bool attackThroughObstacle=false;
		
		[Header("Overwatch")]
		public float overwatchHitPenalty=0.2f;
		public float overwatchCritPenalty=0.1f;
		
		[Header("Fog of War")]
		public bool enableFogOfWar=false;
		public float peekFactor=0.4f;
		
		[Header("Cover System")]
		public bool enableCover=false;
		public int effectiveCoverAngle=45;
		public float exposedCritBonus=0.3f;
		public float fullCoverBonus=0.75f;
		public float halfCoverBonus=0.25f;
		
		[Header("Flanking System")]
		public bool enableFlanking=false;
		public float flankingAngle=120;
		public float flankingBonus=1.5f;
		
		[Header("Camera")]
		public bool enableActionCam=false;
		public float actionCamFreqAttack=0.25f;
		public float actionCamFreqAbility=0.5f;
		
		
		public bool savePerk=true;
		
		
		public static SettingDB LoadDB(){
			GameObject obj=Resources.Load("DB_TBTK/SettingDB", typeof(GameObject)) as GameObject;
			
			#if UNITY_EDITOR
				if(obj==null) obj=CreatePrefab();
			#endif
			
			if(obj==null){
				Debug.Log("no object");
				return null;
			}
			
			return obj.GetComponent<SettingDB>();
		}
		
		
		#if UNITY_EDITOR
			private static GameObject CreatePrefab(){
				GameObject obj=new GameObject();
				obj.AddComponent<UnitDB>();
				GameObject prefab=PrefabUtility.CreatePrefab("Assets/TBTK/Resources/DB_TBTK/SettingDB.prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
				DestroyImmediate(obj);
				AssetDatabase.Refresh ();
				return prefab;
			}
		#endif
		
		
	}
	
}

