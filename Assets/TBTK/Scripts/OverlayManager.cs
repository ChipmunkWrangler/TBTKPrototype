using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{

	public class OverlayManager : MonoBehaviour {

		private Transform thisT;
		
		private int maxUnitCount;
		
		private Transform cursorIndicatorF;
		private Transform cursorIndicatorH;
		private Transform selectIndicator;
		
		private List<Transform> deploymentIndicatorList=new List<Transform>();
		private List<Transform> moveableIndicatorList=new List<Transform>();
		private List<Transform> hostileIndicatorList=new List<Transform>();
		private List<Transform> abRangeIndicatorList=new List<Transform>();
		private List<Transform> abTargetIndicatorList=new List<Transform>();
		
		private List<Transform> movedIndicatorList=new List<Transform>();
		
		[Tooltip("Indicator to show which unit has depleted it's moved")]
		public Transform movedIndicator;
		
		[Header("Hex Indicator")]
		[Tooltip("For cursor hovered over friendly unit")]
		public Transform hexCursorF;
		[Tooltip("For cursor hovered over hostile unit")]
		public Transform hexCursorH;
		[Tooltip("To show on the tile with slected unit")]
		public Transform hexSelected;
		[Tooltip("To show on tiles available for unit deployment")]
		public Transform hexDeployment;
		[Tooltip("To show on empty tiles the selected unit can move into")]
		public Transform hexMoveable;
		[Tooltip("To show on tiles containing hostile the selected unit can attack")]
		public Transform hexHostile;
		[Tooltip("To show on tiles within the range of an ability when selecting a target for ability")]
		public Transform hexAbRange;
		[Tooltip("To show on tiles where the ability can be cast on when selecting a target for ability")]
		public Transform hexAbTarget;
		
		[Header("Square Indicator")]
		[Tooltip("For cursor hovered over friendly unit")]
		public Transform sqCursorF;
		[Tooltip("For cursor hovered over hostile unit")]
		public Transform sqCursorH;
		[Tooltip("To show on the tile with slected unit")]
		public Transform sqSelected;
		[Tooltip("To show on tiles available for unit deployment")]
		public Transform sqDeployment;
		[Tooltip("To show on empty tiles the selected unit can move into")]
		public Transform sqMoveable;
		[Tooltip("To show on tiles containing hostile the selected unit can attack")]
		public Transform sqHostile;
		[Tooltip("To show on tiles within the range of an ability when selecting a target for ability")]
		public Transform sqAbRange;
		[Tooltip("To show on tiles where the ability can be cast on when selecting a target for ability")]
		public Transform sqAbTarget;
		
		[Header("Fog Indicator")]
		public Transform hexFogObj;
		public Transform sqFogObj;
		
		[Header("Cover Indicator")]
		public Transform coverOverlayH;
		public Transform coverOverlayF;
		
		
		//on grid overlays for cover
		private List<Transform> coverOverlayHList=new List<Transform>();
		private List<Transform> coverOverlayFList=new List<Transform>();
		
		
		private static OverlayManager instance;
		void Awake(){
			instance=this;
			thisT=transform;
		}
		
		void Start(){
			Init();
		}
		
		// Use this for initialization
		private bool init=false;
		public void Init(){
			if(init) return;
			init=true;
			
			thisT=transform;
			
			Transform deploymentIndicator=null;
			Transform moveableIndicator=null;
			Transform hostileIndicator=null;
			Transform abRangeIndicator=null;
			Transform abTargetIndicator=null;
			
			if(GridManager.GetTileType()==_TileType.Hex){
				deploymentIndicator=hexDeployment;
				moveableIndicator=hexMoveable;
				hostileIndicator=hexHostile;
				abRangeIndicator=hexAbRange;
				abTargetIndicator=hexAbTarget;
				cursorIndicatorF=(Transform)Instantiate(hexCursorF);
				cursorIndicatorH=(Transform)Instantiate(hexCursorH);
				selectIndicator=(Transform)Instantiate(hexSelected);
			}
			else if(GridManager.GetTileType()==_TileType.Square){
				deploymentIndicator=sqDeployment;
				moveableIndicator=sqMoveable;
				hostileIndicator=sqHostile;
				abRangeIndicator=sqAbRange;
				abTargetIndicator=sqAbTarget;
				cursorIndicatorF=(Transform)Instantiate(sqCursorF);
				cursorIndicatorH=(Transform)Instantiate(sqCursorH);
				selectIndicator=(Transform)Instantiate(sqSelected);
			}
			
			
			cursorIndicatorF.parent=thisT;
			cursorIndicatorH.parent=thisT;
			selectIndicator.parent=thisT;
			
			
			for(int i=0; i<20; i++){
				deploymentIndicatorList.Add((Transform)Instantiate(deploymentIndicator));
				deploymentIndicatorList[i].gameObject.SetActive(false);
				deploymentIndicatorList[i].parent=thisT;
			}
			for(int i=0; i<20; i++){
				moveableIndicatorList.Add((Transform)Instantiate(moveableIndicator));
				moveableIndicatorList[i].gameObject.SetActive(false);
				moveableIndicatorList[i].parent=thisT;
			}
			for(int i=0; i<10; i++){
				hostileIndicatorList.Add((Transform)Instantiate(hostileIndicator));
				hostileIndicatorList[i].gameObject.SetActive(false);
				hostileIndicatorList[i].parent=thisT;
			}
			
			for(int i=0; i<20; i++){
				abRangeIndicatorList.Add((Transform)Instantiate(abRangeIndicator));
				abRangeIndicatorList[i].gameObject.SetActive(false);
				abRangeIndicatorList[i].parent=thisT;
			}
			for(int i=0; i<20; i++){
				abTargetIndicatorList.Add((Transform)Instantiate(abTargetIndicator));
				abTargetIndicatorList[i].gameObject.SetActive(false);
				abTargetIndicatorList[i].parent=thisT;
			}
			
			
			if(TurnControl.GetTurnMode()==_TurnMode.FactionPerTurn){
				//create the moved indicator
				for(int i=0; i<10; i++){
					movedIndicatorList.Add((Transform)Instantiate(movedIndicator));
					movedIndicatorList[i].gameObject.SetActive(false);
					movedIndicatorList[i].parent=thisT;
				}
			}
			
			
			if(GameControl.EnableFogOfWar()){
				Transform fogObj=null;
			
				if(GridManager.GetTileType()==_TileType.Hex) fogObj=hexFogObj;
				else if(GridManager.GetTileType()==_TileType.Square) fogObj=sqFogObj;
				
				List<Tile> tileList=GridManager.GetTileList();
				for(int i=0; i<tileList.Count; i++){
					//if(!tileList[i].walkable) continue;
					tileList[i].SetFogOfWarObj((Transform)Instantiate(fogObj));
				}
			}
			
			if(GameControl.EnableCover()){
				float scaleOffset=GridManager.GetTileType()==_TileType.Hex ? 0.5f : 0.8f ;
				float tileSize=GridManager.GetTileSize();
				
				for(int i=0; i<5; i++){
					coverOverlayHList.Add((Transform)Instantiate(coverOverlayH));
					coverOverlayFList.Add((Transform)Instantiate(coverOverlayF));
					
					coverOverlayHList[i].localScale*=tileSize*scaleOffset;
					coverOverlayHList[i].parent=thisT;
					coverOverlayHList[i].gameObject.SetActive(false);
					
					coverOverlayFList[i].localScale*=tileSize*scaleOffset;
					coverOverlayFList[i].parent=thisT;
					coverOverlayFList[i].gameObject.SetActive(false);
				}
			}
		}
		
		
		public static void ShowCoverOverlay(Tile tile){ instance._ShowCoverOverlay(tile); }
		public void _ShowCoverOverlay(Tile tile){
			for(int i=0; i<tile.coverList.Count; i++){
				if(!tile.coverList[i].enabled) continue;
				
				Transform overlayT=null;
				if(tile.coverList[i].type==CoverSystem._CoverType.Full) overlayT=coverOverlayFList[i];
				if(tile.coverList[i].type==CoverSystem._CoverType.Half) overlayT=coverOverlayHList[i];
				
				overlayT.position=tile.coverList[i].overlayPos;
				overlayT.rotation=tile.coverList[i].overlayRot;
				
				overlayT.gameObject.SetActive(true);
			}
		}
		public static void HideCoverOverlay(){ instance._HideCoverOverlay(); }
		public void _HideCoverOverlay(){
			for(int i=0; i<coverOverlayHList.Count; i++) coverOverlayHList[i].gameObject.SetActive(false);
			for(int i=0; i<coverOverlayFList.Count; i++) coverOverlayFList[i].gameObject.SetActive(false);
		}
		
		
		
		void Update(){
			TileCursor();
		}
		
		
		private bool enableTileCursor=true;
		public static void EnableTileCursor(){ instance.enableTileCursor=true; }
		public static void DisableTileCursor(){ instance.enableTileCursor=false; }
		private void TileCursor(){
			if(!enableTileCursor) return;
			
			if(GridManager.GetHoveredTile()==null){
				cursorIndicatorF.gameObject.SetActive(false);
				cursorIndicatorH.gameObject.SetActive(false);
			}
			else{
				
				cursorIndicatorF.position=GridManager.GetHoveredTile().GetPos()+new Vector3(0, 0.05f, 0);
				cursorIndicatorH.position=GridManager.GetHoveredTile().GetPos()+new Vector3(0, 0.05f, 0);
				
				bool attackable=GridManager.CanAttackTile(GridManager.GetHoveredTile());
				
				//Tile currentTile=GameControl.GetSelectedUnit()!=null ? GameControl.GetSelectedUnit().tile : null ;
				//bool selected=GridManager.GetHoveredTile()==currentTile;
				
				cursorIndicatorH.gameObject.SetActive(attackable);// & !selected);
				cursorIndicatorF.gameObject.SetActive(!attackable);// & !selected);
			}
			
			//~ Tile tile=GridManager.GetHoveredTile(Input.mousePosition);
			//~ bool showIndicator=(tile!=null);// && tile.gameObject.activeInHierarchy);
			//~ if(tile!=null) cursorIndicator.position=tile.GetPos();
			//~ cursorIndicator.gameObject.SetActive(showIndicator);
		}
		
		
		
		
		//new turn, clear all indicator
		private void OnNewTurn(){
			if(TurnControl.GetTurnMode()!=_TurnMode.FactionPerTurn) return;
			
			for(int i=0; i<movedIndicatorList.Count; i++){
				movedIndicatorList[i].gameObject.SetActive(false);
			}
		}
		
		
		//when a unit completes it's move, put an unused indicator on the unit tile
		//this will only be called if the unit in question is player unit
		private void OnUnitMoveDepleted(Unit unit){
			if(TurnControl.GetTurnMode()!=_TurnMode.FactionPerTurn) return;
			
			int index=-1;
			for(int i=0; i<movedIndicatorList.Count; i++){
				if(movedIndicatorList[i].gameObject.activeInHierarchy){
					index=i;
					break;//continue;
				}
			}
			
			if(index==-1){
				Transform moveIndicatorT=(Transform)Instantiate(movedIndicatorList[0]);
				moveIndicatorT.parent=transform;
				movedIndicatorList.Add(moveIndicatorT);
				index=movedIndicatorList.Count-1;
			}
			
			movedIndicatorList[index].position=unit.tile.GetPos();
			movedIndicatorList[index].gameObject.SetActive(true);
		}
		
		
		
		
		public static void ClearDeploymentIndicator(){ instance._ClearDeploymentIndicator(); }
		public void _ClearDeploymentIndicator(){
			for(int i=0; i<deploymentIndicatorList.Count; i++) deploymentIndicatorList[i].gameObject.SetActive(false);
		}
		public static void ShowDeploymentIndicator(List<Tile> list){ instance._ShowDeploymentIndicator(list); }
		public void _ShowDeploymentIndicator(List<Tile> list){
			while(deploymentIndicatorList.Count<list.Count){
				Transform deploymentIndicatorT=(Transform)Instantiate(deploymentIndicatorList[0]);
				deploymentIndicatorT.parent=transform;
				deploymentIndicatorList.Add(deploymentIndicatorT);
			}
			
			for(int i=0; i<list.Count; i++){
				deploymentIndicatorList[i].position=list[i].GetPos();
				deploymentIndicatorList[i].gameObject.SetActive(true);
			}
		}
		
		
		
		
		public static void ClearMoveableIndicator(){ instance._ClearMoveableIndicator(); }
		public void _ClearMoveableIndicator(){
			for(int i=0; i<moveableIndicatorList.Count; i++) moveableIndicatorList[i].gameObject.SetActive(false);
		}
		public static void ShowMoveableIndicator(List<Tile> list){ instance._ShowMoveableIndicator(list); }
		public void _ShowMoveableIndicator(List<Tile> list){
			while(moveableIndicatorList.Count<list.Count){
				Transform moveableIndicatorT=(Transform)Instantiate(moveableIndicatorList[0]);
				moveableIndicatorT.parent=transform;
				moveableIndicatorList.Add(moveableIndicatorT);
			}
			
			for(int i=0; i<list.Count; i++){
				moveableIndicatorList[i].position=list[i].GetPos();
				moveableIndicatorList[i].gameObject.SetActive(true);
				ScaleTransformUp(moveableIndicatorList[i]);
			}
		}
		
		
		
		
		public static void ClearHostileIndicator(){ instance._ClearHostileIndicator(); }
		public void _ClearHostileIndicator(){
			for(int i=0; i<hostileIndicatorList.Count; i++) hostileIndicatorList[i].gameObject.SetActive(false);
		}
		public static void ShowHostileIndicator(List<Tile> list){ instance._ShowHostileIndicator(list); }
		public void _ShowHostileIndicator(List<Tile> list){
			while(hostileIndicatorList.Count<list.Count){
				Transform hostileIndicatorT=(Transform)Instantiate(hostileIndicatorList[0]);
				hostileIndicatorT.parent=transform;
				hostileIndicatorList.Add(hostileIndicatorT);
			}
			
			for(int i=0; i<list.Count; i++){
				hostileIndicatorList[i].position=list[i].GetPos();
				hostileIndicatorList[i].gameObject.SetActive(true);
				ScaleTransformUp(hostileIndicatorList[i]);
			}
		}
		public static void RemoveHostileIndicator(Tile tile){ instance._RemoveHostileIndicator(tile); }
		public void _RemoveHostileIndicator(Tile tile){
			for(int i=0; i<hostileIndicatorList.Count; i++){
				if(hostileIndicatorList[i].position==tile.GetPos()) hostileIndicatorList[i].gameObject.SetActive(false);
			}
		}
		
		
		
		public static void ClearAbilityRangeIndicator(){ instance._ClearAbilityRangeIndicator(); }
		public void _ClearAbilityRangeIndicator(){
			for(int i=0; i<abRangeIndicatorList.Count; i++) abRangeIndicatorList[i].gameObject.SetActive(false);
		}
		public static void ShowAbilityRangeIndicator(List<Tile> list){ instance._ShowAbilityRangeIndicator(list); }
		public void _ShowAbilityRangeIndicator(List<Tile> list){
			while(abRangeIndicatorList.Count<list.Count){
				Transform abRangeIndicatorT=(Transform)Instantiate(abRangeIndicatorList[0]);
				abRangeIndicatorT.parent=transform;
				abRangeIndicatorList.Add(abRangeIndicatorT);
			}
			
			for(int i=0; i<list.Count; i++){
				abRangeIndicatorList[i].position=list[i].GetPos();
				abRangeIndicatorList[i].gameObject.SetActive(true);
				ScaleTransformUp(abRangeIndicatorList[i]);
			}
		}
		
		
		
		public static void ClearAbilityTargetIndicator(){ instance._ClearAbilityTargetIndicator(); }
		public void _ClearAbilityTargetIndicator(){
			for(int i=0; i<abTargetIndicatorList.Count; i++) abTargetIndicatorList[i].gameObject.SetActive(false);
		}
		public static void ShowAbilityTargetIndicator(List<Tile> list){ instance._ShowAbilityTargetIndicator(list); }
		public void _ShowAbilityTargetIndicator(List<Tile> list){
			while(abTargetIndicatorList.Count<list.Count){
				Transform abTargetIndicatorT=(Transform)Instantiate(abTargetIndicatorList[0]);
				abTargetIndicatorT.parent=transform;
				abTargetIndicatorList.Add(abTargetIndicatorT);
			}
			
			for(int i=0; i<list.Count; i++){
				abTargetIndicatorList[i].position=list[i].GetPos();
				abTargetIndicatorList[i].gameObject.SetActive(true);
				ScaleTransformUp(abTargetIndicatorList[i]);
			}
		}
		
		
		
		
		public static void SelectUnit(Unit unit){
			instance.selectIndicator.position=unit.tile.GetPos()+new Vector3(0, 0.05f, 0);
			instance.selectIndicator.gameObject.SetActive(true);
		}
		
		
		public static void ClearSelection(){
			instance.selectIndicator.gameObject.SetActive(false);
			instance._ClearMoveableIndicator();
			instance._ClearHostileIndicator();
		}
		
		
		
		
		void ScaleTransformDown(Transform targetT, float duration=0.125f){ 
			//StartCoroutine(ScaleTransformRoutine(targetT, GridManager.GetTileSize(), 0, duration));
		}
		void ScaleTransformUp(Transform targetT, float duration=0.125f){ 
			//StartCoroutine(ScaleTransformRoutine(targetT, 0, GridManager.GetTileSize(), duration)); 
		}
		IEnumerator ScaleTransformRoutine(Transform targetT, float startVal, float endVal, float dur=0.25f){
			targetT.gameObject.SetActive(true);
			
			float duration=0;
			Vector3 start=new Vector3(startVal, startVal, startVal);
			Vector3 end=new Vector3(endVal, endVal, endVal);
			
			targetT.localScale=start;
			
			while(duration<1){
				targetT.localScale=Vector3.Lerp(start, end, duration);
				duration+=Time.deltaTime*(1f/dur);
				yield return null;
			}
			
			targetT.localScale=end;
		}
		
		
	}

}