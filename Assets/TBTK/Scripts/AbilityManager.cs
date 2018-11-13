using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{

	public class AbilityManager : MonoBehaviour {
		
		private static AbilityManager instance;
		
		public void Awake(){
			instance=this;
		}
		
		void Update(){
			if(!targetMode) return;
			
			if(Input.touchCount>0){
				if(Input.touchCount>=2) ExitAbilityTargetMode();
			}
			else{
				if(Input.GetMouseButtonDown(1)) ExitAbilityTargetMode();
			}
		}
		
		
		
		private static bool targetMode=false;
		public static bool InTargetMode(){ return targetMode; }
		
		public delegate void TargetModeCallBack(Tile tile, int abIndex);
		private TargetModeCallBack targetSelectedCallBack;
		
		public delegate void ExitTargetModeCallBack();
		private ExitTargetModeCallBack exitTargetCallBack;
		
		private int abilityIndex=0;
		
		private int targetModeAOE;
		private _TargetType targetModeType;
		private List<Tile> targetModeTileList=new List<Tile>();
		private List<Tile> targetModeHoveredTileList=new List<Tile>();
		
		
		public void _ActivateTargetMode(int abIndex, int AOE, _TargetType type, TargetModeCallBack sCallBack, ExitTargetModeCallBack eCallBack){
			GridManager.AbilityTargetSelectMode(this.AbilityTargetSelected, this.SetTargetModeHoveredTile, this.ClearTargetModeHoveredTile);
			
			targetMode=true;
			abilityIndex=abIndex;
			
			targetModeAOE=AOE;
			targetModeType=type;
			
			targetSelectedCallBack=sCallBack;
			exitTargetCallBack=eCallBack;
			
			OverlayManager.DisableTileCursor();
			
			TBTK.OnAbilityTargetMode(true);
		}
		
		public void ActivateTargetModeFaction(int AOE, _TargetType type, int abIndex, TargetModeCallBack sCallBack, ExitTargetModeCallBack eCallBack){
			TBTK.OnFactionABTargetMode(abIndex);
			_ActivateTargetMode(abIndex, AOE, type, sCallBack, eCallBack);
		}
		
		
		//~ public void ActivateTargetModeUnit(Tile tile, int range, int AOE, bool normalAttack, bool requireDirectLOS, _TargetType type, TargetModeCallBack sCallBack, TargetModeCallBack eCallBack){
		public void ActivateTargetModeUnit(Tile tile, UnitAbility ability, int abIndex, TargetModeCallBack sCallBack, ExitTargetModeCallBack eCallBack){
			TBTK.OnUnitABTargetMode(abIndex);
			_ActivateTargetMode(abIndex, ability.GetAOERange(), ability.targetType, sCallBack, eCallBack);
			
			if(!ability.AttackInLine()){
				if(!ability.normalAttack){
					if(targetModeType==_TargetType.EmptyTile)
						targetModeTileList=GridManager.GetTilesWithinDistance(tile, ability.GetRange(), true);
					else
						targetModeTileList=GridManager.GetTilesWithinDistance(tile, ability.GetRange());
				}
				else{
					targetModeTileList=new List<Tile>();
					List<Tile> tilesInRangeList=GridManager.GetTilesWithinDistance(tile, ability.GetRange());
					
					int sight=tile.unit.GetSight();
					List<Unit> allFriendlyUnitList=FactionManager.GetAllUnitsOfFaction(tile.unit.factionID);
					
					for(int i=0; i<tilesInRangeList.Count; i++){
						Tile targetTile=tilesInRangeList[i];
						
						if(!GameControl.EnableFogOfWar() && !GameControl.AttackThroughObstacle()){
							if(!FogOfWar.InLOS(tile, targetTile, 0)) continue;
						}
						
						bool inSight=GameControl.EnableFogOfWar() ? false : true;
						if(GameControl.EnableFogOfWar()){
							if(FogOfWar.InLOS(tile, targetTile) && GridManager.GetDistance(tile, targetTile)<=sight){
								inSight=true;
							}
							else if(!ability.requireDirectLOS){
								for(int n=0; n<allFriendlyUnitList.Count; n++){
									if(allFriendlyUnitList[n]==tile.unit) continue;
									if(GridManager.GetDistance(allFriendlyUnitList[n].tile, targetTile)>allFriendlyUnitList[n].GetSight()) continue;
									if(FogOfWar.InLOS(allFriendlyUnitList[n].tile, targetTile)){
										inSight=true;
										break;
									}
								}
							}
						}
						
						if(inSight) targetModeTileList.Add(targetTile);
					}
				}
			}
			else{
				/*
				List<Tile> neighbourList=tile.GetNeighbourList();
				for(int i=0; i<neighbourList.Count; i++){
					bool walkableOnly=(ability.type==UnitAbility._AbilityType.ChargeAttack);
					List<Tile> tileList=GridManager.GetTilesInALine(tile, neighbourList[i], ability.GetRange(), walkableOnly);
					
					if(tileList.Count>0){
						if(targetModeType!=_TargetType.EmptyTile) targetModeTileList.Add(tileList[tileList.Count-1]);
						else if (tileList[tileList.Count-1].unit==null) targetModeTileList.Add(tileList[tileList.Count-1]);
					}
				}
				*/
			}
			
			//for(int i=0; i<targetModeTileList.Count; i++) targetModeTileList[i].SetState(_TileState.Range);
			OverlayManager.ShowAbilityRangeIndicator(targetModeTileList);
		}
		
		
		public static void ExitAbilityTargetMode(){
			if(!targetMode) return;
			
			TBTK.OnFactionABTargetMode();
			TBTK.OnUnitABTargetMode();		//clear ability select mode for UI
			
			targetMode=false;
			
			instance.abilityIndex=0;
			
			instance.targetModeTileList=new List<Tile>();
			
			OverlayManager.EnableTileCursor();
			OverlayManager.ClearAbilityTargetIndicator();
			OverlayManager.ClearAbilityRangeIndicator();
			
			GridManager.ClearTargetSelectMode();
			
			GameControl.ReselectUnit();
			
			if(instance.exitTargetCallBack!=null) instance.exitTargetCallBack();
			
			TBTK.OnAbilityTargetMode(false);
		}
		
		
		//public static void AbilityTargetSelected(Tile tile){ instance._AbilityTargetSelected(tile); }
		public void AbilityTargetSelected(Tile tile){
			if(targetModeTileList.Count>0 && !targetModeTileList.Contains(tile)){ 
				GameControl.DisplayMessage("Out of Range");
				return;
			}
			
			bool invalidFlag=false;
			
			if(targetModeType==_TargetType.AllUnit){
				invalidFlag=(tile.unit==null);
			}
			else if(targetModeType==_TargetType.HostileUnit){
				int currentFacID=FactionManager.GetSelectedFactionID();
				invalidFlag=(tile.unit==null || tile.unit.factionID==currentFacID);
			}
			else if(targetModeType==_TargetType.FriendlyUnit){
				int currentFacID=FactionManager.GetSelectedFactionID();
				invalidFlag=(tile.unit==null || tile.unit.factionID!=currentFacID);
			}
			else if(targetModeType==_TargetType.EmptyTile){
				invalidFlag=(tile.unit!=null || !tile.walkable);
			}
			else if(targetModeType==_TargetType.AllTile){ }
			
			if(invalidFlag) GameControl.DisplayMessage("Invalid target");
			else{
				if(targetSelectedCallBack!=null) targetSelectedCallBack(tile, abilityIndex);
				
				TBTK.OnAbilityActivated();
				
				ExitAbilityTargetMode();
			}
		}
		
		
		private void SetTargetModeHoveredTile(Tile tile){
			ClearTargetModeHoveredTile();
			
			if(targetModeTileList.Count>0 && !targetModeTileList.Contains(tile)) return;
			
			targetModeHoveredTileList=new List<Tile>();
			if(targetModeAOE>0) targetModeHoveredTileList=GridManager.GetTilesWithinDistance(tile, targetModeAOE);
			if(!targetModeHoveredTileList.Contains(tile)) targetModeHoveredTileList.Add(tile);
			
			OverlayManager.ShowAbilityTargetIndicator(targetModeHoveredTileList);
		}
		private void ClearTargetModeHoveredTile(Tile tile=null){
			OverlayManager.ClearAbilityTargetIndicator();
		}
		
		
		public static bool CanCastAbility(Tile tile){ return instance.targetModeTileList.Contains(tile); }
		
		
		
		
		
		
		
		
//****************************************************************************************************************************************
		
		public static void ApplyAbilityEffect(Tile targetTile, Ability ability, int type, Unit srcUnit = null, float critMult = 1.0f){
			instance.StartCoroutine(instance._ApplyAbilityEffect(targetTile, ability, type, srcUnit, critMult));
		}
		IEnumerator _ApplyAbilityEffect(Tile targetTile, Ability ability, int type, Unit srcUnit = null, float critMult = 1.0f){
			if(targetTile!=null && ability.effectObject!=null){
				if(!ability.autoDestroyEffect) ObjectPoolManager.Spawn(ability.effectObject, targetTile.GetPos(), Quaternion.identity);
				else ObjectPoolManager.Spawn(ability.effectObject, targetTile.GetPos(), Quaternion.identity, ability.effectObjectDuration);
			}
			
			if(!ability.useDefaultEffect) yield break;
			if(ability.effectDelayDuration>0) yield return new WaitForSeconds(ability.effectDelayDuration);
			
			if(type==1){	//_AbilityType.Generic
				
				List<Tile> tileList=new List<Tile>();
				if(targetTile!=null){
					if(ability.aoeRange>0) tileList=GridManager.GetTilesWithinDistance(targetTile, ability.aoeRange);
					tileList.Add(targetTile);
				}
				else{
					if(ability.effTargetType==_EffectTargetType.Tile)	tileList=GridManager.GetTileList();
					else{
						List<Unit> unitList=FactionManager.GetAllUnit();
						for(int i=0; i<unitList.Count; i++) tileList.Add(unitList[i].tile);
					}
				}
				
				if(ability.effTargetType==_EffectTargetType.AllUnit){
					for(int i=0; i<tileList.Count; i++){
						if(tileList[i].unit==null) continue;
						tileList[i].unit.ApplyEffect(ability.CloneEffect(critMult), srcUnit);
						SpawnEffectObjTarget(ability, tileList[i]);
					}
				}
				else if(ability.effTargetType==_EffectTargetType.HostileUnit){
					for(int i=0; i<tileList.Count; i++){
						if(tileList[i].unit==null) continue;
						if(tileList[i].unit.factionID==ability.factionID) continue;
						tileList[i].unit.ApplyEffect(ability.CloneEffect(critMult), srcUnit);
						SpawnEffectObjTarget(ability, tileList[i]);
					}
				}
				else if(ability.effTargetType==_EffectTargetType.FriendlyUnit){
					for(int i=0; i<tileList.Count; i++){
						if(tileList[i].unit==null) continue;
						if(tileList[i].unit.factionID!=ability.factionID) continue;
						tileList[i].unit.ApplyEffect(ability.CloneEffect(critMult), srcUnit);
						SpawnEffectObjTarget(ability, tileList[i]);
					}
				}
				else if(ability.effTargetType==_EffectTargetType.Tile){
					for(int i=0; i<tileList.Count; i++){
						tileList[i].ApplyEffect(ability.CloneEffect(critMult));
						SpawnEffectObjTarget(ability, tileList[i]);
					}
				}
			}
			else if(type==2){	//_AbilityType.SpawnNew
				Quaternion rot=srcUnit!=null ? srcUnit.thisT.rotation : Quaternion.identity ;
				GameObject unitObj=(GameObject)Instantiate(ability.spawnUnit.gameObject, targetTile.GetPos(), rot);
				Unit unit=unitObj.GetComponent<Unit>();
				
				unitObj.transform.position=targetTile.GetPos();
				if(srcUnit!=null) unitObj.transform.rotation=srcUnit.thisT.rotation;
				else{
					Faction faction=FactionManager.GetFaction(ability.factionID);
					if(faction!=null) unitObj.transform.rotation=Quaternion.Euler(0, faction.spawnDirection, 0);
				}
				
				FactionManager.InsertUnit(unit, ability.factionID);
				unit.SetNewTile(targetTile);
				//if(GridManager.GetDistance(targetTile, srcUnit.tile)<=srcUnit.GetMoveRange()) GameControl.SelectUnit(srcUnit);
			}
			else if(type==3){	//_AbilityType.ScanFogOfWar)
				List<Tile> targetTileList=GridManager.GetTilesWithinDistance(targetTile, ability.GetAOERange());
				targetTileList.Add(targetTile);
				for(int i=0; i<targetTileList.Count; i++) targetTileList[i].ForceVisible(ability.effect.duration);
			}
			else if(type==4){	//_AbilityType.Overwatch
				targetTile.unit.Overwatch(ability.CloneEffect(critMult));
			}
			else if(type==5){	//_AbilityType.Teleport
				Quaternion wantedRot=Quaternion.LookRotation(targetTile.GetPos()-ability.unit.tile.GetPos());
				ability.unit.thisT.rotation=wantedRot;
				
				GameControl.ClearSelectedUnit();
				ability.unit.SetNewTile(targetTile);
				GameControl.SelectUnit(srcUnit);
			}
			
			
			
			else if(type==6){	//charge attack
				List<Tile> tileList=GridManager.GetTilesInALine(srcUnit.tile, targetTile, ability.GetRange());
				
				for(int i=0; i<tileList.Count; i++){
					while(true){
						float dist=Vector3.Distance(srcUnit.thisT.position, tileList[i].GetPos());
						if(dist<0.05f){
							if(tileList[i].unit!=null){
								tileList[i].unit.ApplyEffect(ability.CloneEffect(critMult));
								SpawnEffectObjTarget(ability, tileList[i]);
							}
							break;
						}
						
						Quaternion wantedRot=Quaternion.LookRotation(tileList[i].GetPos()-srcUnit.thisT.position);
						srcUnit.thisT.rotation=Quaternion.Slerp(srcUnit.thisT.rotation, wantedRot, Time.deltaTime*srcUnit.moveSpeed*6);
						
						Vector3 dir=(tileList[i].GetPos()-srcUnit.thisT.position).normalized;
						srcUnit.thisT.Translate(dir*Mathf.Min(srcUnit.moveSpeed*2*Time.deltaTime, dist), Space.World);
						yield return null;
					}
				}
				
				srcUnit.tile.unit=null;
				srcUnit.tile=tileList[tileList.Count-1];
				tileList[tileList.Count-1].unit=srcUnit;
				GameControl.ReselectUnit();
			}
			else if(type==7){	//attack all unit in a straight line
				List<Tile> tileList=GridManager.GetTilesInALine(srcUnit.tile, targetTile, ability.GetRange());
				for(int i=0; i<tileList.Count; i++){
					if(tileList[i].unit==null) continue;
					tileList[i].unit.ApplyEffect(ability.CloneEffect(critMult));
					SpawnEffectObjTarget(ability, tileList[i]);
				}
			}
			else{
				Debug.LogWarning("Error, unknown ability type ("+type+")");
			}
			
			yield return null;
		}
		
		void SpawnEffectObjTarget(Ability ability, Tile tile){
			if(ability.effectObjectTarget!=null){
				if(!ability.autoDestroyEffectTgt) ObjectPoolManager.Spawn(ability.effectObjectTarget, tile.GetPos(), Quaternion.identity);
				else ObjectPoolManager.Spawn(ability.effectObjectTarget, tile.GetPos(), Quaternion.identity, ability.effectObjectTgtDuration);
			}
		}
		
	}

}