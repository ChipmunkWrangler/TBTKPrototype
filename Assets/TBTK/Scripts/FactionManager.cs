using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TBTK;

namespace TBTK{

	public class FactionManager : MonoBehaviour {
		
		public bool generateUnitOnStart=false;
		
		
		public bool hasAIInGame=true;
		public List<int> playerFactionIDList=new List<int>();
		public static List<int> GetPlayerFactionID(){ return instance.playerFactionIDList; }
		
		public int selectedFactionID=-1;
		public List<Faction> factionList=new List<Faction>();
		public static int GetTotalFactionCount(){ return instance.factionList.Count; }
		public static List<Faction> GetFactionList(){ return instance.factionList; }
		public static int GetSelectedFactionID(){ return instance.selectedFactionID; }
		
		public static Faction GetCurrentFaction(){ return instance.factionList[instance.selectedFactionID]; }
		
		
		[HideInInspector] public int selectedUnitID=-1;	//only use in UnitPerTurn
		[HideInInspector] public int totalUnitCount=0;
		public List<Unit> allUnitList=new List<Unit>();	//all unit from all faction
		public static int GetTotalUnitCount(){ return instance.totalUnitCount; }

		private static FactionManager instance;
		public static FactionManager SetInstance(){ return instance!=null ? instance : instance=(FactionManager)FindObjectOfType(typeof(FactionManager)); }
		public static FactionManager GetInstance(){ return instance; }
		public static Transform GetTransform(){ return instance!=null ? instance.transform : null ; }
		
		
		
		void Awake(){
			if(instance==null) instance=this;
		}
		
		public static void GameOver(){ instance._GameOver(); }
		public void _GameOver(){
			TBData.ClearEndData();
			
			//save the faction back to data if it's loaded from data
			for(int i=0; i<factionList.Count; i++){
				Faction fac=factionList[i];
				if(!fac.loadFromData) continue;
				
				List<TBDataUnit> startDataList=TBData.GetStartData(fac.dataID);
				List<TBDataUnit> list=new List<TBDataUnit>();
				
				for(int m=0; m<startDataList.Count; m++){
					for(int n=0; n<fac.allUnitList.Count; n++){
						if(fac.allUnitList[n].GetDataID()==m){
							list.Add(startDataList[m].Clone(fac.allUnitList[n]));
							
							fac.allUnitList.RemoveAt(n);
							break;
						}
					}
				}
				
				TBData.SetEndData(fac.dataID, list);
			}
			
			TBData.ClearStartData();
		}
		
