using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	public static class CoverSystem{
		
		public class Cover{
			public bool enabled=true;
			public float angle=0;
			public _CoverType type;
			public Vector3 overlayPos;
			public Quaternion overlayRot;
		}
		
		
		public static float fullCoverBonus=0.75f;
		public static float halfCoverBonus=0.25f;
		public static void SetFullCoverDodgeBonus(float val){ fullCoverBonus=val; }
		public static void SetHalfCoverDodgeBonus(float val){ halfCoverBonus=val; }
		public static float GetFullCoverDodgeBonus(){ return fullCoverBonus; }
		public static float GetHalfCoverDodgeBonus(){ return halfCoverBonus; }
		
		public static float exposedCritChanceBonus=0.3f;
		public static void SetExposedCritChanceBonus(float val){ exposedCritChanceBonus=val; }
		public static float GetExposedCritChanceBonus(){ return exposedCritChanceBonus; }
		
		
		public enum _CoverType{None, Half, Full}
		
		
		public static void InitCoverForTile(Tile tile){
			List<Tile> neighbourList=tile.GetNeighbourList();
			List<Cover> coverList=new List<Cover>();
			
			for(int i=0; i<tile.GetNeighbourList().Count; i++){
				Vector3 dir=(neighbourList[i].GetPos()-tile.GetPos()).normalized;
				float dist=GridManager.GetTileSize()*GridManager.GetGridToTileSizeRatio()*.75f;
				
				LayerMask mask=1<<TBTK.GetLayerObstacleFullCover() | 1<<TBTK.GetLayerObstacleHalfCover();
				RaycastHit hit;
				if(Physics.Raycast(tile.GetPos(), dir, out hit, dist, mask)){
					Cover cover=new Cover();
					cover.angle=Mathf.Round(Utilities.Vector2ToAngle(new Vector2(dir.x, dir.z)));
					
					if(GridManager.GetTileType()==_TileType.Square){	//when diagonal neighbour is enabled, avoid adding cover to diagonal neighbour
						if(cover.angle%90!=0) continue;
					}
					
					int layer=hit.transform.gameObject.layer;
					if(layer==TBTK.GetLayerObstacleFullCover()){
						cover.type=_CoverType.Full;
						Debug.DrawLine(tile.GetPos(), tile.GetPos()+dir*dist, Color.red, 2);
					}
					else if(layer==TBTK.GetLayerObstacleHalfCover()){
						cover.type=_CoverType.Half;
						Debug.DrawLine(tile.GetPos(), tile.GetPos()+dir*dist, Color.white, 2);
					}
					
					
					if(GridManager.GetTileType()==_TileType.Square) cover.overlayPos=tile.GetPos()+dir*dist*0.4f;
					else if(GridManager.GetTileType()==_TileType.Hex) cover.overlayPos=tile.GetPos()+dir*dist*0.35f;
					
					float angleY=cover.angle+90;
					if(cover.angle==30) angleY=cover.angle+30;
					else if(cover.angle==150) angleY=cover.angle-30;
					else if(cover.angle==210) angleY=cover.angle+30;
					else if(cover.angle==330) angleY=cover.angle-30;
					cover.overlayRot=Quaternion.Euler(0, angleY, 0);
					
					coverList.Add(cover);
				}
			}
			
			tile.coverList=coverList;
		}
		
		public static _CoverType GetCoverType(Tile attackingTile, Tile targetTile){
			Vector3 dir=attackingTile.GetPos()-targetTile.GetPos();
			
			//float angle=0;
			//if(GridManager.GetTileType()==_TileType.Square) angle=Utilities.VectorToAngle90(new Vector2(dir.x, dir.z));
			//else if(GridManager.GetTileType()==_TileType.Hex) angle=Utilities.VectorToAngle60(new Vector2(dir.x, dir.z));	//round to nearest
			
			float angle=Utilities.Vector2ToAngle(new Vector2(dir.x, dir.z));
			
			_CoverType cover=_CoverType.None;
			for(int i=0; i<targetTile.coverList.Count; i++){
				if(!targetTile.coverList[i].enabled) continue;
				
				float diff=Mathf.Abs(targetTile.coverList[i].angle-angle);
				if(diff>180) diff=360-diff;
				
				if(diff<GameControl.GetEffectiveCoverAngle()){
					cover=targetTile.coverList[i].type;
					if(targetTile.coverList[i].type==_CoverType.Full) break;
				}
			}
			
			return cover;
		}
		
	}
	
}
