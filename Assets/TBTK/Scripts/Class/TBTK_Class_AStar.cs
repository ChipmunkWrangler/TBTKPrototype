using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	//A* information for each tile
	public class TileAStar{
		public Tile tile;
		
		//public int straightNeighbourCount=0;	//number of neighbour tile that is nont diagonal (max4 for square, 6 for hex)
		
		//neighbour list is only setup once the application is playing
		private List<Tile> neighbourList=new List<Tile>();
		public void SetNeighbourList(List<Tile> list){ neighbourList=list; }
		
		private List<Tile> disconnectedNeighbourList=new List<Tile>();
		public List<Tile> GetDCNeighbourList(){ return disconnectedNeighbourList; }
		
		public enum _AStarListState{Unassigned, Open, Close};
		public _AStarListState listState=_AStarListState.Unassigned;
		
		public Tile parent=null;
		public float scoreG;
		public float scoreH;
		public float scoreF;
		public float tempScoreG;
		public int rangeRequired;	//the range-cost required to move to this tile, for grid with height variance
		
		
		public TileAStar(Tile t){ tile=t; }
		
		public bool IsNeighbourDisconnected(Tile tile){
			return disconnectedNeighbourList.Contains(tile) ? true : false ;
		}
		
		public void DisconnectNeighbour(Tile tile){
			if(neighbourList.Contains(tile)){
				neighbourList.Remove(tile);
				disconnectedNeighbourList.Add(tile);
				//straightNeighbourCount-=1;
			}
		}
		public void ConnectNeighbour(Tile tile){
			if(disconnectedNeighbourList.Contains(tile)){
				neighbourList.Add(tile);
				disconnectedNeighbourList.Remove(tile);
			}
		}
		
		
		
		public List<Tile> GetNeighbourList(bool walkableOnly=false){ 
			List<Tile> newList=new List<Tile>();
			if(walkableOnly){
				for(int i=0; i<neighbourList.Count; i++){
					if(neighbourList[i].walkable && neighbourList[i].unit==null) newList.Add(neighbourList[i]);
				}
			}
			else{
				for(int i=0; i<neighbourList.Count; i++) newList.Add(neighbourList[i]);
				for(int i=0; i<disconnectedNeighbourList.Count; i++) newList.Add(disconnectedNeighbourList[i]);
			}
			return newList;
		}
		
		
		//call during a serach to scan through neighbour, check their score against the position passed
		//process walkable neighbours only, used to search for a walkable path via A*
		public void ProcessWalkableNeighbour(Tile targetTile){
			for(int i=0; i<neighbourList.Count; i++){
				TileAStar neighbour=neighbourList[i].aStar;
				if((neighbour.tile.walkable && neighbour.tile.unit==null) || neighbour.tile==targetTile){
					//if the neightbour state is clean (never evaluated so far in the search)
					if(neighbour.listState==_AStarListState.Unassigned){
						//check the score of G and H and update F, also assign the parent to currentNode
						neighbour.scoreG=scoreG+1;
						neighbour.scoreH=Vector3.Distance(neighbour.tile.GetPos(), targetTile.GetPos());
						neighbour.UpdateScoreF();
						neighbour.parent=tile;
					}
					//if the neighbour state is open (it has been evaluated and added to the open list)
					else if(neighbour.listState==_AStarListState.Open){
						//calculate if the path if using this neighbour node through current node would be shorter compare to previous assigned parent node
						tempScoreG=scoreG+1;
						if(neighbour.scoreG>tempScoreG){
							//if so, update the corresponding score and and reassigned parent
							neighbour.parent=tile;
							neighbour.scoreG=tempScoreG;
							neighbour.UpdateScoreF();
						}
					}
				}
			}
		}
		
		
		public void UpdateScoreF(){ scoreF=scoreG+scoreH; }
	}
	
	
	
	
	
	public class AStar{
		//search for a path, through walkable tile only
		//for normal movement, return the path in a list of hexTile
		public static List<Tile> SearchWalkableTile(Tile originTile, Tile destTile){
			
			List<Tile> closeList=new List<Tile>();
			List<Tile> openList=new List<Tile>();
			
			Tile currentTile=originTile;
			
			float currentLowestF=Mathf.Infinity;
			int id=0;
			int i=0;
			
			while(true){
				
				//if we have reach the destination
				if(currentTile==destTile) break;
				
				//move currentNode to closeList;
				closeList.Add(currentTile);
				currentTile.aStar.listState=TileAStar._AStarListState.Close;
				
				//loop through the neighbour of current loop, calculate  score and stuff
				currentTile.aStar.ProcessWalkableNeighbour(destTile);
				
				//put all neighbour in openlist
				foreach(Tile neighbour in currentTile.aStar.GetNeighbourList(true)){
					if(neighbour.aStar.listState==TileAStar._AStarListState.Unassigned || neighbour==destTile){
						//~ //set the node state to open
						neighbour.aStar.listState=TileAStar._AStarListState.Open;
						openList.Add(neighbour);
					}
				}
				
				//clear the current node, before getting a new one, so we know if there isnt any suitable next node
				currentTile=null;
				
				
				currentLowestF=Mathf.Infinity;
				id=0;
				for(i=0; i<openList.Count; i++){
					if(openList[i].aStar.scoreF<currentLowestF){
						currentLowestF=openList[i].aStar.scoreF;
						currentTile=openList[i];
						id=i;
					}
				}
				
				
				//if there's no node left in openlist, path doesnt exist
				if(currentTile==null) {
					break;
				}

				openList.RemoveAt(id);
			}
			
			if(currentTile==null){
				float tileSize=GridManager.GetTileSize()*GridManager.GetGridToTileSizeRatio();
				currentLowestF=Mathf.Infinity;
				for(i=0; i<closeList.Count; i++){
					float dist=Vector3.Distance(destTile.GetPos(), closeList[i].GetPos());
					if(dist<currentLowestF){
						currentLowestF=dist;
						currentTile=closeList[i];
						if(dist<tileSize*1.5f) break;
					}
				}
			}
			
			List<Tile> path=new List<Tile>();
			while(currentTile!=null){
				if(currentTile==originTile || currentTile==currentTile.aStar.parent) break;
				path.Add(currentTile);
				currentTile=currentTile.aStar.parent;
			}
			
			path=InvertTileArray(path);
			
			ResetGraph(destTile, openList, closeList);
			
			return path;
		}
		
		
		
		
		//search the shortest path through all tile reagardless of status
		//this is used to accurately calculate the distance between 2 tiles in term of tile
		//distance calculated applies for line traverse thru walkable tiles only, otherwise it can be calculated using the coordinate
		public static int GetDistance(Tile srcTile, Tile targetTile){
			List<Tile> closeList=new List<Tile>();
			List<Tile> openList=new List<Tile>();
			
			Tile currentTile=srcTile;
			if(srcTile==null) Debug.Log("src tile is null!!!");
			
			float currentLowestF=Mathf.Infinity;
			int id=0;
			int i=0;
			
			while(true){
				//if we have reach the destination
				if(currentTile==targetTile) break;
				
				//move currentNode to closeList;
				closeList.Add(currentTile);
				currentTile.aStar.listState=TileAStar._AStarListState.Close;
				
				//loop through all neighbours, regardless of status 
				//currentTile.ProcessAllNeighbours(targetTile);
				currentTile.aStar.ProcessWalkableNeighbour(targetTile);
				
				//put all neighbour in openlist
				foreach(Tile neighbour in currentTile.aStar.GetNeighbourList()){
					if(neighbour.unit!=null && neighbour!=targetTile) continue;
					if(neighbour.aStar.listState==TileAStar._AStarListState.Unassigned) {
						//set the node state to open
						neighbour.aStar.listState=TileAStar._AStarListState.Open;
						openList.Add(neighbour);
					}
				}
				
				currentTile=null;
				
				currentLowestF=Mathf.Infinity;
				id=0;
				for(i=0; i<openList.Count; i++){
					if(openList[i].aStar.scoreF<currentLowestF){
						currentLowestF=openList[i].aStar.scoreF;
						currentTile=openList[i];
						id=i;
					}
				}
				
				if(currentTile==null) return -1;
				
				openList.RemoveAt(id);
			}
			
			
			int counter=0;
			while(currentTile!=null){
				counter+=1;
				currentTile=currentTile.aStar.parent;
			}
			
			ResetGraph(targetTile, openList, closeList);
			
			return counter-1;
		}
		
		
		
		private static List<Vector3> InvertArray(List<Vector3> p){
			List<Vector3> pInverted=new List<Vector3>();
			for(int i=0; i<p.Count; i++){
				pInverted.Add(p[p.Count-(i+1)]);
			}
			return pInverted;
		}
		
		private static List<Tile> InvertTileArray(List<Tile> p){
			List<Tile> pInverted=new List<Tile>();
			for(int i=0; i<p.Count; i++){
				pInverted.Add(p[p.Count-(i+1)]);
			}
			return pInverted;
		}
		
		
		//reset all the tile, called after a search is complete
		private static void ResetGraph(Tile hTile, List<Tile> oList, List<Tile> cList){
			hTile.aStar.listState=TileAStar._AStarListState.Unassigned;
			hTile.aStar.parent=null;
			
			foreach(Tile tile in oList){
				tile.aStar.listState=TileAStar._AStarListState.Unassigned;
				tile.aStar.parent=null;
			}
			foreach(Tile tile in cList){
				tile.aStar.listState=TileAStar._AStarListState.Unassigned;
				tile.aStar.parent=null;
			}
		}
	}
	
	
	
	
	
	public class PathSmoothing{
		public static List<Tile> SmoothDiagonal(List<Tile> srcPath){
			for(int i=0; i<srcPath.Count-2; i++){
				Vector3 dir=srcPath[i].GetPos()-srcPath[i+2].GetPos();
				if(dir.x*dir.z==0) continue;
				
				if(HasObstacle(srcPath[i], srcPath[i+2])) continue;
				
				List<Tile> neighbourList1=srcPath[i].GetNeighbourList();
				List<Tile> neighbourList2=srcPath[i+2].GetNeighbourList();
				List<Tile> commonNeighbourList=new List<Tile>();
				
				for(int n=0; n<neighbourList1.Count; n++){
					for(int m=0; m<neighbourList2.Count; m++){
						if(neighbourList1[n]==neighbourList2[m]){
							if(!commonNeighbourList.Contains(neighbourList2[m])) 
								commonNeighbourList.Add(neighbourList2[m]);
						}
					}
				}
				
				bool blocked=false;
				for(int n=0; n<commonNeighbourList.Count; n++){
					if(!commonNeighbourList[n].walkable || commonNeighbourList[n].unit!=null){
						blocked=true; 	break;
					}
				}
				
				if(blocked) continue;
					
				srcPath.RemoveAt(i+1);
			}
			return srcPath;
		}
		
		public static bool HasObstacle(Tile tile1, Tile tile2){
			Vector3 pos1=tile1.GetPos();
			Vector3 pos2=tile2.GetPos();
			
			float dist=Vector3.Distance(pos2, pos1);
			Vector3 dir=(pos2-pos1).normalized;
			Vector3 dirO=new Vector3(-dir.z, 0, dir.x).normalized;
			float posOffset=GridManager.GetTileSize()*GridManager.GetGridToTileSizeRatio()*0.4f;
			
			LayerMask mask=1<<TBTK.GetLayerObstacleFullCover() | 1<<TBTK.GetLayerObstacleHalfCover();
			
			if(Physics.Raycast(pos1, dir, dist, mask)) return true;
			if(Physics.Raycast(pos1+dirO*posOffset, dir, dist, mask)) return true;
			if(Physics.Raycast(pos1-dirO*posOffset, dir, dist, mask)) return true;
			
			return false;
		}
		
		
		//smooth an existing path into a list of waypoint with higher sampling rate
		//not in used
		/*
		public static List<Vector3> Smooth(List<Tile> path){
			if(path.Count==1) return new List<Vector3>{ path[0].GetPos()};
			
			float tileSize=GridManager.GetTileSize();
			
			List<Vector3> wpList=new List<Vector3>();
			for(int i=0; i<path.Count; i++){
				if(i==0 || i==path.Count-1) wpList.Add(path[i].GetPos());
				else{
					Vector3 dirR=(path[i-1].GetPos()-path[i].GetPos()).normalized;
					Vector3 dirF=(path[i+1].GetPos()-path[i].GetPos()).normalized;
					
					wpList.Add(path[i].GetPos()+dirR*tileSize*0.35f);
					wpList.Add(path[i].GetPos());
					wpList.Add(path[i].GetPos()+dirF*tileSize*0.35f);
				}
			}
			
			for(int i=0; i<wpList.Count; i++){
				if(i!=0 && i!=wpList.Count-1){
					wpList[i]=(wpList[i-1]+wpList[i]+wpList[i+1])/3;
				}
			}
			
			for(int i=0; i<wpList.Count; i++){
				//path[i].path=new List<Vector3>{ wpList[i] };
				if(i!=0 && i!=wpList.Count-1){
					if((i-1)%3==0){
						List<Vector3> list=new List<Vector3>{wpList[i-1], wpList[i], wpList[i+1]};
						path[(i-1)/3].path=list;
					}
				}
				else{
					if(i==0) path[0].ResetPath();
					else if(i==wpList.Count-1) path[path.Count-1].ResetPath();
				}
			}
			
			for(int i=1; i<wpList.Count; i++){
				//Vector3 offset=new Vector3(0, i, 0);
				//Debug.DrawLine(wpList[i-1]+offset, wpList[i]+offset, Color.blue, 3);
				Debug.DrawLine(wpList[i-1], wpList[i], Color.blue, 3);
			}
			
			return wpList;
		}
		*/
		
	}

	
}