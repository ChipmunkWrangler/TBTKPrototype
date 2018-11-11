using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	public class EffectTracker : MonoBehaviour {
		
		public List<Unit> unitList=new List<Unit>();
		public List<Tile> tileList=new List<Tile>();
		public List<Tile> visibleTileList=new List<Tile>();
		
		
		private static EffectTracker instance;
		
		void Awake(){
			instance=this;
		}
		
		
		
		public static void IterateEffectDuration(){ instance._IterateEffectDuration(); }
		public void _IterateEffectDuration(){
			for(int i=0; i<tileList.Count; i++) tileList[i].IterateEffectDuration();
			for(int i=0; i<unitList.Count; i++) unitList[i].IterateEffectDuration();
			//for(int i=0; i<visibleTileList.Count; i++) unitList[i].IterateEffectDuration();
			for(int i=0; i<visibleTileList.Count; i++) visibleTileList[i].IterateEffectDuration();	//fixed since v2.1.1f1
		}
		
		
		
		public static void Track(Tile tile){ if(!instance.tileList.Contains(tile)) instance.tileList.Add(tile); }
		public static void Track(Unit unit){ if(!instance.unitList.Contains(unit)) instance.unitList.Add(unit); }
		
		public static void Untrack(Tile tile){ instance.tileList.Remove(tile); }
		public static void Untrack(Unit unit){ instance.unitList.Remove(unit); }
		
		public static void TrackVisible(Tile tile){ if(!instance.visibleTileList.Contains(tile)) instance.visibleTileList.Add(tile); }
		public static void UntrackVisible(Tile tile){ instance.visibleTileList.Remove(tile); }
		
		
		
		
		public static void AddTileWithEffect(Tile tile){ if(!instance.tileList.Contains(tile)) instance.tileList.Add(tile); }
		public static void RemoveTileWithEffect(Tile tile){ instance.tileList.Remove(tile); }
		
		
		public static void AddUnitWithEffect(Unit unit){ if(!instance.unitList.Contains(unit)) instance.unitList.Add(unit); }
		public static void RemoveUnitWithEffect(Unit unit){ instance.StartCoroutine(instance._RemoveUnitWithEffect(unit)); }//instance.unitList.Remove(unit); }
		IEnumerator _RemoveUnitWithEffect(Unit unit){
			yield return null;
			unitList.Remove(unit);
		}
		
	}

}


