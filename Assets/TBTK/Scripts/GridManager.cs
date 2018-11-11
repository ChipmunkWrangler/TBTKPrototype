using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{

	public class GridManager : MonoBehaviour {
		
		public bool generateGridOnStart=false;
		
		public _TileType tileType=_TileType.Hex;
		public static _TileType GetTileType(){ return instance.tileType; }
		
		public enum _GridColliderType{ Master, Individual }
		public _GridColliderType gridColliderType=_GridColliderType.Master;
		
		public bool enableDiagonalNeighbour=false;	//for square tile only
		public static bool EnableDiagonalNeighbour(){ return instance.enableDiagonalNeighbour; }
		
		
		public int width=5;
		public int length=5;
		public float tileSize=1;
		public static float GetTileSize(){ return instance.grid.GetTileSize(); }
		
		public float gridToTileRatio=1;
		public static float GetGridToTileSizeRatio(){ return instance.grid.GetGridToTileRatio(); }
		
		public float unwalkableRate=0;
		
		
		
		
		//the prefab for obstacle
		public Transform obstacleWallH;
		public Transform obstacleWallF;
		public Transform obstacleHexF;
		public Transform obstacleHexH;
		public Transform obstacleSqF;
		public Transform obstacleSqH;
		public static Transform GetWallObstacleT(int type=1){ //1-half, 2-full
			return type==1 ? instance.obstacleWallH : instance.obstacleWallF ;
		}
		public static Transform GetObstacleT(int type=1){
			if(instance.tileType==_TileType.Hex) return type==1 ? instance.obstacleHexH : instance.obstacleHexF ;
			if(instance.tileType==_TileType.Square) return type==1 ? instance.obstacleSqH : instance.obstacleSqF ;
			return null;
		}
		
		
		//the prefab for cursor and indicators
		public Transform hexCursor;
		public Transform hexSelected;
		public Transform hexHostile;
		public Transform sqCursor;
		public Transform sqSelected;
		public Transform sqHostile;
		//active cursor and indicator in used during runtime
		private Transform indicatorCursor;	
		private Transform indicatorSelected;
		//private List<Transform> indicatorHostileList=new List<Transform>();
		
		
		//on grid overlays for cover
		public List<Transform> coverHOverlayList=new List<Transform>();
		public List<Transform> coverFOverlayList=new List<Transform>();
		
		
		//material for each individual tile
		public Material hexMatNormal;
		public Material hexMatSelected;
		public Material hexMatWalkable;
		public Material hexMatUnwalkable;
		public Material hexMatHostile;
		public Material hexMatRange;
		public Material hexMatAbilityAll;
		public Material hexMatAbilityHostile;
		public Material hexMatAbilityFriendly;
		public Material hexMatInvisible;
		
		public Material sqMatNormal;
		public Material sqMatSelected;
		public Material sqMatWalkable;
		public Material sqMatUnwalkable;
		public Material sqMatHostile;
		public Material sqMatRange;
		public Material sqMatAbilityAll;
		public Material sqMatAbilityHostile;
		public Material sqMatAbilityFriendly;
		public Material sqMatInvisible;
		
		public static Material GetMatNormal(){ 		return instance._GetMatNormal(); }
		public static Material GetMatSelected(){ 	return instance._GetMatSelected(); }
		public static Material GetMatWalkable(){ 	return instance._GetMatWalkable(); }
		public static Material GetMatUnwalkable(){ return instance._GetMatUnwalkable(); }
		public static Material GetMatHostile(){ 		return instance._GetMatHostile(); }
		public static Material GetMatRange(){ 		return instance._GetMatRange(); }
		public static Material GetMatAbilityAll(){ 	return instance._GetMatABAll(); }
		public static Material GetMatAbilityHostile(){ 	return instance._GetMatABHostile(); }
		public static Material GetMatAbilityFriendly(){ return instance._GetMatABFriendly(); }
		public static Material GetMatInvisible(){ 	return instance._GetMatInvisible(); }
		
		public Material _GetMatNormal(){ 		return tileType==_TileType.Hex ? hexMatNormal : sqMatNormal; }
		public Material _GetMatSelected(){ 	return tileType==_TileType.Hex ? hexMatSelected : sqMatSelected; }
		public Material _GetMatWalkable(){ 	return tileType==_TileType.Hex ? hexMatWalkable : sqMatWalkable; }
		public Material _GetMatUnwalkable(){ return tileType==_TileType.Hex ? hexMatUnwalkable : sqMatUnwalkable; }
		public Material _GetMatHostile(){ 		return tileType==_TileType.Hex ? hexMatHostile : sqMatHostile; }
		public Material _GetMatRange(){ 		return tileType==_TileType.Hex ? hexMatRange : sqMatRange; }
		public Material _GetMatABAll(){ 		return tileType==_TileType.Hex ? hexMatAbilityAll : sqMatAbilityAll; }
		public Material _GetMatABHostile(){ 	return tileType==_TileType.Hex ? hexMatAbilityHostile : sqMatAbilityHostile; }
		public Material _GetMatABFriendly(){ 	return tileType==_TileType.Hex ? hexMatAbilityFriendly : sqMatAbilityFriendly; }
		public Material _GetMatInvisible(){ 	return tileType==_TileType.Hex ? hexMatInvisible : sqMatInvisible; }
		
		
		//the grid instance which contains the current grid in scene
		public Grid grid=null;
		public Grid GetGrid(){ return grid; }
		public static List<Tile> GetTileList(){ return instance.grid.tileList; }
		public static GameObject GetGridObject(){ return instance.grid.GetGridObject(); }
		
		//temporarily tile list for selected unit storing attackable and walkable tiles, reset when a new unit is selected
		private List<Tile> walkableTileList=new List<Tile>();
		private List<Tile> attackableTileList=new List<Tile>();
		
		private static GridManager instance;
		//public static void SetInstance(){ if(instance==null) instance=(GridManager)FindObjectOfType(typeof(GridManager)); }
		public void SetInstance(){ instance=this; }	//called in GridEditor
		public static GridManager GetInstance(){ return instance; }
		
		void Awake(){
			if(instance==null) instance=this;
		}	
		
		// initiate all the indicators and overlay
		void Start () {
			ClearAllTile();
		}
		
		
		//called by GameControl at the start of a scene
		public void Init(){
			if(instance==null) instance=this;
			
			if(generateGridOnStart) GenerateGrid();
			
			grid.Init();
			
			if(gridColliderType==_GridColliderType.Master) GridGenerator.CombineMeshForGrid(grid.gridObj.transform);
		}
		
		//called by GameControl to setup the grid for Fog-of-war
		public static void SetupGridForFogOfWar(){ FogOfWar.InitGrid(instance.grid.tileList); }
		
		
		
		public static void OnUnitDestroyed(Unit unit){ instance._OnUnitDestroyed(unit); }
		void _OnUnitDestroyed(Unit unit){
			if(GameControl.GetSelectedUnit()==null) return;
			
			Tile tile=unit.tile;
			if(attackableTileList.Contains(tile)){
				attackableTileList.Remove(tile);	//remove from target tile
				OverlayManager.RemoveHostileIndicator(tile);
			}
			
			int dist=GetDistance(tile, GameControl.GetSelectedUnit().tile, true);
			
			if(dist>0 && dist<GameControl.GetSelectedUnit().GetMoveRange()){	//if within walkable distance, add to walkable tile since the tile is now open
				walkableTileList.Add(tile);
				OverlayManager.ShowMoveableIndicator(walkableTileList);
			}
		}
		
		
		public static Tile GetTileOnCursor(Vector3 cursorPos){
			LayerMask mask=1<<TBTK.GetLayerTile() | 1<<TBTK.GetLayerTerrain();
			Ray ray = Camera.main.ScreenPointToRay(cursorPos);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
				//Debug.Log(hit.collider.gameObject);
				return GetTileOnPos(hit.point);
			}
			return null;
		}
		
		
		public Tile tile1;
		public Tile tile2;
		
		// Update is called once per frame
		private Tile hoveredTile;
		public static Tile GetHoveredTile(){ return instance.hoveredTile; }
		
		//private int cursorID=-1;
		private bool cursorDown;
		private bool invalidCursor;
		private Vector3 cursorPosition;
		void Update() {
			
			invalidCursor=false;
			//cursorID=-1;
			cursorPosition=Input.mousePosition;
			
			if(Input.touchCount>1) invalidCursor=true;
			else if(Input.touchCount==1){
				cursorPosition=TBTK.GetFirstTouchPosition();
				//cursorID=0;
			}
			
			
			if(!invalidCursor){
				Tile newTile=GetTileOnCursor(cursorPosition);
				if(newTile!=hoveredTile){
					if(newTile==null && hoveredTile!=null) ClearHoveredTile();
					else{
						ClearHoveredTile();
						NewHoveredTile(newTile);
						
						//for debug
						//DebugDrawNeighbour(hoveredTile);
						//CoverSystem.GetCoverType(GameControl.GetSelectedUnit().tile, hoveredTile, true);
					}
				}
			}
		}
		
		
		//for debug only
		public static void DebugDrawNeighbour(Tile tile){
			List<Tile> tileList=tile.GetNeighbourList();
			for(int i=0; i<tileList.Count; i++){
				Debug.DrawLine(tile.GetPos(), tileList[i].GetPos(), Color.white, 1f);
			}
			
			List<Tile> dcTileList=tile.GetDCNeighbourList();
			for(int i=0; i<dcTileList.Count; i++){
				Debug.DrawLine(tile.GetPos(), dcTileList[i].GetPos(), Color.red, 1f);
			}
		}
		
		
		public static void OnCursorDown(int cursorID=-1){ instance._OnCursorDown(cursorID); }
		public void _OnCursorDown(int cursorID=-1){
			//Debug.Log("_OnCursorDown");
			
			if(hoveredTile==null) return;
			
			if(TBTK.IsCursorOnUI(cursorID)) return;
			
			if(!TurnControl.ClearToProceed()) return;
			
			if(GameControl.GetGamePhase()==_GamePhase.Over) return;
			
			if(GameControl.GetGamePhase()==_GamePhase.UnitDeployment){
				if(hoveredTile.unit==null) FactionManager.DeployUnitOnTile(hoveredTile);
				else if(hoveredTile.unit!=null) FactionManager.UndeployUnit(hoveredTile.unit);
				return;
			}
			
			if(AbilityManager.InTargetMode()){
				Debug.Log("AbilityTargetSelected");
				targetModeTargetSelected(hoveredTile);
			}
			else OnTile(hoveredTile);
		}
		
		
		//call when cursor just hover over a new tile
		public static void NewHoveredTile(Tile tile){ instance._NewHoveredTile(tile); }
		void _NewHoveredTile(Tile tile){
			hoveredTile=tile;
			
			if(AbilityManager.InTargetMode()){
				SetTargetModeHoveredTile(tile);
				return;
			}
			
			bool isWalkable=walkableTileList.Contains(tile);
			
			bool isSelectedUnitTile=GameControl.GetSelectedUnit()==null ? false : true;
			if(isSelectedUnitTile) isSelectedUnitTile=GameControl.GetSelectedUnit().tile!=tile ? false : true ;
			
			//show cover overlay if cover-system is enabled
			if(GameControl.EnableCover() && (isWalkable || isSelectedUnitTile)){
				OverlayManager.ShowCoverOverlay(tile);
			}
			
			//highlight potential target for the unit to be moved into this tile
			if(isWalkable && GameControl.GetSelectedUnit().CanAttack()){
				SetWalkableHostileList(tile);
			}
			
			TBTK.OnHoverTile(tile);
		}
		
		//cleared the tile which has just been hovered over by the cursor
		public static void ClearHoveredTile(){ instance._ClearHoveredTile(); }
		void _ClearHoveredTile(){
			if(hoveredTile!=null){
				ClearWalkableHostileList();
			}
			
			ShowHostileIndicator(attackableTileList);
			
			hoveredTile=null;
			
			OverlayManager.HideCoverOverlay();
			
			if(AbilityManager.InTargetMode()) ClearTargetModeHoveredTile();
			
			TBTK.OnHoverTile(null);
		}
		
		
		//for when hover over a walkable tile, show the potential target if move into that tile
		void SetWalkableHostileList(Tile tile){
			ClearHostileIndicator();
			
			List<Tile> tempAttackableTileList=tile.GetHostileInRange();
			for(int i=0; i<tempAttackableTileList.Count; i++){
				if(!tempAttackableTileList[i].IsVisible()){
					tempAttackableTileList.RemoveAt(i);	i-=1;
				}
			}
			ShowHostileIndicator(tempAttackableTileList);
		}
		void ClearWalkableHostileList(){ ClearHostileIndicator(); }
		
		
		//**********************************************************************************************************************
		//these section are related to target tile selecting when using abilities
		public delegate void TargetModeCB(Tile tile);
		private TargetModeCB targetModeSelectCallBack;
		private TargetModeCB targetModeHoverCallBack;
		private TargetModeCB targetModeExitCallBack;
		
		public static void AbilityTargetSelectMode(TargetModeCB sCallBack, TargetModeCB hCallBack, TargetModeCB eCallBack){
			//instance.targetMode=true;
			instance.targetModeSelectCallBack=sCallBack;
			instance.targetModeHoverCallBack=hCallBack;
			instance.targetModeExitCallBack=eCallBack;
			
			ClearAllTile();
		}
		public static void ClearTargetSelectMode(){
			//instance.targetMode=false;
			instance.targetModeSelectCallBack=null;
			instance.targetModeHoverCallBack=null;
			instance.targetModeExitCallBack=null;
			
			OverlayManager.EnableTileCursor();
		}
		
		private void SetTargetModeHoveredTile(Tile tile){
			if(targetModeHoverCallBack!=null) targetModeHoverCallBack(tile);
		}
		private void ClearTargetModeHoveredTile(){
			targetModeExitCallBack(null);
		}
		
		private void targetModeTargetSelected(Tile tile){
			if(targetModeSelectCallBack!=null) targetModeSelectCallBack(tile);
		}
		
		//end target mode related function
		//************************************************************************
		
		
		
		//calculate the tile in the grid based on a position in the world space
		public static Tile GetTileOnPos(Vector3 pos){ //the static function is only called by GridEditor
			return instance!=null ? instance._GetTileOnPos(pos) : null;
		}
		public Tile _GetTileOnPos(Vector3 point){
			Tile tile=null;
			
			Vector3 cPoint=grid.GetGridCenterPoint();
			
			int gridOffsetX=width/2;
			int gridOffsetZ=length/2;
			
			if(tileType==_TileType.Hex){
				float spaceX=GridGenerator.spaceXHex*tileSize*gridToTileRatio;
				float spaceZ=GridGenerator.spaceZHex*tileSize*gridToTileRatio;
				
				//for symetry hex-grid
				float offX=width%2==1 ? spaceX/2 : 0;			//depends on the with of the gird, set the offset of x-axis
				int column=(int)Mathf.Floor((point.x+offX-cPoint.x)/spaceX)+gridOffsetX;

				float offZ=column%2==1 ? spaceZ : spaceZ/2;		//depends on the column, introduce a offset of half a tile (odd number column has more row)
				if(length%2==1) offZ-=spaceZ/2;					//depends on the length of the grid, modify the offset
				int row=(int)Mathf.Floor((point.z-offZ-cPoint.z)/spaceZ)+gridOffsetZ;
				
				int tileID=column*length+row-column/2;
				
				
				/* //for non-symetry hex-grid, not working?
				float offX=width%2==1 ? spaceX/2 : 0;			//depends on the with of the gird, set the offset of x-axis
				int column=(int)Mathf.Floor((point.x+offX-cPoint.x)/spaceX)+gridOffsetX;

				float offZ=column%2==1 ? 0 : spaceZ/2;	//depends on the column, introduce a offset of half a tile (odd number column has more row)
				int row=(int)Mathf.Floor((point.z-offZ-cPoint.z)/spaceZ)+gridOffsetZ;
				
				int tileID=column*length+row;
				*/
				
				
				//Debug.Log(tileID);
				//Debug.DrawLine(grid.tileList[tileID].GetPos(), grid.tileList[tileID].GetPos()+new Vector3(.5f, 0, 0), Color.red, .5f);
				
				if(tileID<0 || tileID>=grid.tileList.Count) return null;
				
				tile=grid.tileList[tileID];
				
				//Debug.Log(Vector3.Distance(tile.GetPos(), point)+"    "+(GridGenerator.spaceZHex*tileSize));
				if(Vector3.Distance(tile.GetPos(), point)>GridGenerator.spaceZHex*tileSize*.5f) return null;
			}
			else if(tileType==_TileType.Square){
				float spaceX=tileSize*gridToTileRatio;
				float spaceZ=tileSize*gridToTileRatio;
				
				float offX=width%2==1 ? spaceX/2 : 0;	//depends on the with of the gird, set the offset of x-axis
				float offZ=length%2==1 ? spaceZ/2 : 0;	//depends on the length of the grid, introduce a offset of half a tile
				
				int column=(int)Mathf.Floor((point.x+offX-cPoint.x)/spaceX)+gridOffsetX;
				int row=(int)Mathf.Floor((point.z+offZ-cPoint.z)/spaceZ)+gridOffsetZ;
				int tileID=column*length+row;
				
				if(tileID<0 || tileID>=grid.tileList.Count) return null;
				
				tile=grid.tileList[tileID];
				
				if(Vector3.Distance(tile.GetPos(), point)>tileSize*.65f) return null;
			}
			
			return tile;
		}
		
		
		
		public void GenerateGrid(){
			Debug.Log("generate grid");
			
			FactionManager factionManager=FactionManager.SetInstance();
			
			if(factionManager!=null){
				factionManager.ClearUnit();
				factionManager.RecordSpawnTilePos();	//this is to record the tile of the spawn and deploy area
			}
			
			if(grid!=null) grid.ClearGrid();
			
			if(tileType==_TileType.Hex){
				grid=GridGenerator.GenerateHexGrid(width, length, tileSize, gridToTileRatio, unwalkableRate, gridColliderType);
			}
			else if(tileType==_TileType.Square){
				grid=GridGenerator.GenerateSquareGrid(width, length, tileSize, gridToTileRatio, unwalkableRate, gridColliderType);
			}
			
			if(grid.gridObj!=null) grid.gridObj.transform.parent=transform.parent;
			
			if(factionManager!=null){
				factionManager.SetStartingTileListBaseOnPos(tileSize*gridToTileRatio);	//this is to set the tiles of the spawn and deploy area bsaed on the stored info earlier
				if(factionManager.generateUnitOnStart) factionManager._GenerateUnit();
			}
		}
		
		
		//when player click on a particular tile
		public static void OnTile(Tile tile){ instance._OnTile(tile); }
		public void _OnTile(Tile tile){
			if(!FactionManager.IsPlayerTurn()) return;
			
			if(tile.unit!=null){
				if(attackableTileList.Contains(tile)) GameControl.GetSelectedUnit().Attack(tile.unit);
				else GameControl.SelectUnit(tile.unit);
			}
			//if the tile is within the move range of current selected unit, move to it
			else if(walkableTileList.Contains(tile)){
				ClearWalkableHostileList();	//in case the unit move into the destination and has insufficient ap to attack
				GameControl.GetSelectedUnit().Move(tile);
			}
			
			ClearHoveredTile();	//clear the hovered tile so all the UI overlay will be cleared
		}
		
		
		//when player right-click on a particular tile
		//only used to set unit, facing
		public static void OnTileAlt(Tile tile){ instance._OnTileAlt(tile); }
		public void _OnTileAlt(Tile tile){
			if(!FactionManager.IsPlayerTurn()) return;
			
			//change the unit facing
			/*
			if(GameControl.selectedUnit!=null){
				if(tile==GameControl.selectedUnit.tile) return;
				
				float x=GameControl.selectedUnit.tile.GetPos().x-tile.GetPos().x;
				float z=GameControl.selectedUnit.tile.GetPos().z-tile.GetPos().z;
				Vector2 dir=new Vector2(x, z);
				
				float angle=Utilities.Vector2ToAngle(dir);
				
				GameControl.selectedUnit.Rotate(Quaternion.Euler(0, 360-angle-90, 0));
			}
			*/
		}
		
		//select a unit, setup the walkable, attackable tiles and what not
		public static void SelectUnit(Unit unit){
			ClearAllTile();
			//unit.tile.SetState(_TileState.Selected);
			if(unit.CanMove()) instance.SetupWalkableTileList(unit);
			if(unit.CanAttack()) instance.SetupAttackableTileList(unit);
			//instance.indicatorSelected.position=unit.tile.GetPos();
			OverlayManager.SelectUnit(unit);
		}
		
		//function to setup and clear walkable tiles in range for current selected unit
		private void ClearWalkableTileList(){
			for(int i=0; i<walkableTileList.Count; i++){
				//walkableTileList[i].SetState(_TileState.Default);
				walkableTileList[i].hostileInRangeList=new List<Tile>();
				walkableTileList[i].distance=0;
			}
			walkableTileList=new List<Tile>();
			OverlayManager.ClearMoveableIndicator();
		}
		private void SetupWalkableTileList(Unit unit){
			ClearWalkableTileList();
			List<Tile> newList=GetTilesWithinDistance(unit.tile, unit.GetEffectiveMoveRange(), true, true);
			for(int i=0; i<newList.Count; i++){
				if(newList[i].unit==null){
					walkableTileList.Add(newList[i]);
					//newList[i].SetState(_TileState.Walkable);
				}
			}
			SetupHostileInRangeforTile(unit, walkableTileList);
			OverlayManager.ShowMoveableIndicator(walkableTileList);
		}
		
		
		//function to setup and clear attackble tiles in range for current selected unit
		private void ClearAttackableTileList(){
			//for(int i=0; i<attackableTileList.Count; i++) attackableTileList[i].SetState(_TileState.Default);
			attackableTileList=new List<Tile>();
		}
		private void SetupAttackableTileList(Unit unit){
			ClearAttackableTileList();
			attackableTileList=unit.tile.SetupHostileInRange();
			//for(int i=0; i<attackableTileList.Count; i++) attackableTileList[i].SetState(_TileState.Hostile);
			
			ShowHostileIndicator(attackableTileList);
		}
		
		
		//given a unit and a list of tiles, setup the attackable tiles with that unit in each of those given tiles. the attackble tile list are stored in each corresponding tile
		public static void SetupHostileInRangeforTile(Unit unit, Tile tile){ SetupHostileInRangeforTile(unit, new List<Tile>{ tile }); }
		public static void SetupHostileInRangeforTile(Unit unit, List<Tile> tileList){
			List<Unit> allUnitList=FactionManager.GetAllUnit();
			List<Unit> allHostileUnitList=new List<Unit>();
			for(int i=0; i<allUnitList.Count; i++){
				if(allUnitList[i].factionID!=unit.factionID) allHostileUnitList.Add(allUnitList[i]);
			}
			
			List<Unit> allFriendlyUnitList=new List<Unit>();
			if(GameControl.EnableFogOfWar()) allFriendlyUnitList=FactionManager.GetAllUnitsOfFaction(unit.factionID);
			
			int range=unit.GetAttackRange();
			int rangeMin=unit.GetAttackRangeMin();
			int sight=unit.GetSight();
			
			for(int i=0; i<tileList.Count; i++){
				Tile srcTile=tileList[i];
				List<Tile> hostileInRangeList=new List<Tile>();
				
				for(int j=0; j<allHostileUnitList.Count; j++){
					Tile targetTile=allHostileUnitList[j].tile;
					
					if(GridManager.GetDistance(srcTile, targetTile)>range) continue;
					if(GridManager.GetDistance(srcTile, targetTile)<rangeMin) continue;
					
					if(!GameControl.EnableFogOfWar() && !GameControl.AttackThroughObstacle()){
						if(!FogOfWar.InLOS(srcTile, targetTile, 0)) continue;
					}
					
					bool inSight=GameControl.EnableFogOfWar() ? false : true;
					if(GameControl.EnableFogOfWar()){
						if(FogOfWar.InLOS(srcTile, targetTile) && GridManager.GetDistance(srcTile, targetTile)<=sight){
							inSight=true;
						}
						else if(!unit.requireDirectLOSToAttack){
							for(int n=0; n<allFriendlyUnitList.Count; n++){
								if(allFriendlyUnitList[n]==unit) continue;
								if(GridManager.GetDistance(allFriendlyUnitList[n].tile, targetTile)>allFriendlyUnitList[n].GetSight()) continue;
								if(FogOfWar.InLOS(allFriendlyUnitList[n].tile, targetTile)){
									inSight=true;
									break;
								}
							}
						}
					}
					
					if(inSight) hostileInRangeList.Add(targetTile);
					
				}
				
				tileList[i].SetHostileInRange(hostileInRangeList);
			}
		}
		
		
		//reset all selection, walkablelist and what not
		public static void ClearAllTile(){
			instance.ClearWalkableTileList();
			instance.ClearAttackableTileList();
			instance.ClearWalkableHostileList();
			
			OverlayManager.ClearSelection();
		}
		
		
		
		public void ClearHostileIndicator(){ OverlayManager.ClearHostileIndicator(); }
		public void ShowHostileIndicator(List<Tile> list){ OverlayManager.ShowHostileIndicator(list); }
		
		
		
		
		//called to setup tile for player's faction unit deployment, they need to be highlighted
		private List<Tile> currentDeployableTileList=new List<Tile>();
		public static void DeployingFaction(int factionID){ instance._DeployingFaction(factionID); }
		public void _DeployingFaction(int factionID){
			currentDeployableTileList=_GetDeployableTileList(factionID);
			//for(int i=0; i<currentDeployableTileList.Count; i++) currentDeployableTileList[i].SetState(_TileState.Range);
			OverlayManager.ShowDeploymentIndicator(currentDeployableTileList);
		}
		public static void FactionDeploymentComplete(){ instance._FactionDeploymentComplete(); }
		public void _FactionDeploymentComplete(){
			currentDeployableTileList=new List<Tile>();
			//for(int i=0; i<currentDeployableTileList.Count; i++) currentDeployableTileList[i].SetState(_TileState.Default);
			OverlayManager.ClearDeploymentIndicator();
		}
		//get delployable tile list for certain faction
		public static List<Tile> GetDeployableTileList(int factionID){ return instance._GetDeployableTileList(factionID); }
		public List<Tile> _GetDeployableTileList(int factionID){
			List<Tile> deployableTileList=new List<Tile>();
			for(int i=0; i<grid.tileList.Count; i++){
				if(grid.tileList[i].deployAreaID==factionID) deployableTileList.Add(grid.tileList[i]);
			}
			return deployableTileList;
		}
		public static int GetDeployableTileListCount(){ return instance._GetDeployableTileListCount(); }
		public int _GetDeployableTileListCount(){
			int count=0;
			for(int i=0; i<currentDeployableTileList.Count; i++){
				if(currentDeployableTileList[i].unit==null) count+=1;
			}
			return count;
		}
		
		
		
		
		public static bool CanAttackTile(Tile tile){ return instance.attackableTileList.Contains(tile); }
		public static bool CanMoveToTile(Tile tile){ return instance.walkableTileList.Contains(tile); }
		public static bool CanCastAbility(Tile tile){ return AbilityManager.CanCastAbility(tile); }
		
		public static bool CanPerformAction(Tile tile){
			return CanAttackTile(tile) | CanMoveToTile(tile) | CanCastAbility(tile);
		}
		
		
		//to get all the tiles within certain distance from a particular tile
		//set distance is only used when called from SetupWalkableTileList (to record the distance of each tile from the source tile)
		public static List<Tile> GetTilesWithinDistance(Tile tile, int dist, bool walkableOnly=false, bool setDistance=false, int minDist=0){
			return instance.grid.GetTilesWithinDistance(tile, dist, walkableOnly, setDistance, minDist);
		}
		
		
		//get the distance (in term of tile) between 2 tiles, 
		public static int GetDistance(Tile tile1, Tile tile2, bool walkable=false){
			if(!walkable) return instance.grid.GetDistance(tile1, tile2);
			else return instance.grid.GetWalkableDistance(tile1, tile2);
		}
		
		
		public static float GetAngleFromTileToCursor(Tile targetTile){
			float angle=0;
			LayerMask mask=1<<TBTK.GetLayerTile();
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
				Vector3 dir=(hit.point-targetTile.GetPos());
				if(GetTileType()==_TileType.Hex) angle=90-Utilities.VectorToAngle60(new Vector2(dir.x, dir.z));
				if(GetTileType()==_TileType.Square) angle=90-Utilities.VectorToAngle90(new Vector2(dir.x, dir.z));
			}
			return angle;
		}
		
		
		
		
		
		
		public static List<Tile> GetTilesInALine(Tile srcTile, Tile tgtTile, int range, bool walkableOnly=true){
			float angle=0;
			
			if(tgtTile==null) angle=GetAngleFromTileToCursor(srcTile);
			else{
				Vector3 dir=(tgtTile.GetPos()-srcTile.GetPos());
				if(GetTileType()==_TileType.Hex) angle=Utilities.VectorToAngle60(new Vector2(dir.x, dir.z));
				if(GetTileType()==_TileType.Square) angle=Utilities.VectorToAngle90(new Vector2(dir.x, dir.z));
			}
			
			List<Tile> tileList=new List<Tile>();
			
			Tile nextTile=null;
			for(int i=0; i<range; i++){
				nextTile=srcTile.GetNeighbourFromAngle(angle);
				
				if(walkableOnly && !nextTile.walkable) break;
				
				if(nextTile!=null ){
					tileList.Add(nextTile);
					srcTile=nextTile;
				}
				else break;
			}
			
			return tileList;
		}
		
	}
	
}
