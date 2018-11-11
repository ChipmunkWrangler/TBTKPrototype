using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	public enum _ShootObjectType{Projectile, Missile, Beam, Effect}
	
	public class ShootObject : MonoBehaviour {

		public _ShootObjectType type;
		
		public float speed=5;
		public float beamDuration=.5f;
		
		private bool hit=false;
		
		//private Unit target;
		private Vector3 targetPos;
		public float maxShootAngle=30f;
		public float maxShootRange=0.5f;
		private float hitThreshold=0.15f;	
		
		public float effectHitDelay=0.25f;
		
		public bool autoSearchLineRenderer=true;
		public List<LineRenderer> lineList=new List<LineRenderer>();
		private List<TrailRenderer> trailList=new List<TrailRenderer>();
		
		//private AttackInstance attInstance;
		
		public delegate void HitCallback();
		public HitCallback hitCallback;
		
		public GameObject shootEffect;
		public bool destroyShootEffect=true;
		public float shootEffectDuration=1f;
		
		public GameObject hitEffect;
		public bool destroyHitEffect=true;
		public float hitEffectDuration=1f;
		
		
		private GameObject thisObj;
		private Transform thisT;
		
		void Awake(){
			thisObj=gameObject;
			thisT=transform;
			
			if(autoSearchLineRenderer){
				LineRenderer[] lines = thisObj.GetComponentsInChildren<LineRenderer>(true);
				for(int i=0; i<lines.Length; i++) lineList.Add(lines[i]);
			}
			
			TrailRenderer[] trails = thisObj.GetComponentsInChildren<TrailRenderer>(true);
			for(int i=0; i<trails.Length; i++) trailList.Add(trails[i]);
			
			if(shootEffect!=null) ObjectPoolManager.New(shootEffect);
			if(hitEffect!=null) ObjectPoolManager.New(hitEffect);
		}
		
		
		void OnEnable(){
			for(int i=0; i<trailList.Count; i++) StartCoroutine(ClearTrail(trailList[i]));
		}
		
		
		public void Shoot(Vector3 tgtPos, float hitTH, HitCallback callback){
			targetPos=tgtPos;
			hitThreshold=hitTH;
			hitCallback=callback;
			
			hit=false;
			
			if(shootEffect!=null){
				if(!destroyShootEffect) ObjectPoolManager.Spawn(shootEffect, thisT.position, thisT.rotation);
				else ObjectPoolManager.Spawn(shootEffect, thisT.position, thisT.rotation, shootEffectDuration);
			}
			
			if(type==_ShootObjectType.Projectile) StartCoroutine(ProjectileRoutine());
			if(type==_ShootObjectType.Beam) StartCoroutine(BeamRoutine());
			if(type==_ShootObjectType.Missile) StartCoroutine(MissileRoutine());
			if(type==_ShootObjectType.Effect) StartCoroutine(EffectRoutine());
		}
		
		
		
		IEnumerator EffectRoutine(){
			if(effectHitDelay>0) yield return new WaitForSeconds(effectHitDelay);
			
			Vector3 dir=(targetPos-thisT.position).normalized;
			thisT.position=targetPos+dir*hitThreshold;
			Hit();
			
			yield return null;
		}
		
		
		IEnumerator BeamRoutine(){
			float dist=Vector3.Distance(thisT.position, targetPos);
			Vector3 targetPosition=thisT.TransformPoint(0, 0, dist-hitThreshold);
			for(int i=0; i<lineList.Count; i++){
				lineList[i].SetPosition(0, thisT.position);
				lineList[i].SetPosition(1, targetPosition);
			}
			
			
			yield return new WaitForSeconds(beamDuration);
			
			thisT.position=targetPosition;
			
			Hit();
		}
		
		
		
		public float GetProjectileShootAngle(Vector3 shootPos, Vector3 targetPos){
			if(type==_ShootObjectType.Projectile)
				return Mathf.Min(1, Vector3.Distance(shootPos, targetPos)/maxShootRange)*maxShootAngle;
			else return 0;
		}
		
		IEnumerator ProjectileRoutine(){
			//if(shootEffect!=null) Instantiate(shootEffect, thisT.position, thisT.rotation);
			
			float timeShot=Time.time;
			
			//make sure the shootObject is facing the target and adjust the projectile angle
			thisT.LookAt(targetPos);
			float angle=GetProjectileShootAngle(thisT.position, targetPos); //Mathf.Min(1, Vector3.Distance(thisT.position, targetPos)/maxShootRange)*maxShootAngle;
			//clamp the angle magnitude to be less than 45 or less the dist ratio will be off
			thisT.rotation=thisT.rotation*Quaternion.Euler(-angle, 0, 0);
			
			Vector3 startPos=thisT.position;
			float iniRotX=thisT.rotation.eulerAngles.x;
			
			float y=Mathf.Min(targetPos.y, startPos.y);
			float totalDist=Vector3.Distance(startPos, targetPos);
			
			//while the shootObject havent hit the target
			while(!hit){
				//calculating distance to targetPos
				Vector3 curPos=thisT.position;
				curPos.y=y;
				float currentDist=Vector3.Distance(curPos, targetPos);
				float curDist=Vector3.Distance(thisT.position, targetPos);
				//if the target is close enough, trigger a hit
				if(curDist<hitThreshold && !hit){
					Hit();
					break;
				}
				
				if(Time.time-timeShot<3.5f){
					//calculate ratio of distance covered to total distance
					float invR=1-Mathf.Min(1, currentDist/totalDist);
					
					//use the distance information to set the rotation, 
					//as the projectile approach target, it will aim straight at the target
					Vector3 wantedDir=targetPos-thisT.position;
					if(wantedDir!=Vector3.zero){
						Quaternion wantedRotation=Quaternion.LookRotation(wantedDir);
						float rotX=Mathf.LerpAngle(iniRotX, wantedRotation.eulerAngles.x, invR);
						
						//make y-rotation always face target
						thisT.rotation=Quaternion.Euler(rotX, wantedRotation.eulerAngles.y, wantedRotation.eulerAngles.z);
					}
				}
				else{
					//this shoot time exceed 3.5sec, abort the trajectory and just head to the target
					thisT.LookAt(targetPos);
				}
					
				//move forward
				thisT.Translate(Vector3.forward*Mathf.Min(speed*Time.deltaTime, curDist));
				
				yield return new WaitForSeconds(Time.fixedDeltaTime);
			}
		}
		
		
		
		
		public float shootAngleY=20;
		private float missileSpeedModifier=1;
		IEnumerator MissileSpeedRoutine(){
			missileSpeedModifier=.05f;
			float duration=0;
			while(duration<1){
				missileSpeedModifier=Mathf.Sin(Mathf.Sin(duration*Mathf.PI/2)*Mathf.PI/2);
				duration+=Time.deltaTime*1f;
				yield return null;
			}
			missileSpeedModifier=1;
		}
		IEnumerator MissileRoutine() {
			StartCoroutine(MissileSpeedRoutine());
			
			float angleX=Random.Range(maxShootAngle/2, maxShootAngle);
			float angleY=Random.Range(shootAngleY/2, maxShootAngle);
			if(Random.Range(0f, 1f)>0.5f) angleY*=-1;
			thisT.LookAt(targetPos);
			thisT.rotation=thisT.rotation;
			Quaternion wantedRotation=thisT.rotation*Quaternion.Euler(-angleX, angleY, 0);
			float rand=Random.Range(4f, 10f);
			
			float totalDist=Vector3.Distance(thisT.position, targetPos);
			
			float estimateTime=totalDist/speed;
			float shootTime=Time.time;
			
			Vector3 startPos=thisT.position;
			
			while(!hit){
				//~ if(target!=null) targetPos=target.GetTargetT().position;
				float currentDist=Vector3.Distance(thisT.position, targetPos);
				
				float delta=totalDist-Vector3.Distance(startPos, targetPos);
				float eTime=estimateTime-delta/speed;
				
				if(Time.time-shootTime>eTime){
					Vector3 wantedDir=targetPos-thisT.position;
					if(wantedDir!=Vector3.zero){
						wantedRotation=Quaternion.LookRotation(wantedDir);
						float val1=(Time.time-shootTime)-(eTime);
						thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRotation, val1/(eTime*currentDist));
					}
				}
				else thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRotation, Time.fixedDeltaTime*rand);
				
				if(currentDist<hitThreshold){
					Hit();
					break;
				}
				
				thisT.Translate(Vector3.forward*Mathf.Min(speed*Time.fixedDeltaTime*missileSpeedModifier, currentDist));
				yield return new WaitForSeconds(Time.fixedDeltaTime);
			}
		}
		
		
		
		
		void Hit(){
			hit=false;
			
			if(hitEffect!=null){
				if(!destroyHitEffect) ObjectPoolManager.Spawn(hitEffect, thisT.position, thisT.rotation);
				else ObjectPoolManager.Spawn(hitEffect, thisT.position, thisT.rotation, hitEffectDuration);
			}
			
			if(hitCallback!=null) hitCallback();
			
			Destroy(thisObj);
		}
		
		
		
		
		IEnumerator ClearTrail(TrailRenderer trail){
			if(trail==null) yield break;
			float trailDuration=trail.time;
			trail.time=-1;
			yield return null;
			trail.time=trailDuration;
		}
		
	}

}