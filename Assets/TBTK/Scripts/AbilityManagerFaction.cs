using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	[RequireComponent (typeof (EffectTracker))]
	public class AbilityManagerFaction : AbilityManager {
		
		public bool startWithFullEnergy=false;
		
		public List<FactionAbility> facAbilityDBList=new List<FactionAbility>();
		public static List<FactionAbility> GetAbilityDBList(){ return instance.facAbilityDBList; }
		
		
		[HideInInspector] public int selectedAbilityID=-1;
		public static int GetSelectedAbilityID(){ return instance.selectedAbilityID; }
		private bool requireTargetSelection=false;	//indicate if current selected Ability require target selection
		
		
		private static AbilityManagerFaction instance;
		
		
		public void Init(){
			instance=this;
			
			facAbilityDBList=FactionAbilityDB.Load();
		}
		
		void Start(){
			List<Faction> factionList=FactionManager.GetFactionList();
			for(int i=0; i<factionList.Count; i++){
				factionList[i].abilityInfo.Init(factionList[i].ID, startWithFullEnergy, factionList[i].isPlayerFaction);
			}
		}
		
		
		public int GetPerkDBIndex(int abID){
			for(int i=0; i<facAbilityDBList.Count; i++){
				if(facAbilityDBList[i].prefabID==abID) return i;
			}
			return -1;
		}
		public static FactionAbility GetFactionAbility(int prefabID){	//used by collectible
			for(int i=0; i<instance.facAbilityDBList.Count; i++){
				if(instance.facAbilityDBList[i].prefabID==prefabID) return instance.facAbilityDBList[i];
			}
			return null;
		}
		
		
		public static void PerkUnlockNewAbility(int ID, int IDX){ if(instance!=null) instance._PerkUnlockNewAbility(ID, IDX); }
		public void _PerkUnlockNewAbility(int ID, int IDX){
			int dbIndex=ID>=0 ? GetPerkDBIndex(ID) : -1 ;
			
			if(dbIndex==-1 && IDX==-1) return;
			
			List<Faction> facList=FactionManager.GetFactionList();
			for(int i=0; i<facList.Count; i++){
				if(!facList[i].isPlayerFaction) continue;
				if(facList[i].abilityInfo==null) continue;
				
				FactionAbilityInfo abInfo=facList[i].abilityInfo;
				int replaceIndex=-1;
				if(IDX>=0){
					for(int n=0; n<abInfo.abilityList.Count; n++){
						if(IDX==abInfo.abilityList[n].prefabID){
							replaceIndex=n;
							break;
						}
					}
				}
				
				if(dbIndex>0){
					FactionAbility ability=facAbilityDBList[dbIndex].Clone();
					ability.factionID=facList[i].ID;
					
					if(replaceIndex>=0) abInfo.abilityList[replaceIndex]=ability;
					else abInfo.abilityList.Add(ability);
				}
				else{
					if(replaceIndex>=0) abInfo.abilityList.RemoveAt(replaceIndex);
				}
			}
			
			if(dbIndex>=0) TBTK.OnNewFactionAbility(facAbilityDBList[dbIndex].Clone());
		}
		
		
		
		public FactionAbility GetAbilityFromCurrentFaction(int index){
			FactionAbilityInfo abilityInfo=FactionManager.GetCurrentFaction().abilityInfo;
			if(index<abilityInfo.abilityList.Count) return abilityInfo.abilityList[index];
			return null;
		}
		
		
		
		
		//called by ability button from UI, select an ability
		public static string SelectAbility(int index){ return instance._SelectAbility(index); }
		public string _SelectAbility(int index){
			AbilityManager.ExitAbilityTargetMode();
			
			FactionAbility ability=GetAbilityFromCurrentFaction(index);
			if(ability==null) return "error";
			
			string exception=ability.IsAvailable();
			if(exception!="") return exception;
			
			requireTargetSelection=ability.requireTargetSelection;
			
			if(!requireTargetSelection) ActivateAbility(null, ability);
			else{
				ActivateTargetModeFaction(ability.GetAOERange(), ability.targetType, index, this.ActivateAbility, null);
			}
			
			return "";
		}
		
		
		
		//callback function for GridManager when a target has been selected for selected ability
		public void ActivateAbility(Tile tile, int index){
			ActivateAbility(tile, GetAbilityFromCurrentFaction(index));
		}
		public void ActivateAbility(Tile tile, FactionAbility ability){
			ability.Use();
			FactionManager.GetCurrentFaction().abilityInfo.energy-=ability.GetCost();
			
			//CastAbility(ability, tile);
			ApplyAbilityEffect(tile, ability, (int)ability.type);
		}
		
		
		
		//energy related function
		public static float GetFactionEnergyFull(int factionID){ return instance._GetFactionEnergyFull(factionID); }
		public float _GetFactionEnergyFull(int factionID){
			Faction faction=FactionManager.GetFaction(factionID);
			return faction!=null ? FactionManager.GetFaction(factionID).abilityInfo.energyFull : 0 ;
		}
		public static float GetFactionEnergy(int factionID){ //return instance._GetFactionEnergy(factionID); }
			Faction faction=FactionManager.GetFaction(factionID);
			return faction!=null ? FactionManager.GetFaction(factionID).abilityInfo.energy : 0 ;
		}
		public static float GetFactionEnergyGainPerTurn(int factionID){ //return instance._GetFactionEnergyGainPerTurn(factionID); }
			Faction faction=FactionManager.GetFaction(factionID);
			return faction!=null ? FactionManager.GetFaction(factionID).abilityInfo.energyGainPerTurn : 0 ;
		}
		
		
		public static List<FactionAbility> GetFactionAbilityList(int factionID){ //return instance._GetFactionAbilityList(factionID); }
			Faction faction=FactionManager.GetFaction(factionID);
			return faction!=null ? FactionManager.GetFaction(factionID).abilityInfo.abilityList : new List<FactionAbility>() ;
		}
		
		
	}
	

}