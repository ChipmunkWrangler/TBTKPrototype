using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	
	public enum _TileType{Hex, Square}
	//public enum _TileState{Default, Selected, Walkable, Hostile, Range}
	
	[System.Serializable]
	public class Wall{
		public bool init=false;
		
		public Tile neighbour;
		public Transform wallObjT;
		public float angle=90;
		
		public Wall(float ang, Transform wallT){ angle=ang; wallObjT=wallT; }
	}
	
	
	public class Tile : MonoBehaviour {
		
		public int index=0;
		
		public _TileType type;
		public bool walkable=true;
		
		private bool visible=true;	//for fog of war, mark as false if hidden in fog of war
		public bool IsVisible(){ return visible; }
		
		[HideInInspector] public GameObject fogOfWarObj;
		public void SetFogOfWarObj(Transform fogObjT){ 
			fogOfWarObj=fogObjT.gameObject;
			fogOfWarObj.transform.position=transform.position;
			fogOfWarObj.transform.parent=transform.parent;
			fogOfWarObj.transform.localScale*=GridManager.GetGridToTileSizeRatio();
			//fogOfWarObj.SetActive(!visible);
		}
		
		//public _TileState state=_TileState.Default;
		//public _TileState GetState(){ return state; }
		
		public List<CoverSystem.Cover> coverList=new List<CoverSystem.Cover>();
		
		[Space(10)]
		public Unit unit=null;
		
		[Space(10)]
		public int spawnAreaID=-1;	//factionID of units that can be generated on the tile, -1 means the tile is close
		public int deployAreaID=-1;	//factionID of units that can be deploy on the tile, -1 means the tile is close
		
		[Space(10)]
		public float x=0;		//coordiate data for hex-tile
		public float y=0;
		public float z=0;
		
		public TileAStar aStar;
		public List<Tile> GetNeighbourList(bool walkableOnly=false){ return aStar.GetNeighbourList(walkableOnly); }
		public List<Tile> GetDCNeighbourList(){ return aStar.GetDCNeighbourList(); }
		//public int GetDirectNeighbourCount(){ return aStar.straightNeighbourCount; }
		
		[HideInInspector] 
		public int distance=0;	//for when the tile is in walkableTileList for the selected unit, indicate the distance from selected unit
		
		/*	//path-smoothing, not in used
		public List<Vector3> path=new List<Vector3>();
		public void ResetPath(){ path=new List<Vector3>{ GetPos() }; }
		public List<Vector3> GetPath(){ 
			if(path.Count==0) ResetPath();
			return path;
		}
		*/
		
		
		private Transform thisT;
		//private Vector3 pos;	//no longer in use
		
		
		public void Init(){
			thisT=transform;
		}
		
		
		
		
		
		
		//disable in mobile so it wont interfere with touch input
		#if !UNITY_IPHONE && !UNITY_ANDROID
			//function called when mouse cursor enter the area of the tile, default MonoBehaviour method
			//~ void OnMouseEnter(){ OnTouchMouseEnter();	}
			//function called when mouse cursor leave the area of the tile, default MonoBehaviour method
			//~ void OnMouseExit(){ OnTouchMouseExit(); }
			//function called when mouse cursor enter the area of the tile, default MonoBehaviour method
			//~ public void OnMouseDown(){ GridManager.OnTileCursorDown(this); }
			
			//onMouseDown for right click
			//function called when mouse cursor enter the area of the tile, default MonoBehaviour method
			//used to detech right mouse click on the tile
			/*
			void OnMouseOver(){
				if(Input.GetMouseButtonDown(1)) OnRightClick();
			}
			public void OnRightClick(){
				
			}
			*/

		#endif
		
		
		//for when using inividual tile collider
		//~ public void OnTouchMouseEnter(){
			//~ GridManager.NewHoveredTile(this);
		//~ }
		//~ public void OnTouchMouseExit(){
			//~ GridManager.ClearHoveredTile();
		//~ }
		
		
		//code execution for when a left mouse click happen on a tile
		//~ public void OnTouchMouseDown(){
			//~ if(UIUtilities.IsCursorOnUI ()) return;

			//~ if(!TurnControl.ClearToProceed()) return;
			
			//~ if(GameControl.GetGamePhase()==_GamePhase.Over) return;
			
			//~ if(GameControl.GetGamePhase()==_GamePhase.UnitDeployment){
				//~ if(unit==null) FactionManager.DeployUnitOnTile(this);
				//~ else if(unit!=null) FactionManager.UndeployUnit(unit);
				//~ return;
			//~ }
			
			//~ GridManager.OnTile(this);
		//~ }
		//~ public void OnTouchMouseDownAlt(){
			//~ GridManager.OnTileAlt(this);
		//~ }
		
		
		
		
		/*
		public void SetState1(_TileState tileState=_TileState.Default){
			//state=tileState;
			
			//if(!visible) return;
			
			if(obstacleT!=null) return;
			
			if(!walkable){
				GetComponent<Renderer>().material=GridManager.GetMatUnwalkable();
				gameObject.SetActive(false);
				return;
			}
			else{
				gameObject.SetActive(true);
			}
			
			return;
			
			//~ if(state==_TileState.Default){ 
				//~ if(visible) GetComponent<Renderer>().material=GridManager.GetMatNormal();
				//~ else SetVisible(visible);
			//~ }
			//~ else if(state==_TileState.Selected) GetComponent<Renderer>().material=GridManager.GetMatSelected();
			//~ else if(state==_TileState.Walkable) GetComponent<Renderer>().material=GridManager.GetMatWalkable();
			//~ else if(state==_TileState.Hostile) GetComponent<Renderer>().material=GridManager.GetMatHostile();
			//~ else if(state==_TileState.Range) GetComponent<Renderer>().material=GridManager.GetMatRange();
			
			//if(Application.isPlaying){
				//if(state==_TileState.Default) renderer.enabled=false;
				//else renderer.enabled=true;
				//renderer.enabled=true;
			//}
		}
		*/
		
		//used in ability target mode to assign a material directly without changing state
		//public void SetMaterial(Material mat){
			//GetComponent<Renderer>().material=mat;
			//GetComponent<Renderer>().enabled=true;
		//}
		
		
		
		
		//stored for the current selected unit, or for AI algorithm
		[HideInInspector] 
		public List<Tile> hostileInRangeList=new List<Tile>();
		public void SetHostileInRange(List<Tile> list){ hostileInRangeList=list; }
		public void ClearHostileInRange(){ hostileInRangeList=new List<Tile>(); }
		public List<Tile> GetHostileInRange(){ return hostileInRangeList; }
		public List<Tile> SetupHostileInRange(Unit srcUnit=null){
			hostileInRangeList=new List<Tile>();
			
			if(srcUnit==null) srcUnit=unit;
			if(srcUnit!=null) GridManager.SetupHostileInRangeforTile(srcUnit, this);//unit.tile);
			
			return hostileInRangeList;
		}
		
		
		
		//********************************************************************************************************************************
		//these section are related to FogOfWar
		
		//to set the visibility of the tile after checking fog-of-war
		public void SetVisible(bool flag){
			if(forcedVisibleDuration.duration>0 && !flag) return;
			
			visible=flag;
			if(fogOfWarObj!=null) fogOfWarObj.SetActive(!visible);
			
			if(!flag){
				if(unit!=null){
					unit.gameObject.layer=TBTK.GetLayerUnitInvisible();
					Utilities.SetLayerRecursively(unit.transform, TBTK.GetLayerUnitInvisible());
				}
			}
			else{
				if(unit!=null){
					unit.gameObject.layer=TBTK.GetLayerUnit();
					Utilities.SetLayerRecursively(unit.transform, TBTK.GetLayerUnit());
				}
			}
		}
		
		//for ability which reveal fog-of-war for certain duration
		[Space(10)]
		public TBDuration forcedVisibleDuration=new TBDuration();
		//public DurationCounter forcedVisible=new DurationCounter();
		public void ForceVisible(int dur=1){
			if(!GameControl.EnableFogOfWar()) return;
			
			SetVisible(true);
			//if(fogOfWarObj!=null) fogOfWarObj.SetActive(!visible);
			
			if(forcedVisibleDuration.duration<dur){
				forcedVisibleDuration.Set(dur);
				EffectTracker.TrackVisible(this);
			}
			
			//forcedVisible.Count(dur);
			//GameControl.onIterateTurnE += IterateForcedVisible;
		}
		//called by EffectTracker
		public void IterateForcedVisibleDuration(){
			forcedVisibleDuration.Iterate();
			if(forcedVisibleDuration.Due()){
				SetVisible(FogOfWar.CheckTileVisibility(this));
				EffectTracker.UntrackVisible(this);
			}
			//~ forcedVisible.Iterate();
			//~ if(forcedVisible.duration<=0){
				//~ SetVisible(FogOfWar.CheckTileVisibility(this));
				//~ if(fogOfWarObj!=null) fogOfWarObj.SetActive(!visible);
				//~ //GameControl.onIterateTurnE -= IterateForcedVisible;
			//~ }
		}
		
		//end FogOfWar section
		//********************************************************************************************************************************
		
		
		
		
		
		public Vector3 GetPos(){ return thisT==null ? transform.position : thisT.position; }
		public Vector3 GetPosG(){ 
			Vector3 pos=thisT==null ? transform.position : thisT.position;
			return new Vector3(pos.x, 0, pos.z);
		}
		
		public Tile GetNeighbourFromAngle(float angle){
			List<Tile> neighbourList=aStar.GetNeighbourList();
			for(int n=0; n<neighbourList.Count; n++){
				Vector3 dir=neighbourList[n].GetPos()-GetPos();
				float angleN=Utilities.Vector2ToAngle(new Vector2(dir.x, dir.z));
				if(Mathf.Abs(angle-angleN)<2) return neighbourList[n];
			}
			return null;
		}
		
		
		
		
		
		
		
		
		[HideInInspector] public int hostileCount=0;
		[HideInInspector] public int coverScore=0;
		//CoverRating: 0-no cover at all, 1-halfcover, 2-fullcover
		public float GetCoverRating(){ return hostileCount>0 ? (float)coverScore/(float)hostileCount : 0 ; }
	
		
		
		
		//********************************************************************************************************************************
		//these section are related to obstacle and wall
		[Space(10)]
		public Transform obstacleT;
		public bool HasObstacle(){ return obstacleT==null ? false : true ; }
		
		public void AddObstacle(int obsType, float gridSize){
			if(obstacleT!=null){
				if(obstacleT.gameObject.layer==TBTK.GetLayerObstacleHalfCover() && obsType==1) return;
				if(obstacleT.gameObject.layer==TBTK.GetLayerObstacleFullCover() && obsType==2) return;
				
				#if UNITY_EDITOR
				Undo.DestroyObjectImmediate(obstacleT.gameObject);
				#endif
			}
			
			if(wallList.Count>0){
				if(!Application.isPlaying){
					Grid grid=GridManager.GetInstance().GetGrid();
					while(wallList.Count>0) RemoveWall(wallList[0].angle, grid.GetNeighbourInDir(this, wallList[0].angle));
				}
				else{
					while(wallList.Count>0) RemoveWall(wallList[0].angle, GetNeighbourFromAngle(wallList[0].angle));
				}
			}
			
			//float gridSize=GridManager.GetTileSize();
			
			#if UNITY_EDITOR
				Transform obsT=(Transform)PrefabUtility.InstantiatePrefab(GridManager.GetObstacleT(obsType));
				Undo.RecordObject(this, "Tile");
				Undo.RecordObject(GetComponent<Renderer>(), "TileRenderer");
				Undo.RegisterCreatedObjectUndo(obsT.gameObject, "Obstacle");
			#else
				Transform obsT=(Transform)Instantiate(GridManager.GetObstacleT(obsType));
			#endif
			
			
			
			//~ float offsetY=0;
			//~ if(type==_TileType.Square){
				//~ if(obsType==1)offsetY=obsT.localScale.z*gridSize/4;
				//~ if(obsType==2)offsetY=obsT.localScale.z*gridSize/2;
			//~ }
			//~ else if(type==_TileType.Hex) offsetY=obsT.localScale.z*gridSize/4;
			
			obsT.position=GetPos();//+new Vector3(0, offsetY, 0);
			
			obsT.localScale*=gridSize*GridManager.GetGridToTileSizeRatio();
			obsT.parent=transform;
			
			obstacleT=obsT;
			walkable=false;
			
			GetComponent<Renderer>().enabled=false;
			
			//SetState(_TileState.Default);
		}
		public void RemoveObstacle(){
			#if UNITY_EDITOR
				if(obstacleT!=null) Undo.DestroyObjectImmediate(obstacleT.gameObject);
				Undo.RecordObject(this, "Tile");
				Undo.RecordObject(GetComponent<Renderer>(), "TileRenderer");
			#else
				if(obstacleT!=null) DestroyImmediate(obstacleT.gameObject);
			#endif
			
			walkable=true;
			GetComponent<Renderer>().enabled=true;
		}
		
		
		public void CheckDiagonal(){
			LayerMask mask=1<<TBTK.GetLayerObstacleHalfCover() | 1<<TBTK.GetLayerObstacleFullCover();
			
			List<Tile> neighbourList=new List<Tile>( aStar.GetNeighbourList() );
			for(int n=0; n<neighbourList.Count; n++){
				Vector3 dir=neighbourList[n].GetPos()-GetPos();
				float angleN=Utilities.Vector2ToAngle(new Vector2(dir.x, dir.z));
				
				if(angleN%90!=0){
					if(Physics.Linecast(GetPos(), neighbourList[n].GetPos(), mask)){
						aStar.DisconnectNeighbour(neighbourList[n]);
					}
				}
			}
		}
		
		
		public List<Wall> wallList=new List<Wall>();
		
		//used in edit mode only
		public void AddWall(float angle, Tile neighbour, int wallType=0){
			if(neighbour==null) return;
			
			if(angle>360) angle-=360;
			
			if(IsWalled(angle)) return;
			
			float gridSize=GridManager.GetTileSize()*GridManager.GetGridToTileSizeRatio();
			if(type==_TileType.Square) gridSize*=2;
			
			#if UNITY_EDITOR
				Transform wallT=(Transform)PrefabUtility.InstantiatePrefab(GridManager.GetWallObstacleT(wallType));
				Undo.RecordObject(this, "Tile");
				Undo.RecordObject(neighbour, "NeighbourTile");
				Undo.RegisterCreatedObjectUndo(wallT.gameObject, "Wall");
			#else
				Transform wallT=(Transform)Instantiate(GridManager.GetWallObstacleT(wallType));
			#endif
			
			float wallTAngle=angle+90;
			
			if(type==_TileType.Square) wallTAngle=360-(angle-90);
			else if(type==_TileType.Hex) wallTAngle=360-(angle-90);
			
			wallT.rotation=Quaternion.Euler(0, wallTAngle, 0);
			wallT.position=(GetPos()+neighbour.GetPos())/2;
			wallT.localScale*=gridSize;
			wallT.parent=transform;
			wallList.Add(new Wall(angle, wallT));
			
			if((angle+=180)>=360) angle-=360;
			
			neighbour.wallList.Add(new Wall(angle, wallT));
		}
		public void RemoveWall(float angle, Tile neighbour){
			Debug.Log(angle+"    "+neighbour);
			
			if(angle>360) angle-=360;
			for(int i=0; i<wallList.Count; i++){ 
				Debug.Log(wallList[i].angle+"     "+angle);
				if(wallList[i].angle==angle){
					
					#if UNITY_EDITOR
						
					#endif
					
					#if UNITY_EDITOR
						if(wallList[i].wallObjT!=null) Undo.DestroyObjectImmediate(wallList[i].wallObjT.gameObject);
						Undo.RecordObject(this, "Tile");
					#else
						if(wallList[i].wallObjT!=null) DestroyImmediate(wallList[i].wallObjT.gameObject);
					#endif
					
					wallList.RemoveAt(i);
					break;
				}
			}
			
			if(neighbour==null) return;
			
			if((angle+=180)>=360) angle-=360;
			for(int i=0; i<neighbour.wallList.Count; i++){ 
				Debug.Log(neighbour.wallList[i].angle+"     "+angle);
				if(neighbour.wallList[i].angle==angle){
					
					#if UNITY_EDITOR
						Undo.RecordObject(neighbour, "NeighbourTile");
					#endif
					
					neighbour.wallList.RemoveAt(i);
					break;
				}
			}
		}
		
		
		public bool IsWalled(float angle){
			for(int i=0; i<wallList.Count; i++){ if(wallList[i].angle==angle) return true; }
			return false;
		}
		
		//called during grid initiation
		public void InitWall(){
			if(wallList.Count==0) return;
			
			for(int i=0; i<wallList.Count; i++){
				Wall wall=wallList[i];
				
				if(wall.init) continue;
				
				Tile neighbour=GetNeighbourFromAngle(wall.angle);
				if(neighbour!=null){
					wall.init=true;
					wall.neighbour=neighbour;
					aStar.DisconnectNeighbour(neighbour);
					neighbour.CreateWall(this, wall.angle+180);
				}
			}
		}
		//call by other tile in InitWall to create a wall instance, to avoid duplication or running the same code twice
		public void CreateWall(Tile neighbour, float angle){
			if(angle>360) angle-=360;
			for(int i=0; i<wallList.Count; i++){
				Wall wall=wallList[i];
				if(wall.angle==angle){
					wall.init=true;
					wall.neighbour=neighbour;
					aStar.DisconnectNeighbour(neighbour);
					break;
				}
			}
		}
		
		//for enable/disable wall in runtime, not being used
		public void DisableWall(Transform wallObjT){
			for(int i=0; i<wallList.Count; i++){
				if(wallList[i].wallObjT==wallObjT){
					Tile neighbour=wallList[i].neighbour;
					aStar.ConnectNeighbour(neighbour);
					
					for(int n=0; n<neighbour.wallList.Count; n++){
						if(neighbour.wallList[n].wallObjT==wallObjT){
							aStar.ConnectNeighbour(neighbour.wallList[n].neighbour);
							
							if(GameControl.EnableCover()){
								for(int j=0; j<neighbour.coverList.Count; j++){
									if(Mathf.Abs(coverList[j].angle-neighbour.wallList[n].angle)<1){
										coverList[j].enabled=false;
									}
								}
							}
							
							break;
						}
					}
					
					if(GameControl.EnableCover()){
						for(int m=0; m<coverList.Count; m++){
							if(Mathf.Abs(coverList[m].angle-wallList[i].angle)<1){
								coverList[m].enabled=false;
							}
						}
					}
					
					break;
				}
			}
		}
		public void EnableWall(Transform wallObjT){
			for(int i=0; i<wallList.Count; i++){
				if(wallList[i].wallObjT==wallObjT){
					Tile neighbour=wallList[i].neighbour;
					aStar.DisconnectNeighbour(neighbour);
					
					for(int n=0; n<neighbour.wallList.Count; n++){
						if(neighbour.wallList[n].wallObjT==wallObjT){
							aStar.DisconnectNeighbour(neighbour.wallList[n].neighbour);
							
							if(GameControl.EnableCover()){
								for(int j=0; j<neighbour.coverList.Count; j++){
									if(Mathf.Abs(coverList[j].angle-neighbour.wallList[n].angle)<1){
										coverList[j].enabled=true;
									}
								}
							}
							
							break;
						}
					}
					
					if(GameControl.EnableCover()){
						for(int m=0; m<coverList.Count; m++){
							if(Mathf.Abs(coverList[m].angle-wallList[i].angle)<1){
								coverList[m].enabled=true;
							}
						}
					}
					
					break;
				}
				
			}
		}
		
		//end obstacle and wall related function
		//*********************************************************************************************************************************
		
		
		
		
		
		
		//********************************************************************************************************************************
		//these section are related to collectible on tile
		
		public Collectible collectible;
		
		public void TriggerCollectible(Unit unit){
			if(collectible==null) return;
			
			collectible.Trigger(unit);
			
			collectible=null;
		}
		
		
		//end collectible
		//*********************************************************************************************************************************
		
		
		
		
		
		
		
		//********************************************************************************************************************************
		//these section are related to effects on tile
		
		[Space(10)]
		public Effect fixedEffect=new Effect();
		
		public List<Effect> effectList=new List<Effect>();
		public void ApplyEffect(Effect eff){
			if(eff.duration<=0) return;
			
			eff.Init();
			effectList.Add(eff);
			UpdateActiveEffect();
			
			EffectTracker.Track(this);
		}
		
		public void IterateEffectDuration(){
			bool changed=false;
			for(int i=0; i<effectList.Count; i++){
				effectList[i].Iterate();
				if(effectList[i].Due()){
					effectList.RemoveAt(i);	i-=1;
					changed=true;
				}
			}
			if(changed){
				if(effectList.Count>0) UpdateActiveEffect();
				else{
					activeEffect=new Effect();
					EffectTracker.Untrack(this);
				}
			}
		}
		
		public Effect activeEffect=new Effect();
		public void UpdateActiveEffect(){
			activeEffect=new Effect();
			
			AddToActiveEffect(fixedEffect);
			
			for(int i=0; i<effectList.Count; i++) AddToActiveEffect(effectList[i]);
		}
		public void AddToActiveEffect(Effect eff){
			activeEffect.HPPerTurn+=eff.HPPerTurn;
			activeEffect.APPerTurn+=eff.APPerTurn;
			
			activeEffect.moveAPCost+=eff.moveAPCost;
			activeEffect.attackAPCost+=eff.attackAPCost;
			
			activeEffect.moveRange+=eff.moveRange;
			activeEffect.attackRange+=eff.attackRange;
			activeEffect.sight+=eff.sight;
			
			activeEffect.damage+=eff.damage;
			
			activeEffect.hitChance+=eff.hitChance;
			activeEffect.dodgeChance+=eff.dodgeChance;
			
			activeEffect.critChance+=eff.critChance;
			activeEffect.critAvoidance+=eff.critAvoidance;
			activeEffect.critMultiplier+=eff.critMultiplier;
			
			activeEffect.stunChance+=eff.stunChance;
			activeEffect.stunAvoidance+=eff.stunAvoidance;
			activeEffect.stunDuration+=eff.stunDuration;
			
			activeEffect.silentChance+=eff.silentChance;
			activeEffect.silentAvoidance+=eff.silentAvoidance;
			activeEffect.silentDuration+=eff.silentDuration;
			
			activeEffect.flankingBonus+=eff.flankingBonus;
			activeEffect.flankedModifier+=eff.flankedModifier;
		}
		
		public float GetHPPerTurn(){ return activeEffect.HPPerTurn; }
		public float GetAPPerTurn(){ return activeEffect.APPerTurn; }
		
		public float GetMoveAPCost(){ return activeEffect.APPerTurn; }
		public float GetAttackAPCost(){ return activeEffect.APPerTurn; }
		
		public int GetMoveRange(){ return activeEffect.attackRange; }
		public int GetAttackRange(){ return activeEffect.attackRange; }
		public int GetSight(){ return activeEffect.sight; }
		
		public float GetDamage(){ return activeEffect.damage; }
		
		public float GetHitChance(){ return activeEffect.hitChance; }
		public float GetDodgeChance(){ return activeEffect.dodgeChance; }
		
		public float GetCritChance(){ return activeEffect.critChance; }
		public float GetCritAvoidance(){ return activeEffect.critAvoidance; }
		public float GetCritMultiplier(){ return activeEffect.attackRange; }
		
		public float GetStunChance(){ return activeEffect.stunChance; }
		public float GetStunAvoidance(){ return activeEffect.stunAvoidance; }
		public int GetStunDuration(){ return activeEffect.stunDuration; }
		
		public float GetSilentChance(){ return activeEffect.silentChance; }
		public float GetSilentAvoidance(){ return activeEffect.silentAvoidance; }
		public int GetSilentDuration(){ return activeEffect.silentDuration; }
		
		public float GetFlankingBonus(){ return activeEffect.flankingBonus; }
		public float GetFlankedModifier(){ return activeEffect.flankedModifier; }
		
		
		
		//end effect related function
		//*********************************************************************************************************************************
		
	}
	
}