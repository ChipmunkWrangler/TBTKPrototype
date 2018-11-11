using UnityEngine;
using System.Collections;

using TBTK;

namespace TBTK{
	
	[System.Serializable]
	public class Effect : UnitStat{
		public Sprite icon;
		public string name;
		public string desp;
		
		public int duration=-1;	//how many round the effect last, a round is when all unit or all faction has been moved (depends on the turn/move order combination)
										//when set to 0, the effect only last as long as the unit is active (until end turn button is pressed)
										//when set to -1, none of the buff effect will apply, only immediate value (ie. HPMin, HPMax, stun...) will apply
		
		public TBDuration durationCounter=new TBDuration();
		public void Init(){ durationCounter.Set(duration); }
		public void Iterate(){ durationCounter.Iterate(); }
		public bool Due(){ return durationCounter.Due(); }
		public int GetRemainingDuration(){ return durationCounter.duration; }
		
		public int damageType=0;
		public float HPMin=0;	//direct dmg/gain for the HP
		public float HPMax=0;
		public float APMin=0;	//direct dmg/gain for the AP
		public float APMax=0;
		
		public bool stun=false;
		public bool silence=false;
		
		public Effect Clone(string newName=""){
			Effect eff=new Effect();
			
			//eff.prefabID=prefabID;
			eff.icon=icon;
			eff.name=newName=="" ? name : newName;
			eff.desp=desp;
			
			eff.duration=duration;
			
			eff.damageType=damageType;
			eff.HPMin=HPMin;	//direct dmg/gain for the HP
			eff.HPMax=HPMax;
			
			eff.APMin=APMin;	//direct dmg/gain for the AP
			eff.APMax=APMax;
			
			eff.stun=stun;
			eff.silence=silence;
			
			eff.HP=HP;
			eff.AP=AP;
			
			eff.HPPerTurn=HPPerTurn;
			eff.APPerTurn=APPerTurn;
			
			eff.moveAPCost=moveAPCost;
			eff.attackAPCost=attackAPCost;
			
			eff.turnPriority=turnPriority;
			
			eff.moveRange=moveRange;
			eff.attackRange=attackRange;
			eff.sight=sight;
			
			eff.movePerTurn=movePerTurn;
			eff.attackPerTurn=attackPerTurn;
			eff.counterPerTurn=counterPerTurn;	//counter attack
			
			eff.damage=damage;
			eff.damageMin=damageMin;
			eff.damageMax=damageMax;
			
			eff.hitChance=hitChance;
			eff.dodgeChance=dodgeChance;
			
			eff.critChance=critChance;
			eff.critAvoidance=critAvoidance;
			eff.critMultiplier=critMultiplier;
			
			eff.stunChance=stunChance;
			eff.stunAvoidance=stunAvoidance;
			eff.stunDuration=stunDuration;
			
			eff.silentChance=silentChance;
			eff.silentAvoidance=silentAvoidance;
			eff.silentDuration=silentDuration;
			
			eff.flankingBonus=flankingBonus;
			eff.flankedModifier=flankedModifier;
			
			return eff;
		}
	}
	
	public enum _AbilityType{None, Generic, SpawnNew, ScanFogOfWar}
	
	[System.Serializable]
	public class FactionAbility : Ability {
		public _AbilityType type;
		
		public FactionAbility(){ isFactionAbility=true; }
		
		public void Init(int facID){
			factionID=facID;
			base.Init();
		}
		
		public virtual string IsAvailable(){
			if(AbilityManagerFaction.GetFactionEnergy(factionID)<GetCost()) return "Insufficient AP";
			if(!currentCD.Due()) return name+"  Ability on cooldown   ";//+currentCD.duration;
			
			int limit=GetUseLimit();
			if(limit>=1 && useCount>=limit) return "Ability is used up";
			
			return "";
		}
		
		public virtual FactionAbility Clone(bool useDefaultValue=true){
			FactionAbility facAB=new FactionAbility();
			
			facAB.type=type;
			
			facAB.Copy(this, useDefaultValue);
			facAB.effect=effect.Clone();
			
			return facAB;
		}
	}
	
	
	
	[System.Serializable]
	public class UnitAbility : Ability {
		public enum _AbilityType{None, Generic, SpawnNew, ScanFogOfWar, Overwatch, Teleport}//, ChargeAttack, LineAttack}	//clear all buff?
		public _AbilityType type;
		
		public bool AttackInLine(){ return false; }//type==_AbilityType.ChargeAttack || type==_AbilityType.LineAttack; }
		
		public bool applyToAllUnit=false;
		
