using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	[CustomEditor(typeof(GameControl))]
	public class GameControlInspector : TBEditorInspector{

		private static GameControl instance;
		private static TurnControl turnControl;
		private static GridManager gridManager;
		private static FactionManager factionManager;
		private static CollectibleManager collectibleManager;
		private static SettingDB settingDB;
		
		private string[] turnModeLabel;
		private string[] turnModeTooltip;
		private string[] moveOrderLabel;
		private string[] moveOrderTooltip;
		
		void Awake(){
			instance = (GameControl)target;
			LoadDB();
			
			turnControl = (TurnControl)FindObjectOfType(typeof(TurnControl));
			gridManager = (GridManager)FindObjectOfType(typeof(GridManager));
			factionManager = (FactionManager)FindObjectOfType(typeof(FactionManager));
			collectibleManager = (CollectibleManager)FindObjectOfType(typeof(CollectibleManager));
			settingDB=SettingDB.LoadDB();
			
			InitLabel();
		}
		
		void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_TurnMode)).Length;
			turnModeLabel=new string[enumLength];
			turnModeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				turnModeLabel[i]=((_TurnMode)i).ToString();
				if((_TurnMode)i==_TurnMode.FactionPerTurn) 
					turnModeTooltip[i]="Always show the tile currently being hovered over by the cursor";
				if((_TurnMode)i==_TurnMode.FactionUnitPerTurn) 
					turnModeTooltip[i]="Only show the tile currently being hovered over by the cursor if it's available to be built on";
				if((_TurnMode)i==_TurnMode.UnitPerTurn) 
					turnModeTooltip[i]="Never show the tile currently being hovered over by the cursor";
			}
			
			enumLength = Enum.GetValues(typeof(_MoveOrder)).Length;
			moveOrderLabel=new string[enumLength];
			moveOrderTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				moveOrderLabel[i]=((_MoveOrder)i).ToString();
				if((_MoveOrder)i==_MoveOrder.Free) 
					moveOrderTooltip[i]="Always show the tile currently being hovered over by the cursor";
				if((_MoveOrder)i==_MoveOrder.Random) 
					moveOrderTooltip[i]="Only show the tile currently being hovered over by the cursor if it's available to be built on";
				if((_MoveOrder)i==_MoveOrder.StatsBased) 
					moveOrderTooltip[i]="Never show the tile currently being hovered over by the cursor";
			}
		}
		
		
		public override void OnInspectorGUI(){
			EditorGUIUtility.labelWidth=150;
			
			base.OnInspectorGUI();
			
			GUI.changed = false;
			Undo.RecordObject(instance, "GameControl");
			Undo.RecordObject(turnControl, "TurnControl");
			Undo.RecordObject(gridManager, "GridManager");
			Undo.RecordObject(factionManager, "FactionManager");
			Undo.RecordObject(settingDB, "SettingDB");
			
			
			EditorGUILayout.Space();
			
				cont=new GUIContent("Use Global Setting:", "Check to use a global setting. This setting will be used for all the scene in the project that have this flag checked");
				instance.useGlobalSetting=EditorGUILayout.Toggle(cont, instance.useGlobalSetting);
			
			EditorGUILayout.LabelField("_____________________________________________________________________________________");
			EditorGUILayout.Space();
			
			
				if(instance.useGlobalSetting){
					
						EditorGUIUtility.labelWidth=180;
					
						if(gridManager==null) EditorGUILayout.HelpBox("There's no GridManager in the scene", MessageType.Warning);
						if(factionManager==null) EditorGUILayout.HelpBox("There's no FactionManager in the scene", MessageType.Warning);
					
						cont=new GUIContent("Generate Grid On Start:", "Check to regenerate grid upon game start, any preset (including units) on the grid will be override");
						settingDB.generateGridOnStart=EditorGUILayout.Toggle(cont, settingDB.generateGridOnStart);
					
						cont=new GUIContent("Generate Unit On Start:", "Check to regenerate unit upon game start, any existing unit on the grid will be removed");
						settingDB.generateUnitOnStart=EditorGUILayout.Toggle(cont, settingDB.generateUnitOnStart);
					
						cont=new GUIContent("Generate Collectible On Start:", "Check to regenerate collectibles upon game start, any existing collectibles on the grid will be removed");
						settingDB.generateCollectibleOnStart=EditorGUILayout.Toggle(cont, settingDB.generateCollectibleOnStart);
					
						EditorGUIUtility.labelWidth=150;
					
					EditorGUILayout.Space();
					
						int turnMode=(int)settingDB.turnMode;
						cont=new GUIContent("Turn Mode:", "The turn logic to be used. Determine how each faction takes turn");
						contL=new GUIContent[turnModeLabel.Length];
						for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turnModeLabel[i], turnModeTooltip[i]);
						turnMode = EditorGUILayout.Popup(cont, turnMode, contL);
						settingDB.turnMode=(_TurnMode)turnMode;
						
						if(turnControl.turnMode!=_TurnMode.UnitPerTurn){
							int moveOrder=(int)settingDB.moveOrder;
							cont=new GUIContent("Move Order:", "The move order to be used. Determine which unit gets to move first");
							contL=new GUIContent[moveOrderLabel.Length];
							for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(moveOrderLabel[i], moveOrderTooltip[i]);
							moveOrder = EditorGUILayout.Popup(cont, moveOrder, contL);
							settingDB.moveOrder=(_MoveOrder)moveOrder;
						}
						else{
							cont=new GUIContent("Move Order:", "The move order to be used. Determine which unit gets to move first");
							EditorGUILayout.LabelField(cont, new GUIContent("N/A"));
						}
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable Unit Deployment:", "Check to enable player to manually deploy their predetermined starting unit, otherwise they will be deployed automatically");
						settingDB.enableManualUnitDeployment=EditorGUILayout.Toggle(cont, settingDB.enableManualUnitDeployment);
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable ActionAfterAttack:", "Check to enable unit to perform other action after attacking a target");
						settingDB.enableActionAfterAttack=EditorGUILayout.Toggle(cont, settingDB.enableActionAfterAttack);
						
						cont=new GUIContent("Enable CounterAttack:", "Check to enable unit to counter attack when attacked. The counter is subject to the unit attack range, counter move remain, AP, etc.");
						settingDB.enableCounter=EditorGUILayout.Toggle(cont, settingDB.enableCounter);
						
						cont=new GUIContent(" - Damage Multiplier:", "Multiplier to damage inflicted when the unit are attacking via a counter attack move. Takes value from 0 and above with 0.6 being 60% of the default damage");
						if(settingDB.enableCounter) settingDB.counterDamageMultiplier=EditorGUILayout.FloatField(cont, settingDB.counterDamageMultiplier);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Counter AP Multiplier:", "Multiplier to attack AP cost when the unit are attacking via a counter attack move. Takes value from 0 and above with 0.7 being 70% of the default AP cost");
						if(settingDB.enableCounter && settingDB.useAPForAttack) settingDB.counterAPMultiplier=EditorGUILayout.FloatField(cont, settingDB.counterAPMultiplier);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Restore Unit AP on Turn:", "Check to have unit's AP restored to full on each turn");
						settingDB.restoreUnitAPOnTurn=EditorGUILayout.Toggle(cont, settingDB.restoreUnitAPOnTurn);
						
						cont=new GUIContent("Use AP For Move:", "Check to have unit use AP for each move");
						settingDB.useAPForMove=EditorGUILayout.Toggle(cont, settingDB.useAPForMove);
						
						cont=new GUIContent("Use AP For Attack:", "Check to have unit use AP for each attack");
						settingDB.useAPForAttack=EditorGUILayout.Toggle(cont, settingDB.useAPForAttack);
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Attack Through Obstacle:", "Check to enable unit to attack through obstacle.\nOnly applies when Fog-of-War is disabled\n\nNote: only obstacle wth full cover can obstruct an attack. Unit can always attack through obstacle with half cover");
						if(settingDB.enableFogOfWar) EditorGUILayout.LabelField(cont, new GUIContent("-"));
						else settingDB.attackThroughObstacle=EditorGUILayout.Toggle(cont, settingDB.attackThroughObstacle);
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Overwatch Hit Penalty:", "Hit chance penalty for any overwatch attack.\nTakes value from 0-1 with 0.2 being 20% reduction in hit chance");
						settingDB.overwatchHitPenalty=EditorGUILayout.FloatField(cont, settingDB.overwatchHitPenalty);
						
						cont=new GUIContent("Overwatch Crit Penalty:", "Crit chance penalty for any overwatch attack.\nTakes value from 0-1 with 0.2 being 20% reduction in critical chance");
						settingDB.overwatchCritPenalty=EditorGUILayout.FloatField(cont, settingDB.overwatchCritPenalty);
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable Fog-Of-War:", "Check to enable Fog-of-War in the game");
						settingDB.enableFogOfWar=EditorGUILayout.Toggle(cont, settingDB.enableFogOfWar);
						
						cont=new GUIContent(" - Peek Factor:", "A value indicate if the units can peek around a obstacle to see what's on the other end.\nTakes value from 0-0.5\nWhen set to 0, unit cannot peek at all (can only see 45degree from the obstacle)\nWhen set to 0.5, unit can peek and will be able to see what's behind the obstacle");
						if(settingDB.enableFogOfWar) settingDB.peekFactor=EditorGUILayout.FloatField(cont, settingDB.peekFactor);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
					EditorGUILayout.Space();
						
						cont=new GUIContent("Enable Cover System:", "Check to enable cover system in the game. Unit will get a hit penalty when attacking target behind a wall/obstacle (in cover) as well as getting a critical bonus when attacking a target not in cover.");
						settingDB.enableCover=EditorGUILayout.Toggle(cont, settingDB.enableCover);
					
						cont=new GUIContent(" - Effective Angle:", "The maximum angle from the attacking unit to target's cover facing for the cover to have effect. Anything beyond the angle, the target is considered not in covered");
						if(settingDB.enableCover) settingDB.effectiveCoverAngle=EditorGUILayout.IntField(cont, settingDB.effectiveCoverAngle);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Exposed Critical Bonus:", "The citical chance bonus for attacking a unit not in cover. Value is used to modify the critical chance directly. ie. 0.25 means 25% increase in critical chance");
						if(settingDB.enableCover) settingDB.exposedCritBonus=EditorGUILayout.FloatField(cont, settingDB.exposedCritBonus);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Full Cover Bonus:", "The dodge bonus for unit attacked from behind a 'full' cover. Value is used to modify the unit dodge directly. ie. 0.25 means 25% increase in dodge chance");
						if(settingDB.enableCover) settingDB.fullCoverBonus=EditorGUILayout.FloatField(cont, settingDB.fullCoverBonus);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Half Cover Bonus:", "The dodge bonus for unit attacked from behind a 'half' cover. Value is used to modify the unit dodge directly. ie. 0.25 means 25% increase in dodge chance");
						if(settingDB.enableCover) settingDB.halfCoverBonus=EditorGUILayout.FloatField(cont, settingDB.halfCoverBonus);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable Flanking:", "Check to enable flanking, unit will get a damage bonus when attacking a target from the rear");
						settingDB.enableFlanking=EditorGUILayout.Toggle(cont, settingDB.enableFlanking);
						
						cont=new GUIContent(" - Flanking Angle:", "The angle at which the target will be considered flanked. This angle origin from target's front. ie, when set to 80, the target is considered flanked when attacked from the side");
						if(settingDB.enableFlanking) settingDB.flankingAngle=EditorGUILayout.FloatField(cont, settingDB.flankingAngle);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Flanking Bonus:", "The damage multiplier to be applied to the damage. Takes value from 0 and above with 0.2 being increase damage by 20%");
						if(settingDB.enableFlanking) instance.flankingBonus=EditorGUILayout.FloatField(cont, settingDB.flankingBonus);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable Action Cam:", "Check to enable dynamic action camera for attack and ability casting. Note that this is still subject to the frequency of each event");
						settingDB.enableActionCam=EditorGUILayout.Toggle(cont, settingDB.enableActionCam);
					
						cont=new GUIContent(" - Attack Frequency:", "The chance at which action camera will trigger when a unit is attacking. Takes value from 0-1 with 0.2 being 20% chance to trigger.");
						if(settingDB.enableActionCam) settingDB.actionCamFreqAttack=EditorGUILayout.FloatField(cont, settingDB.actionCamFreqAttack);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Ability Frequency:", "The chance at which action camera will trigger when a unit is casting an ability. Takes value from 0-1 with 0.2 being 20% chance to trigger.");
						if(settingDB.enableActionCam) settingDB.actionCamFreqAbility=EditorGUILayout.FloatField(cont, settingDB.actionCamFreqAbility);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
					
				}
				else{
					
						EditorGUIUtility.labelWidth=180;
					
						if(gridManager!=null){
							cont=new GUIContent("Generate Grid On Start:", "Check to regenerate grid upon game start, any preset (including units) on the grid will be override");
							gridManager.generateGridOnStart=EditorGUILayout.Toggle(cont, gridManager.generateGridOnStart);
						}
						else EditorGUILayout.HelpBox("There's no GridManager in the scene", MessageType.Warning);
						
						if(factionManager!=null){
							cont=new GUIContent("Generate Unit On Start:", "Check to regenerate unit upon game start, any existing unit on the grid will be removed");
							factionManager.generateUnitOnStart=EditorGUILayout.Toggle(cont, factionManager.generateUnitOnStart);
						}
						else EditorGUILayout.HelpBox("There's no FactionManager in the scene", MessageType.Warning);
						
						if(collectibleManager!=null){
							cont=new GUIContent("Generate Collectible On Start:", "Check to regenerate collectibles upon game start, any existing collectibles on the grid will be removed");
							collectibleManager.generateCollectibleOnStart=EditorGUILayout.Toggle(cont, collectibleManager.generateCollectibleOnStart);
						}
						else EditorGUILayout.HelpBox("There's no CollectibleManager in the scene", MessageType.Warning);
						
						EditorGUIUtility.labelWidth=150;
					
					EditorGUILayout.Space();
					
						int turnMode=(int)turnControl.turnMode;
						cont=new GUIContent("Turn Mode:", "The turn logic to be used. Determine how each faction takes turn");
						contL=new GUIContent[turnModeLabel.Length];
						for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(turnModeLabel[i], turnModeTooltip[i]);
						turnMode = EditorGUILayout.Popup(cont, turnMode, contL);
						turnControl.turnMode=(_TurnMode)turnMode;
						
						if(turnControl.turnMode!=_TurnMode.UnitPerTurn){
							int moveOrder=(int)turnControl.moveOrder;
							cont=new GUIContent("Move Order:", "The move order to be used. Determine which unit gets to move first");
							contL=new GUIContent[moveOrderLabel.Length];
							for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(moveOrderLabel[i], moveOrderTooltip[i]);
							moveOrder = EditorGUILayout.Popup(cont, moveOrder, contL);
							turnControl.moveOrder=(_MoveOrder)moveOrder;
						}
						else{
							cont=new GUIContent("Move Order:", "The move order to be used. Determine which unit gets to move first");
							EditorGUILayout.LabelField(cont, new GUIContent("-"));
						}
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable Unit Deployment:", "Check to enable player to manually deploy their predetermined starting unit, otherwise they will be deployed automatically");
						instance.enableManualUnitDeployment=EditorGUILayout.Toggle(cont, instance.enableManualUnitDeployment);
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable ActionAfterAttack:", "Check to enable unit to perform other action after attacking a target");
						instance.enableActionAfterAttack=EditorGUILayout.Toggle(cont, instance.enableActionAfterAttack);
						
						cont=new GUIContent("Enable CounterAttack:", "Check to enable unit to counter attack when attacked. The counter is subject to the unit attack range, counter move remain, AP, etc.");
						instance.enableCounter=EditorGUILayout.Toggle(cont, instance.enableCounter);
						
						cont=new GUIContent(" - Damage Multiplier:", "Multiplier to damage inflicted when the unit are attacking via a counter attack move. Takes value from 0 and above with 0.6 being 60% of the default damage");
						if(instance.enableCounter) instance.counterDamageMultiplier=EditorGUILayout.FloatField(cont, instance.counterDamageMultiplier);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Counter AP Multiplier:", "Multiplier to attack AP cost when the unit are attacking via a counter attack move. Takes value from 0 and above with 0.7 being 70% of the default AP cost");
						if(instance.enableCounter && instance.useAPForAttack) instance.counterAPMultiplier=EditorGUILayout.FloatField(cont, instance.counterAPMultiplier);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Restore Unit AP on Turn:", "Check to have unit's AP restored to full on each turn");
						instance.restoreUnitAPOnTurn=EditorGUILayout.Toggle(cont, instance.restoreUnitAPOnTurn);
						
						cont=new GUIContent("Use AP For Move:", "Check to have unit use AP for each move");
						instance.useAPForMove=EditorGUILayout.Toggle(cont, instance.useAPForMove);
						
						cont=new GUIContent("Use AP For Attack:", "Check to have unit use AP for each attack");
						instance.useAPForAttack=EditorGUILayout.Toggle(cont, instance.useAPForAttack);
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Attack Through Obstacle:", "Check to enable unit to attack through obstacle.\nOnly applies when Fog-of-War is disabled\n\nNote: only obstacle wth full cover can obstruct an attack. Unit can always attack through obstacle with half cover");
						if(instance.enableFogOfWar) EditorGUILayout.LabelField(cont, new GUIContent("-"));
						else instance.attackThroughObstacle=EditorGUILayout.Toggle(cont, instance.attackThroughObstacle);
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Overwatch Hit Penalty:", "Hit chance penalty for any overwatch attack.\nTakes value from 0-1 with 0.2 being 20% reduction in hit chance");
						instance.overwatchHitPenalty=EditorGUILayout.FloatField(cont, instance.overwatchHitPenalty);
						
						cont=new GUIContent("Overwatch Crit Penalty:", "Crit chance penalty for any overwatch attack.\nTakes value from 0-1 with 0.2 being 20% reduction in critical chance");
						instance.overwatchCritPenalty=EditorGUILayout.FloatField(cont, instance.overwatchCritPenalty);
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable Fog-Of-War:", "Check to enable Fog-of-War in the game");
						instance.enableFogOfWar=EditorGUILayout.Toggle(cont, instance.enableFogOfWar);
						
						cont=new GUIContent(" - Peek Factor:", "A value indicate if the units can peek around a obstacle to see what's on the other end.\nTakes value from 0-0.5\nWhen set to 0, unit cannot peek at all (can only see 45degree from the obstacle)\nWhen set to 0.5, unit can peek and will be able to see what's behind the obstacle");
						if(instance.enableFogOfWar) instance.peekFactor=EditorGUILayout.FloatField(cont, instance.peekFactor);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
					EditorGUILayout.Space();
						
						cont=new GUIContent("Enable Cover System:", "Check to enable cover system in the game. Unit will get a hit penalty when attacking target behind a wall/obstacle (in cover) as well as getting a critical bonus when attacking a target not in cover.");
						instance.enableCover=EditorGUILayout.Toggle(cont, instance.enableCover);
					
						cont=new GUIContent(" - Effective Angle:", "The maximum angle from the attacking unit to target's cover facing for the cover to have effect. Anything beyond the angle, the target is considered not in covered");
						if(instance.enableCover) instance.effectiveCoverAngle=EditorGUILayout.IntField(cont, instance.effectiveCoverAngle);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Exposed Critical Bonus:", "The citical chance bonus for attacking a unit not in cover. Value is used to modify the critical chance directly. ie. 0.25 means 25% increase in critical chance");
						if(instance.enableCover) instance.exposedCritBonus=EditorGUILayout.FloatField(cont, instance.exposedCritBonus);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Full Cover Bonus:", "The dodge bonus for unit attacked from behind a 'full' cover. Value is used to modify the unit dodge directly. ie. 0.25 means 25% increase in dodge chance");
						if(instance.enableCover) instance.fullCoverBonus=EditorGUILayout.FloatField(cont, instance.fullCoverBonus);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Half Cover Bonus:", "The dodge bonus for unit attacked from behind a 'half' cover. Value is used to modify the unit dodge directly. ie. 0.25 means 25% increase in dodge chance");
						if(instance.enableCover) instance.halfCoverBonus=EditorGUILayout.FloatField(cont, instance.halfCoverBonus);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
					
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable Flanking:", "Check to enable flanking, unit will get a damage bonus when attacking a target from the rear");
						instance.enableFlanking=EditorGUILayout.Toggle(cont, instance.enableFlanking);
						
						cont=new GUIContent(" - Flanking Angle:", "The angle at which the target will be considered flanked. This angle origin from target's front. ie, when set to 80, the target is considered flanked when attacked from the side");
						if(instance.enableFlanking) instance.flankingAngle=EditorGUILayout.FloatField(cont, instance.flankingAngle);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Flanking Bonus:", "The damage multiplier to be applied to the damage. Takes value from 0 and above with 0.2 being increase damage by 20%");
						if(instance.enableFlanking) instance.flankingBonus=EditorGUILayout.FloatField(cont, instance.flankingBonus);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
					EditorGUILayout.Space();
					
						cont=new GUIContent("Enable Action Cam:", "Check to enable dynamic action camera for attack and ability casting. Note that this is still subject to the frequency of each event");
						instance.enableActionCam=EditorGUILayout.Toggle(cont, instance.enableActionCam);
					
						cont=new GUIContent(" - Attack Frequency:", "The chance at which action camera will trigger when a unit is attacking. Takes value from 0-1 with 0.2 being 20% chance to trigger.");
						if(instance.enableActionCam) instance.actionCamFreqAttack=EditorGUILayout.FloatField(cont, instance.actionCamFreqAttack);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
						
						cont=new GUIContent(" - Ability Frequency:", "The chance at which action camera will trigger when a unit is casting an ability. Takes value from 0-1 with 0.2 being 20% chance to trigger.");
						if(instance.enableActionCam) instance.actionCamFreqAbility=EditorGUILayout.FloatField(cont, instance.actionCamFreqAbility);
						else EditorGUILayout.LabelField(cont, new GUIContent("-"));
					
				}
			
			
			EditorGUILayout.LabelField("_____________________________________________________________________________________");
			EditorGUILayout.Space();
			
				
				EditorGUIUtility.labelWidth=120;
				cont=new GUIContent("Next Scene Name:", "Scene's name to be loaded when this level is completed");
				instance.nextScene=EditorGUILayout.TextField(cont, instance.nextScene);
				cont=new GUIContent("Main Menu Name:", "Scene's name of the main menu to be loaded when return to menu on UI is called");
				instance.mainMenu=EditorGUILayout.TextField(cont, instance.mainMenu);
				
			
			EditorGUILayout.LabelField("_____________________________________________________________________________________");
			EditorGUILayout.Space();
			
			if(!Application.isPlaying){
				if(gridManager!=null){
					if(GUILayout.Button("Generate Grid", GUILayout.MaxWidth(258))) gridManager.GenerateGrid();
				}
				if(factionManager!=null){
					if(GUILayout.Button("Generate Unit", GUILayout.MaxWidth(258))) FactionManager.GenerateUnit();
				}
			}
				
				
			EditorGUILayout.Space();
				
				
			
			
			EditorGUILayout.Space();
			
			DefaultInspector();
			
			
			if(GUI.changed){
				EditorUtility.SetDirty(instance);
				EditorUtility.SetDirty(turnControl);
				EditorUtility.SetDirty(gridManager);
				EditorUtility.SetDirty(factionManager);
				EditorUtility.SetDirty(settingDB);
			}
		}
	}
}