		//called by GameControl to initiate the factions, 
		//load from data when needed, spawn the startingUnit, initiate the unit (abillities), check if unit deployment is required....
		public void Init(){
			if(instance==null) instance=this;
			
			if(generateUnitOnStart) GenerateUnit();
			
			//setup all the unit in the game
			for(int i=0; i<factionList.Count; i++){
				for(int n=0; n<factionList[i].allUnitList.Count; n++){
					if(factionList[i].allUnitList[n]==null){ factionList[i].allUnitList.RemoveAt(n);	n--; continue; }
					factionList[i].allUnitList[n].InitUnitAbility();
					factionList[i].allUnitList[n].isAIUnit=!factionList[i].isPlayerFaction;
				}
			}
			
			Vector3 pos=new Vector3(0, 99999, 0);
			Quaternion rot=Quaternion.identity;
			for(int i=0; i<factionList.Count; i++){
				Faction fac=factionList[i];
				
				//if load from data, then load the list from data and then put it to startingUnitList
				/*
				if(fac.loadFromData){ 
					fac.dataList=TBData.GetStartData(fac.dataID);
					if(fac.dataList!=null && fac.dataList.Count>0){
						fac.startingUnitList=new List<Unit>();
						for(int n=0; n<fac.dataList.Count; n++) fac.startingUnitList.Add(fac.dataList[n].unit);
						
						//put the data list back into the end data first, to save the current starting lineup for next menu loading
						//in case the player didnt finish the level and GameOver is not called
						TBData.SetEndData(fac.dataID, fac.dataList);
					}
					else fac.loadFromData=false;
				}
				if(!fac.loadFromData){	
					//if using default startingUnitList, make sure none of the element in startingUnitList is empty
					for(int n=0; n<fac.startingUnitList.Count; n++){
						if(fac.startingUnitList[n]==null){ fac.startingUnitList.RemoveAt(n); n-=1; }
					}
				}
				*/
				
				if(fac.loadFromData){ 
					fac.startingUnitList=new List<Unit>();
					fac.dataList=TBData.GetStartData(fac.dataID);
					if(fac.dataList==null){
						Debug.LogWarning("TBTK faction's data not setup properly", this);
						continue;
					}
					Debug.Log("unit from data: "+fac.dataList.Count);
					for(int n=0; n<fac.dataList.Count; n++) fac.startingUnitList.Add(fac.dataList[n].unit);
					
					//put the data list back into the end data first, to save the current starting lineup for next menu loading
					//in case the player didnt finish the level and GameOver is not called
					TBData.SetEndData(fac.dataID, fac.dataList);
				}
				else{	
					//if using default startingUnitList, make sure none of the element in startingUnitList is empty
					for(int n=0; n<fac.startingUnitList.Count; n++){
						if(fac.startingUnitList[n]==null){ fac.startingUnitList.RemoveAt(n); n-=1; }
					}
				}
				
				for(int n=0; n<fac.startingUnitList.Count; n++){
					GameObject unitObj=(GameObject)Instantiate(fac.startingUnitList[n].gameObject, pos, rot);
					fac.startingUnitList[n]=unitObj.GetComponent<Unit>();
					fac.startingUnitList[n].InitUnitAbility();
					fac.startingUnitList[n].isAIUnit=!fac.isPlayerFaction;
					unitObj.transform.parent=transform;
					unitObj.SetActive(false);
					
					if(fac.loadFromData) //fac.startingUnitList[n].ModifyStatsToData(fac.dataList[n], n);
						fac.dataList[n].CopyStatsToUnit(fac.startingUnitList[n], n);
				}
				
				if(fac.isPlayerFaction && fac.startingUnitList.Count>0 && !requireDeployment){
					if(deployingFactionID==-1) deployingFactionID=i;
					requireDeployment=true;
				}
			}
			
			if(!GameControl.EnableManualUnitDeployment()){
				for(int i=0; i<factionList.Count; i++){
					if(factionList[i].startingUnitList.Count<=0) continue;
					
					AutoDeployFaction(i);
					for(int n=0; n<deployedUnitList.Count; n++){
						deployedUnitList[n].factionID=factionList[i].ID;
						factionList[i].allUnitList.Add(deployedUnitList[n]);
					}
					deployedUnitList=new List<Unit>();
				}
			}
		}
		
		//called by GameControl just before the game start to initiate all the faction, after unit deployment is done
		//sort out the unit move order, reset trigger status and what not.
		public static void SetupFaction(){ instance._SetupFaction(); }
		void _SetupFaction(){
			for(int i=0; i<factionList.Count; i++){
				if(factionList[i].isPlayerFaction){
					playerFactionIDList.Add(factionList[i].ID);
					
					for(int n=0; n<factionList[i].allUnitList.Count; n++){
						UpdateHostileUnitTriggerStatus(factionList[i].allUnitList[n]);
					}
				}
				
				if(TurnControl.GetTurnMode()!=_TurnMode.UnitPerTurn){
					if(TurnControl.GetMoveOrder()!=_MoveOrder.Random){
						factionList[i].allUnitList=ArrangeUnitListToMovePriority(factionList[i].allUnitList);
					}
				}
				
				factionList[i].NewTurn(true);	//pass true to call new turn on all unit
				//for(int n=0; n<factionList[i].allUnitList.Count; n++) factionList[i].allUnitList[n].ResetUnitTurnData();
				
				totalUnitCount+=factionList[i].allUnitList.Count;
			}
			
			if(factionList.Count==playerFactionIDList.Count) hasAIInGame=false;
			
			Debug.Log("SetupFaction  "+TurnControl.GetTurnMode());
			if(TurnControl.GetTurnMode()==_TurnMode.UnitPerTurn){
				for(int i=0; i<factionList.Count; i++){
					for(int n=0; n<factionList[i].allUnitList.Count; n++) allUnitList.Add(factionList[i].allUnitList[n]);
				}
				
				allUnitList=ArrangeUnitListToMovePriority(allUnitList);
				Debug.Log("SetupFaction  allUnitList");
			}
			
		}
		
		
		
