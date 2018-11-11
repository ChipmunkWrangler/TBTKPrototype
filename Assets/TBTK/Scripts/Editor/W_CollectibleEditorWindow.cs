using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class NewCollectibleEditorWindow : TBEditorWindow {
		
		private static NewCollectibleEditorWindow window;
		
		public static void Init (int prefabID=-1) {
			// Get existing open window or if none, make a new one:
			window = (NewCollectibleEditorWindow)EditorWindow.GetWindow(typeof (NewCollectibleEditorWindow), false, "Collectible Editor");
			window.minSize=new Vector2(420, 300);
			
			LoadDB();
			
			//InitLabel();
			
			if(prefabID>=0) window.selectID=TBEditor.GetCollectibleIndex(prefabID)-1;
			
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
			
			if(collectibleDB.ClearEmptyElement()) UpdateLabel_Collectible();
			List<Collectible> collectibleList=collectibleDB.collectibleList;
			
			Undo.RecordObject(this, "window");
			Undo.RecordObject(collectibleDB, "collectibleDB");
			if(collectibleList.Count>0) Undo.RecordObject(collectibleList[selectID], "unit");
			
			if(GUI.Button(new Rect(Math.Max(260, window.position.width-120), 5, 100, 25), "Save")) SetDirtyTB();
			
			
			EditorGUI.LabelField(new Rect(5, 7, 150, 17), "Add New Item:");
			Collectible newCollectible=null;
			newCollectible=(Collectible)EditorGUI.ObjectField(new Rect(100, 7, 150, 17), newCollectible, typeof(Collectible), false);
			if(newCollectible!=null) Select(NewItem(newCollectible));
			
			
			float startX=5;	float startY=55;
			
			if(minimiseList){
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), ">>")) minimiseList=false;
			}
			else{
				if(GUI.Button(new Rect(startX, startY-20, 30, 18), "<<")) minimiseList=true;
			}
			
			Vector2 v2=DrawCollectibleList(startX, startY, collectibleList);	
			startX=v2.x+25;
			
			if(collectibleList.Count==0) return true;
			
			
			Rect visibleRect=new Rect(startX, startY, window.position.width-startX-10, window.position.height-startY-5);
			Rect contentRect=new Rect(startX, startY, contentWidth, contentHeight);
			
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				v2=DrawCollectibleConfigurator(startX, startY, collectibleList[selectID]);
				contentWidth=v2.x-startX;
				contentHeight=v2.y-55;
			
			GUI.EndScrollView();
			
			if(GUI.changed) SetDirtyTB();
			
			return true;
		}
		
		
		
		
		
		
		protected float DrawItemBasicInfo(float startX, float startY, Collectible item){
				TBEditor.DrawSprite(new Rect(startX, startY, 60, 60), item.icon);
			
			startX+=65;
			
				cont=new GUIContent("Name:", "The collectible name to be displayed in game");
				EditorGUI.LabelField(new Rect(startX, startY+=5, width, height), cont);
				item.itemName=EditorGUI.TextField(new Rect(startX+spaceX-65, startY, width, height), item.itemName);
				if(GUI.changed) UpdateLabel_Collectible();
				
				cont=new GUIContent("Icon:", "The collectible icon to be displayed in game, must be a sprite");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				item.icon=(Sprite)EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width, height), item.icon, typeof(Sprite), false);
				
				cont=new GUIContent("Prefab:", "The prefab object of the collectible\nClick this to highlight it in the ProjectTab");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				EditorGUI.ObjectField(new Rect(startX+spaceX-65, startY, width, height), item.gameObject, typeof(GameObject), false);
			
			startX-=65;
			startY+=spaceY*2;	
			
				cont=new GUIContent("Trigger Effect:", "The effect object to be spawned when the item is triggered\nThis is entirely optional");
				EditorGUI.LabelField(new Rect(startX, startY, width, height), cont);
				item.triggerEffectObj=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width, height), item.triggerEffectObj, typeof(GameObject), false);
				
				cont=new GUIContent(" - AutoDestroy:", "Check if the effect object needs to be removed from the game");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(item.triggerEffectObj!=null) item.destroyTriggerEffect=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), item.destroyTriggerEffect);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
				cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(item.triggerEffectObj!=null && item.destroyTriggerEffect) 
					item.triggerEffectDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), item.triggerEffectDuration);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
			
			return startY+spaceY;
		}
		
		
		
		private bool foldInstant=true;
		private bool foldBuff=true;
		protected float DrawEffect(float startX, float startY, Effect effect){
			
			string text="Instant Effect "+(!foldInstant ? "(show)" : "(hide)");
			foldInstant=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldInstant, text, foldoutStyle);
			if(foldInstant){
				startX+=15;
				cont=new GUIContent("Damage Type:", "The type of the damage inflicted by the ability. Only applicable if effect damages target's HP.\n\nDamage type can be configured in Damage Armor Table Editor");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.damageType=EditorGUI.Popup(new Rect(startX+spaceX, startY, width-15, height), effect.damageType, damageTypeLabel);
				
				cont=new GUIContent("HP Min/Max:", "Potential HitPoint value (random between minimum and maximum) to be added to the target HP. Damage when value is negative, heal when value is positive. Doesnt apply to tile");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.HPMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.HPMin);
				effect.HPMax=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.HPMax);
				
				cont=new GUIContent("AP Min/Max:", "Potential actionPoint value (random between minimum and maximum) to be applied to the target AP. Damage when value is negative, restore when value is positive. Doesnt apply to tile");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.APMin=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.APMin);
				effect.APMax=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.APMax);
				
				startY+=10;
				startX-=15;
			}
			
			text="Buff/Debuff On Target's stats "+(!foldBuff ? "(show)" : "(hide)");
			foldBuff=EditorGUI.Foldout(new Rect(startX, startY+=spaceY, width, height), foldBuff, text, foldoutStyle);
			if(foldBuff){
				startX+=15;
				//startY+=5;
				
				cont=new GUIContent("Duration:", "");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.duration=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.duration);
				
				startY+=5;
				
				cont=new GUIContent("Stun:", "Duration of stun (in turn) to be applied to the target. Doesnt apply to tile");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.stun=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), effect.stun);
				
				cont=new GUIContent("Silent:", "Duration of stun (in turn) to be applied to the target. Doesnt apply to tile");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.silence=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), effect.silence);
				
				startY+=5;
				
				cont=new GUIContent("HP/AP Buff:", "HitPoint (HP) multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.HP=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.HP);
				effect.AP=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.AP);
				
				cont=new GUIContent("HP/AP PerTurn:", "Multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
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
				
				cont=new GUIContent("StunDuration:", "Multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.stunDuration=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.stunDuration);
				
				startY+=5;
				
				cont=new GUIContent("Silent/Avoid Chance:", "Multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.silentChance=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.silentChance);
				effect.silentAvoidance=EditorGUI.FloatField(new Rect(startX+spaceX+widthS+2, startY, widthS, height), effect.silentAvoidance);
				
				cont=new GUIContent("SilentDuration:", "Multiplier to be applied to the target. ie, 0.2 being increment by 20%, -0.1 being reduction by 10%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.silentDuration=EditorGUI.IntField(new Rect(startX+spaceX, startY, widthS, height), effect.silentDuration);
				
				startY+=5;
				
				cont=new GUIContent("Flanking Bonus:", "Damage multiplier to be applied to the unit when flanking a target. Takes value from 0 and above with 0.2 being increase the damage by 20%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.flankingBonus=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.flankingBonus);
				
				cont=new GUIContent("Flanked Modifier:", "Damage multiplier to be applied to the unit when being flanked. Takes value from 0 and above with 0.2 being reduce the incoming damage by 20%");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				effect.flankedModifier=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), effect.flankedModifier);
				
				startX-=15;
			}
			
			return startY;
		}
		
		
		
		Vector2 DrawCollectibleConfigurator(float startX, float startY, Collectible item){
			float maxX=startX;
			
			startY=DrawItemBasicInfo(startX, startY, item)-10;
			
			if(GUI.Button(new Rect(startX+spaceX+width-100, startY+=spaceY, 100, height), "Ability Editor"))
				NewFactionAbilityEditorWindow.Init();
			
			string tooltip="The ability to cast when the item is triggered";
			tooltip+="\n\nUses ability setup in Faction Ability Editor but does not cost energy";
			tooltip+="\n\n*The ability target setting still applies, the effect will be applied to all unit if the 'require target selection' option is not checked";
			tooltip+="\n\nRandom ability will be chosen when multiple abilities has been assigned, useful for item with random effects";
			cont=new GUIContent("Trigger Ability:", tooltip);
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
		
			
			//~ int index=TBEditor.GetFactionAbilityIndex(item.facAbilityID);
			//~ index=EditorGUI.Popup(new Rect(startX+spaceX, startY, width, height), index, fAbilityLabel);
			//~ if(index>0) item.facAbilityID=fAbilityDB.abilityList[index-1].prefabID;
			//~ else item.facAbilityID=-1;
			
				startY-=spaceY;
				int count=item.facAbilityIDList.Count+1;
			
				for(int i=0; i<count; i++){
					EditorGUI.LabelField(new Rect(startX+spaceX-10, startY+spaceY, width, height), "-");
					
					int index=(i<item.facAbilityIDList.Count) ? TBEditor.GetFactionAbilityIndex(item.facAbilityIDList[i]) : 0;
					index=EditorGUI.Popup(new Rect(startX+spaceX, startY+=spaceY, width, height), index, fAbilityLabel);
					if(index>0){
						int abID=fAbilityDB.abilityList[index-1].prefabID;
						if(!item.facAbilityIDList.Contains(abID)){
							if(i<item.facAbilityIDList.Count) item.facAbilityIDList[i]=abID;
							else item.facAbilityIDList.Add(abID);
						}
					}
					else if(i<item.facAbilityIDList.Count){ item.facAbilityIDList.RemoveAt(i); i-=1; }
					
					if(i<item.facAbilityIDList.Count && GUI.Button(new Rect(startX+width+spaceX+5, startY, 20, height-1), "-")){
						item.facAbilityIDList.RemoveAt(i); i-=1;
					}
				}
			
			
			
			
			startY+=spaceY;
			
			startY=DrawEffect(startX, startY+spaceY, item.effect);
			
			startY+=spaceY;
			
			GUIStyle style=new GUIStyle("TextArea");
			style.wordWrap=true;
			cont=new GUIContent("Collectible description (for runtime and editor): ", "");
			EditorGUI.LabelField(new Rect(startX, startY+=spaceY, 400, 20), cont);
			item.desp=EditorGUI.TextArea(new Rect(startX, startY+spaceY-3, 270, 100), item.desp, style);
			
			return new Vector2(maxX, startY+120);
		}
		
		
		
		protected Vector2 DrawCollectibleList(float startX, float startY, List<Collectible> collectibleList){
			List<Item> list=new List<Item>();
			for(int i=0; i<collectibleList.Count; i++){
				Item item=new Item(collectibleList[i].prefabID, collectibleList[i].itemName, collectibleList[i].icon);
				list.Add(item);
			}
			return DrawList(startX, startY, window.position.width, window.position.height, list);
		}
		
		
		
		public static int NewItem(Collectible collectible){ return window._NewItem(collectible); }
		int _NewItem(Collectible collectible){
			if(collectibleDB.collectibleList.Contains(collectible)) return selectID;
			
			collectible.prefabID=GenerateNewID(collectibleIDList);
			collectibleIDList.Add(collectible.prefabID);
			
			collectibleDB.collectibleList.Add(collectible);
			
			UpdateLabel_Collectible();
			
			return collectibleDB.collectibleList.Count-1;
		}
		void DeleteItem(){
			collectibleIDList.Remove(collectibleDB.collectibleList[deleteID].prefabID);
			collectibleDB.collectibleList.RemoveAt(deleteID);
			
			UpdateLabel_Collectible();
		}
		
		void ShiftItemUp(){ 	if(selectID>0) ShiftItem(-1); }
		void ShiftItemDown(){ if(selectID<collectibleDB.collectibleList.Count-1) ShiftItem(1); }
		void ShiftItem(int dir){
			Collectible collectible=collectibleDB.collectibleList[selectID];
			collectibleDB.collectibleList[selectID]=collectibleDB.collectibleList[selectID+dir];
			collectibleDB.collectibleList[selectID+dir]=collectible;
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
