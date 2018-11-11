using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	
	[System.Serializable]
	public class Faction {

		public int ID=0;
		public string name="Faction";
		public Color color=Color.white;
		
		
		public Faction(){}
		public Faction(string n){ name=n; }
		
		
		public List<Unit> startingUnitList=new List<Unit>();	//unit to be deployed at the start
		public List<Tile> deployableTileList=new List<Tile>();	//tiles available for unit deployment
		
		public List<Unit> allUnitList=new List<Unit>(); 
		
		public List<Unit> movedUnitList=new List<Unit>();	//functional but not really in used
		
		public bool isPlayerFaction=false;
		
		public bool loadFromData=false;
		public int dataID=0;
		//information of unit to replace default startingUnitList, for loadFromData, refer to TBTK_Class_Data.cs for class declaration of DataUnit
		[HideInInspector] public List<TBDataUnit> dataList=new List<TBDataUnit>();		
		
		
		[RangeAttribute(0, 360)]
		public float spawnDirection=0;	//the y-axis rotation when the unit of the faction is placed on the grid
		
		
		public int selectedUnitID=-1;	//used when move-order are not free, cycle through allUnitList
		
		[HideInInspector] public bool foldSpawnInfo=false;	//for editor use only, show or hide spawnInfoList
		public List<FactionSpawnInfo> spawnInfoList=new List<FactionSpawnInfo>();
		
		
		public  bool useDefaultAIMode=true;
		public _AIMode aiMode=_AIMode.Passive;
		
		
		
		public FactionAbilityInfo abilityInfo=new FactionAbilityInfo();
		//~ public float fullEnergy=100;
		//~ public float energyGainPerTurn=20;
		//for factionAbility, ID of the ability which are not available for this faction
		//~ public List<int> unavailableAbilityIDList=new List<int>();	//ID list of ability not available for this level, modified in editor
		
		
		public void UnitMoveDepleted(Unit unit){ movedUnitList.Add(unit); }
		
		
		public bool SelectRandomAvailableUnit(){
			List<int> availableIDList=new List<int>();
			for(int i=0; i<allUnitList.Count; i++){
				if(!allUnitList[i].IsAllActionCompleted() && !allUnitList[i].IsStunned()) availableIDList.Add(i);
			}
			
			if(availableIDList.Count>0){
				selectedUnitID=availableIDList[Random.Range(0, availableIDList.Count)];
				Debug.Log("selectedUnitID "+selectedUnitID);
				if(allUnitList[selectedUnitID].isAIUnit) AIManager.MoveUnit(allUnitList[selectedUnitID]);
				else GameControl.SelectUnit(allUnitList[selectedUnitID], false);
				//GameControl.SelectUnit(allUnitList[selectedUnitID]);
				return true;
			}
			
			GameControl.ClearSelectedUnit();
			
			return false;
		}
		
		
		//loop through all unit, select the first one that hasnt complete all action
		//used in FactionPerTurn, FreeMove order only
		public bool SelectFirstAvailableUnit(){
			int index=-1;
			for(int i=0; i<allUnitList.Count; i++){
				//skipped stunned unit
				//if(!allUnitList[i].IsAllActionCompleted() && allUnitList[i].IsStunned()){
				if(!allUnitList[i].IsAllActionCompleted()){
					index=i;		break;
				}
			}
			
			if(index>=0){
				selectedUnitID=index;
				GameControl.SelectUnit(allUnitList[selectedUnitID]);
				return true;
			}
			
			GameControl.ClearSelectedUnit();
			
			return false;
		}
		
		
		//for FactionPerTurn mode, breakWhenExceedLimit is set to true, where selectUnitID resets when it reachs end
		//for FactionUnitPerTurn mode, breakWhenExceedLimit is set to false, where selectUnitID loops forever
		public bool SelectNextUnitInQueue(bool breakWhenExceedLimit=false){
			//if(TurnControl.GetMoveOrder()==_MoveOrder.Free) return true;
			
			selectedUnitID+=1;
			
			//skipped stunned unit
			//while(allUnitList[selectedUnitID].IsStunned()){
			//	selectedUnitID+=1;
			//	if(selectedUnitID>=allUnitList.Count) break;
			//}
			if(selectedUnitID>=allUnitList.Count){
				if(breakWhenExceedLimit){	//for FactionPerTurn mode, reset the ID
					selectedUnitID=-1;
					return true;
				}
				selectedUnitID=0;
			}
			
			if(TurnControl.GetTurnMode()!=_TurnMode.FactionPerTurn){
				bool isUnitActive=allUnitList[selectedUnitID].NewTurn();	//in case unit is destroyed by damage over time effect
				if(!isUnitActive) return SelectNextUnitInQueue();
			}
			
			if(allUnitList[selectedUnitID].isAIUnit) AIManager.MoveUnit(allUnitList[selectedUnitID]);
			else GameControl.SelectUnit(allUnitList[selectedUnitID], false);
			
			return false;
		}
		
		
		public void RemoveUnit(Unit unit){
			int ID=allUnitList.IndexOf(unit);
			if(ID<=selectedUnitID){
				selectedUnitID-=1;
			}
			allUnitList.Remove(unit);
		}
		
		
		//called by FactionManager.SelectNextFaction in FactionPerTurn mode (resetSelectedID=true)
		public void NewTurn(bool processUnit=false){
			selectedUnitID=-1;
			movedUnitList=new List<Unit>();
			
			if(abilityInfo!=null)
				abilityInfo.energy=Mathf.Min(abilityInfo.energy+abilityInfo.energyGainPerTurn, abilityInfo.energyFull);
			
			if(processUnit){
				for(int i=0; i<allUnitList.Count; i++) allUnitList[i].NewTurn();
			}
			
			if(TurnControl.GetTurnMode()==_TurnMode.UnitPerTurn) return;
			
			if(TurnControl.GetMoveOrder()==_MoveOrder.Random){
				//random order, shuffle the unit order
				List<Unit> newList=new List<Unit>();
				while(allUnitList.Count>0){
					int rand=Random.Range(0, allUnitList.Count);
					newList.Add(allUnitList[rand]);
					allUnitList.RemoveAt(rand);
				}
				allUnitList=newList;
			}
		}
		
		
		//called to when generating unit on grid, spawn unit based on the spawnInfo
		public void Spawn(){
			if(allUnitList.Count!=0) ClearUnit();
			for(int i=0; i<spawnInfoList.Count; i++){
				List<Unit> spawnedUnitList=spawnInfoList[i].Spawn(ID);
				for(int n=0; n<spawnedUnitList.Count; n++){
					allUnitList.Add(spawnedUnitList[n]);
				}
			}
		}
		//clear all unit in allUnitList
		public void ClearUnit(){
			for(int i=0; i<allUnitList.Count; i++){
				if(allUnitList[i]!=null) MonoBehaviour.DestroyImmediate(allUnitList[i].gameObject);
			}
			allUnitList=new List<Unit>(); 
		}
		
		//used in edit mode only, temporary record the position of the spawn tile on spawnInfo, as well as tiles in deployableTileList
		//called when the grid are to be destroyed and regenerated
		public void RecordSpawnTilePos(){
			for(int i=0; i<spawnInfoList.Count; i++) spawnInfoList[i].RecordSpawnTilePos();
			
			deployTilePosList=new List<Vector3>();
			for(int i=0; i<deployableTileList.Count; i++) deployTilePosList.Add(deployableTileList[i].GetPos());
			
			deployableTileList=new List<Tile>();
		}
		//used in edit mode only, re-allocate the spawn tile on spawnInfo based on the infor recorded on RecordSpawnTilePos(), and deployableTileList too
		//called when the grid has been regenerated
		public void SetStartingTileListBaseOnPos(float tileSize=1){
			for(int i=0; i<spawnInfoList.Count; i++) spawnInfoList[i].SetStartingTileListBaseOnPos(tileSize, ID);
			
			GridManager gridManager = GridManager.GetInstance();
			deployableTileList=new List<Tile>();
			for(int i=0; i<deployTilePosList.Count; i++){
				int count=0;
				for(int n=0; n<gridManager.grid.tileList.Count; n++){
					float dist=Vector3.Distance(gridManager.grid.tileList[n].GetPos(), deployTilePosList[i]);
					if(dist<tileSize/2 && !deployableTileList.Contains(gridManager.grid.tileList[n])){
						count+=1;
						gridManager.grid.tileList[n].deployAreaID=ID;
						deployableTileList.Add(gridManager.grid.tileList[n]);
					}
				}
			}
			
			deployTilePosList=new List<Vector3>();
		}
		
		//temporary stored positions of the deployTileList
		private List<Vector3> deployTilePosList=new List<Vector3>();
	}
	
	
	
	//spawnInfo for a certain area on the grid for a faction
	[System.Serializable]
	public class FactionSpawnInfo{	
		public enum _LimitType{UnitCount, UnitValue}
		
		public _LimitType limitType;
		public int limit=2;
		
		public List<Unit> unitPrefabList=new List<Unit>();	//the prefab to be spawned
		public List<int> unitLimitList=new List<int>();		//the limite of each unit, match to the count of unitPrefab
		
		public List<Tile> startingTileList=new List<Tile>(); 	//the tiles which the unit will be deployed on	
																				//when no tile has been specified, no unit will be spawned
		
		private List<Vector3> startingTilePosList=new List<Vector3>();
		
		[RangeAttribute(0, 360)]
		public float spawnDirection=0;	//the y-axis rotation when the unit of the spawnInfo is placed on the grid
		
		
		public void RecordSpawnTilePos(){
			startingTilePosList=new List<Vector3>();
			for(int i=0; i<startingTileList.Count; i++) startingTilePosList.Add(startingTileList[i].GetPos());
			startingTileList=new List<Tile>();
		}
		public void SetStartingTileListBaseOnPos(float tileSize=1, int factionID=0){
			GridManager gridManager = GridManager.GetInstance();
			
			startingTileList=new List<Tile>();
			for(int i=0; i<startingTilePosList.Count; i++){
				int count=0;
				for(int n=0; n<gridManager.grid.tileList.Count; n++){
					float dist=Vector3.Distance(gridManager.grid.tileList[n].GetPos(), startingTilePosList[i]);
					if(dist<tileSize/2 && !startingTileList.Contains(gridManager.grid.tileList[n])){
						count+=1;
						gridManager.grid.tileList[n].spawnAreaID=factionID;
						startingTileList.Add(gridManager.grid.tileList[n]);
					}
				}
			}
			
			startingTilePosList=new List<Vector3>();
		}
		
		
		public List<Unit> Spawn(int factionID=0){
			float currentLimit=0;
			
			if(unitLimitList.Count<unitPrefabList.Count) unitLimitList.Add(1);
			
			List<Unit> cloneUnitList=new List<Unit>();	//use a dummy list so the original list wont get altered if this is run in EditMode
			for(int i=0; i<unitPrefabList.Count; i++) cloneUnitList.Add(unitPrefabList[i]);	
			
			List<int> cloneLimitList=new List<int>();	//use a dummy list so the original list wont get altered if this is run in EditMode
			for(int i=0; i<unitLimitList.Count; i++) cloneLimitList.Add(unitLimitList[i]);	
			
			List<Tile> cloneTileList=new List<Tile>();	//use a dummy list so the original list wont get altered if this is run in EditMode
			for(int i=0; i<startingTileList.Count; i++){ 
				if(startingTileList[i].walkable && startingTileList[i].unit==null) cloneTileList.Add(startingTileList[i]);
			}
			
			
			List<Unit> unitList=new List<Unit>();
			
			while(currentLimit<limit){
				if(cloneUnitList.Count==0 || cloneTileList.Count==0) break;
				
				int randU=Random.Range(0, cloneUnitList.Count);
				int randT=Random.Range(0, cloneTileList.Count);
				
				#if UNITY_EDITOR
					GameObject unitObj=(GameObject)PrefabUtility.InstantiatePrefab(cloneUnitList[randU].gameObject);
				#else
					GameObject unitObj=(GameObject)MonoBehaviour.Instantiate(cloneUnitList[randU].gameObject);
				#endif
				Unit unit=unitObj.GetComponent<Unit>();
				unit.factionID=factionID;
				
				unit.tile=cloneTileList[randT];
				cloneTileList[randT].unit=unit;
				
				unitObj.transform.position=unit.tile.GetPos();
				unitObj.transform.rotation=Quaternion.Euler(0, spawnDirection, 0);
				
				unitObj.transform.parent=FactionManager.GetTransform();
				
				unitList.Add(unit);
				
				if(limitType==_LimitType.UnitValue) currentLimit+=cloneUnitList[randU].value;
				else if(limitType==_LimitType.UnitCount) currentLimit+=1;
				
				cloneLimitList[randU]-=1;
				if(cloneLimitList[randU]<=0){
					cloneLimitList.RemoveAt(randU);
					cloneUnitList.RemoveAt(randU);
				}
				
				cloneTileList.RemoveAt(randT);
			}
			
			return unitList;
		}
		
	}
	
	
	
	
	
	
	//class contains the each faction's FactionAbility info
	[System.Serializable]
	public class FactionAbilityInfo{
		public int factionID=-1;	//correspond to the factionID in FactionManager
		
		public float energyFull=100;
		public float energy=0;
		public float energyGainPerTurn=20;
		public float GetEnergyFull(){ return energyFull; }//+PerkManager.GetEnergyCapModifier(); }
		public float GetEnergy(){ return energy; }
		public float GetEnergyGainPerTurn(){ return energyGainPerTurn; }
		
		public List<int> availableIDList=new List<int>();	//ID list of ability available for this level, modified in editor
		public List<int> unavailableIDList=new List<int>();	//used to verify with availableIDList and add new faction ability
		
		public List<FactionAbility> abilityList=new List<FactionAbility>();
		
		//load up the ability and setup the energy
		public void Init(int facID, bool startWithFullEnergy=false, bool isPlayerFaction=true){
			factionID=facID;
			
			if(startWithFullEnergy) energy=energyFull;
			
			List<FactionAbility> dbList=AbilityManagerFaction.GetAbilityDBList();
			
			abilityList=new List<FactionAbility>();
			for(int i=0; i<dbList.Count; i++){
				if(dbList[i].onlyAvailableViaPerk) continue;
				if(availableIDList.Contains(dbList[i].prefabID)) abilityList.Add(dbList[i].Clone());
				//if(!unavailableIDList.Contains(dbList[i].prefabID)) abilityList.Add(dbList[i].Clone());
			}
			
			//add ability unlocked via perk (PerkManager is carried forth from last level)
			if(isPlayerFaction){
				List<int> perkAbilityXIDList=PerkManager.GetFactionAbilityXIDList();
				for(int n=0; n<perkAbilityXIDList.Count; n++){
					for(int i=0; i<abilityList.Count; i++){
						if(perkAbilityXIDList[n]==abilityList[i].prefabID){
							abilityList.RemoveAt(i);
							i-=1;
						}
					}
				}
				
				List<int> perkAbilityIDList=PerkManager.GetFactionAbilityIDList();
				for(int n=0; n<perkAbilityIDList.Count; n++){
					for(int i=0; i<dbList.Count; i++){
						if(dbList[i].prefabID==perkAbilityIDList[n]){
							abilityList.Add(dbList[i].Clone());
							break;
						}
					}
				}
			}
			
			
			for(int i=0; i<abilityList.Count; i++){
				abilityList[i].Init(factionID);
			}
		}
		
		public void GainEnergy(int value){
			energy+=value;
			energy=Mathf.Min(energy, GetEnergyFull());
		}
	}
	
	

}