		public static void StartUnitDeploymentPhase(){
			GridManager.DeployingFaction(instance.deployingFactionID);
			TBTK.OnUnitDeployment(true);
		}
		
		
		public static void OnUnitDestroyed(Unit unit){ instance._OnUnitDestroyed(unit); }
		public void _OnUnitDestroyed(Unit unit){
			totalUnitCount-=1;
			
			//assume isObjectUnit is the boolean flag
			if(unit.isObjectUnit){
				GameControl.GameOver(0);	//assume 0 is the player unit faction
				return;
			}
			
			//remove unit from allUnitList so it no longers reserve a turn
			if(TurnControl.GetTurnMode()==_TurnMode.UnitPerTurn){
				int ID=allUnitList.IndexOf(unit);
				if(ID<=selectedUnitID){
					selectedUnitID-=1;
				}
				allUnitList.Remove(unit);
			}
			
			//remove the unit from the faction, and if the faction has no active unit remain, the faction itself is remove (out of the game)
			for(int i=0; i<factionList.Count; i++){
				if(factionList[i].ID==unit.factionID){
					factionList[i].RemoveUnit(unit);
					if(factionList[i].allUnitList.Count==0){
						TurnControl.OnFactionDestroyed();	//to track cd on ability and effect
						TBTK.OnFactionDestroyed(factionList[i].ID);
						
						factionList.RemoveAt(i);
						
						if(selectedFactionID>i) selectedFactionID-=1;
						else if(selectedFactionID==i) TurnControl.EndTurn();
					}
					break;
				}
			}
			
			//if there's only 1 faction remain (since faction with no active unit will be removed), then faction has won the game
			if(factionList.Count==1) GameControl.GameOver(factionList[0].ID);
		}
		
		//called when a unit has its turn priority changed, to update the move order
		public static void UnitTurnPriorityChanged(List<int> facIDList){ instance._UnitTurnPriorityChanged(facIDList); }
		public void _UnitTurnPriorityChanged(List<int> facIDList){
			if(TurnControl.GetTurnMode()==_TurnMode.UnitPerTurn){
				allUnitList=ArrangeUnitListToMovePriority(allUnitList);
			}
			else if(TurnControl.GetMoveOrder()!=_MoveOrder.Random){
				for(int i=0; i<factionList.Count; i++){
					if(facIDList.Contains(factionList[i].ID)){
						factionList[i].allUnitList=ArrangeUnitListToMovePriority(factionList[i].allUnitList);
					}
				}
			}
		}
		
