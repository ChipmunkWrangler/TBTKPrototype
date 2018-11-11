using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	public class Collectible : MonoBehaviour {
		[HideInInspector] public int prefabID=-1;
		
		[Space(10)]
		public Sprite icon;
		public string itemName="Collectible";
		public string desp="";
		
		[Space(10)]
		//public int facAbilityID=-1;
		public List<int> facAbilityIDList=new List<int>();
		
		[Space(10)]
		public Effect effect;
		
		[Space(10)]
		public GameObject triggerEffectObj;
		public bool destroyTriggerEffect;
		public float triggerEffectDuration;
		
		public void Trigger(Unit unit){
			if(!destroyTriggerEffect) ObjectPoolManager.Spawn(triggerEffectObj, transform.position, Quaternion.identity);
			else ObjectPoolManager.Spawn(triggerEffectObj, transform.position, Quaternion.identity, triggerEffectDuration);
			
			if(facAbilityIDList.Count>0){
				int facAbilityID=facAbilityIDList[Random.Range(0, facAbilityIDList.Count)];
				
				FactionAbility ability=AbilityManagerFaction.GetFactionAbility(facAbilityID);
				if(ability!=null){
					if(!ability.requireTargetSelection) AbilityManager.ApplyAbilityEffect(null, ability.Clone(), (int)ability.type);
					else AbilityManager.ApplyAbilityEffect(unit.tile, ability.Clone(), (int)ability.type);
				}
			}
			
			unit.ApplyEffect(CloneEffect());
			
			CollectibleManager.TriggerCollectible(this);
			
			Destroy(gameObject);
		}
		
		
		public Effect CloneEffect(){
			Effect eff=effect.Clone();
			
			eff.icon=icon;
			eff.name=itemName;
			eff.desp=desp;
			
			return eff;
		}
	}

}