		public bool normalAttack=false;		//indicate if the ability is subjected to normal attack rule, block by los, limited by fog-of-war and so on
		public bool requireDirectLOS=false;	//indicate if the ability require direct LOS (only applies if normalAttack is true)
		
		public UnitAbility(){ isFactionAbility=false; }
		
		
		public void Init(Unit un){ 
			unit=un;
			factionID=unit.factionID;
			base.Init();
		}
		
		public string IsAvailable(){
			if(unit.abilityRemain<=0) return "Unit cannot use anymore ability for the turn";
			if(unit.IsSilenced()) return "Unit is silenced";
			if(unit.IsStunned()) return "Unit is stunned";
			if(unit.DisableAbilities()) return "Cannot use abilities";
			if(unit.AP<GetCost()) return "Insufficient AP";
			if(!currentCD.Due()) return "Ability on cooldown";
			
			int limit=GetUseLimit();
			if(limit>=1 && useCount>=limit) return "Ability is used up";
			
			return "";
		}
		
		
		public UnitAbility Clone(bool useDefaultValue=false){
			UnitAbility uAB=new UnitAbility();
			
			uAB.type=type;
			
			uAB.applyToAllUnit=applyToAllUnit;
			
			uAB.chanceToHit=chanceToHit;
			uAB.normalAttack=normalAttack;
			uAB.requireDirectLOS=requireDirectLOS;
			
			uAB.unit=unit;
			
			uAB.Copy(this, useDefaultValue);
			uAB.effect=effect.Clone();
			
			return uAB;
		}
	}
	
	
	
	public enum _TargetType{AllUnit, HostileUnit, FriendlyUnit, AllTile, EmptyTile}
	public enum _EffectTargetType{AllUnit, HostileUnit, FriendlyUnit, Tile}
	
	[System.Serializable]
	public class Ability {
		public int prefabID;
		public Sprite icon;
		public string name;
		public string desp;
		
		public bool isFactionAbility=false;
		public bool onlyAvailableViaPerk=false;
		
		public int factionID=0;	//the ID of the faction in factionManager, assigned in runtime
		public Unit unit=null;	//the source unit of the ability, for UnitAbility
		
		public float cost=5;
		public int cooldown=5;
		public TBDuration currentCD=new TBDuration();
		
		public int useLimit=-1;
		public int useCount=0;
		
		public float chanceToHit=1;				//hitChance is in in base class to modify unit's hit chance 
		
		public _EffectTargetType effTargetType;
		
		public bool requireTargetSelection;
		public _TargetType targetType;
		public int range=5;
		public int aoeRange=0;
		
		//public bool shoot=false;
		public GameObject shootObject;
		
		public bool enableMoveAfterCast=true;
		public bool enableAttackAfterCast=true;
		public bool enableAbilityAfterCast=true;
		
		public GameObject effectObject;				//to be use when hit/cast
		public bool autoDestroyEffect=true;
		public float effectObjectDuration=1.5f;
		
		public GameObject effectObjectTarget;		//to be used on individual target
		public bool autoDestroyEffectTgt=true;
		public float effectObjectTgtDuration=1.5f;
		
		public bool useDefaultEffect=true;
		public float effectDelayDuration=0.25f;
		public Effect effect=new Effect();
		
		public Unit spawnUnit;
		
		
		public void Init(){
			effect.name=name;
			effect.icon=icon;
			effect.desp=desp;
		}
		
		public void Use(){
			currentCD.Set(GetCooldown());
			useCount+=1;
		}
		
		public int GetCooldownRemain(){
			return currentCD.duration;
		}
		
		public int GetUseRemain(){
			return useLimit<0 ? -1 : useLimit-useCount;
		}
		
		
		
		public void Copy(Ability ability, bool useDefaultValue=true){
			prefabID	=ability.prefabID;
			icon		=ability.icon;
			name		=ability.name;
			desp		=ability.desp;
			
			factionID=ability.factionID;
			unit		=ability.unit;
			
			onlyAvailableViaPerk		=ability.onlyAvailableViaPerk;
			targetType		=ability.targetType;
			
			cost			=ability.cost;
			cooldown	=ability.cooldown;
			useLimit		=ability.useLimit;
			useCount	=ability.useCount;
			
			effTargetType	=ability.effTargetType;
			
			requireTargetSelection	=ability.requireTargetSelection;
			targetType					=ability.targetType;
			range			=ability.range;
			aoeRange	=ability.aoeRange;
			
			shootObject	=ability.shootObject;
			
			enableMoveAfterCast	=ability.enableMoveAfterCast;
			enableAttackAfterCast	=ability.enableAttackAfterCast;
			enableAbilityAfterCast	=ability.enableAbilityAfterCast;
			
			effectObject	=ability.effectObject;
			autoDestroyEffect	=ability.autoDestroyEffect;
			effectObjectDuration	=ability.effectObjectDuration;
			
			effectObjectTarget	=ability.effectObjectTarget;
			autoDestroyEffectTgt	=ability.autoDestroyEffectTgt;
			effectObjectTgtDuration	=ability.effectObjectTgtDuration;
			
			useDefaultEffect		=ability.useDefaultEffect;
			effectDelayDuration	=ability.effectDelayDuration;
			effect=ability.effect.Clone();
			
			spawnUnit		=ability.spawnUnit;
			
		}
		
		
		