		//generic function to sort a unit-list based on the unit turn priority
		public static List<Unit> ArrangeUnitListToMovePriority(List<Unit> list){
			List<Unit> newList=new List<Unit>();
			
			while(list.Count>0){
				float highest=0;
				int highestID=0;
				for(int i=0; i<list.Count; i++){
					float priority=list[i].GetTurnPriority();
					if(priority>highest){
						highest=priority;
						highestID=i;
					}
				}
				
				newList.Add(list[highestID]);
				list.RemoveAt(highestID);
			}
			
			return newList;
		}
		
		
		//called to update AI unit trigger status whenever a player unit moved to a new tile
		public static void UpdateHostileUnitTriggerStatus(Unit unit){
			if(!instance.hasAIInGame) return;
			
			List<Unit> unitList=GetAllHostileUnit(unit.factionID);
			for(int i=0; i<unitList.Count; i++){
				if(GridManager.GetDistance(unit.tile, unitList[i].tile)<=unitList[i].GetSight()){
					unitList[i].trigger=true;
				}
			}
		}
		
		
		//functional but not really required atm
		//moved unit to movedUnitList in faction
		public static void UnitMoveDepleted(Unit unit){ instance._UnitMoveDepleted(unit); }
		public void _UnitMoveDepleted(Unit unit){
			if(TurnControl.GetTurnMode()!=_TurnMode.FactionPerTurn){
				for(int i=0; i<factionList.Count; i++){
					if(factionList[i].ID==unit.factionID){
						factionList[i].UnitMoveDepleted(unit);
					}
				}
			}
		}
		
		
		
		//used in FactionPerTurn mode only
		public static void EndTurn_FactionPerTurn(){ instance._EndTurn_FactionPerTurn(); }
		public void _EndTurn_FactionPerTurn(){
			GameControl.ClearSelectedUnit();
			
			selectedFactionID+=1;
			if(selectedFactionID>=factionList.Count) selectedFactionID=0;
			factionList[selectedFactionID].NewTurn(true);
			
			if(factionList[selectedFactionID].isPlayerFaction){	//if it's a player's faction, select a unit
				if(TurnControl.GetMoveOrder()==_MoveOrder.Free) _SelectNextUnitInFaction_Free();
				else _SelectNextUnitInFaction_NotFree();
			}
			else{															//if it's a AI's faction, execute AI move
				if(TurnControl.GetMoveOrder()==_MoveOrder.Free) AIManager.MoveFaction(factionList[selectedFactionID]);
				else _SelectNextUnitInFaction_NotFree();
			}
		}
		//used in FactionPerTurn mode only
		public static bool SelectNextUnitInFaction_Free(){ return instance._SelectNextUnitInFaction_Free(); }
		public bool _SelectNextUnitInFaction_Free(){
			//return true is there's unit available
			return factionList[selectedFactionID].SelectFirstAvailableUnit();
		}
		public static bool SelectNextUnitInFaction_NotFree(){ return instance._SelectNextUnitInFaction_NotFree(); }
		public bool _SelectNextUnitInFaction_NotFree(){
			//return true is there's unit available, false when the queue is depleted
			bool allUnitCycled=factionList[selectedFactionID].SelectNextUnitInQueue(true);
			return !allUnitCycled;
		}
		
		
		/*
		void OnGUI(){
			if(GUI.Button(new Rect(50, 200, 100, 20), "next unit")){
				Debug.Log("click");
				//SelectNextUnitInFaction_Free();
				SelectNextUnitInFaction_NotFree();
				//CameraControl.SetPosition(GameControl.GetSelectedUnit().tile.GetPos());
			}
		}
		*/
		
		
		
		//used in FactionUnitPerTurn mode only
		public static void EndTurn_FactionUnitPerTurn(){ instance._EndTurn_FactionUnitPerTurn(); }
		public void _EndTurn_FactionUnitPerTurn(){
			selectedFactionID+=1;
			if(selectedFactionID>=factionList.Count) selectedFactionID=0;
			if(TurnControl.GetMoveOrder()==_MoveOrder.Free){
				factionList[selectedFactionID].NewTurn(true);	//enabled if there's no need to cycle through all unit
				factionList[selectedFactionID].SelectRandomAvailableUnit();
			}
			else factionList[selectedFactionID].SelectNextUnitInQueue();
		}
		
