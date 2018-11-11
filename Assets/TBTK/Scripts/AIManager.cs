using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	public enum _AIMode{
		Passive, 	//the unit wont move unless the there are hostile within the faction's sight (using unit sight value even when Fog-Of-War is not used)
		Trigger, 		//the unit wont move unless it's being triggered, when it spotted any hostile or attacked
		Aggressive,	//the unit will be on move all the time, looking for potential target
	}
	
	public class AIManager : MonoBehaviour {
		
		public _AIMode mode=_AIMode.Passive;
		public static _AIMode GetAIMode(){ return instance.mode; }
		
		public float unitInterval=0.1f;
		
		public bool untriggeredUnitMove=false;
		
		[Space(10)] public bool debugMode=false;
		
		private static AIManager instance;
		
		void Awake(){
			instance=this;
		}
		
		//called in FactionManager to move the whole faction
		public static void MoveFaction(Faction faction){ instance._MoveFaction(faction); }
		public void _MoveFaction(Faction faction){
			StartCoroutine(FactionRoutine(faction));
		}
		
		//called in GameControl when a AI unit is selected to move that particular unit
		public static void MoveUnit(Unit unit){ instance._MoveUnit(unit); }
		public void _MoveUnit(Unit unit){
			StartCoroutine(SingleUnitRoutine(unit));
		}
		
		public void AIDebug(string msg){
			if(debugMode) Debug.Log("AI Debug - "+msg);
		}
		
		//move the whole faction, unit by unit
		IEnumerator FactionRoutine(Faction faction){
			GameControl.DisplayMessage("AI's Turn");
			yield return new WaitForSeconds(0.3f);
			
			//create a new list so no unit will be skipped is one of them is somehow destroyed (by counter attack);
			List<Unit> unitList=new List<Unit>( faction.allUnitList );	
			
			for(int i=0; i<unitList.Count; i++){
				AIDebug("Start moving unit "+unitList[i].gameObject.name);
				
				yield return new WaitForSeconds(unitInterval);
				
				if(unitList[i].IsStunned()) continue;
				
				_AIMode activeAIMode=!faction.useDefaultAIMode ? faction.aiMode : mode ;
				
				while(unitList[i].moveRemain>0 || unitList[i].CanAttack()){
					AIDebug("Moving unit "+unitList[i].gameObject.name+"     (move remain:"+(unitList[i].moveRemain>0)+", can attack:"+unitList[i].CanAttack()+")");
					StartCoroutine(MoveUnitRoutine(unitList[i], activeAIMode));
					while(movingUnit) yield return null;
					if(unitList[i]==null) break;
					if(unitList[i].moveRemain>0 || unitList[i].CanAttack()) yield return new WaitForSeconds(.1f);
				}
				
				//~ StartCoroutine(MoveUnitRoutine(unitList[i], activeAIMode));
				//~ while(movingUnit) yield return null;
				//~ yield return new WaitForSeconds(0.25f);
				
				if(GameControl.GetGamePhase()==_GamePhase.Over) yield break;
			}
			
			GameControl.EndTurn();
		}
		//move a single unit only
		IEnumerator SingleUnitRoutine(Unit unit){
			GameControl.DisplayMessage("AI's Turn");
			
			yield return new WaitForSeconds(0.1f);
			
			if(!unit.IsStunned()){
				Faction faction=FactionManager.GetFaction(unit.factionID);
				_AIMode activeAIMode=!faction.useDefaultAIMode ? faction.aiMode : mode ;
				
				while(unit.moveRemain>0 || unit.CanAttack()){
					StartCoroutine(MoveUnitRoutine(unit, activeAIMode));
					while(movingUnit) yield return null;
					if(unit==null) break;
					if(unit.attackRemain>0 || unit.CanAttack()) yield return new WaitForSeconds(.1f);
				}
				
				//~ StartCoroutine(MoveUnitRoutine(unit, activeAIMode));
				//~ while(movingUnit) yield return null;
				//~ yield return new WaitForSeconds(0.25f);
			}
			
			GameControl.EndTurn();
		}
		
		
		IEnumerator DelayEndTurn(){
			yield return new WaitForSeconds(1f);
			while(!TurnControl.ClearToProceed()) yield return null;
			GameControl.EndTurn();
		}
		
		
		
		
		private bool movingUnit=false;		//set to true when a unit is being moved
		IEnumerator MoveUnitRoutine(Unit unit, _AIMode activeMode){
			TBTK.OnUnitSelected(unit);
			
			movingUnit=true;
			//Debug.Log("moving unit");
			if(activeMode!=_AIMode.Aggressive && !unit.trigger){
				AIDebug("unit "+unit.gameObject.name+" is not triggered");
				if(!untriggeredUnitMove){
					StartCoroutine(EndMoveUnitRoutine());
					unit.moveRemain=0;	unit.attackRemain=0;
				}
				else{
					if(Random.value<0.5f){
						StartCoroutine(EndMoveUnitRoutine());
						unit.moveRemain=0;	unit.attackRemain=0;
					}
					else{
						AIDebug("Randomly move unit "+unit.gameObject.name+" anyway");
						List<Tile> walkableTilesInRange=GridManager.GetTilesWithinDistance(unit.tile, Mathf.Min(1, unit.GetEffectiveMoveRange()/2), true);
						if(walkableTilesInRange.Count>0) unit.Move(walkableTilesInRange[Random.Range(0, walkableTilesInRange.Count)]);
					}
				}
				AIDebug("End unit "+unit.gameObject.name+" turn");
				StartCoroutine(EndMoveUnitRoutine());
				yield break;
			}
			
			Tile targetTile=Analyse(unit, activeMode);
			
			
			if(CameraControl.CenterOnSelectedUnit()){
				bool visible=false;
				if(unit.tile.IsVisible()) visible=true;
				else if(targetTile!=unit.tile){
					List<Tile> path=unit.GetPathForAI(targetTile);
					for(int i=0; i<path.Count; i++){
						if(path[i].IsVisible()){
							visible=true;
							break;
						}
					}
					targetTile=path[path.Count-1];
					Debug.DrawLine(unit.tile.GetPos(), targetTile.GetPos(), Color.red, 2);
				}
				
				if(visible){
					CameraControl.OnUnitSelected(unit, false);
					while(CameraControl.IsLerping()) yield return null;
				}
			}
			
			
			//first move to the targetTile
			if(targetTile!=unit.tile){
				unit.Move(targetTile);
				yield return new WaitForSeconds(.1f);	//wait until the unit has moved into the targetTile
				while(!TurnControl.ClearToProceed()){
					//AIDebug("waiting, unit "+unit.gameObject.name+" is moving");
					AIDebug("waiting while unit is moving");
					yield return null;
				}
			}
			
			if(unit==null || unit.HP<=0){	//in case unit is destroyed by overwatch
				StartCoroutine(EndMoveUnitRoutine());
				yield break;
			}
			
			for(int i=0; i<targetTile.hostileInRangeList.Count; i++){
				if(targetTile.hostileInRangeList[i].unit==null || targetTile.hostileInRangeList[i].unit.factionID==unit.factionID){
					targetTile.hostileInRangeList.RemoveAt(i);		i-=1;
				}
			}
			
			//if there's hostile within range, attack it
			if(targetTile.hostileInRangeList.Count>0){
				if(unit.CanAttack()){
					AIDebug("waiting, unit "+unit.gameObject.name+" is attacking");
					
					//~ if(targetTile!=unit.tile){	//wait until the unit has moved into the targetTile
						//~ yield return new WaitForSeconds(.25f);
						//~ while(!TurnControl.ClearToProceed()) yield return null;
					//~ }
					
					int rand=Random.Range(0, targetTile.hostileInRangeList.Count);
					unit.Attack(targetTile.hostileInRangeList[rand].unit);
				}
				else{
					if(unit.moveRemain>0) unit.moveRemain-=1;
				}
			}
			else{
				if(unit.moveRemain<=0) unit.attackRemain=0;
			}
			
			AIDebug("End unit "+unit.gameObject.name+" turn");
			StartCoroutine(EndMoveUnitRoutine());
			
			yield return null;
		}
		
		//clear movingUnit flag so the next unit can be moved
		IEnumerator EndMoveUnitRoutine(){
			while(!TurnControl.ClearToProceed()){
				AIDebug("Waiting for all sequence to complete");
				yield return null;
			}
			yield return null;
			movingUnit=false;
		}
		
		
		//analyse the grid to know where the unit should move to
		private Tile Analyse(Unit unit, _AIMode activeMode){
			
			//get all wakable tiles in range first
			List<Tile> walkableTilesInRange=GridManager.GetTilesWithinDistance(unit.tile, unit.GetEffectiveMoveRange(), true);
			walkableTilesInRange.Add(unit.tile);
			
			//get all visible hostile
			List<Unit> allHostileInSight=FactionManager.GetAllHostileUnit(unit.factionID);
			if(GameControl.EnableFogOfWar()){
				for(int i=0; i<allHostileInSight.Count; i++){
					if(!FogOfWar.IsTileVisibleToFaction(allHostileInSight[i].tile, unit.factionID)){
						allHostileInSight.RemoveAt(i);	i-=1;
					}
				}
			}
			
			//if cover system is in used
			if(GameControl.EnableCover()){
				Tile tile=AnalyseCoverSystem(unit, walkableTilesInRange, allHostileInSight);
				if(tile!=null) return tile;
			}
			
			
			//if there are hostile
			if(allHostileInSight.Count>0){
				//fill up the walkableTilesInRange hostile list 
				//then filter thru walkableTilesInRange, those that have a hostile in range will be add to a tilesWithHostileInRange
				List<Tile> tilesWithHostileInRange=new List<Tile>();
				GridManager.SetupHostileInRangeforTile(unit, walkableTilesInRange);
				for(int i=0; i<walkableTilesInRange.Count; i++){
					if(walkableTilesInRange[i].GetHostileInRange().Count>0) tilesWithHostileInRange.Add(walkableTilesInRange[i]);
				}
				
				//if the tilesWithHostileInRange is not empty after the process, means there's tiles which the unit can move into and attack
				//return one of those in the tilesWithHostileInRange so the unit can attack
				if(tilesWithHostileInRange.Count>0){
					//if the unit current tile is one of those tiles with hostile, just stay put and attack
					if(tilesWithHostileInRange.Contains(unit.tile)){
						//randomize it a bit so the unit do move around but not stay in place all the time
						if(Random.Range(0f, 1f)>0.25f) return unit.tile;
					}
					return tilesWithHostileInRange[Random.Range(0, tilesWithHostileInRange.Count)];
				}
			}
			
			
			//if there's not potential target at all, check if the unit has any previous attacker
			//if there are, go after the last attacker
			if(unit.lastAttacker!=null) return unit.lastAttacker.tile;
			
			
			//for aggresive mode with FogOfWar disabled, try move towards the nearest unit
			if(activeMode==_AIMode.Aggressive && Random.Range(0f, 1f)>0.25f){
				List<Unit> allHostile=FactionManager.GetAllHostileUnit(unit.factionID);
				float nearest=Mathf.Infinity;	int nearestIndex=0;
				for(int i=0; i<allHostile.Count; i++){
					float dist=GridManager.GetDistance(allHostile[i].tile, unit.tile);
					if(dist<nearest){
						nearest=dist;
						nearestIndex=i;
					}
				}
				return allHostile[nearestIndex].tile;
			}
			
			
			//if there's really no hostile to go after, then just move randomly in one of the walkable
			int rand=Random.Range(0, walkableTilesInRange.Count);
			//clear in hostileInRange list for all moveable tile so, just in case the list is not empty (hostileInRange dont clear after each move)
			//so the unit dont try to attack anything when it moves into the targetTile
			walkableTilesInRange[rand].SetHostileInRange(new List<Tile>());
			
			return walkableTilesInRange[rand];
		}
		
		
		private Tile AnalyseCoverSystem(Unit unit, List<Tile> walkableTilesInRange, List<Unit> allHostileInSight){
			List<Tile> walkableTilesInRangeAlt=new List<Tile>();	//for cover system, secondary tiles with less cover
			
			List<Tile> halfCoveredList=new List<Tile>();	//a list for all the tiles with half Cover
			List<Tile> fullCoveredList=new List<Tile>();	//a list for all the tiles with full Cover
			
			if(allHostileInSight.Count==0) fullCoveredList=walkableTilesInRange;
			else{	//if there are hostile in sight
				//loop through all the walkable, record their score based on
				for(int i=0; i<walkableTilesInRange.Count; i++){
					Tile tile=walkableTilesInRange[i];
					tile.hostileCount=0;
					tile.coverScore=0;
					
					//iterate through all hostile, add the count, and cover type to the tile, this will then be used in tile.GetCoverRating() when this loop is complete
					for(int n=0; n<allHostileInSight.Count; n++){
						// if the hostile is out of range, ignore it
						int hostileRange=allHostileInSight[n].GetMoveRange()+allHostileInSight[n].GetAttackRange();
						if(GridManager.GetDistance(allHostileInSight[n].tile, tile)>hostileRange) continue;
						
						tile.hostileCount+=1;
						
						CoverSystem._CoverType coverType=CoverSystem.GetCoverType(allHostileInSight[n].tile, walkableTilesInRange[i]);
						if(coverType==CoverSystem._CoverType.Half) tile.coverScore+=1;
						else if(coverType==CoverSystem._CoverType.Full) tile.coverScore+=2;
					}
					
					//get cover rating for the tile
					//if score is >=2, the tile has full cover from all hostile, so add it to fullCoveredList
					//if score is >=1 && <2, the tile has half cover from all hostile, so add it to halfCoveredList
					//if anything <1, the tile is exposed to hostile in some manner
					if(tile.GetCoverRating()>=2) fullCoveredList.Add(tile);
					else if(tile.GetCoverRating()>=1) halfCoveredList.Add(tile);
				}
			}
			
			//if either of the CoveredList is not empty, replace walkableTilesInRange with that since there's no need to consider to move into tiles without cover
			if(fullCoveredList.Count!=0){
				walkableTilesInRange=fullCoveredList;
				walkableTilesInRangeAlt=halfCoveredList;
			}
			else if(halfCoveredList.Count!=0) walkableTilesInRange=halfCoveredList;
			
			//if there are hostile
			if(allHostileInSight.Count>0){
				//fill up the walkableTilesInRange hostile list 
				//then filter thru walkableTilesInRange, those that have a hostile in range will be add to a tilesWithHostileInRange
				List<Tile> tilesWithHostileInRange=new List<Tile>();
				GridManager.SetupHostileInRangeforTile(unit, walkableTilesInRange);
				for(int i=0; i<walkableTilesInRange.Count; i++){
					if(walkableTilesInRange[i].GetHostileInRange().Count>0) tilesWithHostileInRange.Add(walkableTilesInRange[i]);
				}
				
				if(tilesWithHostileInRange.Count==0){
					GridManager.SetupHostileInRangeforTile(unit, walkableTilesInRangeAlt);
					for(int i=0; i<walkableTilesInRangeAlt.Count; i++){
						if(walkableTilesInRangeAlt[i].GetHostileInRange().Count>0) tilesWithHostileInRange.Add(walkableTilesInRangeAlt[i]);
					}
				}
				
				//if the tilesWithHostileInRange is not empty after the process, means there's tiles which the unit can move into and attack
				//return one of those in the tilesWithHostileInRange so the unit can attack
				if(tilesWithHostileInRange.Count>0){
					//if the unit current tile is one of those tiles with hostile, just stay put and attack
					if(tilesWithHostileInRange.Contains(unit.tile)){
						//randomize it a bit so the unit do move around but not stay in place all the time
						if(Random.Range(0f, 1f)>0.25f) return unit.tile;
					}
					return tilesWithHostileInRange[Random.Range(0, tilesWithHostileInRange.Count)];
				}
			}
			
			return null;
		}
		
	}
	
}

