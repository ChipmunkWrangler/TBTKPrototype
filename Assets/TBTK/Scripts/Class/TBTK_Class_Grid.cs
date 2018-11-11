using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{

	[System.Serializable]
	public class Grid {
		
		[SerializeField] private bool initiated=false;
		public bool IsInitiated(){ return initiated; }
		
		[SerializeField] private _TileType type=_TileType.Square;
		[SerializeField] private int width=5;
		[SerializeField] private int length=5;
		[SerializeField] private float tileSize=1;
		[SerializeField] private float gridToTileRatio=1;
		
		public _TileType GetTileType(){ return type; }
		public int GetWidth(){ return width; }
		public int GetLength(){ return length; }
		public float GetTileSize(){ return tileSize; }
		public float GetGridToTileRatio(){ return gridToTileRatio; }
		
		
		
		public Grid(){}
		public Grid(_TileType t, int w, int l, float size, float ratio){
			initiated=true;	type=t;	width=w;	length=l;	tileSize=size;	gridToTileRatio=ratio;
		}
		
		
		public GameObject gridObj;
		public GameObject GetGridObject(){ return gridObj; }
		public Vector3 GetGridCenterPoint(){ return gridObj.transform.position; }
		
		public List<Tile> tileList=new List<Tile>();
		
		//public List<Transform> tileTList=new List<Transform>();	//not in used, for when tile is not monobehaviour
		
		
		public void AssignIndex(){
			for(int i=0; i<tileList.Count; i++) tileList[i].index=i;
		}
		
		
		//used in editor only
		public Tile GetNeighbourInDir(Tile tile, float angle){
			int ID=tileList.IndexOf(tile);
			
			GridManager gridManager=GridManager.GetInstance();
			
			if(gridManager.tileType==_TileType.Square){
				
				float distTH=gridManager.tileSize*gridManager.gridToTileRatio*1.1f;
				
				if(angle==90){
					if(ID+1<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[ID+1].GetPosG())<=distTH) return tileList[ID+1];
				}
				else if(angle==180){	
					ID-=gridManager.length;
					if(ID>=0 && Vector3.Distance(tile.GetPosG(), tileList[ID].GetPosG())<=distTH) return tileList[ID];
				}
				else if(angle==270){	
					if(ID-1>=0 && Vector3.Distance(tile.GetPosG(), tileList[ID-1].GetPosG())<=distTH) return tileList[ID-1];
				}
				else if(angle==0){	
					ID+=gridManager.length;
					if(ID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[ID].GetPosG())<=distTH) return tileList[ID];
				}
			}
			else if(gridManager.tileType==_TileType.Hex){
				
				float distTH=GridGenerator.spaceZHex*gridManager.tileSize*gridManager.gridToTileRatio*1.1f;
				
				if(angle==90){
					if(ID+1<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[ID+1].GetPosG())<=distTH) return tileList[ID+1];
				}
				else if(angle==150){
					ID-=gridManager.length-1;
					if(ID>=0 && Vector3.Distance(tile.GetPosG(), tileList[ID].GetPosG())<=distTH) return tileList[ID];
				}
				else if(angle==210){
					ID-=gridManager.length;
					if(ID>=0 && Vector3.Distance(tile.GetPosG(), tileList[ID].GetPosG())<=distTH) return tileList[ID];
				}
				else if(angle==270){
					if(ID-1>0 && Vector3.Distance(tile.GetPosG(), tileList[ID-1].GetPosG())<=distTH) return tileList[ID-1];
				}
				else if(angle==330){
					ID+=gridManager.length-1;
					if(ID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[ID].GetPosG())<=distTH) return tileList[ID];
				}
				else if(angle==30){
					ID+=gridManager.length;
					if(ID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[ID].GetPosG())<=distTH) return tileList[ID];
				}
			}
			
			return null;
		}
		
		public void Init(){
			for(int i=0; i<tileList.Count; i++) tileList[i].Init();
			
			GridManager gridManager=GridManager.GetInstance();
			
			//setup the neighbour of each tile
			if(gridManager.tileType==_TileType.Square){
				float distTH=gridManager.tileSize*gridManager.gridToTileRatio*1.1f;
				for(int i=0; i<tileList.Count; i++){
					Tile tile=tileList[i];
					tile.aStar=new TileAStar(tile);
					
					List<Tile> neighbourList=new List<Tile>();
					
					int nID=i+1;
					if(nID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					nID=i+gridManager.length;
					if(nID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					nID=i-1;
					if(nID>=0 && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					nID=i-gridManager.length;
					if(nID>=0 && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					//tile.aStar.straightNeighbourCount=neighbourList.Count;
					
					//diagonal neighbour, not in used  
					if(GridManager.EnableDiagonalNeighbour()){
						nID=(i+1)+gridManager.length;
						if(nID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH*1.5f)
							neighbourList.Add(tileList[nID]);
						
						nID=(i+1)-gridManager.length;
						if(nID>=0 && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH*1.5f)
							neighbourList.Add(tileList[nID]);
						
						nID=(i-1)+gridManager.length;
						if(nID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH*1.5f)
							neighbourList.Add(tileList[nID]);
						
						nID=(i-1)-gridManager.length;
						if(nID>=0 && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH*1.5f)
							neighbourList.Add(tileList[nID]);
						
					}
					
					tile.aStar.SetNeighbourList(neighbourList);
				}
			}
			else if(gridManager.tileType==_TileType.Hex){
				float distTH=GridGenerator.spaceZHex*gridManager.tileSize*gridManager.gridToTileRatio*1.1f;
				for(int i=0; i<tileList.Count; i++){
					Tile tile=tileList[i];
					tile.aStar=new TileAStar(tile);
					
					List<Tile> neighbourList=new List<Tile>();
					
					int nID=i+1;		
					if(nID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					nID=i+gridManager.length;
					if(nID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					nID=i+gridManager.length-1;
					if(nID<tileList.Count && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					nID=i-1;
					if(nID>=0 && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					nID=i-gridManager.length;
					if(nID>=0 && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					nID=i-gridManager.length+1;
					if(nID>=0 && Vector3.Distance(tile.GetPosG(), tileList[nID].GetPosG())<=distTH)
						neighbourList.Add(tileList[nID]);
					
					tile.aStar.SetNeighbourList(neighbourList);
					
					//tile.aStar.straightNeighbourCount=neighbourList.Count;
				}
			}
			
			//setup the wall
			for(int i=0; i<tileList.Count; i++){
				tileList[i].InitWall();
			}
			
			if(GridManager.EnableDiagonalNeighbour()){
				for(int i=0; i<tileList.Count; i++) tileList[i].CheckDiagonal();
			}
			
			//setup the cover
			if(GameControl.EnableCover()){
				for(int i=0; i<tileList.Count; i++) CoverSystem.InitCoverForTile(tileList[i]);
			}
		}
		
		//get all the tiles within certain distance from a given tile
		public List<Tile> GetTilesWithinDistance(Tile srcTile, int dist, bool walkableOnly=false, bool setDistance=false, int minDist=0){
			List<Tile> neighbourList=srcTile.GetNeighbourList(walkableOnly);
			
			List<Tile> closeList=new List<Tile>();
			List<Tile> openList=new List<Tile>();
			List<Tile> newOpenList=new List<Tile>();
			
			for(int m=0; m<neighbourList.Count; m++){
				Tile neighbour=neighbourList[m];
				if(!newOpenList.Contains(neighbour)) newOpenList.Add(neighbour);
			}
			
			for(int i=0; i<dist; i++){
				openList=newOpenList;
				newOpenList=new List<Tile>();
				
				for(int n=0; n<openList.Count; n++){
					neighbourList=openList[n].GetNeighbourList(walkableOnly);
					for(int m=0; m<neighbourList.Count; m++){
						Tile neighbour=neighbourList[m];
						if(!closeList.Contains(neighbour) && !openList.Contains(neighbour) && !newOpenList.Contains(neighbour)){
							newOpenList.Add(neighbour);
						}
					}
				}
				
				for(int n=0; n<openList.Count; n++){
					Tile tile=openList[n];
					if(tile!=srcTile && !closeList.Contains(tile)){
						closeList.Add(tile);
						if(setDistance) tile.distance=i+1;
					}
				}
			}
			
			/*
			if(minDist>1){
				for(int i=0; i<closeList.Count; i++){
					if(GetDistance(srcTile, closeList[i])<minDist){ closeList.RemoveAt(i);	i-=1; }
				}
			}
			*/
			
			return closeList;
		}
		
		//get a direct distance, regardless of the walkable state
		public int GetDistance(Tile tile1, Tile tile2){
			if(GridManager.GetTileType()==_TileType.Hex){
				float x=Mathf.Abs(tile1.x-tile2.x);
				float y=Mathf.Abs(tile1.y-tile2.y);
				float z=Mathf.Abs(tile1.z-tile2.z);
				return (int)((x + y + z)/2);
			}
			else{
				float tileSize=GridManager.GetTileSize()*GridManager.GetGridToTileSizeRatio();
				Vector3 pos1=tile1.GetPos();
				Vector3 pos2=tile2.GetPos();
				
				int dx=(int)Mathf.Round(Mathf.Abs(pos1.x-pos2.x)/tileSize);
				int dz=(int)Mathf.Round(Mathf.Abs(pos1.z-pos2.z)/tileSize);
				
				if(GridManager.EnableDiagonalNeighbour()){
					int min=Mathf.Min(dx, dz);
					int max=Mathf.Max(dx, dz);
					
					return min + (max - min);
				}
				
				return dx+dz;
			}
		}
		
		//get distance when restricted to walkable tile, using A*
		public int GetWalkableDistance(Tile tile1, Tile tile2){
			return AStar.GetDistance(tile1, tile2);
		}
		
		public void ClearGrid(){
			MonoBehaviour.DestroyImmediate(gridObj);
			tileList=new List<Tile>();
			//for(int i=0; i<tileList.Count; i++) MonoBehaviour.DestroyImmediate(tileList[i].gameObject);
		}
		
	}

}