		//used in UnitPerTurn mode only, select the next unit in turn
		public static void EndTurn_UnitPerTurn(){ instance._EndTurn_UnitPerTurn(); }
		public void _EndTurn_UnitPerTurn(){
			selectedUnitID+=1;
			if(selectedUnitID>=allUnitList.Count) selectedUnitID=0;
			
			selectedFactionID=allUnitList[selectedUnitID].factionID;
			
			bool isUnitActive=allUnitList[selectedUnitID].NewTurn();	//in case unit is destroyed by damage over time effect
			if(isUnitActive){
				// Debug.LogWarning(allUnitList[selectedUnitID].unitName + " "  + allUnitList[selectedUnitID].canDrawCards);
				if (allUnitList[selectedUnitID].canDrawCards)
					Unit.DrawCard(allUnitList.Where(u => u.factionID == selectedFactionID).ToList());
			} else {
				_EndTurn_UnitPerTurn();
				return;
			}
			
			if(allUnitList[selectedUnitID].isAIUnit) AIManager.MoveUnit(allUnitList[selectedUnitID]);
			else GameControl.SelectUnit(allUnitList[selectedUnitID], false);
		}
		
		
		
		public static bool IsPlayerTurn(){
			return (instance.playerFactionIDList.Contains(instance.selectedFactionID)) ? true : false;
		}
		public static bool IsPlayerFaction(int factionID){
			return (instance.playerFactionIDList.Contains(factionID)) ? true : false;
		}
		
		
		//get the number of active unit on the grid
		public static int GetAllUnitCount(){ return GetAllUnit().Count; }
		public static List<Unit> GetAllUnit(){ return instance._GetAllUnit(); }
		public List<Unit> _GetAllUnit(){
			if(TurnControl.GetTurnMode()==_TurnMode.UnitPerTurn) return allUnitList;
			else{
				List<Unit> list=new List<Unit>();
				for(int i=0; i<factionList.Count; i++){
					for(int n=0; n<factionList[i].allUnitList.Count; n++) list.Add(factionList[i].allUnitList[n]);
				}
				return list;
			}
		}
		
		//get all units that is hostile based on a factionID (basically all units that has different factionID)
		public static List<Unit> GetAllHostileUnit(int factionID){ return instance._GetAllHostileUnit(factionID); }
		public List<Unit> _GetAllHostileUnit(int factionID){
			List<Unit> list=new List<Unit>();
			for(int i=0; i<factionList.Count; i++){
				if(factionID==factionList[i].ID) continue;
				for(int n=0; n<factionList[i].allUnitList.Count; n++)  list.Add(factionList[i].allUnitList[n]);
			}
			return list;
		}
		
		//get all units that belong to all player factions
		public static List<Unit> GetAllPlayerUnits(){ return instance._GetAllPlayerUnits(); }
		public List<Unit> _GetAllPlayerUnits(){
			List<Unit> list=new List<Unit>();
			for(int i=0; i<factionList.Count; i++){
				if(!factionList[i].isPlayerFaction) continue;
				for(int n=0; n<factionList[i].allUnitList.Count; n++)  list.Add(factionList[i].allUnitList[n]);
			}
			return list;
		}
		
		//get all unit that belong to a certain faction based on the factionID
		public static List<Unit> GetAllUnitsOfFaction(int factionID){ return instance._GetAllUnitsOfFaction(factionID); }
		public List<Unit> _GetAllUnitsOfFaction(int factionID){
			for(int i=0; i<factionList.Count; i++){
				if(factionID==factionList[i].ID) return factionList[i].allUnitList;
			}
			return new List<Unit>();
		}
		
		//get a certain faction based on factionID
		public static Faction GetFaction(int factionID){ return instance._GetFaction(factionID); }
		public Faction _GetFaction(int factionID){
			for(int i=0; i<factionList.Count; i++){
				if(factionList[i].ID==factionID) return factionList[i];
			}
			Debug.LogWarning("Faction with ID: "+factionID+" doesnt exist");
			return null;
		}
		
		//how many player's faction are there in the game
		public static int GetPlayerFactionCount(){ return instance.playerFactionIDList.Count; }
		
		
		
