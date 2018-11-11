using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	public static class FogOfWar{
		
		public static void InitGrid(List<Tile> tileList){
			if(!GameControl.EnableFogOfWar()) return;
			
			for(int i=0; i<tileList.Count; i++) tileList[i].SetVisible(false);
			
			List<Unit> unitList=FactionManager.GetAllPlayerUnits();
			for(int i=0; i<unitList.Count; i++){
				unitList[i].SetupFogOfWar(true);
			}
		}
		
		
		//check a tile visiblility to player's faction
		public static bool CheckTileVisibility(Tile tile){
			List<Unit> unitList=FactionManager.GetAllPlayerUnits();
			for(int i=0; i<unitList.Count; i++){
				if(GridManager.GetDistance(tile, unitList[i].tile)<=unitList[i].GetSight()){ //return true;
					//if(InLOS(tile, unitList[i].tile, true)) return true;		//for showing LOS cast
					if(InLOS(tile, unitList[i].tile)) return true;
				}
			}
			return false;
		}
		
		//used to check if AI faction can see a given tile
		public static bool IsTileVisibleToFaction(Tile tile, int factionID){
			List<Unit> unitList=FactionManager.GetAllUnitsOfFaction(factionID);
			for(int i=0; i<unitList.Count; i++){
				if(GridManager.GetDistance(tile, unitList[i].tile)<=unitList[i].GetSight()){ //return true;
					if(InLOS(tile, unitList[i].tile)) return true;
				}
			}
			return false;
		}
		
		//used to check if a particular tile is visible to a particular unit
		public static bool IsTileVisibleToUnit(Tile tile, Unit unit){
			if(GridManager.GetDistance(tile, unit.tile)<=unit.GetSight()){
				if(InLOS(tile, unit.tile)) return true;
			}
			return false;
		}
		
		
		
		public static bool LOSRaycast(Vector3 pos, Vector3 dir, float dist, LayerMask mask, bool debugging=false){
			float debugDuration=1.5f;
			RaycastHit[] hits=Physics.RaycastAll(pos, dir, dist, mask);
			if(hits.Length!=0){
				if(debugging) Debug.DrawLine(pos, pos+dir*dist, Color.red, debugDuration);
				return true;
			}
			else{
				if(debugging) Debug.DrawLine(pos, pos+dir*dist, Color.white, debugDuration);
				return false;
			}
		}
		
		
		public static bool InLOS(Tile tile1, Tile tile2, bool debugging=false){ return InLOS(tile1,tile2, -1, debugging); }
		public static bool InLOS(Tile tile1, Tile tile2, float peekFactor, bool debugging=false){
			Vector3 pos1=tile1.GetPos();
			Vector3 pos2=tile2.GetPos();
			
			if(peekFactor<0) peekFactor=GameControl.GetPeekFactor();
			
			float dist=Vector3.Distance(pos2, pos1);
			Vector3 dir=(pos2-pos1).normalized;
			Vector3 dirO=new Vector3(-dir.z, 0, dir.x).normalized;
			float posOffset=GridManager.GetTileSize()*GridManager.GetGridToTileSizeRatio()*peekFactor;
			
			LayerMask mask=1<<TBTK.GetLayerObstacleFullCover();// | 1<<LayerManager.GetLayerObstacleHalfCover();
			
			bool flag=false;
			
			if(!LOSRaycast(pos1, dir, dist, mask, debugging)){
				if(debugging) flag=true;
				else return true;
			}
			
			if(posOffset==0) return flag;
			
			if(!LOSRaycast(pos1+dirO*posOffset, dir, dist, mask, debugging)){
				if(debugging) flag=true;
				else return true;
			}
			if(!LOSRaycast(pos1-dirO*posOffset, dir, dist, mask, debugging)){
				if(debugging) flag=true;
				else return true;
			}
			
			return flag;
		}
		
	}
	
}
