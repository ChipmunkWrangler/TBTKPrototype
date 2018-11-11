using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class NewPerkEditorWindow : TBEditorWindow {

		private static NewPerkEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			// Get existing open window or if none, make a new one:
			window = (NewPerkEditorWindow)EditorWindow.GetWindow(typeof (NewPerkEditorWindow), false, "Perk Editor");
			window.minSize=new Vector2(420, 300);
			
			LoadDB();
			
			InitLabel();
			
			//if(prefabID>=0) window.selectID=TDEditor.GetTowerIndex(prefabID)-1;
			
			window.SetupCallback();
		}
		
		
		public void SetupCallback(){
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
		}
		
		
		private static string[] perkTypeLabel;
		private static string[] perkTypeTooltip;
		
		private static void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_PerkType)).Length;
			perkTypeLabel=new string[enumLength];
			perkTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				perkTypeLabel[i]=((_PerkType)i).ToString();
				if((_PerkType)i==_PerkType.Unit) 			perkTypeTooltip[i]="Modify stats of certains unit(s)";
				if((_PerkType)i==_PerkType.Unit_All) 	perkTypeTooltip[i]="Modify stats of all units";
				
				if((_PerkType)i==_PerkType.UnitAbility) 			perkTypeTooltip[i]="Modify stats of certains unit's ability(s)";
				if((_PerkType)i==_PerkType.UnitAbility_All) 	perkTypeTooltip[i]="Modify stats of all unit's abilities";
				
				if((_PerkType)i==_PerkType.FactionAbility) 		perkTypeTooltip[i]="Modify stats of certains faction's ability(s)";
				if((_PerkType)i==_PerkType.FactionAbility_All) 	perkTypeTooltip[i]="Modify stats of all faction's abilities";
				
				if((_PerkType)i==_PerkType.NewUnitAbility) 		perkTypeTooltip[i]="Add a unit ability to certain unit(s)";
				if((_PerkType)i==_PerkType.NewFactionAbility) 	perkTypeTooltip[i]="Add a faction ability";
			}
		}
		
		
		
		
		
		public override bool OnGUI() {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			List<Perk> perkList=perkDB.perkList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(perkDB, "PerkDB");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTB();
			
			if(GUI.Button(new Rect(5, 5, 120, 25), "Create New")) Select(NewItem());
			if(perkList.Count>0 && GUI.Button(new Rect(130, 5, 100, 25), "Clone Selected")) Select(NewItem(selectID));
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawPerkList(startX, startY, perkList);	
			
			startX=v2.x+25;
			
			if(perkList.Count==0) return true;
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth-startY, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				v2=DrawPerkConfigurator(startX, startY, perkList[selectID]);
				contentWidth=v2.x+35;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			
			if(GUI.changed) SetDirtyTB();
			
			return true;
		}
		
		
		
		private bool foldGeneral=true;
		protected float DrawGeneralSetting(float startX, float startY, Perk perk){
			string text="General Setting "+(!foldGeneral ? "(show)" : "(hide)");
			foldGeneral=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldGeneral, text, foldoutStyle);
			if(foldGeneral){
				startX+=15;
				
				cont=new GUIContent("Cost:", "How many perk currency is required to purchase the perk");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.cost=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perk.cost);
				
				cont=new GUIContent("Min PerkPoint req:", "Minimum perk point to have before the perk becoming available");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.minPerkPoint=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), perk.minPerkPoint);
				
				startY+=5;
				
				cont=new GUIContent("Prerequisite Perk:", "Perks that needs to be purchased before this perk is unlocked and become available");
				EditorGUI.LabelField(new Rect(startX, startY+spaceY, width, height), cont);
				
				for(int i=0; i<perk.prereq.Count+1; i++){
					EditorGUI.LabelField(new Rect(startX+spaceX-10, startY+spaceY, width, height), "-");
					
					int index=(i<perk.prereq.Count) ? TBEditor.GetPerkIndex(perk.prereq[i]) : 0;
					index=EditorGUI.Popup(new Rect(startX+spaceX, startY+=spaceY, width-20, height), index, perkLabel);
					if(index>0){
						int perkID=perkDB.perkList[index-1].prefabID;
						if(perkID!=perk.prefabID && !perk.prereq.Contains(perkID)){
							if(i<perk.prereq.Count) perk.prereq[i]=perkID;
							else perk.prereq.Add(perkID);
						}
					}
					else if(i<perk.prereq.Count){ perk.prereq.RemoveAt(i); i-=1; }
					
					if(i<perk.prereq.Count && GUI.Button(new Rect(startX+spaceX+width-15, startY, 20, height-1), "-")){
						perk.prereq.RemoveAt(i); i-=1;
					}
				}
				
				if(perk.prereq.Count>0){
					startY+=spaceY+5;
					for(int i=0; i<perk.prereq.Count; i++){
						int index=TBEditor.GetPerkIndex(perk.prereq[i])-1;
						TBEditor.DrawSprite(new Rect(startX+(i*45), startY, 40, 40), perkDB.perkList[index].icon);
					}
					startY+=45-spaceY;
				}
			}
			
			return startY+spaceY;
		}
		
		
		private bool foldStats=true;
		protected float DrawStatsModifier(float startX, float startY, UnitStat effect){
			string text="Stats Modifier "+(!foldStats ? "(show)" : "(hide)");
			foldStats=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldStats, text, foldoutStyle);
			if(foldStats){
				startX+=15;	spaceX+=10;
				
				cont=new GUIContent("HP/AP Buff:", "HitPoint(HP)/ActionPoint(AP) multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.HP=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.HP);
				effect.AP=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.AP);
				
				cont=new GUIContent("HP/AP PerTurn:", "HitPoint(HP)/ActionPoint(AP) gain per turn modifier to be applied to the target.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.HPPerTurn=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.HPPerTurn);
				effect.APPerTurn=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.APPerTurn);
				
				startY+=5;
				
				cont=new GUIContent("MoveAPCost:", "Move AP cost modifier to be applied to the target.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.moveAPCost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.moveAPCost);
				
				cont=new GUIContent("AttackAPCost:", "Attack AP cost modifier to be applied to the target.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.attackAPCost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.attackAPCost);
				
				startY+=5;
				
				cont=new GUIContent("Turn Priority:", "Turn Priority modifier to be applied to the target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.turnPriority=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.turnPriority);
				
				startY+=5;
				
				cont=new GUIContent("MoveRange:", "Movement range modifier (in tile) to be applied to the target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.moveRange=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.moveRange);
				
				cont=new GUIContent("AttackRange:", "Attack range modifier (in tile) to be applied to the target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.attackRange=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.attackRange);
				
				cont=new GUIContent("Sight:", "modifier to be applied to the target. ");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.sight=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.sight);
				
				startY+=5;
				
				cont=new GUIContent("MovePerTurn:", "Move-per-turn modifier to be applied to the target. Doesnt apply to tile");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.movePerTurn=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.movePerTurn);
				
				cont=new GUIContent("AttackPerTurn:", "Attack-per-turn modifier to be applied to the target. Doesnt apply to tile");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.attackPerTurn=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.attackPerTurn);
				
				cont=new GUIContent("CounterPerTurn:", "CounterAttack-per-turn modifier to be applied to the target. Doesnt apply to tile");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.counterPerTurn=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.counterPerTurn);
				
				startY+=5;
				
				cont=new GUIContent("Damage:", "Damage multiplier to be applied to the target's both min and max damage. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.damage=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.damage);
				
				cont=new GUIContent("Hit/Dodge Chance:", "Hit Chance multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.hitChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.hitChance);
				effect.dodgeChance=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.dodgeChance);
				
				startY+=5;
				
				cont=new GUIContent("Crit/Avoid Chance:", "Multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.critChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.critChance);
				effect.critAvoidance=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.critAvoidance);
				
				cont=new GUIContent("CritMultiplier:", "Critical damage multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.critMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.critMultiplier);
				
				startY+=5;
				
				cont=new GUIContent("Stun/Avoid Chance:", "Multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.stunChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.stunChance);
				effect.stunAvoidance=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.stunAvoidance);
				
				cont=new GUIContent("StunDuration:", "Stun duration modifier to be applied to the target. ");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.stunDuration=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.stunDuration);
				
				startY+=5;
				
				cont=new GUIContent("Silent/Avoid Chance:", "Multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.silentChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.silentChance);
				effect.silentAvoidance=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.silentAvoidance);
				
				cont=new GUIContent("SilentDuration:", "Silent duration modifier to be applied to the target. ");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.silentDuration=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.silentDuration);
				
				startY+=5;
				
				cont=new GUIContent("Flanking Bonus:", "Damage multiplier to be applied to the unit when flanking a target. Takes value from 0 and above with 0.2 being increase the damage by 20%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.flankingBonus=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.flankingBonus);
				
				cont=new GUIContent("Flanked Modifier:", "Damage multiplier to be applied to the unit when being flanked. Takes value from 0 and above with 0.2 being reduce the incoming damage by 20%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.flankedModifier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.flankedModifier);
				
				spaceX-=10;
			}
			
			return startY+spaceY;
		}
		
		
		private bool foldProperty=true;
		protected float DrawPerkProperty(float startX, float startY, Perk perk){
			string text="Perk Property "+(!foldProperty ? "(show)" : "(hide)");
			foldProperty=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldProperty, text, foldoutStyle);
			if(foldProperty){
				startX+=15;
				
				int type=(int)perk.type;	
				cont=new GUIContent("Perk Type:", "What the perk does");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				contL=new GUIContent[perkTypeLabel.Length];
				for(int i=0; i<contL.Length; i++) contL[i]=new GUIContent(perkTypeLabel[i], perkTypeTooltip[i]);
				type = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), new GUIContent(""), type, contL);
				perk.type=(_PerkType)type;
				
				startY+=8;
				spaceX-=5;	startX+=5;
				
				if(perk.type==_PerkType.Unit || perk.type==_PerkType.Unit_All){
					if(perk.type==_PerkType.Unit){
						cont=new GUIContent("Target Units:", "Unit that will be affected by the perk");
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, width, height), cont, perk.unitIDList.Count>0 ? new GUIStyle("Label") : conflictStyle);
						
						for(int i=0; i<perk.unitIDList.Count+1; i++){
							EditorGUI.LabelField(new Rect(startX+spaceX-10, startY+spaceY, width, height), "-");
							
							int index=(i<perk.unitIDList.Count) ? TBEditor.GetUnitIndex(perk.unitIDList[i]) : 0;
							index=EditorGUI.Popup(new Rect(startX+spaceX, startY+=spaceY, width-20, height), index, unitLabel);
							if(index>0){
								int unitID=unitDB.unitList[index-1].prefabID;
								if(!perk.unitIDList.Contains(unitID)){
									if(i<perk.unitIDList.Count) perk.unitIDList[i]=unitID;
									else perk.unitIDList.Add(unitID);
								}
							}
							else if(i<perk.unitIDList.Count){ perk.unitIDList.RemoveAt(i); i-=1; }
							
							if(i<perk.unitIDList.Count && GUI.Button(new Rect(startX+spaceX+width-15, startY, 20, height-1), "-")){
								perk.unitIDList.RemoveAt(i); i-=1;
							}
						}
					}
					
					startY=DrawStatsModifier(startX, startY+spaceY+5, perk.stats);
				}
				
				if(perk.type==_PerkType.UnitAbility || perk.type==_PerkType.UnitAbility_All){
					if(perk.type==_PerkType.UnitAbility){
						cont=new GUIContent("Target Abilities:", "Unit Abilities that will be affected by the perk");
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, width, height), cont, perk.unitAbilityIDList.Count>0 ? new GUIStyle("Label") : conflictStyle);
						
						for(int i=0; i<perk.unitAbilityIDList.Count+1; i++){
							EditorGUI.LabelField(new Rect(startX+spaceX-10, startY+spaceY, width, height), "-");
							
							int index=(i<perk.unitAbilityIDList.Count) ? TBEditor.GetUnitAbilityIndex(perk.unitAbilityIDList[i]) : 0;
							index=EditorGUI.Popup(new Rect(startX+spaceX, startY+=spaceY, width-20, height), index, uAbilityLabel);
							if(index>0){
								int abID=uAbilityDB.abilityList[index-1].prefabID;
								if(!perk.unitAbilityIDList.Contains(abID)){
									if(i<perk.unitAbilityIDList.Count) perk.unitAbilityIDList[i]=abID;
									else perk.unitAbilityIDList.Add(abID);
								}
							}
							else if(i<perk.unitAbilityIDList.Count){ perk.unitAbilityIDList.RemoveAt(i); i-=1; }
							
							if(i<perk.unitAbilityIDList.Count && GUI.Button(new Rect(startX+spaceX+width-15, startY, 20, height-1), "-")){
								perk.unitAbilityIDList.RemoveAt(i); i-=1;
							}
						}
					}
					
					startY=DrawAbilityModifier(startX, startY+spaceY+10, perk, true)+5;
					startY=DrawStatsModifier(startX, startY, perk.stats);
				}
				
				if(perk.type==_PerkType.FactionAbility || perk.type==_PerkType.FactionAbility_All){
					if(perk.type==_PerkType.FactionAbility){
						cont=new GUIContent("Target Abilities:", "Faction Abilities that will be affected by the perk");
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, width, height), cont, perk.facAbilityIDList.Count>0 ? new GUIStyle("Label") : conflictStyle);
						
						for(int i=0; i<perk.facAbilityIDList.Count+1; i++){
							EditorGUI.LabelField(new Rect(startX+spaceX-10, startY+spaceY, width, height), "-");
							
							int index=(i<perk.facAbilityIDList.Count) ? TBEditor.GetFactionAbilityIndex(perk.facAbilityIDList[i]) : 0;
							index=EditorGUI.Popup(new Rect(startX+spaceX, startY+=spaceY, width-20, height), index, fAbilityLabel);
							if(index>0){
								int abID=fAbilityDB.abilityList[index-1].prefabID;
								if(!perk.facAbilityIDList.Contains(abID)){
									if(i<perk.facAbilityIDList.Count) perk.facAbilityIDList[i]=abID;
									else perk.facAbilityIDList.Add(abID);
								}
							}
							else if(i<perk.facAbilityIDList.Count){ perk.facAbilityIDList.RemoveAt(i); i-=1; }
							
							if(i<perk.facAbilityIDList.Count && GUI.Button(new Rect(startX+spaceX+width-15, startY, 20, height-1), "-")){
								perk.facAbilityIDList.RemoveAt(i); i-=1;
							}
						}
					}
					
					startY=DrawAbilityModifier(startX, startY+spaceY+10, perk, false)+5;
					startY=DrawStatsModifier(startX, startY, perk.stats);
				}
				
				if(perk.type==_PerkType.NewUnitAbility){
					int index=TBEditor.GetUnitAbilityIndex(perk.newUnitAbilityID);
					
					cont=new GUIContent("New Abilitiy:", "New unit ability to be added to the selected units");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, index==0 ? conflictStyle : new GUIStyle("Label"));
					
					index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, uAbilityLabel);
					if(index>0) perk.newUnitAbilityID=uAbilityDB.abilityList[index-1].prefabID;
					else perk.newUnitAbilityID=-1;
					
					
					cont=new GUIContent("Replacing:", "(Optional) Existing unit ability to be replaced by the new ability.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					
					index=TBEditor.GetUnitAbilityIndex(perk.subUnitAbilityID);
					index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, uAbilityLabel);
					if(index>0 && uAbilityDB.abilityList[index-1].prefabID!=perk.newUnitAbilityID)
						perk.subUnitAbilityID=uAbilityDB.abilityList[index-1].prefabID;
					else perk.subUnitAbilityID=-1;
					
					startY+=5;
					
					cont=new GUIContent("Add to all Unit:", "Check if the ability is to add to all player unit");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.addAbilityToAllUnit=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), perk.addAbilityToAllUnit);
					
					if(!perk.addAbilityToAllUnit){
						cont=new GUIContent("Target Units:", "Units that will gain the new abilities");
						EditorGUI.LabelField(new Rect(startX, startY+spaceY, width, height), cont, perk.newABUnitIDList.Count==0 ? conflictStyle : new GUIStyle("Label"));
						
						for(int i=0; i<perk.newABUnitIDList.Count+1; i++){
							EditorGUI.LabelField(new Rect(startX+spaceX-10, startY+spaceY, width, height), "-");
							
							int uIndex=(i<perk.newABUnitIDList.Count) ? TBEditor.GetUnitIndex(perk.newABUnitIDList[i]) : 0;
							uIndex=EditorGUI.Popup(new Rect(startX+spaceX, startY+=spaceY, width-20, height), uIndex, unitLabel);
							if(uIndex>0){
								int unitID=unitDB.unitList[uIndex-1].prefabID;
								if(!perk.newABUnitIDList.Contains(unitID)){
									if(i<perk.newABUnitIDList.Count) perk.unitIDList[i]=unitID;
									else perk.newABUnitIDList.Add(unitID);
								}
							}
							else if(i<perk.newABUnitIDList.Count){ perk.newABUnitIDList.RemoveAt(i); i-=1; }
							
							if(i<perk.newABUnitIDList.Count && GUI.Button(new Rect(startX+spaceX+width-15, startY, 20, height-1), "-")){
								perk.newABUnitIDList.RemoveAt(i); i-=1;
							}
						}
					}
				}
			
				if(perk.type==_PerkType.NewFactionAbility){
					int index=TBEditor.GetFactionAbilityIndex(perk.newFacAbilityID);
					
					cont=new GUIContent("New Abilitiy:", "New faction Ability to be added to the game");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont, index==0 ? conflictStyle : new GUIStyle("Label"));
					
					index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, fAbilityLabel);
					if(index>0) perk.newFacAbilityID=fAbilityDB.abilityList[index-1].prefabID;
					else perk.newFacAbilityID=-1;
					
					cont=new GUIContent("Replacing:", "(Optional) Existing faction ability to be replaced by the new ability.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					
					index=TBEditor.GetFactionAbilityIndex(perk.subFacAbilityID);
					index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, fAbilityLabel);
					if(index>0 && fAbilityDB.abilityList[index-1].prefabID!=perk.newFacAbilityID)
						perk.subFacAbilityID=fAbilityDB.abilityList[index-1].prefabID;
					else perk.subFacAbilityID=-1;
				}
				
				spaceX+=5;	startX-=5;
			}
			
			return startY+spaceY;
		}
		
		
		private bool foldAbility=true;
		float DrawAbilityModifier(float startX, float startY, Perk perk, bool isUnitAbility){
			string text="Ability Modifiers "+(!foldAbility ? "(show)" : "(hide)");
			foldAbility=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldAbility, text, foldoutStyle);
			if(foldAbility){
				startX+=15;
			
				cont=new GUIContent("Cost:", "Energy cost multiplier to be applied to the ability. Takes value from 0 and above with 0.3 being reduce cost by 30%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abCostMod=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.abCostMod);
				
				cont=new GUIContent("Cooldown:", "Cooldown duration modifier to be applied to the ability. Value is used to directly modify the base value. ie. -2 decrease the cooldown duration by 2");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abCooldownMod=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), perk.abCooldownMod);
				
				cont=new GUIContent("UseLimit:", "Limit modifier to be applied to the ability. Value is used to directly modify the base value. ie. -2 decrease the use limit by 2");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abUseLimitMod=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), perk.abUseLimitMod);
				
				cont=new GUIContent("Hit Chance:", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abHitChanceMod=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.abHitChanceMod);
				
				startY+=5;
				
				if(isUnitAbility){
					cont=new GUIContent("Range:", "Range modifier to be applied to the ability. Value is used to directly modify the base value. ie, 2 incrase range by 2");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					perk.abRangeMod=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), perk.abRangeMod);
				}
				
				cont=new GUIContent("AOE Range:", "AOE range modifier to be applied to the ability. Value is used to directly modify the base value. ie, 2 incrase aoe range by 2");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abAOERangeMod=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), perk.abAOERangeMod);
				
				startY+=5;
				
				cont=new GUIContent("HP :", "HP effect (modify target's HP directly) multiplier to be applied to the ability. Takes value from 0 and above with 0.3 being increase the ability HP effect by 30%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abHPMod=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.abHPMod);
				
				cont=new GUIContent("AP :", "HP effect (modify target's AP directly) multiplier to be applied to the ability. Takes value from 0 and above with 0.3 being increase the ability HP effect by 30%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abAPMod=EditorGUI.FloatField(new Rect(startX+spaceX, startY, 40, height), perk.abAPMod);
				
				startY+=5;
				
				cont=new GUIContent("Duration:", "Effect duration modifier to be applied to the ability. Value is used to directly modify the base value. ie. 2 increase the effect duration by 2");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				perk.abDurationMod=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), perk.abDurationMod);
				
			}
			
			return startY+spaceY;
		}
		
		
		
		private Vector2 DrawPerkConfigurator(float startX, float startY, Perk perk){
			
			TBEditor.DrawSprite(new Rect(startX, startY, 60, 60), perk.icon);
			startX+=65;
			
			cont=new GUIContent("Name:", "The perk name to be displayed in game");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY/4, width, height), cont);
			perk.name=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width-5, height), perk.name);
			if(GUI.changed) UpdateLabel_Perk();
			
			cont=new GUIContent("Icon:", "The perk icon to be displayed in game, must be a sprite");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			perk.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width-5, height), perk.icon, typeof(Sprite), false);
			
			cont=new GUIContent("PerkID:", "The ID used to associate a perk item in perk menu to a perk when configuring perk menu manually");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
			EditorGUI.LabelField(new Rect(startX+spaceX-65, startY, width-5, height), perk.prefabID.ToString());
			
			startX-=65;
			startY+=10+spaceY-spaceY/2;	//cachedY=startY;
			
				startY=DrawGeneralSetting(startX, startY+spaceY, perk);
				
				startY=DrawPerkProperty(startX, startY+spaceY, perk);
			
			startY+=10;
			
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Description (for runtime and editor): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			perk.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 100), perk.desp, style);
			
			
			return new Vector2(startX, startY+spaceY+100);
		}
		
		
		
		
		
		protected Vector2 DrawPerkList(float startX, float startY, List<Perk> perkList){
			List<Item> list=new List<Item>();
			for(int i=0; i<perkList.Count; i++){
				Item item=new Item(perkList[i].prefabID, perkList[i].name, perkList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		int NewItem(int cloneID=-1){
			Perk perk=null;
			
			if(cloneID==-1){
				perk=new Perk();
				perk.name="New Perk";
			}
			else{
				perk=perkDB.perkList[selectID].Clone();
			}
			
			perk.prefabID=GenerateNewID(perkIDList);
			perkIDList.Add(perk.prefabID);
			
			perkDB.perkList.Add(perk);
			
			UpdateLabel_Perk();
			
			return perkDB.perkList.Count-1;
		}
		void DeleteItem(){
			perkIDList.Remove(perkDB.perkList[deleteID].prefabID);
			perkDB.perkList.RemoveAt(deleteID);
			
			UpdateLabel_Perk();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<perkDB.perkList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Perk perk=perkDB.perkList[selectID];
			perkDB.perkList[selectID]=perkDB.perkList[selectID+dir];
			perkDB.perkList[selectID+dir]=perk;
			selectID+=dir;
		}
		
	}

}