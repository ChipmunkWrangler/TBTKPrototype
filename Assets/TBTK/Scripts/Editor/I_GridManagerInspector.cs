using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	[CustomEditor(typeof(GridManager))]
	public class GridManagerInspector : TBEditorInspector{

		private static GridManager instance;
		
		private static FactionManager factionManager;
		private static CollectibleManager collectibleManager;
		
		private string[] tileTypeLabel;
		private string[] tileTypeTooltip;
		
		private string[] gridColliderTypeLabel;
		private string[] gridColliderTypeTooltip;
		
		void Awake(){
			instance = (GridManager)target;
			instance.SetInstance();
			
			//factionManager = (FactionManager)FindObjectOfType(typeof(FactionManager));
			factionManager = FactionManager.SetInstance();
			collectibleManager = CollectibleManager.SetInstance();
			
			LoadDB();
			
			InitLabel();
		}
		
		void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_TileType)).Length;
			tileTypeLabel=new string[enumLength];
			tileTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				tileTypeLabel[i]=((_TileType)i).ToString();
				if((_TileType)i==_TileType.Hex) tileTypeTooltip[i]="using Hex grid";
				if((_TileType)i==_TileType.Square) tileTypeTooltip[i]="using square grid";
			}
			
			enumLength = Enum.GetValues(typeof(GridManager._GridColliderType)).Length;
			gridColliderTypeLabel=new string[enumLength];
			gridColliderTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				gridColliderTypeLabel[i]=((GridManager._GridColliderType)i).ToString();
				if((GridManager._GridColliderType)i==GridManager._GridColliderType.Master) 
					gridColliderTypeTooltip[i]="using a single master collider for all the tile on the grid. Allow bigger grid but the tiles on the grid cannot be adjusted";
				if((GridManager._GridColliderType)i==GridManager._GridColliderType.Individual) 
					gridColliderTypeTooltip[i]="using individual collider for each tile on the grid. This allow positional adjustment of individual tile but severely limited the grid size. Not recommend for any grid beyond 35x35.";
			}
		}
		
		
		private bool showGridSetting=true;
		private bool showGridPrefab=false;
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			
			GUI.changed = false;
			
			Undo.RecordObject(this, "GridManagerEditor");
			Undo.RecordObject(instance, "GridManager");
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			
				if(instance.grid!=null){
					if(instance.grid.IsInitiated()){
						string text="";
						text+="Current Grid Dimension : "+instance.grid.GetWidth()+"x"+instance.grid.GetLength()+"\n";
						text+="Current Grid Tile Size    :  "+instance.grid.GetTileSize().ToString();
						EditorGUILayout.HelpBox(text, MessageType.Info);
					}
					else EditorGUILayout.HelpBox("Grid properties is not properly set.\nPlease regenerate the grid", MessageType.Warning);
				}
				else{
					EditorGUILayout.HelpBox("There's no grid", MessageType.Warning);
				}
				
			GUILayout.Label("________________________________________________________________________________________________________", GUILayout.Width(editorWidth-30));
			EditorGUILayout.Space();
			
				int tileType=(int)instance.tileType;
				cont=new GUIContent("Tile Type:", "The type of grid to use (Hex or Square)");
				contL=new GUIContent[tileTypeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(tileTypeLabel[i], tileTypeTooltip[i]);
				tileType = EditorGUILayout.Popup(cont, tileType, contL);
				instance.tileType=(_TileType)tileType;
			
				int gridColliderType=(int)instance.gridColliderType;
				cont=new GUIContent("Grid Collider Type:", "The type of collider to use (The collider are used for cursor detection)");
				contL=new GUIContent[gridColliderTypeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(gridColliderTypeLabel[i], gridColliderTypeTooltip[i]);
				gridColliderType = EditorGUILayout.Popup(cont, gridColliderType, contL);
				instance.gridColliderType=(GridManager._GridColliderType)gridColliderType;
				
				cont=new GUIContent("Diagonal Neighbour:", "Check to enable diagonal neighbour\nOnly applicable when using square tile");
				if(instance.grid.GetTileType()!=_TileType.Square) EditorGUILayout.LabelField(cont, new GUIContent("-"));
				else instance.enableDiagonalNeighbour=EditorGUILayout.Toggle(cont, instance.enableDiagonalNeighbour);
				
			EditorGUILayout.Space();
			
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
				showGridSetting=EditorGUILayout.Foldout(showGridSetting, "Show Grid Setting");//, foldoutStyle);
				EditorGUILayout.EndHorizontal();
				
				if(showGridSetting){
					cont=new GUIContent("   - Grid Width:", "The number of tile in the grid in x-axis");
					instance.width=EditorGUILayout.IntField(cont, instance.width);
				
					cont=new GUIContent("   - Grid Length:", "The number of tile in the grid in z-axis");
					instance.length=EditorGUILayout.IntField(cont, instance.length);
				
					cont=new GUIContent("   - Tile Size:", "The space occupied by of the individual tile in the grid");
					instance.tileSize=EditorGUILayout.FloatField(cont, instance.tileSize);
				
					cont=new GUIContent("   - GridToTileRatio:", "The ratio of the actual space each individual tile are occupying to the visible size of the individual tile\n\nThis is used to give some spacing between each tile when visualizing the grid\nRecommended value are 1 to 1.2");
					instance.gridToTileRatio=EditorGUILayout.FloatField(cont, instance.gridToTileRatio);
					
					cont=new GUIContent("   - UnwalkableRate:", "The percentage of the unwalkable tile on the grid. Takes value from 0-1 with 0.25 means 25% of the grid will not be walkabe");
					instance.unwalkableRate=EditorGUILayout.FloatField(cont, instance.unwalkableRate);
				}
				
			EditorGUILayout.Space();
			
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
				showGridPrefab=EditorGUILayout.Foldout(showGridPrefab, "Show Grid Prefab Assignment Setting");//, foldoutStyle);
				EditorGUILayout.EndHorizontal();
				
				if(showGridPrefab){
					cont=new GUIContent("   - Wall H:", "Wall prefab with full-cover. This applies to both hex and square grid");
					instance.obstacleWallH=(Transform)EditorGUILayout.ObjectField(cont, instance.obstacleWallH, typeof(Transform), true);
					cont=new GUIContent("   - Wall F:", "Wall prefab with half-cover. This applies to both hex and square grid");
					instance.obstacleWallF=(Transform)EditorGUILayout.ObjectField(cont, instance.obstacleWallF, typeof(Transform), true);
					
					cont=new GUIContent("   - Obstacle Hex H:", "Obstacle prefab with full-cover for hex-grid");
					instance.obstacleHexF=(Transform)EditorGUILayout.ObjectField(cont, instance.obstacleHexF, typeof(Transform), true);
					cont=new GUIContent("   - Obstacle Hex F:", "Obstacle prefab with half-cover for hex-grid");
					instance.obstacleHexH=(Transform)EditorGUILayout.ObjectField(cont, instance.obstacleHexH, typeof(Transform), true);
					
					cont=new GUIContent("   - Obstacle Sq H:", "Obstacle prefab with full-cover for square-grid");
					instance.obstacleSqF=(Transform)EditorGUILayout.ObjectField(cont, instance.obstacleSqF, typeof(Transform), true);
					cont=new GUIContent("   - Obstacle Sq F:", "Obstacle prefab with half-cover for square-grid");
					instance.obstacleSqH=(Transform)EditorGUILayout.ObjectField(cont, instance.obstacleSqH, typeof(Transform), true);
				}
				
			EditorGUILayout.Space();
			
				if(!Application.isPlaying){
					if(GUILayout.Button("Generate Grid")){
						instance.GenerateGrid();
						EditorUtility.SetDirty(instance);
					}
					if(GUILayout.Button("Generate Unit")){
						FactionManager.GenerateUnit();
						EditorUtility.SetDirty(instance);
					}
					if(GUILayout.Button("Generate Collectible")){
						CollectibleManager.GenerateCollectible();
						EditorUtility.SetDirty(instance);
					}
				}
				
			EditorGUILayout.Space();
			
				DrawGridEditor();
			
			EditorGUILayout.Space();
				
			DefaultInspector();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
			
		}
		
		
		
		
		
		private enum _EditMode{ Tile, Unit, Collectible, }
		private static _EditMode editMode;
		
		private Color colorOn=new Color(.25f, 1f, 1f, 1f);
		
		private bool rotatingView=false;
		
		private bool showGridEditor=true;
		private bool enableEditing=false;
		private void DrawGridEditor(){
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				showGridEditor=EditorGUILayout.Foldout(showGridEditor, "Show Grid editor", foldoutStyle);
				if(EditorGUI.EndChangeCheck()) enableEditing=showGridEditor;
				
				GUI.color=enableEditing ? colorOn : new Color(0.5f, 0.5f, 0.5f, 1);
				if(GUILayout.Button("Enable Edit", GUILayout.MaxWidth(80))){
					enableEditing=!enableEditing;
					if(enableEditing) showGridEditor=true;
				}
			EditorGUILayout.EndHorizontal();
			
			if(!showGridEditor) return;
			
			if(Application.isPlaying){
				EditorGUILayout.HelpBox("Grid editing is not allowed during runtime", MessageType.Warning);
				return;
			}
			
			EditorGUILayout.Space();
			
			GUILayout.Label("Edit Type:", GUILayout.MaxWidth(60));
			
			EditorGUILayout.BeginHorizontal();
			
				GUI.color=editMode==_EditMode.Tile ? colorOn : Color.white ;
				if(GUILayout.Button("Tile")) editMode=_EditMode.Tile;
				
				GUI.color=editMode==_EditMode.Unit ? colorOn : Color.white ;
				if(GUILayout.Button("Unit")) editMode=_EditMode.Unit;
			
				GUI.color=editMode==_EditMode.Collectible ? colorOn : Color.white ;
				if(GUILayout.Button("Collectible")) editMode=_EditMode.Collectible;
				
				GUI.color=Color.white;
			
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Label("________________________________________________________________________________________________________", GUILayout.Width(editorWidth-30));
			
			editorWidth=Screen.width;
			if(editMode==_EditMode.Tile) DrawEditModeTileUI();
			if(editMode==_EditMode.Unit) DrawEditModeUnitUI();
			if(editMode==_EditMode.Collectible) DrawEditModeCollectibleUI();
		}
		
		
		private float editorWidth=1;		//used to determined the width of the inspector
		
		private enum _TileStateE {Unwalkable, Default, WallH, WallF, ObstacleH, ObstacleF, SpawnArea, Deployment}
		private static _TileStateE tileState=_TileStateE.Default;
		private int spawnAreaFactionID=0;	//factionID of the spawnArea
		private int spawnAreaInfoID=0;			//spawnInfoID of the spawnArea (each faction could have multiple spawnInfo)
		private int deployAreaFactionID=0;
		
		private void DrawStateButton(_TileStateE state, string text){
			GUI.color=tileState==state ? colorOn : Color.white; 
			if(GUILayout.Button(text, GUILayout.MaxWidth(editorWidth/2))) tileState=state;
			GUI.color=Color.white;
		}
		
		void DrawEditModeTileUI(){
			GUILayout.Label("Tile State:");
			
			EditorGUILayout.BeginHorizontal();
				DrawStateButton(_TileStateE.Unwalkable, "Unwalkable");
				DrawStateButton(_TileStateE.Default, "Default");
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				DrawStateButton(_TileStateE.WallH, "Wall (Half)");
				DrawStateButton(_TileStateE.WallF, "Wall (Full)");
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				DrawStateButton(_TileStateE.ObstacleH, "Obstacle (Half)");
				DrawStateButton(_TileStateE.ObstacleF, "Obstacle (Full)");
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
				DrawStateButton(_TileStateE.SpawnArea, "SpawnArea");
				DrawStateButton(_TileStateE.Deployment, "Deployment");
			EditorGUILayout.EndHorizontal();
			
			
			if(tileState==_TileStateE.SpawnArea){
				GUILayout.Label("________________________________________________________________________________________________________", GUILayout.Width(editorWidth-30));
				
				for(int i=0; i<factionManager.factionList.Count; i++){
					EditorGUILayout.Space();
					
					GUI.color=spawnAreaFactionID==i ? colorOn : Color.white ;
					if(GUILayout.Button(factionManager.factionList[i].name.ToString(), GUILayout.MaxWidth(editorWidth))) spawnAreaFactionID=i;
					GUI.color=Color.white;
					
					if(spawnAreaFactionID==i){
						Faction fac=factionManager.factionList[i];
						spawnAreaInfoID=Mathf.Clamp(spawnAreaInfoID, 0, fac.spawnInfoList.Count);
						
						for(int n=0; n<fac.spawnInfoList.Count; n++){
							EditorGUILayout.BeginHorizontal();
								GUILayout.Label("   - ", GUILayout.MaxWidth(25));
								GUI.color=spawnAreaInfoID==n ? colorOn : Color.white ;
								if(GUILayout.Button("SpawnArea "+(n+1), GUILayout.MaxWidth((editorWidth-25)*0.7f-10))) spawnAreaInfoID=n;
							
								GUI.color=Color.white;
								if(GUILayout.Button("Clear All ", GUILayout.MaxWidth((editorWidth-25)*0.3f))){
									Undo.RecordObject(factionManager, "FactionManager");
									for(int m=0; m<fac.spawnInfoList[n].startingTileList.Count; m++){
										if(fac.spawnInfoList[n].startingTileList[m]!=null)
										fac.spawnInfoList[n].startingTileList[m].spawnAreaID=-1;
									}
									fac.spawnInfoList[n].startingTileList=new List<Tile>();
									SceneView.RepaintAll();
								}
							EditorGUILayout.EndHorizontal();
						}
					}
				}
			}
			if(tileState==_TileStateE.Deployment){
				GUILayout.Label("________________________________________________________________________________________________________", GUILayout.Width(editorWidth-30));
			
				for(int i=0; i<factionManager.factionList.Count; i++){
					EditorGUILayout.Space();
					EditorGUILayout.BeginHorizontal();
						GUI.color=deployAreaFactionID==i ? colorOn : Color.white ;
						if(GUILayout.Button(factionManager.factionList[i].name.ToString(), GUILayout.MaxWidth(editorWidth*0.7f))) deployAreaFactionID=i;
						GUI.color=Color.white;
					
						if(GUILayout.Button("Clear All", GUILayout.MaxWidth(editorWidth*0.3f))){
							Undo.RecordObject(factionManager, "FactionManager");
							Faction facAlt=factionManager.factionList[i];
							for(int n=0; n<facAlt.deployableTileList.Count; n++){
								if(facAlt.deployableTileList[n]!=null) facAlt.deployableTileList[n].deployAreaID=-1;
							}
							facAlt.deployableTileList=new List<Tile>();
							SceneView.RepaintAll();
						}
					EditorGUILayout.EndHorizontal();
				}
			}
			
			GUI.color=Color.white;
			
			EditorGUILayout.Space();
		}
		
		
		
		
		
		private int unitNumInRow=1;	//the number of unit button on display in a row
		
		private int unitFactionID=0;	//factionID of the unit to be plop on the grid
		private Unit selectedUnit;		//currently select unit to be plop on the grid
		
		void DrawEditModeUnitUI(){
			GUILayout.Label("Unit's Faction:");
			
				for(int i=0; i<factionManager.factionList.Count; i++){
					GUI.color=unitFactionID==i ? colorOn : Color.white ;
					if(GUILayout.Button(factionManager.factionList[i].name.ToString(), GUILayout.MaxWidth(editorWidth))) unitFactionID=i;
				}
			
			GUILayout.Label("________________________________________________________________________________________________________", GUILayout.Width(editorWidth-30));
			GUILayout.Label("Unit To Deploy:");
				
				unitDB.ClearEmptyElement();
				if(unitDB.ClearEmptyElement()) UpdateLabel_Unit();
				List<Unit> unitList=unitDB.unitList;
				unitNumInRow=(int)Mathf.Max(1, Mathf.Floor((editorWidth)/50));
				Rect rect=new Rect(0, 0, 0, 0);
				
				EditorGUILayout.BeginHorizontal();
					for(int i=0; i<unitList.Count; i++){
						if(i%unitNumInRow==0){
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
						}
						
						GUI.color=selectedUnit==unitList[i] ? colorOn : Color.white ;
						if(unitList[i].iconSprite==null) cont=new GUIContent((Texture)null, unitList[i].unitName);
						else cont=new GUIContent(unitList[i].iconSprite.texture, unitList[i].unitName);
						
						if(GUILayout.Button(cont, GUILayout.Width(45), GUILayout.Height(45))) selectedUnit=unitList[i];
						if(selectedUnit==unitList[i]) rect=GUILayoutUtility.GetLastRect();
					}
					
					if(selectedUnit!=null){
						rect.x+=3; rect.y+=3; rect.width-=6; rect.height-=6;
						TBEditorUtility.DrawSprite(rect, selectedUnit.iconSprite, selectedUnit.name, false);
					}
				EditorGUILayout.EndHorizontal();
				
				GUI.color=Color.white;
				
			EditorGUILayout.Space();
		}
		
		
		
		private Collectible selectedCollectible;		//currently select collectible to be plop on the grid
		
		void DrawEditModeCollectibleUI(){
			GUILayout.Label("Collectible To Deploy:");
			
				if(collectibleDB.ClearEmptyElement()) UpdateLabel_Collectible();
				List<Collectible> collectibleList=collectibleDB.collectibleList;
				unitNumInRow=(int)Mathf.Max(1, Mathf.Floor((editorWidth)/50));
				Rect rect=new Rect(0, 0, 0, 0);
				
				EditorGUILayout.BeginHorizontal();
					for(int i=0; i<collectibleList.Count; i++){
						if(i%unitNumInRow==0){
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
						}
						
						GUI.color=selectedCollectible==collectibleList[i] ? colorOn : Color.white ;
						if(collectibleList[i].icon==null) cont=new GUIContent((Texture)null, collectibleList[i].itemName);
						else cont=new GUIContent(collectibleList[i].icon.texture, collectibleList[i].itemName);
						
						if(GUILayout.Button(cont, GUILayout.Width(45), GUILayout.Height(45))) selectedCollectible=collectibleList[i];
						if(selectedCollectible==collectibleList[i]) rect=GUILayoutUtility.GetLastRect();
					}
					
					if(selectedCollectible!=null){
						rect.x+=3; rect.y+=3; rect.width-=6; rect.height-=6;
						TBEditorUtility.DrawSprite(rect, selectedCollectible.icon, selectedCollectible.itemName, false);
					}
				EditorGUILayout.EndHorizontal();
				
				GUI.color=Color.white;
				
			EditorGUILayout.Space();
		}
		
		
		
		
		//********************************************************************************************************************************************//
		
		
		void OnSceneGUI(){
			if(Application.isPlaying) return;
			
			
			Event current = Event.current;
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			
			switch (current.type)
			{
				case EventType.MouseDown:
					Edit(current);
					break;
				
				case EventType.MouseDrag:
					if(current.button==0) Edit(current);
					break;
				
				case EventType.KeyDown:
					if(Event.current.keyCode==(KeyCode.RightAlt) || Event.current.keyCode==(KeyCode.LeftAlt)) rotatingView=true;
					break;
					
				case EventType.KeyUp:
					if(Event.current.keyCode==(KeyCode.RightAlt) || Event.current.keyCode==(KeyCode.LeftAlt)) rotatingView=false;
					break;
		 
				case EventType.Layout:
					HandleUtility.AddDefaultControl(controlID);
					break;
			}
		}
		
		
		void Edit(Event current){
			if(!enableEditing) return;
			if(rotatingView) return;
			
			LayerMask mask=1<<TBTK.GetLayerTile();
			Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask)){
				Tile tile=null;
				if(instance.gridColliderType==GridManager._GridColliderType.Individual)
					tile=hit.transform.gameObject.GetComponent<Tile>();
				else if(instance.gridColliderType==GridManager._GridColliderType.Master)
					tile=instance._GetTileOnPos(hit.point);
				
				if(tile==null) return;
				
				Undo.RecordObject(tile, "tile");
				
				if(editMode==_EditMode.Tile) EditTileState(tile, current.button, hit.point);
				else if(editMode==_EditMode.Unit) EditTileUnit(tile, current.button, hit.point);
				else if(editMode==_EditMode.Collectible) EditTileCollectible(tile, current.button, hit.point);
				
				EditorUtility.SetDirty(tile);
			}
		}
		
		
		
		void EditTileState(Tile tile, int mouseClick=0, Vector3 hitPos=default(Vector3)){
			Undo.RecordObject(tile, "Tile");
			
			if(tileState==_TileStateE.Unwalkable){
				if(tile.HasObstacle()){ Debug.LogWarning("Cannot set tile to unwalkable. Clear obstacle on tile first", this); return; }
				Undo.RecordObject(tile.gameObject, "Tile");
				tile.walkable=false;
				tile.gameObject.SetActive(false);
			}
			else if(tileState==_TileStateE.Default){
				if(tile.HasObstacle()){ Debug.LogWarning("Cannot set tile to walkable. Clear obstacle on tile first", this); return; }
				Undo.RecordObject(tile.gameObject, "Tile");
				tile.walkable=true;
				tile.gameObject.SetActive(true);
			}
			else if(tileState==_TileStateE.WallH || tileState==_TileStateE.WallF){
				if(tile.HasObstacle()){ Debug.LogWarning("Cannot add/remove wall. Clear obstacle on tile first", this); return; }
				
				Grid grid=instance.GetGrid();
				Vector3 dir=hitPos-tile.GetPos();
				float angle=0;
				if(instance.tileType==_TileType.Square) angle=Utilities.VectorToAngle90(new Vector2(dir.x, dir.z));
				else if(instance.tileType==_TileType.Hex) angle=Utilities.VectorToAngle60(new Vector2(dir.x, dir.z));
				
				Tile neighbourTile=grid.GetNeighbourInDir(tile, angle);
				if(neighbourTile==null){ Debug.LogWarning("Cannot add/remove wall. There's no adjacent tile", this); return; }
				if(neighbourTile.HasObstacle()){ Debug.LogWarning("Cannot add/remove wall. Clear obstacle on neighbour tile first", this); return; }
				
				if(mouseClick==0) tile.AddWall(angle, neighbourTile, tileState==_TileStateE.WallH ? 1 : 2);
				else tile.RemoveWall(angle, neighbourTile);
			}
			else if(tileState==_TileStateE.ObstacleH || tileState==_TileStateE.ObstacleF){
				if(mouseClick==1) tile.RemoveObstacle();
				else{
					ClearSpawnTile(tile);
					ClearDeployableTile(tile);
					tile.AddObstacle(tileState==_TileStateE.ObstacleH ? 1 : 2, instance.grid.GetTileSize());
				}
			}
			else if(tileState==_TileStateE.SpawnArea){
				if(tile.HasObstacle()) return;
				
				if(spawnAreaFactionID>=factionManager.factionList.Count) return;
				if(spawnAreaInfoID>=factionManager.factionList[spawnAreaFactionID].spawnInfoList.Count) return;
				
				Undo.RecordObject(factionManager, "FactionManager");
				
				Faction fac=null;
				
				if(mouseClick==1) ClearSpawnTile(tile);
				else{
					fac=factionManager.factionList[spawnAreaFactionID];
					if(tile.spawnAreaID!=fac.ID) ClearSpawnTile(tile);
					
					if(!fac.spawnInfoList[spawnAreaInfoID].startingTileList.Contains(tile)){
						fac.spawnInfoList[spawnAreaInfoID].startingTileList.Add(tile);
						tile.spawnAreaID=fac.ID;
					}
				}
			}
			else if(tileState==_TileStateE.Deployment){
				if(tile.HasObstacle()) return;
				
				if(deployAreaFactionID>=factionManager.factionList.Count) return;
				
				Undo.RecordObject(factionManager, "FactionManager");
				
				Faction fac=null;
				
				if(mouseClick==1) ClearDeployableTile(tile);
				else{
					fac=factionManager.factionList[deployAreaFactionID];
					if(tile.deployAreaID!=fac.ID) ClearDeployableTile(tile);
					
					if(!fac.deployableTileList.Contains(tile)){
						fac.deployableTileList.Add(tile);
						tile.deployAreaID=fac.ID;
					}
				}
				
			}
		}
		
		void ClearSpawnTile(Tile tile){
			if(tile.spawnAreaID!=-1){
				for(int i=0; i<factionManager.factionList.Count; i++){
					Faction facAlt=factionManager.factionList[i];
					for(int n=0; n<facAlt.spawnInfoList.Count; n++){
						if(facAlt.spawnInfoList[n].startingTileList.Contains(tile)){
							Undo.RecordObject(factionManager, "FactionManager");
							facAlt.spawnInfoList[n].startingTileList.Remove(tile);
							tile.spawnAreaID=-1;
							break;
						}
					}
				}
			}
		}
		void ClearDeployableTile(Tile tile){
			if(tile.deployAreaID==-1) return;
			for(int i=0; i<factionManager.factionList.Count; i++){
				Faction facAlt=factionManager.factionList[i];
				if(facAlt.deployableTileList.Contains(tile)){
					Undo.RecordObject(factionManager, "FactionManager");
					facAlt.deployableTileList.Remove(tile);
					tile.deployAreaID=-1;
					break;
				}
			}
		}
		
		
		
		
		void EditTileUnit(Tile tile, int mouseClick=0, Vector3 hitPos=default(Vector3)){
			if(mouseClick==0){
				if(!tile.walkable){ Debug.LogWarning("Cannot place unit on unwalkable tile", this); return; }
				if(tile.obstacleT!=null){ Debug.LogWarning("Cannot place unit on tile with obstacle", this); return; }
				if(selectedUnit==null){ Debug.LogWarning("No unit selected. Select a unit from the editor first", this); return; }
				
				if(tile.unit!=null) RemoveUnit(tile.unit);
				if(tile.collectible!=null) RemoveCollectible(tile.collectible);
				
				Vector3 dir=hitPos-tile.GetPos();
				float angle=0;
				if(instance.tileType==_TileType.Square) angle=360-(Utilities.VectorToAngle90(new Vector2(dir.x, dir.z))-90);
				else if(instance.tileType==_TileType.Hex) angle=360-(Utilities.VectorToAngle60(new Vector2(dir.x, dir.z))-90);
				
				GameObject unitObj=(GameObject)PrefabUtility.InstantiatePrefab(selectedUnit.gameObject);
				
				Undo.RegisterCreatedObjectUndo(unitObj, "CreatedUnit");
				
				unitObj.transform.position=tile.GetPos();
				unitObj.transform.rotation=Quaternion.Euler(0, angle, 0);
				unitObj.transform.parent=FactionManager.GetTransform();
				
				Unit unit=unitObj.GetComponent<Unit>();
				tile.unit=unit;	unit.tile=tile;
				
				factionManager.factionList[unitFactionID].allUnitList.Add(unit);
				unit.factionID=factionManager.factionList[unitFactionID].ID;
			}
			else if(mouseClick==1){
				if(tile.unit!=null){
					RemoveUnit(tile.unit);
					tile.unit=null;
				}
			}
		}
		
		
		
		void EditTileCollectible(Tile tile, int mouseClick=0, Vector3 hitPos=default(Vector3)){
			if(mouseClick==0){
				if(!tile.walkable){ Debug.LogWarning("Cannot place item on unwalkable tile", this); return; }
				if(tile.obstacleT!=null){ Debug.LogWarning("Cannot place item on tile with obstacle", this); return; }
				if(selectedCollectible==null){ Debug.LogWarning("No item selected. Select a unit from the editor first", this); return; }
				
				if(tile.unit!=null) RemoveUnit(tile.unit);
				if(tile.collectible!=null) RemoveCollectible(tile.collectible);
				
				GameObject itemObj=(GameObject)PrefabUtility.InstantiatePrefab(selectedCollectible.gameObject);
				
				Undo.RegisterCreatedObjectUndo(itemObj, "CreatedItem");
				
				itemObj.transform.position=tile.GetPos();
				itemObj.transform.rotation=Quaternion.identity;
				itemObj.transform.parent=tile.transform;
				
				Collectible collectible=itemObj.GetComponent<Collectible>();
				tile.collectible=collectible;	
				
				collectibleManager.PlaceItemAtTile(itemObj, tile);
			}
			else if(mouseClick==1){
				if(tile.collectible!=null){
					RemoveCollectible(tile.collectible);
					tile.collectible=null;
				}
			}
		}
		
		
		
		void RemoveUnit(Unit unit){
			for(int i=0; i<factionManager.factionList.Count; i++){
				if(factionManager.factionList[i].allUnitList.Contains(unit)){
					factionManager.factionList[i].allUnitList.Remove(unit);
					break;
				}
			}
			Undo.DestroyObjectImmediate(unit.gameObject);
		}
		void RemoveCollectible(Collectible item){
			collectibleManager.RemoveItem(item);
			Undo.DestroyObjectImmediate(item.gameObject);
		}
		
	}
}