		public static void SelectUnit(){ instance._SelectUnit(); }
		public void _SelectUnit(){
			if(!GameControl.EnableCover()) return;
			
			for(int n=0; n<factionList.Count; n++){
				for(int i=0; i<factionList[n].allUnitList.Count; i++){
					factionList[n].allUnitList[i].UpdateCoverStatus();
				}
			}
		}
		
		
		
		//insert a unit to the grid in the middle of the game
		public static void InsertUnit(Unit unit, int factionID=0){ instance._InsertUnit(unit, factionID); }
		public void _InsertUnit(Unit unit, int factionID=0){ 
			unit.factionID=factionID;

			unit.InitUnitAbility();
			
			for(int i=0; i<factionList.Count; i++){
				if(factionList[i].ID==factionID){
					factionList[i].allUnitList.Add(unit);
					unit.isAIUnit=!factionList[i].isPlayerFaction;
				}
			}
			
			if(TurnControl.GetTurnMode()==_TurnMode.UnitPerTurn) allUnitList.Add(unit);
			
			_UnitTurnPriorityChanged( new List<int>{ unit.factionID} );
			
			unit.UpdateVisibility();
			
			//if(onInsertUnitE!=null) onInsertUnitE(unit);
			TBTK.OnNewUnit(unit);
		}
		
		
		
		
		
		
		
		
		//********************************************************************************************************************************
		//these section are related to starting unit deployment of the factions
		
		private bool requireDeployment=false;	//set to true when user need to manually deploy the starting units
		public static bool RequireManualUnitDeployment(){ return GameControl.EnableManualUnitDeployment() & instance.requireDeployment; }
		
		[HideInInspector] public int deployingFactionID=-1;		//Index of current faction which units are being deployed
		[HideInInspector] public int deployingUnitID=0;			//index of the unit currently being deployed in the startingUnitList of the faction
		public static int GetDeployingFactionID(){ return instance.deployingFactionID; }
		public static int GetDeployingUnitID(){ return instance.deployingUnitID; }
		public List<Unit> deployedUnitList=new List<Unit>();	//a list of unit store the deployed unit before the deployment is complete, 
																					//this is to keep track of which unit is deployed and which unit is not so the deployed unit can be remove from the grid to be deployed as well
																					//the list will be cleared as soon as the deployment for the faction is done
		
		public static void PrevDeployingUnitID(){ instance._PrevDeployingUnitID(); }	//rewind deployingUnitID
		public void _PrevDeployingUnitID(){
			deployingUnitID-=1;
			if(deployingUnitID<0) deployingUnitID=factionList[deployingFactionID].startingUnitList.Count-1;
		}
		public static void NextDeployingUnitID(){ instance._NextDeployingUnitID(); }	//next deployingUnitID
		public void _NextDeployingUnitID(){
			deployingUnitID+=1;
			if(deployingUnitID>=factionList[deployingFactionID].startingUnitList.Count) deployingUnitID=0;
		}
		public static void SetDeployingUnitID(int ID){ instance._SetDeployingUnitID(ID); }
		public void _SetDeployingUnitID(int ID){
			if(deployingUnitID<0 && deployingUnitID>=factionList[deployingFactionID].startingUnitList.Count) Debug.LogWarning("error", null);
			deployingUnitID=ID;
		}
		
