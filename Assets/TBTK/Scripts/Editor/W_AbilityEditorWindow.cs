using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class NewAbilityEditorWindow : TBEditorWindow {
		
		protected static string[] effTargetTypeLabel;
		protected static string[] effTargetTypeTooltip;
		
		protected static string[] targetTypeLabel;
		protected static string[] targetTypeTooltip;
		
		public virtual void InitLabel(){
			int enumLength = Enum.GetValues(typeof(_EffectTargetType)).Length;
			effTargetTypeLabel=new string[enumLength];
			effTargetTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				effTargetTypeLabel[i]=((_EffectTargetType)i).ToString();
				if((_EffectTargetType)i==_EffectTargetType.AllUnit) 		effTargetTypeTooltip[i]="Apply the effect on all units on target tile(s)";
				if((_EffectTargetType)i==_EffectTargetType.HostileUnit) 	effTargetTypeTooltip[i]="Applythe  effect on just the hostile unit on target tile(s)";
				if((_EffectTargetType)i==_EffectTargetType.FriendlyUnit) effTargetTypeTooltip[i]="Apply the effect on just the friendly unit on target tile(s)";
				if((_EffectTargetType)i==_EffectTargetType.Tile) 			effTargetTypeTooltip[i]="Apply the effect on the target tile(s)";
			}
			
			enumLength = Enum.GetValues(typeof(_TargetType)).Length;
			targetTypeLabel=new string[enumLength];
			targetTypeTooltip=new string[enumLength];
			for(int i=0; i<enumLength; i++){
				targetTypeLabel[i]=((_TargetType)i).ToString();
				if((_TargetType)i==_TargetType.AllUnit) 		targetTypeTooltip[i]="Ability can only be cast on tile with unit (either friendly or hostile)";
				if((_TargetType)i==_TargetType.HostileUnit) 	targetTypeTooltip[i]="Ability can only be cast on tile with Hostile Units";
				if((_TargetType)i==_TargetType.FriendlyUnit) targetTypeTooltip[i]="Ability can only be cast on tile with Friendly Units";
				if((_TargetType)i==_TargetType.EmptyTile) 	targetTypeTooltip[i]="Ability can only be cast on empty tile only";
				if((_TargetType)i==_TargetType.AllTile) 		targetTypeTooltip[i]="Ability can target any tiles";
			}
		}
		
		
		
		private bool foldVisualEffect=true;
		protected float DrawAbilityVisualEffect(float startX, float startY, Ability ability){
			string text="Visual Effects Setting "+(!foldVisualEffect ? "(show)" : "(hide)");
			foldVisualEffect=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldVisualEffect, text, foldoutStyle);
			if(foldVisualEffect){
				startX+=15;
				
				cont=new GUIContent("Effect Object:", "The effect object to be spawned at the target position when the ability is cast.");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.effectObject=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width-5, height), ability.effectObject, typeof(GameObject), false);
				
				cont=new GUIContent(" - AutoDestroy:", "Check if the visual effect object needs to be removed from the game");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.effectObject!=null) ability.autoDestroyEffect=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), ability.autoDestroyEffect);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
				cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.effectObject!=null && ability.autoDestroyEffect) 
					ability.effectObjectDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), ability.effectObjectDuration);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
				startY+=10;
				
				cont=new GUIContent("Effect On Target:", "The effect object to be spawned on each individual target unit in range when the ability is cast.\n\nThis is intended for ability with Area-of-Effective range");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				ability.effectObjectTarget=(GameObject)EditorGUI.ObjectField(new Rect(startX+spaceX, startY, width-5, height), ability.effectObjectTarget, typeof(GameObject), false);
				
				cont=new GUIContent(" - AutoDestroy:", "Check if the visual effect object needs to be removed from the game");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.effectObjectTarget!=null) ability.autoDestroyEffectTgt=EditorGUI.Toggle(new Rect(startX+spaceX, startY, widthS, height), ability.autoDestroyEffectTgt);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
				cont=new GUIContent(" - EffectDuration:", "The delay in seconds before the effect object is destroyed");
				EditorGUI.LabelField(new Rect(startX, startY+=spaceY, width, height), cont);
				if(ability.effectObjectTarget!=null && ability.autoDestroyEffectTgt) 
					ability.effectObjectTgtDuration=EditorGUI.FloatField(new Rect(startX+spaceX, startY, widthS, height), ability.effectObjectTgtDuration);
				else EditorGUI.LabelField(new Rect(startX+spaceX, startY, widthS, height), new GUIContent("-", ""));
				
			}
			
			return startY+spaceY;
		}
		
		
		
		
		
		
		
		//private bool foldEffect=true;
		private bool foldInstant=true;
		private bool foldBuff=true;
		protected float DrawEffect(float startX, float startY, Effect effect){
			//~ string text="Ability Effect "+(!foldEffect ? "(show)" : "(hide)");
			//~ foldEffect=EditorGUI.Foldout(new Rect(startX, startY, width, height), foldEffect, text, foldoutStyle);
			//~ if(foldEffect){
				
			//~ }
			
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
		
	}

}


