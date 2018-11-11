using UnityEngine;
using System.Collections;

using TBTK;

namespace TBTK {

	public class UnitAnimation : MonoBehaviour {

		private Unit unit;
		
		public GameObject aniRootObj;
		
		[HideInInspector] public Animator anim;
		
		
		public AnimationClip clipIdle;
		public AnimationClip clipMove;
		public AnimationClip clipAttack;
		public AnimationClip clipAttackMelee;
		public AnimationClip clipHit;
		public AnimationClip clipDestroy;
		
		public float attackDelay=0;
		public float attackDelayMelee=0;
		
		
		void Awake () {
			unit=gameObject.GetComponent<Unit>();
			
			if(unit!=null){
				if(aniRootObj==null) return;
				anim=aniRootObj.GetComponent<Animator>();
				if(anim!=null) unit.SetAnimation(this);
			}
			else return;
			
			AnimatorOverrideController overrideController = new AnimatorOverrideController();
			overrideController.runtimeAnimatorController = anim.runtimeAnimatorController;
	 
			//Debug.Log(overrideController.clips.Length);
			//foreach(AnimationClipPair clipP in overrideController.clips) Debug.Log(clipP.originalClip+"  !  "+clipP.overrideClip);
			
			overrideController["Idle"] = clipIdle;
			overrideController["Move"] = clipMove;
			overrideController["Attack"] = clipAttack;
			overrideController["AttackMelee"] = clipAttackMelee;
			overrideController["Hit"] = clipHit;
			overrideController["Destroy"] = clipDestroy;
			
			//foreach(AnimationClipPair clipP in overrideController.clips) Debug.Log(clipP.originalClip+"  !  "+clipP.overrideClip);
			
			//Debug.Log("this needs fixing");
			
			anim.runtimeAnimatorController = overrideController;
		}
		
		// Update is called once per frame
		void Update () {
			//anim.SetBool("Move", unit.IsMoving());
		}
		
		
		public void Move(){
			anim.SetBool("Move", true);
		}
		public void StopMove(){
			anim.SetBool("Move", false);
		}
		
		public void Attack(){
			anim.SetTrigger("Attack");
		}
		public void AttackMelee(){
			anim.SetTrigger("AttackMelee");
		}
		public void Hit(){
			anim.SetTrigger("Hit");
		}
		public float Destroy(){
			anim.SetTrigger("Destroy");
			//return anim.GetNextAnimatorStateInfo(0).length;
			return clipDestroy!=null ? clipDestroy.length : 0;
		}
		
		
	}

}