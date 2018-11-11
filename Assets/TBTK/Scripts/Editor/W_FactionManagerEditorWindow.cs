using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {
	
	public class NewFactionManagerEditorWindow : TBEditorWindow {
		
		private static FactionManager instance;
		private static NewFactionManagerEditorWindow window;
		
		public static void Init (FactionManager facManager=null) {
			// Get existing open window or if none, make a new one:
			window = (NewFactionManagerEditorWindow)EditorWindow.GetWindow(typeof (NewFactionManagerEditorWindow), false, "FactionManager Editor");
			window.minSize=new Vector2(420, 300);
			
			LoadDB();
			
			InitLabel();
			
			if(facManager!=null) instance=facManager;
		}
		
		
		
		private static string[] limitTypeLabel=new string[0];
		private static string[] limitTypeTooltip=new string[0];
		
		private static string[] AIModeLabel=new string[0];
		private static string[] AIModeTooltip=new string[0];
		
		public static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(FactionSpawnInfo._LimitType)).Length;
			limitTypeLabel=new string[enumLength];
			limitTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				limitTypeLabel[i]=((FactionSpawnInfo._LimitType)i).ToString();
				if((FactionSpawnInfo._LimitType)i==FactionSpawnInfo._LimitType.UnitCount) limitTypeTooltip[i]="Limited to an arbitary number";
				else if((FactionSpawnInfo._LimitType)i==FactionSpawnInfo._LimitType.UnitValue) limitTypeTooltip[i]="Limited based on the total value of the added unit";
			}
			
			enumLength = Enum.GetValues(typeof(_AIMode)).Length;
			AIModeLabel=new string[enumLength];
			AIModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				AIModeLabel[i]=((_AIMode)i).ToString();
				if((_AIMode)i==_AIMode.Passive) AIModeTooltip[i]="the unit wont move unless the there are hostile within the faction's sight (using unit sight value even when Fog-Of-War is not used)";
				else if((_AIMode)i==_AIMode.Trigger) AIModeTooltip[i]="the unit wont move unless it's being triggered, when it spotted any hostile or attacked";
				else if((_AIMode)i==_AIMode.Aggressive) AIModeTooltip[i]="the unit will be on move all the time, looking for potential target";
			}
		}
		
		
		
		private int playerCount=0;
		
		public override bool OnGUI() {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			if(instance==null && !GetFactionManager()) return true;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(instance, "FactionManager");
			
			
			float startX=5;
			float startY=5;
			
			if(GUI.Button(new Rect(window.position.width-130, 5, 125, 25), "Generate Unit")) instance._GenerateUnit();
			
			cont=new GUIContent("Generate Unit On Start: ", "Check to have generate unit on the grid based on each faction spawn info. Any existing unit will be wiped from the grid.");
			EditorGUI.LabelField(new Rect(startX, startY, width+50, height), cont);
			instance.generateUnitOnStart=EditorGUI.Toggle(new Rect(startX+150, startY, width, height), instance.generateUnitOnStart);
			
			startY+=10;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+50, height), "Total Faction Count: "+instance.factionList.Count);
			if(GUI.Button(new Rect(startX+150, startY, 120, height), "Add New Faction")) instance.factionList.Add(new Faction("Faction "+(instance.factionList.Count+1)));
			
			if(playerCount==0) EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+150, height), "WARNING: No player's faction!", headerStyle);
			else{
				string text=playerCount>1 ? " (Hotseat Mode)" : "";
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+150, height), "Total Player Faction: "+playerCount+text);
			}
			
			startY+=10;
			
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+50, height), "Faction List: ");
			startY+=spaceY-5; //startX+=10;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX-5, startY-5, contentWidth-25, contentHeight-startY+spaceY);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
			float cachedY=startY;
			float maxY=0;
			
			playerCount=0;
			for(int i=0; i<instance.factionList.Count; i++){
				instance.factionList[i].ID=i;
				
				if(instance.factionList[i].isPlayerFaction) playerCount+=1;
				
				startY=DrawFactionConfigurator(startX, cachedY, spaceX+width, instance.factionList[i]);
				if(maxY<startY) maxY=startY;
				startX+=(spaceX+width)+25;
			}
			
			contentHeight=maxY;
			contentWidth=startX+25;
			
			GUI.EndScrollView();
			
			if(GUI.changed) EditorUtility.SetDirty(instance);
			
			//if(GUI.changed) SetDirtyTB();
			
			return true;
		}
		
		
		float DrawFactionConfigurator(float startX, float startY, float contWidth, Faction faction){
			
			GUI.Box(new Rect(startX, startY, contWidth+12, Mathf.Max(contentHeight-startY+5, window.position.height-startY-spaceY)), "");
			startX+=5; startY+=5;
			
			if(deleteID!=faction.ID){
				if(GUI.Button(new Rect(startX, startY, 120, height), "Remove Faction")) deleteID=faction.ID;
			}
			else{
				if(GUI.Button(new Rect(startX, startY, 120, height), "Cancel")) deleteID=-1;
				GUI.color=Color.red;
				if(GUI.Button(new Rect(startX+125, startY, 60, height), "Remove")){
					deleteID=-1;
					instance.factionList.RemoveAt(faction.ID);
					return startY+spaceY;
				}
				GUI.color=Color.white;
			}
			
			startY+=10;
			
			cont=new GUIContent("Faction Name:", "The name of the faction. Just for user reference when editing the grid. Has no real effect in game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			faction.name=EditorGUI.TextField(new Rect(startX+spaceX, startY, width, height), faction.name);
			
			cont=new GUIContent("Gizmo Color:", "The color used for the gizmo associated with the faction. Just for user reference. Has no real effect in game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			faction.color=EditorGUI.ColorField(new Rect(startX+spaceX, startY, width, height), faction.color);
			
			startY+=15;
			
			cont=new GUIContent("Player Faction:", "Check if the faction is to be controlled by a player. Otherwise it will be run by AI");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			faction.isPlayerFaction=EditorGUI.Toggle(new Rect(startX+spaceX, startY, width, height), faction.isPlayerFaction);
			
			cont=new GUIContent(" - Default AI-Mode:", "Check if the faction is to use the default AI mode set in AIManager");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(faction.isPlayerFaction) EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
			else faction.useDefaultAIMode=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), faction.useDefaultAIMode);
			
			int aiMode=(int)faction.aiMode;
			cont=new GUIContent(" - AI Mode:", "The type of AI Mode to use for this faction.");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			if(!faction.isPlayerFaction && !faction.useDefaultAIMode){
				cont=new GUIContent("", "");	contL=new GUIContent[AIModeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(AIModeLabel[i], AIModeTooltip[i]);
				aiMode = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), cont, aiMode, contL);
				faction.aiMode=(_AIMode)aiMode;
				
			}
			else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
			
			
			cont=new GUIContent("Spawn Direction:", "The default unit rotation in y-axis rotation\nValue from 0-360");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY+10, width, height), cont);
			faction.spawnDirection=EditorGUI.FloatField(new Rect(startX+spaceX, startY, width/2, height), faction.spawnDirection);
			faction.spawnDirection=Mathf.Clamp(faction.spawnDirection, 0, 360);
			
			
			startY=DrawStartUnitSetting(startX, startY+spaceY+15, faction);
			
			startY=DrawSpawnInfo(startX, startY+spaceY, faction);
			
			startY=DrawFactionAbilityInfo(startX, startY+spaceY, faction);
			
			return startY+spaceY;
		}
		
		
		private bool foldAbilityInfo=true;
		protected float DrawFactionAbilityInfo(float startX, float startY, Faction faction){
			string text="Faction Abilities "+(!foldAbilityInfo ? "(show)" : "(hide)");
			foldAbilityInfo=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldAbilityInfo, text, foldoutStyle);
			if(foldAbilityInfo){
				startX+=15;	spaceX-=15;
				
				FactionAbilityInfo abInfo=faction.abilityInfo;
				
				cont=new GUIContent(" - Full Energy:", "The maximum energy pool available to use ability for the faction");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				abInfo.energyFull=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), abInfo.energyFull);
				
				cont=new GUIContent(" - Energy Rate:", "The amount of energy gained each turn");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				abInfo.energyGainPerTurn=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), abInfo.energyGainPerTurn);
				
				
				cont=new GUIContent(" - Abilities:", "Abilities available for this faction");
				GUI.Label(new Rect(startX, startY+spaceY, width, height), cont);
				
				for(int i=0; i<fAbilityDB.abilityList.Count; i++){
					if(fAbilityDB.abilityList[i].onlyAvailableViaPerk) continue;
					if(abInfo.unavailableIDList.Contains(fAbilityDB.abilityList[i].prefabID)) continue;
					if(!abInfo.availableIDList.Contains(fAbilityDB.abilityList[i].prefabID)) abInfo.availableIDList.Add(fAbilityDB.abilityList[i].prefabID);
				}
				
				for(int i=0; i<abInfo.availableIDList.Count+1; i++){
					EditorGUI.LabelField(new Rect(startX+spaceX-15, startY+spaceY, width, height), "-");
					
					int index=(i<abInfo.availableIDList.Count) ? TBEditor.GetFactionAbilityIndex(abInfo.availableIDList[i]) : 0;
					index=EditorGUI.Popup(new Rect(startX+spaceX, startY+=spaceY, width-22, height), index, fAbilityLabel);
					if(index>0){
						int abID=fAbilityDB.abilityList[index-1].prefabID;
						if(fAbilityDB.abilityList[index-1].onlyAvailableViaPerk){
							Debug.LogWarning("Ability '"+fAbilityDB.abilityList[index-1].name+"' can only be unlocked via perk", this);
						}
						else if(!abInfo.availableIDList.Contains(abID)){
							if(i<abInfo.availableIDList.Count) abInfo.availableIDList[i]=abID;
							else abInfo.availableIDList.Add(abID);
						}
					}
					else if(i<abInfo.availableIDList.Count){ abInfo.availableIDList.RemoveAt(i); i-=1; }
					
					if(i<abInfo.availableIDList.Count && GUI.Button(new Rect(startX+spaceX+width-20, startY, 20, height-1), "-")){
						abInfo.availableIDList.RemoveAt(i); i-=1;
					}
				}
				
				abInfo.unavailableIDList=new List<int>();
				for(int i=0; i<fAbilityDB.abilityList.Count; i++){
					if(fAbilityDB.abilityList[i].onlyAvailableViaPerk) continue;
					if(abInfo.availableIDList.Contains(fAbilityDB.abilityList[i].prefabID)) continue;
					abInfo.unavailableIDList.Add(fAbilityDB.abilityList[i].prefabID);
				}
				
				spaceX+=15;
			}
			
			return startY;
		}
		
		
		private int deleteIDGroup=-1;
		private int deleteIDFac=-1;
		private bool foldSpawnInfo=true;
		protected float DrawSpawnInfo(float startX, float startY, Faction faction){
			string text="Spawn Info "+(!foldSpawnInfo ? "(show)" : "(hide)");
			foldSpawnInfo=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldSpawnInfo, text, foldoutStyle);
			if(foldSpawnInfo){
				startX+=15;	spaceX-=15;
				
				if(GUI.Button(new Rect(startX+spaceX, startY+=spaceY, width, height), "Add Spawn Group")) faction.spawnInfoList.Add(new FactionSpawnInfo());
				
				for(int i=0; i<faction.spawnInfoList.Count; i++){
					if(i==0) GUI.Label(new Rect(startX, startY+8, spaceX+width, height), "____________________________________________");
					
					startY+=spaceY+8;
					if(deleteIDGroup!=i || deleteIDFac!=faction.ID){
						if(GUI.Button(new Rect(startX+spaceX+width-115, startY, 115, height), "Remove Group")){ deleteIDGroup=i; 	deleteIDFac=faction.ID; }
					}
					else{
						if(GUI.Button(new Rect(startX+spaceX+width-55, startY, 55, height), "Cancel")){ deleteIDGroup=-1;	deleteIDFac=-1; }
						GUI.color=Color.red;
						if(GUI.Button(new Rect(startX+spaceX+width-115, startY, 60, height), "Remove")){
							deleteIDGroup=-1;	deleteIDFac=-1;
							faction.spawnInfoList.RemoveAt(i);
							continue;
						}
						GUI.color=Color.white;
					}
					
					startY=DrawSpawnGroup(startX, startY, faction.spawnInfoList[i]);	
				}
				
				startX-=15;	spaceX+=15;
			}
			
			return startY+spaceY;
		}
		protected float DrawSpawnGroup(float startX, float startY, FactionSpawnInfo spInfo){
			cont=new GUIContent("Tiles Count:", "The number of tiles assigned to this SpawnGroup in which the unit can be placed on. This can be Edit in GridEditor");
			EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
			EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), spInfo.startingTileList.Count.ToString());
			
			cont=new GUIContent("Limit Type:", "The type of spawn limit applied to this spawn group.");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			
			int limitType=(int)spInfo.limitType;
			cont=new GUIContent("", "");	contL=new GUIContent[limitTypeLabel.Length];
			for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(limitTypeLabel[i], limitTypeTooltip[i]);
			limitType = EditorGUI.Popup(new Rect(startX+spaceX, startY, width-widthS-5, height), cont, limitType, contL);
			spInfo.limitType=(FactionSpawnInfo._LimitType)limitType;
			
			
			spInfo.limit=EditorGUI.IntField(new Rect(startX+spaceX+width-widthS, startY, widthS, height), spInfo.limit);
			spInfo.limit=Mathf.Clamp(spInfo.limit, 1, 99);
			
			
			cont=new GUIContent("Prefab List:", "Unit to be spawned for this spawn group");
			GUI.Label(new Rect(startX, startY+spaceY, width, height), cont);
			
			for(int i=0; i<spInfo.unitPrefabList.Count+1; i++){
				int index=i>=spInfo.unitPrefabList.Count ? 0 : TBEditor.GetUnitIndex(spInfo.unitPrefabList[i].prefabID);
				
				GUI.Label(new Rect(startX+spaceX-15, startY+=spaceY, width, height), " - ");
				index = EditorGUI.Popup(new Rect(startX+spaceX, startY, width-widthS-5, height), index, unitLabel);
				
				if(index>0){
					Unit unit=unitDB.unitList[index-1];
					if(!spInfo.unitPrefabList.Contains(unit)){
						if(i<spInfo.unitPrefabList.Count) spInfo.unitPrefabList[i]=unit;
						else spInfo.unitPrefabList.Add(unit);
					}
				}
				else if(i<spInfo.unitPrefabList.Count){ spInfo.unitPrefabList.RemoveAt(i);	i-=1; }
				
				while(spInfo.unitLimitList.Count<spInfo.unitPrefabList.Count) spInfo.unitLimitList.Add(50);
				while(spInfo.unitLimitList.Count>spInfo.unitPrefabList.Count) spInfo.unitLimitList.RemoveAt(spInfo.unitLimitList.Count-1);
				
				if(i<spInfo.unitPrefabList.Count && i>=0)
					spInfo.unitLimitList[i]=EditorGUI.IntField(new Rect(startX+spaceX+width-widthS, startY, widthS, height), spInfo.unitLimitList[i]);
			}
			
			cont=new GUIContent("Spawn Direction:", "The default unit rotation in y-axis rotation for this spawn group\nValue from 0-360");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			spInfo.spawnDirection=EditorGUI.FloatField(new Rect(startX+spaceX, startY, width/2, height), spInfo.spawnDirection);
			spInfo.spawnDirection=Mathf.Clamp(spInfo.spawnDirection, 0, 360);
			
			GUI.Label(new Rect(startX, startY+7, spaceX+width, height), "____________________________________________");
			
			return startY;
		}
		
		
		private bool foldUnit=true;
		protected float DrawStartUnitSetting(float startX, float startY, Faction faction){
			string text="Starting Units "+(!foldUnit ? "(show)" : "(hide)");
			foldUnit=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldUnit, text, foldoutStyle);
			if(foldUnit){
				startX+=15;	spaceX-=15;
			
				cont=new GUIContent("Load From Data:", "Check to have the starting unit load from data setup prior to the level start.\n\nMUST HAVE ASSIGNED UNIT USING TBData.SetStartData()\n\nRefer to the section 'Integrating TBTK To Your Game' in documentation;");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				faction.loadFromData=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), faction.loadFromData);
				
				if(faction.loadFromData){
					cont=new GUIContent("Data-ID:", "The reference ID use to load the matching data as there can be multiple set of data.");
					EditorGUI.LabelField(new Rect(startX+spaceX+20, startY, width, height), cont);
					faction.dataID=EditorGUI.IntField(new Rect(startX+spaceX+75, startY, widthS, height), faction.dataID);
					
					cont=new GUIContent(" - Starting Unit:", "Unit to be spawned and deployed before the start of the game");
					GUI.Label(new Rect(startX, startY+spaceY, width, height), cont);
					GUI.Label(new Rect(startX+spaceX, startY+spaceY, width, height), "-");
					
					startY+=spaceY;
				}
				else{
					cont=new GUIContent(" - Starting Unit:", "Unit to be spawned and deployed before the start of the game");
					GUI.Label(new Rect(startX, startY+spaceY, width, height), cont);
					
					for(int i=0; i<faction.startingUnitList.Count+1; i++){
						int index=i>=faction.startingUnitList.Count ? 0 : TBEditor.GetUnitIndex(faction.startingUnitList[i].prefabID);
						
						GUI.Label(new Rect(startX+spaceX-15, startY+=spaceY, width, height), " - ");
						index = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, unitLabel);
						
						if(index>0){
							Unit unit=unitDB.unitList[index-1];
							//if(!faction.startingUnitList.Contains(unit)){
								if(i<faction.startingUnitList.Count) faction.startingUnitList[i]=unit;
								else faction.startingUnitList.Add(unit);
							//}
						}
						else if(i<faction.startingUnitList.Count){ faction.startingUnitList.RemoveAt(i);	i-=1; }
					}
				}
				
				spaceX+=15;
			}
			
			return startY+spaceY;
		}
			
		
		
		
		private bool GetFactionManager(){
			instance=(FactionManager)FindObjectOfType(typeof(FactionManager));
			return instance==null ? false : true ;
		}
	}
	
}