		public static void DeployUnitOnTile(Tile tile){ instance._DeployUnitOnTile(tile); }
		public void _DeployUnitOnTile(Tile tile){
			if(tile.deployAreaID!=factionList[deployingFactionID].ID) return;
			if(factionList[deployingFactionID].startingUnitList.Count<=0) return;
			Unit unit=factionList[deployingFactionID].startingUnitList[deployingUnitID];
			deployedUnitList.Add(unit);
			tile.unit=unit;
			unit.tile=tile;
			
			unit.transform.position=tile.GetPos();
			unit.transform.rotation=Quaternion.Euler(0, factionList[deployingFactionID].spawnDirection, 0);
			
			unit.gameObject.SetActive(true);
			factionList[deployingFactionID].startingUnitList.RemoveAt(deployingUnitID);
			if(factionList[deployingFactionID].startingUnitList.Count<=0) deployingUnitID=0;
			else{
				if(deployingUnitID>=factionList[deployingFactionID].startingUnitList.Count) deployingUnitID-=1;
			}
			//if(onUnitDeployedE!=null) onUnitDeployedE(null);
			TBTK.OnUnitDeployed(null);
		}
		public static void UndeployUnit(Unit unit){ instance._UndeployUnit(unit); }
		public void _UndeployUnit(Unit unit){
			if(!deployedUnitList.Contains(unit)) return;
			deployedUnitList.Remove(unit);
			unit.tile.unit=null;
			unit.tile=null;
			unit.gameObject.SetActive(false);
			if(deployingUnitID==0) factionList[deployingFactionID].startingUnitList.Add(unit);
			else factionList[deployingFactionID].startingUnitList.Insert(deployingUnitID-1, unit);
			//if(onUnitDeployedE!=null) onUnitDeployedE(null);
			TBTK.OnUnitDeployed(null);
		}
		
		//called to complete unit deployment of a particular faction
		//move on to next faction needs unit deployment
		public static void CompleteDeployment(){ instance._CompleteDeployment(); }	
		public void _CompleteDeployment(){
			if(!_IsDeploymentComplete()) return;
			
			for(int i=0; i<deployedUnitList.Count; i++){
				deployedUnitList[i].factionID=factionList[deployingFactionID].ID;
				factionList[deployingFactionID].allUnitList.Add(deployedUnitList[i]);
			}
			
			deployedUnitList=new List<Unit>();
			deployingUnitID=-1;	
			
			GridManager.FactionDeploymentComplete();
			
			//iterate thru all faction, get the next faction needs unit deployment
			for(int i=deployingFactionID+1; i<factionList.Count; i++){
				if(factionList[i].startingUnitList.Count>0){
					if(factionList[i].isPlayerFaction){
						deployingFactionID=i;
						deployingUnitID=0;	//set to >=0 to indicate there's a player faction needs deployment
						break;
					}
					else{	//if it's an AI faction, automatically deploy it
						AutoDeployFaction(i);
						
						for(int n=0; n<deployedUnitList.Count; n++){
							//deployedUnitList[n].factionID=factionList[i].ID;
							factionList[i].allUnitList.Add(deployedUnitList[n]);
						}
					}
				}
			}
			
			if(deployingUnitID==-1){	//no more faction needs deloyment, start the game
				GameControl.StartGame();
				//if(onUnitDeploymentPhaseE!=null) onUnitDeploymentPhaseE(false);
				TBTK.OnUnitDeployment(false);
			}
			else{							//another player's faction needs deloyment, initiate the process
				GridManager.DeployingFaction(deployingFactionID);
				//if(onUnitDeploymentPhaseE!=null) onUnitDeploymentPhaseE(true);
				TBTK.OnUnitDeployment(true);
			}
		}
		
