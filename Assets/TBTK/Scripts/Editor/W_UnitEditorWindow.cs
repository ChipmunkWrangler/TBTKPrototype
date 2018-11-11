using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class NewUnitEditorWindow : TBEditorWindow {
		
		private static NewUnitEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			// Get existing open window or if none, make a new one:
			window = (NewUnitEditorWindow)EditorWindow.GetWindow(typeof (NewUnitEditorWindow), false, "Unit Editor");
			window.minSize=new Vector2(420, 300);
			
			LoadDB();
			
			//InitLabel();
			
			if(prefabID>=0) window.selectID=TBEditor.GetUnitIndex(prefabID)-1;
			
			window.SetupCallback();
		}
		
		public void SetupCallback(){
			selectCallback=this.SelectItem;
			shiftItemUpCallback=this.ShiftItemUp;
			shiftItemDownCallback=this.ShiftItemDown;
			deleteItemCallback=this.DeleteItem;
			
			SelectItem();
		}
		
		
		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			if(unitDB.ClearEmptyElement()) UpdateLabel_Unit();
			List<Unit> unitList=unitDB.unitList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(unitDB, "unitDB");
			if(unitList.Count>0) Undo.RecordObject(unitList[selectID], "unit");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTB();
			
			
			EditorGUI.LabelField(new Rect(5, 7, 150, 17), "Add New Unit:");
			Unit newUnit=null;
			newUnit=(Unit)EditorGUI.ObjectField(new Rect(115, 7, 150, 17), newUnit, typeof(Unit), false);
			if(newUnit!=null) Select(NewItem(newUnit));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawUnitList(startX, startY, unitList);	
			startX=v2.x+25;
			
			if(unitList.Count==0) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				v2=DrawUnitConfigurator(startX, startY, unitList[selectID]);
				contentWidth=v2.x-startX;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			if(GUI.changed) SetDirtyTB();
			
			return true;
		}
		
		
		
		
		private bool foldHitPoint=true;
		protected float DrawUnitBasicStats(float startX, float startY, Unit unit){
				TBEditor.DrawSprite(new Rect(startX, startY, 60, 60), unit.iconSprite);
			
			startX+=65;
			
				//bool enter=Event.current.Equals (Event.KeyboardEvent ("return"));
				cont=new GUIContent("Name:", "The unit name to be displayed in game");
				EditorGUI.LabelField(new Rect(startX, startY+=5, width, height), cont);
				unit.unitName=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width, height), unit.unitName);
				//if(enter) UpdateLabel_Unit();
				if(GUI.changed) UpdateLabel_Unit();
				
				
				cont=new GUIContent("Icon:", "The unit icon to be displayed in game, must be a sprite");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.iconSprite=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width, height), unit.iconSprite, typeof(Sprite), false);
				
				cont=new GUIContent("Prefab:", "The prefab object of the unit\nClick this to highlight it in the ProjectTab");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width, height), unit.gameObject, typeof(GameObject), false);
			
			startX-=65;
			startY+=spaceY*2;	
			
			string text="Basic Unit Info "+(!foldHitPoint ? "(show)" : "(hide)");
			foldHitPoint=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldHitPoint, text, foldoutStyle);
			if(foldHitPoint){
				startX+=15;
			
					cont=new GUIContent("HitPoint (HP):", "The unit's base Hit-Point.\nDetermine how much the damage the unit can take before it's destroyed");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.defaultHP=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.defaultHP);
					
					cont=new GUIContent(" - Regen (PerTurn):", "HP regeneration rate. The amount of HP to be regenerated each turn");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.HPPerTurn=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.HPPerTurn);
				
				//startX+=140;		startY=cachedY; 	
				startY+=5;
				
					cont=new GUIContent("ActionPoint (AP):", "The unit's base Action-Point. Used by the unit to performed various action");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.defaultAP=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.defaultAP);
					
					cont=new GUIContent(" - Regen (PerTurn):", "AP regeneration rate. The amount of AP to be regenerated each turn");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.APPerTurn=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.APPerTurn);
				
				//startX-=140;
				startY+=5;
				
				cont=new GUIContent("Unit Value:", "The value of the unit. Used in unit generation on grid if the limit mode is set to be based on value.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.value=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.value);
				
			}
			
			return startY+spaceY;
		}
		
		
		private bool foldDefensive=true;
		protected float DrawUnitDefensiveSetting(float startX, float startY, Unit unit){
			string text="Defensive Setting "+(!foldDefensive ? "(show)" : "(hide)");
			foldDefensive=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldDefensive, text, foldoutStyle);
			if(foldDefensive){
				startX+=15;
					
					cont=new GUIContent("Armor Type:", "The armor type of the unit\nArmor type can be configured in Damage Armor Table Editor");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.armorType=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), unit.armorType, armorTypeLabel);
				
				startY+=8;
				
					int objID=GetObjectIDFromHList(unit.targetPoint, objHList);
					cont=new GUIContent("TargetPoint:", "The transform object which indicate the center point of the unit\nThis would be the point where the shootObject and effect will be aiming at");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
					unit.targetPoint = (objHList[objID]==null) ? null : objHList[objID].transform;
					
					cont=new GUIContent("Hit Threshold:", "The range from the targetPoint where a shootObject is considered reached the target");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.hitThreshold=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.hitThreshold);
				
				startY+=8;
				
					cont=new GUIContent("Destroyed Effect:", "The effect object to be spawned when the unit is destroyed\nThis is entirely optional");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.destroyEffectObj=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.destroyEffectObj, typeof(GameObject), false);
					
					cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.destroyEffectObj!=null) unit.autoDestroyEffect=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.autoDestroyEffect);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
					
					cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.destroyEffectObj!=null && unit.autoDestroyEffect) 
						unit.destroyEffectDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.destroyEffectDuration);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
			}
			
			return startY+spaceY;
		}
		
		
		private bool foldOffensive=true;
		protected float DrawUnitOffensiveSetting(float startX, float startY, Unit unit, bool isTower=true){
			string text="Offensive Setting "+(!foldOffensive ? "(show)" : "(hide)");
			foldOffensive=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldOffensive, text, foldoutStyle);
			if(foldOffensive){
				startX+=15;
				
					int objID;
				
					cont=new GUIContent("Damage Type:", "The damage type of the unit\nDamage type can be configured in Damage Armor Table Editor");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.damageType=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), unit.damageType, damageTypeLabel);
				
				startY+=8;
				
					cont=new GUIContent("Require LOS to Attack:", "Check if the unit require target to be in direct line-of-sight to attack. Otherwise the unit can attack any target as long as it's seen by other friendly unit.\nThis only applicable if Fog-Of-War is enabled");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.requireDirectLOSToAttack=EditorGUI.Toggle(new Rect(startX+spaceX+20, startY, widthS, height), unit.requireDirectLOSToAttack);
					
				
					cont=new GUIContent("ShootObject:", "The shootObject that the unit used. All unit must have one in order to attack.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.shootObject=(ShootObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.shootObject, typeof(ShootObject), false);
			
					cont=new GUIContent("ShootPoint:", "The transform which indicate the position where the shootObject will be fired from (Optional)\nEach shootPoint assigned will fire a shootObject instance in each attack\nIf left empty, the unit transform itself will be use as the shootPoint\nThe orientation of the shootPoint matter as they dictate the orientation of the shootObject starting orientation.\n");
					shootPointFoldout=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, spaceX, height), shootPointFoldout, cont);
					int shootPointCount=unit.shootPointList.Count;
					shootPointCount=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), shootPointCount);
					
					if(shootPointCount!=unit.shootPointList.Count){
						while(unit.shootPointList.Count<shootPointCount) unit.shootPointList.Add(null);
						while(unit.shootPointList.Count>shootPointCount) unit.shootPointList.RemoveAt(unit.shootPointList.Count-1);
					}
						
					if(shootPointFoldout){
						for(int i=0; i<unit.shootPointList.Count; i++){
							objID=GetObjectIDFromHList(unit.shootPointList[i], objHList);
							EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), "    - Element "+(i+1));
							objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
							unit.shootPointList[i] = (objHList[objID]==null) ? null : objHList[objID].transform;
						}
					}
					
					cont=new GUIContent("Shots delay Between ShootPoint:", "Delay in second between shot fired at each shootPoint. When set to zero all shootPoint fire simulteneously");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+60, height), cont);
					if(unit.shootPointList.Count>1) 
						unit.delayBetweenShootPoint=EditorGUI.FloatField(new Rect(startX+spaceX+90, startY-1, widthS, height-1), unit.delayBetweenShootPoint);
					else EditorGUI.LabelField(new Rect(startX+spaceX+90, startY-1, widthS, height-1), new GUIContent("-", ""));
					
				startY+=8;	
					
					cont=new GUIContent("Hybrid Unit:", "Check if the unit can switch between melee or range attack depends on the attack range");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					unit.isHybridUnit=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), unit.isHybridUnit);
					
					cont=new GUIContent(" - Melee Range:", "The range at which the unit will switch to melee attack. Otherwise the unit will use range attack");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.isHybridUnit) unit.meleeRange=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.meleeRange);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					
					cont=new GUIContent(" - ShootObject:", "The shootObject that the unit used for melee attack.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					if(unit.isHybridUnit) unit.shootObjectMelee=(ShootObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), unit.shootObjectMelee, typeof(ShootObject), false);
					else EditorGUI.LabelField(new Rect(startX+spaceX, startY, width, height), "-");
					
				startY+=8;	
					
					objID=GetObjectIDFromHList(unit.turretObject, objHList);
					cont=new GUIContent("TurretObject:", "The object under unit's hierarchy which is used to aim toward target (Optional). When left unassigned, no aiming will be done.");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
					unit.turretObject = (objHList[objID]==null) ? null : objHList[objID].transform;
					
					objID=GetObjectIDFromHList(unit.barrelObject, objHList);
					cont=new GUIContent("BarrelObject:", "The object under unit's hierarchy which is used to aim toward target (Optional). This is only required if the unit barrel and turret rotates independently on different axis");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
					objID = EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), objID, objHLabelList);
					unit.barrelObject = (objHList[objID]==null) ? null : objHList[objID].transform;
					
				startY+=5;	
					
					cont=new GUIContent("Rotate turret only when aiming:", "Check if the unit only rotate it's turret only to aim when attacking");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+35, height), cont);
					unit.rotateTurretOnly=EditorGUI.Toggle(new Rect(startX+spaceX+75, startY, widthS, height), unit.rotateTurretOnly);
					
					cont=new GUIContent("Rotate in x-axis when aiming:", "Check if the unit turret/barrel can rotate in x-axis (elevation) to aim when attacking");
					EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width+35, height), cont);
					unit.rotateTurretAimInXAxis=EditorGUI.Toggle(new Rect(startX+spaceX+75, startY, widthS, height), unit.rotateTurretAimInXAxis);
			
			}			
			
			return startY+spaceY;
		}
		
		
		private bool foldStats=true;
		protected float DrawUnitStats(float startX, float startY, Unit unit){
			string text="Unit's Stats "+(!foldStats ? "(show)" : "(hide)");
			foldStats=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldStats, text, foldoutStyle);
			if(foldStats){
				startX+=15;
				
				cont=new GUIContent("Move AP Cost:", "AP cost per tile when moving, only applicable when useAPForMove flag in GameControl is checked");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.moveAPCost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.moveAPCost);
				
				cont=new GUIContent("Attack AP Cost:", "AP cost for each attack, only applicable when useAPForAttack flag in GameControl is checked");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.attackAPCost=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.attackAPCost);
				
				startY+=5;
				
				cont=new GUIContent("Turn Priority:", "Value to determine the move order of the unit. The unit with highest value move first. Only used in UnitPerTurn mode or when MoveOrder is not free");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.turnPriority=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.turnPriority);
				
				startY+=5;
				
				cont=new GUIContent("Move Range:", "Movement range in term of tile");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.moveRange=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.moveRange);
				
				cont=new GUIContent("Attack R. Min/Max:", "Attack range in term of tile");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.attackRangeMin=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.attackRangeMin);
				unit.attackRange=EditorGUI.IntField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), unit.attackRange);
				
				cont=new GUIContent("Sight Range:", "How far the unit can see. For AI unit in trigger mode, this is the range at which the unit will be actived. For player's units, this only applicable if Fog-Of-War is enabled");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.sight=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.sight);
				
				startY+=5;
				
				cont=new GUIContent("Move Per Turn:", "Number of move the unit can do in a turn");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.movePerTurn=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.movePerTurn);
				
				cont=new GUIContent("Attack Per Turn:", "Number of attack the unit can do in a turn");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.attackPerTurn=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.attackPerTurn);
				
				cont=new GUIContent("Counter Per Turn:", "Number of counter attack the unit can do in a turn");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.counterPerTurn=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.counterPerTurn);
				
				startY+=5;
				
				cont=new GUIContent("Damage Min/Max:", "The minimum/maximum damage inflicted on target when hit");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.damageMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.damageMin);
				unit.damageMax=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), unit.damageMax);
				
				cont=new GUIContent(" - Melee Min/Max:", "The minimum damage inflicted on target when hit (only applies when melee attack is used)");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(unit.isHybridUnit){
					unit.damageMinMelee=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.damageMinMelee);
					unit.damageMaxMelee=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), unit.damageMaxMelee);
				}
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
					
				startY+=5;
				
				cont=new GUIContent("Hit/Dodge Chance:", "Chance to hit/dodge when attacking/attacked.\n\nTakes value from 0 and above with 0.1 being 10%.\n\nEffective chance to a succesful attack is calculated using attcker's hit chance and target's dodge chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.hitChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.hitChance);
				unit.dodgeChance=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), unit.dodgeChance);
				
				cont=new GUIContent(" - Melee Hit:", "Chance to hit the target in an melee attack. Takes value from 0 and above with 0.7f being 70% to hit. Value is further modifed with target's dodge chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(unit.isHybridUnit) unit.hitChanceMelee=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.hitChanceMelee);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
				
				startY+=5;
				
				cont=new GUIContent("Crit/Avoid Chance:", "Chance to score/avoid a cirtical hit when attacking/attacked.\n\nTakes value from 0 and above with 0.1% being 10%.\n\nEffective chance to a succesful critical attack is calculated using attcker's crit chance and target's avoid chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.critChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.critChance);
				unit.critAvoidance=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), unit.critAvoidance);
				
				cont=new GUIContent(" - Melee Crit:", "Chance to score a cirtical hit when performing an melee attack. Takes value from 0 and above with 0.4% being 40%. Value is further modified with attacker's Critical Avoidance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(unit.isHybridUnit) unit.critChanceMelee=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.critChanceMelee);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
				
				cont=new GUIContent(" - CritMultiplier:", "Multiplier for the damage value when a cirtical hit is scored. Takes value from 0 and above with 0.2 being 20% increase in damage cause");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(unit.critChance>0 || (unit.isHybridUnit && unit.hitChanceMelee>0)) 
					unit.critMultiplier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.critMultiplier);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
				
				startY+=5;
				
				cont=new GUIContent("Stun/Avoid Chance:", "Chance to score/avoid a stun attack when attacking/attacked.\n\nTakes value from 0 and above with 0.1% being 10%.\n\nEffective chance to a succesful stun attack is calculated using attcker's stun chance and target's avoid chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.stunChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.stunChance);
				unit.stunAvoidance=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), unit.stunAvoidance);
				
				cont=new GUIContent(" - StunDuration:", "The stun duration of target (in turn) when an attack succesfully stuns the target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(unit.stunChance>0) unit.stunDuration=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), unit.stunDuration);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), "-");
				
				startY+=5;
				
				cont=new GUIContent("Silent/Avoid Chance:", "Chance to score/avoid a silencing attack(render target unabled to use ability) when attacking/attacked.\n\nTakes value from 0 and above with 0.1% being 10%.\n\nEffective chance to a succesful silent attack is calculated using attcker's silence chance and target's avoid chance");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.silentChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.silentChance);
				unit.silentAvoidance=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), unit.silentAvoidance);
				
				cont=new GUIContent(" - SilentDuration:", "The silent duration of target (in turn) when an attack succesfully silences the target");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(unit.silentChance>0) unit.silentDuration=EditorGUI.IntField(new Rect(startX+spaceX, startY, 40, height), unit.silentDuration);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, 40, height), "-");
				
				startY+=5;
				
				cont=new GUIContent("FlankingBonus:", "Damge multiplier when flanking a unit. Takes value from 0 and above with 0.4% being 40% bonus. Value is further modified with target's Flanked-Modifier");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.flankingBonus=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.flankingBonus);
				
				cont=new GUIContent("FlankedModifier:", "Damge multiplier when being flanked by an attacker. Takes value from 0 and above with 0.2% being 20% reduction in damage. Value is further modified with attacker's Flanking Bonus");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				unit.flankedModifier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), unit.flankedModifier);
			}
			
			return startY+spaceY;
		}
		
		private bool foldAbilities=true;
		protected float DrawUnitAbilities(float startX, float startY, Unit unit){
			string text="Unit Abilities "+(!foldAbilities ? "(show)" : "(hide)");
			foldAbilities=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldAbilities, text, foldoutStyle);
			if(foldAbilities){
				startX+=15;
				
				if(unit.abilityIDList.Count>0){
					startY+=spaceY;
					for(int i=0; i<unit.abilityIDList.Count; i++){
						int index=TBEditor.GetUnitAbilityIndex(unit.abilityIDList[i]);
						
						if(index<0){ 
							unit.abilityIDList.RemoveAt(i);
							i-=1; 	continue;
						}
						
						TBEditor.DrawSprite(new Rect(startX+(i*45), startY, 40, 40), uAbilityDB.abilityList[index-1].icon);
					}
					
					startY+=45-spaceY;
				}
				
				cont=new GUIContent("Abilities:", "Abilities possesed by the unit");
				EditorGUI.LabelField(new Rect(startX, startY+spaceY, width, height), cont);
				
				int count=Mathf.Min(unit.abilityIDList.Count+1, 6);
			
				for(int i=0; i<count; i++){
					EditorGUI.LabelField(new Rect(startX+55, startY+spaceY, width, height), "-");
					
					int index=(i<unit.abilityIDList.Count) ? TBEditor.GetUnitAbilityIndex(unit.abilityIDList[i]) : 0;
					index=EditorGUI.Popup(new Rect(startX+65, startY+=spaceY, width, height), index, uAbilityLabel);
					if(index>0){
						int abID=uAbilityDB.abilityList[index-1].prefabID;
						if(!unit.abilityIDList.Contains(abID)){
							if(i<unit.abilityIDList.Count) unit.abilityIDList[i]=abID;
							else unit.abilityIDList.Add(abID);
						}
					}
					else if(i<unit.abilityIDList.Count){ unit.abilityIDList.RemoveAt(i); i-=1; }
					
					if(i<unit.abilityIDList.Count && GUI.Button(new Rect(startX+67+width, startY, 20, height-1), "-")){
						unit.abilityIDList.RemoveAt(i); i-=1;
					}
				}
				
			}
			
			return startY+spaceY;
		}
		
		
		Vector2 DrawUnitConfigurator(float startX, float startY, Unit unit){
			float maxX=startX;
			
			startY=DrawUnitBasicStats(startX, startY, unit);
			
			startY=DrawUnitDefensiveSetting(startX, startY+spaceY, unit);
			
			startY=DrawUnitOffensiveSetting(startX, startY+spaceY, unit);
			
			startY=DrawUnitStats(startX, startY+spaceY, unit);
			
			startY=DrawUnitAbilities(startX, startY+spaceY, unit);
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Unit description (for runtime and editor): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			unit.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 100), unit.desp, style);
			
			return new Vector2(maxX, startY+120);
		}
		
		
		private bool fold=true;
		protected float DrawUnitXSetting(float startX, float startY, Unit unit){
			string text="Defensive Setting "+(!fold ? "(show)" : "(hide)");
			fold=EditorGUI.Foldout(new Rect(startX, startY, width, height), fold, text, foldoutStyle);
			if(fold){
				startX+=15;
				
			}
			
			return startY+spaceY;
		}
		
		
		
		
		
		protected Vector2 DrawUnitList(float startX, float startY, List<Unit> unitList){
			List<Item> list=new List<Item>();
			for(int i=0; i<unitList.Count; i++){
				Item item=new Item(unitList[i].prefabID, unitList[i].unitName, unitList[i].iconSprite);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		public static int NewItem(Unit unit){ return window._NewItem(unit); }
		int _NewItem(Unit unit){
			if(unitDB.unitList.Contains(unit)) return selectID;
			
			unit.prefabID=GenerateNewID(unitIDList);
			unitIDList.Add(unit.prefabID);
			
			unitDB.unitList.Add(unit);
			
			UpdateLabel_Unit();
			
			return unitDB.unitList.Count-1;
		}
		void DeleteItem(){
			unitIDList.Remove(unitDB.unitList[deleteID].prefabID);
			unitDB.unitList.RemoveAt(deleteID);
			
			UpdateLabel_Unit();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<unitDB.unitList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Unit unit=unitDB.unitList[selectID];
			unitDB.unitList[selectID]=unitDB.unitList[selectID+dir];
			unitDB.unitList[selectID+dir]=unit;
			selectID+=dir;
		}
		
		void SelectItem(){ SelectItem(selectID); }
		void SelectItem(int newID){ 
			selectID=newID;
			if(unitDB.unitList.Count<=0) return;
			selectID=Mathf.Clamp(selectID, 0, unitDB.unitList.Count-1);
			UpdateObjectHierarchyList(unitDB.unitList[selectID].gameObject);
		}
	}
	
}