		public float GetCost(){ return cost+PerkManager.GetAbilityCost(prefabID, isFactionAbility); }					
		public int GetCooldown(){ return cooldown+PerkManager.GetAbilityCD(prefabID, isFactionAbility); }		
		public int GetUseLimit(){ return useLimit+PerkManager.GetAbilityUseLimit(prefabID, isFactionAbility); }			
		public float GetHitChance(){ return chanceToHit+PerkManager.GetAbilityHit(prefabID, isFactionAbility); }	
		
		public int GetRange(){ return range+PerkManager.GetAbilityRange(prefabID, isFactionAbility); }				
		public int GetAOERange(){ return aoeRange+PerkManager.GetAbilityAOERange(prefabID, isFactionAbility); }	
		
		public float GetEffectiveHP(){ return Random.Range(GetEffHPMin(), GetEffHPMax()); }
		public float GetEffectiveAP(){ return Random.Range(GetEffAPMin(), GetEffAPMax()); }
		
		public float GetEffHPMin(){ return effect.HPMin+PerkManager.GetAbilityHP(prefabID, isFactionAbility); }		
		public float GetEffHPMax(){ return effect.HPMax+PerkManager.GetAbilityHP(prefabID, isFactionAbility); }	
		public float GetEffAPMin(){ return effect.APMin+PerkManager.GetAbilityAP(prefabID, isFactionAbility); }		
		public float GetEffAPMax(){ return effect.APMax+PerkManager.GetAbilityAP(prefabID, isFactionAbility); }	
		
		public int GetEffDuration(){ return effect.duration+PerkManager.GetAbilityDuration(prefabID, isFactionAbility); }	
		
		public float GetEffHPBuff(){ return effect.HP+PerkManager.GetAbilityHPBuff(prefabID, isFactionAbility); }		
		public float GetEffAPBuff(){ return effect.AP+PerkManager.GetAbilityAPBuff(prefabID, isFactionAbility); }		
		
		public float GetEffHPPerTurn(){ return effect.HPPerTurn+PerkManager.GetAbilityHPPerTurn(prefabID, isFactionAbility); }		
		public float GetEffAPPerTurn(){ return effect.APPerTurn+PerkManager.GetAbilityAPPerTurn(prefabID, isFactionAbility); }
		
		
		public float GetEffMoveAPCost(){ return effect.moveAPCost+PerkManager.GetAbilityMoveAPCost(prefabID, isFactionAbility); }		
		public float GetEffAttackAPCost(){ return effect.attackAPCost+PerkManager.GetAbilityAttackAPCost(prefabID, isFactionAbility); }		
		
		public float GetEffTurnPriority(){ return effect.turnPriority+PerkManager.GetAbilityTurnPriority(prefabID, isFactionAbility); }		
		
		public int GetEffMoveRange(){ return effect.moveRange+PerkManager.GetAbilityMoveRange(prefabID, isFactionAbility); }		
		public int GetEffAttackRange(){ return effect.attackRange+PerkManager.GetAbilityAttackRange(prefabID, isFactionAbility); }	
		public int GetEffSight(){ return effect.sight+PerkManager.GetAbilitySight(prefabID, isFactionAbility); }		
		
		public int GetEffMovePerTurn(){ return effect.movePerTurn+PerkManager.GetAbilityMovePerTurn(prefabID, isFactionAbility); }		
		public int GetEffAttackPerTurn(){ return effect.attackPerTurn+PerkManager.GetAbilityAttackPerTurn(prefabID, isFactionAbility); }		
		public int GetEffCounterPerTurn(){ return effect.counterPerTurn+PerkManager.GetAbilityCounterPerTurn(prefabID, isFactionAbility); }		
		
		public float GetEffDamage(){ return effect.damage+PerkManager.GetAbilityDamage(prefabID, isFactionAbility); }		
		//public float GetDamageMin(){ return unitStat.damageMin+PerkManager.GetAbilityDamage(prefabID, isFactionAbility); }		
		//public float GetDamageMax(){ return unitStat.damageMax+PerkManager.GetAbilityDamage(prefabID, isFactionAbility); }		
		