		//automatically deployed the unit for the current deploying faction, used for both AI and player faction
		public static void AutoDeployCurrentFaction(){ instance.AutoDeployFaction(); }
		public void AutoDeployFaction(int factionID=-1){
			if(factionID==-1) factionID=deployingFactionID;
			
			bool setToInvisible=GameControl.EnableFogOfWar() & !factionList[factionID].isPlayerFaction;
			
			List<Unit> unitList=factionList[factionID].startingUnitList;
			List<Tile> tileList=GridManager.GetDeployableTileList(factionList[factionID].ID);
			
			for(int i=0; i<tileList.Count; i++){
				if(!tileList[i].walkable || tileList[i].unit!=null || tileList[i].obstacleT!=null){ 
					tileList.RemoveAt(i);		i-=1;
				}
			}
			
			int count=0;
			for(int i=0; i<unitList.Count; i++){
				if(tileList.Count==0) break;
				Unit unit=unitList[i];
				
				int rand=Random.Range(0, tileList.Count);
				Tile tile=tileList[rand];
				tileList.RemoveAt(rand);
				tile.unit=unit;
				unit.tile=tile;
				unit.transform.position=tile.GetPos();
				unit.transform.rotation = Quaternion.Euler(0, factionList[factionID].spawnDirection, 0);
				unit.gameObject.SetActive(true);
				
				deployedUnitList.Add(unit);
				
				if(setToInvisible && !tile.IsVisible()){
					unit.gameObject.layer=TBTK.GetLayerUnitInvisible();
					Utilities.SetLayerRecursively(unit.transform, TBTK.GetLayerUnitInvisible());
				}
				
				count+=1;
				
				unit.factionID=factionList[factionID].ID;
			}
			
			for(int i=0; i<count; i++) unitList.RemoveAt(0);
			
			TBTK.OnUnitDeployed(null);
		}
		
		//check if a faction has deployed all its startingUnitList,
		//the deployment is considered complete when eitehr the startingUnitList is empty or there are no more tile available
		public static bool IsDeploymentComplete(){ return instance._IsDeploymentComplete(); }
		public bool _IsDeploymentComplete(){
			bool flag1=factionList[deployingFactionID].startingUnitList.Count>0;
			bool flag2=GridManager.GetDeployableTileListCount()>0;
			if(flag1 && flag2) return false;
			return true;
		}
		
		public static List<Unit> GetDeployingUnitList(){
			return instance.factionList[instance.deployingFactionID].startingUnitList;
		}
		
		//end faction deployment mode related function
		//**************************************************************************************
		
		
		
		
		
		public static void GenerateUnit(){ instance._GenerateUnit(); }
		public void _GenerateUnit(){
			for(int i=0; i<factionList.Count; i++) factionList[i].Spawn();
		}
		public void ClearUnit(){
			for(int i=0; i<factionList.Count; i++) factionList[i].ClearUnit();
		}
		
		//use in edit mode only
		public void RecordSpawnTilePos(){
			for(int i=0; i<factionList.Count; i++) factionList[i].RecordSpawnTilePos();
		}
		//use in edit mode only
		public void SetStartingTileListBaseOnPos(float tileSize=1){
			for(int i=0; i<factionList.Count; i++) factionList[i].SetStartingTileListBaseOnPos(tileSize);
		}
		
		
		
		private float gizmoSize1=0.25f;
		private float gizmoSize2=0.35f;
		
		void OnDrawGizmos(){
			for(int i=0; i<factionList.Count; i++){
				
				Faction fac=factionList[i];
				Gizmos.color=fac.color;
				
				for(int n=0; n<fac.spawnInfoList.Count; n++){
					for(int m=0; m<fac.spawnInfoList[n].startingTileList.Count; m++){
						if(fac.spawnInfoList[n].startingTileList[m]==null) continue;
						Vector3 pos=fac.spawnInfoList[n].startingTileList[m].GetPos();
						Gizmos.DrawSphere(pos, gizmoSize1);
						Gizmos.DrawLine(pos, pos+Quaternion.Euler(0, fac.spawnInfoList[n].spawnDirection, 0)*Vector3.forward*gizmoSize2*2);
					}
				}
				
				for(int n=0; n<fac.deployableTileList.Count; n++){
					if(fac.deployableTileList[n]==null) continue;
					Vector3 pos=fac.deployableTileList[n].GetPos();
					Gizmos.DrawLine(pos+new Vector3(gizmoSize2, 0, gizmoSize2), pos+new Vector3(-gizmoSize2, 0, -gizmoSize2));
					Gizmos.DrawLine(pos+new Vector3(-gizmoSize2, 0, gizmoSize2), pos+new Vector3(gizmoSize2, 0, -gizmoSize2));
				}
			}
		}
		
	}

}