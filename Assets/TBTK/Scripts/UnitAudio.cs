using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {
	
	[RequireComponent (typeof (Unit))]
	public class UnitAudio : MonoBehaviour {
		
		public bool loopMoveSound=false;
		
		public AudioClip selectSound;
		public AudioClip moveSound;
		public AudioClip attackSound;
		public AudioClip attackMeleeSound;
		public AudioClip hitSound;
		public AudioClip destroySound;
		
		private AudioSource audioSrc;
		
		// Use this for initialization
		void Awake () {
			if(moveSound!=null){
				audioSrc=gameObject.GetComponent<AudioSource>();
				if(audioSrc==null){
					audioSrc=gameObject.AddComponent<AudioSource>();
					audioSrc.playOnAwake=false;
					audioSrc.loop=loopMoveSound;
					audioSrc.clip=moveSound;
				}
			}
			
			Unit unit=gameObject.GetComponent<Unit>();
			if(unit!=null) unit.SetAudio(this);
			//else DestroyImmediate(this);
		}
		
		
		public void Select(){ if(selectSound!=null) AudioManager.PlaySound(selectSound);	}
		
		
		public void Move(){ if(audioSrc!=null && audioSrc.clip!=null) audioSrc.Play();	}
		public void StopMove(){ if(audioSrc!=null) audioSrc.Stop();	}
		
		//public void Move(){ if(moveSound!=null)AudioManager.PlaySound(moveSound);	}
		//public void StopMove(){ AudioManager.PlaySound(moveSound);	}
		
		public void Attack(){ if(attackSound!=null)AudioManager.PlaySound(attackSound);	}
		public void AttackMelee(){ if(attackMeleeSound!=null)AudioManager.PlaySound(attackMeleeSound);	}
		
		public void Hit(){ if(hitSound!=null)AudioManager.PlaySound(hitSound);	}
		
		public float Destroy(){ 
			if(destroySound!=null){
				AudioManager.PlaySound(destroySound);	
				return destroySound.length;
			}
			return 0;
		}
		
	}

}
