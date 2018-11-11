using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class TBTK : MonoBehaviour {
		
		public static int GetLayerUnit(){ return 31; }
		//public static int GetLayerUnitAI(){ return layerUnit; }
		public static int GetLayerUnitInvisible(){ return 29; }
		
		public static int GetLayerTile(){ return 28; }
		public static int GetLayerObstacleHalfCover(){ return 27; }
		public static int GetLayerObstacleFullCover(){ return 26; }
		public static int GetLayerTerrain(){ return 25; }
		
		public static int LayerUI(){ return 5; }	//layer5 is named UI by Unity's default
		
		
		
		
		public delegate void GameMessageHandler(string msg); 
		public static event GameMessageHandler onGameMessageE;
		public static void OnGameMessage(string msg){ if(onGameMessageE!=null) onGameMessageE(msg); }
		
		public delegate void OverlayTextHandler(string msg); 
		public static event OverlayTextHandler onOverlayTextE;
		public static void OnOverlayText(string msg){ if(onOverlayTextE!=null) onOverlayTextE(msg); }
		
		
		//from GameControl
		public delegate void UnitSelectedHandler(Unit unit);
		public static event UnitSelectedHandler onUnitSelectedE;								
		public static void OnUnitSelected(Unit unit){ if(onUnitSelectedE!=null) onUnitSelectedE(unit); }
		
		public delegate void GameStartHandler();
		public static event GameStartHandler onGameStartE;								
		public static void OnGameStart(){ if(onGameStartE!=null) onGameStartE(); }	//not in used, default UI uses onNewTurn
		
		public delegate void GameOverHandler(int winningFactionID);
		public static event GameOverHandler onGameOverE;								
		public static void OnGameOver(int winningFactionID){ if(onGameOverE!=null) onGameOverE(winningFactionID); }
		
		
		//From GridManager
		public delegate void HoverWalkableHandler(Tile tile);
		public static event HoverWalkableHandler onHoverWalkableTileE;								
		public static void OnHoverWalkableTile(Tile tile){ if(onHoverWalkableTileE!=null) onHoverWalkableTileE(tile); }
		
		public delegate void HoverTileHandler(Tile tile);
		public static event HoverTileHandler onHoverTileE;								
		public static void OnHoverTile(Tile tile){ if(onHoverTileE!=null) onHoverTileE(tile); }
		
		
		//from TurnControl
		public delegate void GameInActionHandler(bool flag);
		public static event GameInActionHandler onGameInActionE;								
		public static void OnGameInAction(bool flag){ if(onGameInActionE!=null) onGameInActionE(flag); }
		
		public delegate void NewTurnHandler(bool isPlayer);
		public static event NewTurnHandler onNewTurnE;								
		public static void OnNewTurn(bool isPlayer){ if(onNewTurnE!=null) onNewTurnE(isPlayer); }
		
		public delegate void AllUnitOutOfMoveHandler();
		public static event AllUnitOutOfMoveHandler onAllUnitOutOfMoveE;								
		public static void OnAllUnitOutOfMove(){ if(onAllUnitOutOfMoveE!=null) onAllUnitOutOfMoveE(); }
		
		
		//from FactionManager
		public delegate void UnitDeploymentHandler(bool flag);	//enter or exit unit deployment
		public static event UnitDeploymentHandler onUnitDeploymentE;								
		public static void OnUnitDeployment(bool flag){ if(onUnitDeploymentE!=null) onUnitDeploymentE(flag); }
		
		public delegate void UnitDeployedHandler(Unit unit);		//when a unit has been deployed/undeployed
		public static event UnitDeployedHandler onUnitDeployedE;								
		public static void OnUnitDeployed(Unit unit){ if(onUnitDeployedE!=null) onUnitDeployedE(unit); }
		
		public delegate void NewUnitHandler(Unit unit);	//when a new unit has been added to the grid
		public static event NewUnitHandler onNewUnitE;								
		public static void OnNewUnit(Unit unit){ if(onNewUnitE!=null) onNewUnitE(unit); }
		
		public delegate void FactionDestroyedHandler(int facID);	//when a new unit has been added to the grid
		public static event FactionDestroyedHandler onFactionDestroyedE;								
		public static void OnFactionDestroyed(int facID){ if(onFactionDestroyedE!=null) onFactionDestroyedE(facID); }
		
		
		//from AbilityManager
		public delegate void AbilityTargetModeHandler(bool flag);
		public static event AbilityTargetModeHandler onAbilityTargetModeE;								
		public static void OnAbilityTargetMode(bool flag){ if(onAbilityTargetModeE!=null) onAbilityTargetModeE(flag); }
		
		public delegate void AbilityActivatedHandler();
		public static event AbilityActivatedHandler onAbilityActivatedE;								
		public static void OnAbilityActivated(){ if(onAbilityActivatedE!=null) onAbilityActivatedE(); }
		
		public delegate void ABTargetModeHandler(int index);
		public static event ABTargetModeHandler onUnitABTargetModeE;								
		public static event ABTargetModeHandler onFactionABTargetModeE;								
		public static void OnUnitABTargetMode(int index=-1){ if(onUnitABTargetModeE!=null) onUnitABTargetModeE(index); }
		public static void OnFactionABTargetMode(int index=-1){ if(onFactionABTargetModeE!=null) onFactionABTargetModeE(index); }
		
		public delegate void NewAbilityHandler(Ability ability);
		public static event NewAbilityHandler onNewFactionAbilityE;								
		public static event NewAbilityHandler onNewUnitAbilityE;								
		public static void OnNewFactionAbility(Ability ability){ if(onNewFactionAbilityE!=null) onNewFactionAbilityE(ability); }
		public static void OnNewUnitAbility(Ability ability){ if(onNewUnitAbilityE!=null) onNewUnitAbilityE(ability); }
		
		
		//from Unit
		public delegate void UnitDestroyedHandler(Unit unit);
		public static event UnitDestroyedHandler onUnitDestroyedE;								
		public static event UnitDestroyedHandler onAIUnitDestroyedE;								
		public static event UnitDestroyedHandler onPlayerUnitDestroyedE;								
		public static void OnAIUnitDestroyed(Unit unit){ 
			if(onUnitDestroyedE!=null) onUnitDestroyedE(unit);
			if(onAIUnitDestroyedE!=null) onAIUnitDestroyedE(unit);
		}
		public static void OnPlayerUnitDestroyed(Unit unit){ 
			if(onUnitDestroyedE!=null) onUnitDestroyedE(unit);
			if(onPlayerUnitDestroyedE!=null) onPlayerUnitDestroyedE(unit);
		}
		
		
		//from PerkManager
		public delegate void PerkPointHandler(int value);
		public static event PerkPointHandler onPerkPointE;								
		public static void OnPerkPoint(int value){ if(onPerkPointE!=null) onPerkPointE(value); }
		
		public delegate void PerkCurrencyHandler(int value);
		public static event PerkCurrencyHandler onPerkCurrencyE;								
		public static void OnPerkCurrency(int value){ if(onPerkCurrencyE!=null) onPerkCurrencyE(value); }
		
		public delegate void PerkPurchasedHandler(Perk perk);
		public static event PerkPurchasedHandler onPerkPurchasedE;								
		public static void OnPerkPurchased(Perk perk){ if(onPerkPurchasedE!=null) onPerkPurchasedE(perk); }
		
		
		
		
		
		
		
		//inputID=-1 - mouse cursor, 	inputID>=0 - touch finger index
		public static bool IsCursorOnUI(int inputID=-1){
			EventSystem eventSystem = EventSystem.current;
			return ( eventSystem.IsPointerOverGameObject( inputID ) );
		}
		
		public static Vector3 GetFirstTouchPosition(){
			if(Input.touchCount==1) return Input.touches[0].position;
			return new Vector3(0, -50, 0);
		}
		
		public static bool OnTouchUp(){
			if(Input.touchCount==1) return Input.touches[0].phase==TouchPhase.Ended;
			return false;
		}
		
	}

}