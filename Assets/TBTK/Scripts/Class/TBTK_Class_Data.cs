using UnityEngine;
using System.Collections;
using System.Collections.Generic;


using TBTK;

namespace TBTK{

	public class TBData {//: MonoBehaviour {
		
		//~ private static bool postBattle=false;
		//~ public static void SetPostBattle(bool flag){ postBattle=flag; }
		//~ public static bool IsPostBattle(){ return postBattle; }

		private static List<List<TBDataUnit>> factionStartList=new List<List<TBDataUnit>>();	//set before the start of the game
		private static List<List<TBDataUnit>> factionEndList=new List<List<TBDataUnit>>();	//set at the end of the game
		
		//*******************************************************************//	
		//load
		public static void ClearStartData(){
			factionStartList=new List<List<TBDataUnit>>();
		}
		
		//ID is used as the index for the factionStartList which the list should be assign to
		//ID corresponds to the faction's dataID (set in  FactionManager) to load the unit
		public static void SetStartData(int ID, List<TBDataUnit> list){
			for(int i=0; i<list.Count; i++){
				if(list[i].unit==null){ list.RemoveAt(i); i-=1; }
			}
			
			if(ID==factionStartList.Count) factionStartList.Add(list);
			else if(ID<factionStartList.Count) factionStartList[ID]=list;
			else{
				while(factionStartList.Count<ID) factionStartList.Add(null);
				factionStartList.Add(list);
			}
		}
		
		public static List<TBDataUnit> GetStartData(int ID){
			if(ID<0 || ID>=factionStartList.Count) return null;
			return factionStartList[ID];
		}
		
		
		
		//*******************************************************************//	
		//end
		public static bool BattleEndDataExist(){
			return factionEndList.Count==0 ? false : true ;
		}
		
		public static void ClearEndData(){
			factionEndList=new List<List<TBDataUnit>>();
		}
		
		//ID is used as the index for the factionEndList which the list should be assign to
		//ID corresponds to the faction's dataID (set in  FactionManager) 
		public static void SetEndData(int ID, List<TBDataUnit> list){
			for(int i=0; i<list.Count; i++){
				if(list[i].unit==null){ list.RemoveAt(i); i-=1; }
			}
			
			if(ID==factionEndList.Count) factionEndList.Add(list);
			else if(ID<factionEndList.Count) factionEndList[ID]=list;
			else{
				while(factionEndList.Count<ID) factionEndList.Add(null);
				factionEndList.Add(list);
			}
		}
		
		public static List<TBDataUnit> GetEndData(int ID){
			if(ID<0 || ID>=factionEndList.Count) return null;
			return factionEndList[ID];
		}
	}

	
	
	public class TBDataUnit{
		public Unit unit;	//reference to prefab
		
		public int level=1;	
		public UnitStat stats=new UnitStat();
		
		public TBDataUnit(){ }
		public TBDataUnit(Unit unit){ Setup(unit); }
		
		public void CopyStatsToUnit(Unit unit, int dataID=-1){
			if(dataID>=0) unit.SetDataID(dataID);
			unit.SetLevel(level);
			stats.CopyToUnit(unit);
		}
		
		public void CopyStatsFromUnit(Unit unit){
			level=Mathf.Min(1, unit.GetLevel());
			stats.CopyFromUnit(unit);
		}
		
		public void Setup(Unit unitInstance){
			if(unitInstance==null){
				Debug.LogWarning("Data's unit is not set", null);
				return;
			}
			
			unit=unitInstance;
			
			CopyStatsFromUnit(unitInstance);
		}
		
		public TBDataUnit Clone(Unit overrideUnit=null){
			TBDataUnit data=new TBDataUnit();
			data.unit=unit;
			
			if(overrideUnit!=null) CopyStatsFromUnit(overrideUnit);
			else CopyStatsFromUnit(unit);
			
			return data;
		}
	}
	
}