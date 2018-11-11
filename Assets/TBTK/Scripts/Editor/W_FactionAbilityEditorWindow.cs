using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {
	
	public class NewFactionAbilityEditorWindow : NewAbilityEditorWindow {
		
		private static NewFactionAbilityEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			// Get existing open window or if none, make a new one:
			window = (NewFactionAbilityEditorWindow)EditorWindow.GetWindow(typeof (NewFactionAbilityEditorWindow), false, "Faction Ability Editor");
			window.minSize=new Vector2(420, 300);
			
			LoadDB();
			
			window.InitLabel();
			
			//if(prefabID>=0) window.selectID=TDEditor.GetTowerIndex(prefabID)-1;
			
			window.SetupCallback();
		}
		
		public void SetupCallback(){
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
		}
		
		
		private static string[] abilityTypeLabel;
		private static string[] abilityTypeTooltip;
		
		public override void InitLabel(){
			base.InitLabel();
			
			int enumLength = Enum.GetValues(typeof(_AbilityType)).Length;
			abilityTypeLabel=new string[enumLength];
			abilityTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				abilityTypeLabel[i]=((_AbilityType)i).ToString();
				if((_AbilityType)i==_AbilityType.Generic) 			abilityTypeTooltip[i]="Modify stats";
				if((_AbilityType)i==_AbilityType.SpawnNew) 		abilityTypeTooltip[i]="Deploy an additional unit";
				if((_AbilityType)i==_AbilityType.ScanFogOfWar) 	abilityTypeTooltip[i]="Reveal fog of war";
			}
		}
		
		
		
		
		public override bool OnGUI() {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<FactionAbility> abilityList=fAbilityDB.abilityList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(fAbilityDB, "AbilityDB");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTB();
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(abilityList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawAbilityList(startX, startY, abilityList);	
			
			startX=v2.x+25;
			
			if(abilityList.Count==0) return true;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				v2=DrawAbilityConfigurator(startX, startY, abilityList[selectID]);
				contentWidth=v2.x+35;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) SetDirtyTB();
			
			return true;
		}
		
		
		
		private bool foldGeneral=true;
		protected float DrawGeneralSetting(float startX, float startY, FactionAbility ability){
			string text="General Setting "+(!foldGeneral ? "(show)" : "(hide)");
			foldGeneral=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldGeneral, text, foldoutStyle);
			if(foldGeneral){
				startX+=15;
				
				cont=new GUIContent("Cost (AP):", "AP cost to use the ability");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.cost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), ability.cost);
				
				cont=new GUIContent("Cooldown:", "The cooldown period (in turn) of the ability after used");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.cooldown=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), ability.cooldown);
				
				cont=new GUIContent("Use Limit:", "How many time the ability can be used in a single battle");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.useLimit=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), ability.useLimit);
				
			}
			
			return startY+spaceY;
		}
		
		
		private bool foldTargeting=true;
		protected float DrawTargetingSetting(float startX, float startY, FactionAbility ability){
			string text="Targeting Setting "+(!foldTargeting ? "(show)" : "(hide)");
			foldTargeting=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldTargeting, text, foldoutStyle);
			if(foldTargeting){
				startX+=15;
				
				int effTargetType=(int)ability.effTargetType;
				cont=new GUIContent("Effect Target:", "Specify which target the ability's effect can be applied to");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				contL=new GUIContent[effTargetTypeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(effTargetTypeLabel[i], effTargetTypeTooltip[i]);
				effTargetType = EditorGUI.Popup(new Rect(startX+spaceX, startY, width-40, 15), new GUIContent(""), effTargetType, contL);
				ability.effTargetType=(_EffectTargetType)effTargetType;
				
				startY+=5;
				
				cont=new GUIContent("Require Target Selection:", "Check if the ability require target selection. Otherwise the ability will be apply to the unit which uses it.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.requireTargetSelection=EditorGUI.Toggle(new Rect(startX+spaceX+30, startY, widthS, height), ability.requireTargetSelection);
				
				cont=new GUIContent(" - Target Type:", "Indicate what tile can be constitued as a valid target during target selection");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.requireTargetSelection){
					int targetType=(int)ability.targetType;
					contL=new GUIContent[targetTypeLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(targetTypeLabel[i], targetTypeTooltip[i]);
					targetType = EditorGUI.Popup(new Rect(startX+spaceX+30, startY, width-40, 15), new GUIContent(""), targetType, contL);
					ability.targetType=(_TargetType)targetType;
				}
				else EditorGUI.LabelField(new Rect(startX+spaceX+30, startY, widthS, height), "-");
				
				cont=new GUIContent(" - AOE Range:", "The range of of ability Area-of-Effective");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.requireTargetSelection) ability.aoeRange=EditorGUI.IntField(new Rect(startX+spaceX+30, startY, widthS, height), ability.aoeRange);
				else EditorGUI.LabelField(new Rect(startX+spaceX+30, startY, widthS, height), "-");
			}
			
			return startY+spaceY;
		}
		
		
		private bool foldEffect=true;
		protected float DrawAbilityEffect(float startX, float startY, FactionAbility ability){
			string text="Ability Effects Setting "+(!foldEffect ? "(show)" : "(hide)");
			foldEffect=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldEffect, text, foldoutStyle);
			if(foldEffect){
				startX+=15;
				
				//~ cont=new GUIContent("Use Default Effect:", "Check if the ability use default TBTK effect.\n\nAlternatively you can use your own custom effect by putting a custom script to on the spawn visual effect object");
				//~ EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				//~ ability.useDefaultEffect=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), ability.useDefaultEffect);
				
				if(ability.useDefaultEffect){
				
					cont=new GUIContent("Effect Delay:", "Delay in second before the effect actually take place.\nThis is for the visual effect to kicks in");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(ability.type==_AbilityType.None) EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					ability.effectDelayDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), ability.effectDelayDuration);
					
					startY+=5;
					
					int type=(int)ability.type;
					cont=new GUIContent("Ability Effect Type:", "Type of the ability's effect. Define what the ability do");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					contL=new GUIContent[abilityTypeLabel.Length];
					for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(abilityTypeLabel[i], abilityTypeTooltip[i]);
					type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), type, contL);
					ability.type=(_AbilityType)type;
					
					//startX+=15;	spaceX-=15;
					
					if(ability.type==_AbilityType.Generic){
						startY=DrawEffect(startX, startY+spaceY, ability.effect);
					}
					
					//~ if(ability.type==UnitAbility._AbilityType.Teleport){
						//~ GUI.Label(new Rect(startX+spaceX, startY+=spaceY, width, height), "- No input required -");
					//~ }
					
					if(ability.type==_AbilityType.ScanFogOfWar){
						//GUI.Label(new Rect(startX+spaceX, startY+=spaceY, width, height), "- No input required -");
						
						cont=new GUIContent(" - Duration:", "");
						EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
						ability.effect.duration=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), ability.effect.duration);
					}
					
					if(ability.type==_AbilityType.SpawnNew){
						cont=new GUIContent(" - Unit To Spawn:", "Unit to be spawned");
						GUI.Label(new Rect(startX, startY+=spaceY, width, height), cont);
						int index=ability.spawnUnit!=null ? TBEditor.GetUnitIndex(ability.spawnUnit.prefabID) : 0;
						index = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, unitLabel);
						if(index>0) ability.spawnUnit=unitDB.unitList[index-1];
						else if(index==0) ability.spawnUnit=null;
					}
					
					
					if(ability.type==_AbilityType.SpawnNew){
						if(ability.targetType!=_TargetType.EmptyTile) Debug.LogWarning(ability.type+" ability must have 'EmptyTile' TargetType");
						if(!ability.requireTargetSelection) Debug.LogWarning(ability.type+" ability requires target selection");
						if(ability.type!=_AbilityType.ScanFogOfWar && ability.aoeRange>0)
							Debug.LogWarning(ability.type+" ability's aoe-range must be zero");
						
						ability.targetType=_TargetType.EmptyTile;
						ability.requireTargetSelection=true;
						if(ability.type!=_AbilityType.ScanFogOfWar) ability.aoeRange=0;
					}
					
					
					//startX-=15;	spaceX+=15;
				}
			}
			
			return startY+spaceY;
		}
		
		
		
		private Vector2 DrawAbilityConfigurator(float startX, float startY, FactionAbility ability){
			
			TBEditor.DrawSprite(new Rect(startX, startY, 60, 60), ability.icon);
			startX+=65;
			
			cont=new GUIContent("Name:", "The ability name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY/4, width, height), cont);
			ability.name=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), ability.name);
			if(GUI.changed) UpdateLabel_FactionAbility();
			
			cont=new GUIContent("Icon:", "The ability icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			ability.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), ability.icon, typeof(Sprite), false);
			
			cont=new GUIContent("AbilityID:", "The ID used to associate a perk item in perk menu to a perk when configuring perk menu manually");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.LabelField(new Rect(startX+spaceX-65, startY, width-5, height), ability.prefabID.ToString());
			
			startX-=65;
			startY+=10+spaceY-spaceY/2;	//cachedY=startY;
			
				cont=new GUIContent("Only Available Via Perk:", "Check if the ability can only be added by perk ");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.onlyAvailableViaPerk=EditorGUI.Toggle(new Rect(startX+spaceX+20, startY, widthS, height), ability.onlyAvailableViaPerk);
			
			startY+=10;
			
				startY=DrawGeneralSetting(startX, startY+spaceY, ability);
				
				startY=DrawTargetingSetting(startX, startY+spaceY, ability);
				
				startY=DrawAbilityVisualEffect(startX, startY+spaceY, ability);
			
				startY=DrawAbilityEffect(startX, startY+spaceY, ability);
			
			startY+=10;
			
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Description (for runtime and editor): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			ability.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 100), ability.desp, style);
			
			
			return new Vector2(startX, startY+spaceY+100);
		}
		
		
		
		
		protected Vector2 DrawAbilityList(float startX, float startY, List<FactionAbility> abilityList){
			List<Item> list=new List<Item>();
			for(int i=0; i<abilityList.Count; i++){
				Item item=new Item(abilityList[i].prefabID, abilityList[i].name, abilityList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		int NewItem(int cloneID=-1){
			FactionAbility ability=null;
			
			if(cloneID==-1){
				ability=new FactionAbility();
				ability.name="New Ability";
			}
			else{
				ability=fAbilityDB.abilityList[selectID].Clone();
			}
			
			ability.prefabID=GenerateNewID(fAbilityIDList);
			fAbilityIDList.Add(ability.prefabID);
			
			fAbilityDB.abilityList.Add(ability);
			
			UpdateLabel_FactionAbility();
			
			return fAbilityDB.abilityList.Count-1;
		}
		void DeleteItem(){
			fAbilityIDList.Remove(fAbilityDB.abilityList[deleteID].prefabID);
			fAbilityDB.abilityList.RemoveAt(deleteID);
			
			UpdateLabel_FactionAbility();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<fAbilityDB.abilityList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			FactionAbility ability=fAbilityDB.abilityList[selectID];
			fAbilityDB.abilityList[selectID]=fAbilityDB.abilityList[selectID+dir];
			fAbilityDB.abilityList[selectID+dir]=ability;
			selectID+=dir;
		}
		
	}
	
}