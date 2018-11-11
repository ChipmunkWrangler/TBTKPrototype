using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{

	[RequireComponent (typeof (EffectTracker))]
	public class AbilityManagerUnit : AbilityManager{
		
		public static AbilityManagerUnit instance;
		
		private List<UnitAbility> unitAbilityDBList=new List<UnitAbility>();
		
		public void Init(){
			instance=this;
			
			unitAbilityDBList=UnitAbilityDB.LoadClone();
		}
		
		
		public int GetAbilityDBIndex(int abID){
			for(int i=0; i<unitAbilityDBList.Count; i++){
				if(unitAbilityDBList[i].prefabID==abID) return i;
			}
			return -1;
		}
		
		
		public static void PerkUnlockNewAbility(List<int> unitIDList, int abID, int abIDSub){ if(instance!=null) instance._PerkUnlockNewAbility(unitIDList, abID, abIDSub); }
		public void _PerkUnlockNewAbility(List<int> unitIDList, int abID, int abIDSub){ 
			int dbIndex=GetAbilityDBIndex(abID);
			
			if(dbIndex==-1 && abIDSub==-1) return;
			
			List<Unit> unitList=FactionManager.GetAllUnit();
			
			for(int i=0; i<unitList.Count; i++){
				Unit unit=unitList[i];
				if(unit.isAIUnit) continue;
				
				if(unitIDList==null || unitIDList.Contains(unit.prefabID)){
					int replaceIndex=-1;
					if(abIDSub>=0){
						for(int n=0; n<unit.abilityList.Count; n++){
							if(unit.abilityList[n].prefabID==abIDSub){
								replaceIndex=n;	//cached the replaceIndex to be used for adding the new ability later
								break;
							}
						}
					}
					
					if(dbIndex>=0){										//add the new ability
						UnitAbility unitAbility=unitAbilityDBList[dbIndex].Clone();
						unitAbility.Init(unit);
						if(replaceIndex>=0){							//if we need to replacing something that alreayd existed
							unit.abilityIDList[replaceIndex]=abID;
							unit.abilityList[replaceIndex]=unitAbility;
						}
						else{												//otherwise just add the new ability
							unit.abilityIDList.Add(abID);
							unit.abilityList.Add(unitAbility);
						}
					}
				}
			}
			
			GameControl.ReselectUnit();	//reselect current unit to refresh the UI
			
			if(dbIndex>=0) TBTK.OnNewUnitAbility(unitAbilityDBList[dbIndex].Clone());
		}
		
		
		
		
		//to setup abilities of individual unit
		public static List<UnitAbility> GetAbilityListBasedOnIDList(List<int> IDList){ 
			if(instance==null) return null;
			return instance._GetAbilityListBasedOnIDList(IDList);
		}
		public List<UnitAbility> _GetAbilityListBasedOnIDList(List<int> IDList){
			List<UnitAbility> newList=new List<UnitAbility>();
			for(int i=0; i<IDList.Count; i++){
				for(int n=0; n<unitAbilityDBList.Count; n++){
					if(unitAbilityDBList[n].prefabID==IDList[i]){
						//if(!unitAbilityDBList[n].onlyAvailableViaPerk) 
							newList.Add(unitAbilityDBList[n].Clone());
						break;
					}
				}
			}
			return newList;
		}
		
		public static UnitAbility GetAbilityBasedOnID(int ID){ return instance._GetAbilityBasedOnID(ID); }
		public UnitAbility _GetAbilityBasedOnID(int ID){
			for(int n=0; n<unitAbilityDBList.Count; n++){
				if(unitAbilityDBList[n].prefabID==ID) return unitAbilityDBList[n].Clone();
			}
			return null;
		}
		
		
		
		
		public static void ActivateTargetMode(Tile tile, UnitAbility ability, int abIndex, TargetModeCallBack sCallBack, ExitTargetModeCallBack eCallBack){
			instance.ActivateTargetModeUnit(tile, ability, abIndex, sCallBack, eCallBack);
		}
		
	}

}
