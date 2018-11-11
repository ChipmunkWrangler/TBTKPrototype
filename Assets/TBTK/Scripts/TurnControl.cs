using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{

	public enum _TurnMode{
		FactionPerTurn, 				//each faction take turn to move all units in each round
		FactionUnitPerTurn,			//each faction take turn to move a single unit in each round
		
		//doesnt use move order
		UnitPerTurn,					//all units (regardless of faction) take turn to move according to the stats, when all unit is moves, the round is completed
	}

	public enum _MoveOrder{
		Free, 				//unit switching is enabled
		Random, 		//random fix an order and follow the order throughout
		StatsBased	//arrange the order based on unit's stats
	}
	
	public class TurnControl : MonoBehaviour{
		
		[HideInInspector] public _TurnMode turnMode;
		public static _TurnMode GetTurnMode(){ return instance.turnMode; }
		
		[HideInInspector] public _MoveOrder moveOrder;
		public static _MoveOrder GetMoveOrder(){ return instance.moveOrder; }
		
		
		//this is the flag/counter indicate how many action are on-going, no new action should be able to start as long as this is not clear(>0)
		private static int actionInProgress=0;
		
		private int currentTurnID=-1;	//indicate how many turn has passed, not in used
		
		private bool moved=false;	//to check if player has make a move on the turn, used to lock unit switching in FactionUnitPerTurn-FreeMoveOrder mode
		public static void CheckPlayerMoveFlag(bool flag=true){ instance.moved=flag; }
		public static bool HasMoved(){ return instance.moved; }
		
		public static TurnControl instance;
		
		void Awake(){
			if(instance==null) instance=this;
		}
		
		public void Init(){
			if(instance==null) instance=this;
			
			actionInProgress=0;
			
			currentTurnID=-1;
			
			if(turnMode==_TurnMode.UnitPerTurn) moveOrder=_MoveOrder.StatsBased;
		}
		
		
		
		//call by unit when all action is depleted
		public static void SelectedUnitMoveDepleted(){
			if(GameControl.GetGamePhase()==_GamePhase.Over) return;
			
			if(instance.turnMode==_TurnMode.FactionPerTurn){
				//if not in free move order, cant switch to next unit without end turn
				if(instance.moveOrder!=_MoveOrder.Free){
					TBTK.OnAllUnitOutOfMove();
					if(GameControl.GetSelectedUnit().HP<=0) instance.StartCoroutine(instance.AutoEndTurn());
				}
				else{
					if(!FactionManager.SelectNextUnitInFaction_Free()) TBTK.OnAllUnitOutOfMove();
				}
			}
			else if(instance.turnMode==_TurnMode.FactionUnitPerTurn){
				TBTK.OnAllUnitOutOfMove();
				if(GameControl.GetSelectedUnit().HP<=0) instance.StartCoroutine(instance.AutoEndTurn());
			}
			else if(instance.turnMode==_TurnMode.UnitPerTurn){
				TBTK.OnAllUnitOutOfMove();
				if(GameControl.GetSelectedUnit().HP<=0) instance.StartCoroutine(instance.AutoEndTurn());
			}
		}
		
		
		//for when selected unit is destroyed during counter attack
		public IEnumerator AutoEndTurn(){
			while(!ClearToProceed()) yield return null;
			yield return new WaitForSeconds(0.5f);
			if(GameControl.GetGamePhase()!=_GamePhase.Over) GameControl.EndTurn();
		}
		
		
		
		public static void StartGame(){ instance._StartGame(); }
		public void _StartGame(){
			if(turnMode==_TurnMode.FactionPerTurn && moveOrder!=_MoveOrder.Free){
				FactionManager.EndTurn_FactionPerTurn();
				TBTK.OnNewTurn(IsPlayerTurn());
			}
			else EndTurn();
		}
		
		//called in GameControl when endTurn button is pressed, move the turn forward
		//also used when the game first started
		public static void EndTurn(){ instance.StartCoroutine(instance._EndTurn()); }
		public IEnumerator _EndTurn(){
			if(GameControl.GetGamePhase()==_GamePhase.Over) yield break;
			
			yield return new WaitForSeconds(0.2f);
			
			currentTurnID+=1;
			
			if(turnMode==_TurnMode.FactionPerTurn){
				if(moveOrder==_MoveOrder.Free) FactionManager.EndTurn_FactionPerTurn();
				else{
					if(FactionManager.SelectNextUnitInFaction_NotFree()){
						TBTK.OnNewTurn(IsPlayerTurn());
						yield break;
					}
					else FactionManager.EndTurn_FactionPerTurn();
				}
			}
			else if(turnMode==_TurnMode.FactionUnitPerTurn){
				CheckPlayerMoveFlag(false);
				FactionManager.EndTurn_FactionUnitPerTurn();
			}
			else if(turnMode==_TurnMode.UnitPerTurn){
				FactionManager.EndTurn_UnitPerTurn();
			}
			
			IterateEndTurn();
			
			TBTK.OnNewTurn(IsPlayerTurn());
		}
		
		
		
		public void IterateEndTurn(){
			List<Faction> facList=FactionManager.GetFactionList();
			
			for(int i=0; i<facList.Count; i++){
				Faction fac=facList[i];
				
				for(int n=0; n<fac.abilityInfo.abilityList.Count; n++) fac.abilityInfo.abilityList[n].currentCD.Iterate();
				
				for(int n=0; n<fac.allUnitList.Count; n++){
					Unit unit=fac.allUnitList[n];
					
					for(int m=0; m<unit.abilityList.Count; m++) unit.abilityList[m].currentCD.Iterate();
				}
			}
			
			EffectTracker.IterateEffectDuration();
		}
		
		
		public static void OnFactionDestroyed(){
			if(TurnControl.GetTurnMode()==_TurnMode.UnitPerTurn) return;
			instance.IterateEndTurn();
		}
		public static void OnUnitDestroyed(){
			if(TurnControl.GetTurnMode()!=_TurnMode.UnitPerTurn) return;
			instance.IterateEndTurn();
		}
		
		
		
		
		public static bool IsPlayerTurn(){
			return FactionManager.IsPlayerTurn();
		}
		
		
		
		//called by all to check if a new action can take place (shoot, move, ability, etc)
		public static bool ClearToProceed(){
			return (actionInProgress==0) ? true : false;
		}
		public static bool ClearToProceedFromOverWatch(){
			return (actionInProgress<=1) ? true : false;
		}
		
		
		//called to indicate that an action has been started, prevent any other action from starting
		public static void ActionCommenced(){
			if(actionInProgress==0) TBTK.OnGameInAction(true);
			actionInProgress+=1;
		}
		
		
		//called to indicate that an action has been completed
		public static void ActionCompleted(float delay=0){ instance.StartCoroutine(instance._ActionCompleted(delay)); }
		IEnumerator _ActionCompleted(float delay=0){
			if(delay>0) yield return new WaitForSeconds(delay);
			actionInProgress=Mathf.Max(0, actionInProgress-=1);
			//yield return null;
			
			if(actionInProgress==0) TBTK.OnGameInAction(false);
		}
		
		
		
		
	}

}