		public float GetEffHitChance(){ return effect.hitChance+PerkManager.GetAbilityHitChance(prefabID, isFactionAbility); }		
		public float GetEffDodgeChance(){ return effect.dodgeChance+PerkManager.GetAbilityDodgeChance(prefabID, isFactionAbility); }	
		
		public float GetEffCritChance(){ return effect.critChance+PerkManager.GetAbilityCritChance(prefabID, isFactionAbility); }	
		public float GetEffCritAvoidance(){ return effect.critAvoidance+PerkManager.GetAbilityCritAvoidance(prefabID, isFactionAbility); }		
		public float GetEffCritMultiplier(){ return effect.critMultiplier+PerkManager.GetAbilityCritMultiplier(prefabID, isFactionAbility); }		
		
		public float GetEffStunChance(){ return effect.stunChance+PerkManager.GetAbilityStunChance(prefabID, isFactionAbility); }		
		public float GetEffStunAvoidance(){ return effect.stunAvoidance+PerkManager.GetAbilityStunAvoidance(prefabID, isFactionAbility); }		
		public int GetEffStunDuration(){ return effect.stunDuration+PerkManager.GetAbilityStunDuration(prefabID, isFactionAbility); }	
		
		public float GetEffSilentChance(){ return effect.silentChance+PerkManager.GetAbilitySilentChance(prefabID, isFactionAbility); }		
		public float GetEffSilentAvoidance(){ return effect.silentAvoidance+PerkManager.GetAbilitySilentAvoidance(prefabID, isFactionAbility); }		
		public int GetEffSilentDuration(){ return effect.silentDuration+PerkManager.GetAbilitySilentDuration(prefabID, isFactionAbility); }	
		
		public float GetEffFlankingBonus(){ return effect.flankingBonus+PerkManager.GetAbilityFlankingBonus(prefabID, isFactionAbility); }		
		public float GetEffFlankedModifier(){ return effect.flankedModifier+PerkManager.GetAbilityFlankedModifier(prefabID, isFactionAbility); }	
		
		
		
		
		public Effect CloneEffect(){
			Effect eff=new Effect();
			
			//eff.prefabID=prefabID;
			eff.icon=icon;
			eff.name=name;
			eff.desp=desp;
			
			//eff.duration=effect.duration;//GetDuration();
			eff.duration=GetEffDuration();//effect.duration;//GetDuration();
			
			eff.damageType=effect.damageType;
			eff.HPMin=GetEffHPMin();	//direct dmg/gain for the HP
			eff.HPMax=GetEffHPMax();
			
			eff.APMin=GetEffAPMin();	//direct dmg/gain for the AP
			eff.APMax=GetEffAPMax();
			
			eff.stun=effect.stun;
			eff.silence=effect.silence;
			
			eff.HP=GetEffHPBuff();
			eff.AP=GetEffAPBuff();
			
			eff.HPPerTurn=GetEffHPPerTurn();
			eff.APPerTurn=GetEffAPPerTurn();
			
			eff.moveAPCost=GetEffMoveAPCost();
			eff.attackAPCost=GetEffAttackAPCost();
			
			eff.turnPriority=GetEffTurnPriority();
			
			eff.moveRange=GetEffMoveRange();
			eff.attackRange=GetEffAttackRange();
			eff.sight=GetEffSight();
			
			eff.movePerTurn=GetEffMovePerTurn();
			eff.attackPerTurn=GetEffAttackPerTurn();
			eff.counterPerTurn=GetEffCounterPerTurn();	//counter attack
			
			eff.damage=GetEffDamage();
			//eff.damageMin=GetDamageMin;
			//eff.damageMax=GetDamageMax;
			
			eff.hitChance=GetEffHitChance();
			eff.dodgeChance=GetEffDodgeChance();
			
			eff.critChance=GetEffCritChance();
			eff.critAvoidance=GetEffCritAvoidance();
			eff.critMultiplier=GetEffCritMultiplier();
			
			eff.stunChance=GetEffStunChance();
			eff.stunAvoidance=GetEffStunAvoidance();
			eff.stunDuration=GetEffStunDuration();
			
			eff.silentChance=GetEffSilentChance();
			eff.silentAvoidance=GetEffSilentAvoidance();
			eff.silentDuration=GetEffSilentDuration();
			
			eff.flankingBonus=GetEffFlankingBonus();
			eff.flankedModifier=GetEffFlankedModifier();
			
			return eff;
		}
		
		
	}
	
	
	
	
	
	
	
	
}