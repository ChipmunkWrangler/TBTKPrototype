using UnityEngine;
using System.Collections;

using TBTK;

namespace TBTK {

	public class CameraControl : MonoBehaviour {

		private float initialMousePosX;
		private float initialMousePosY;
		
		private float initialRotX;
		private float initialRotY;
		
		#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
			private Vector3 lastTouchPos=new Vector3(9999, 9999, 9999);
			private Vector3 moveDir=Vector3.zero;
			private float moveMagnitude=0;
			
			private float touchZoomSpeed;
		#endif
		
		
		[HideInInspector] public Transform camT;
		
		public float panSpeed=5;
		public float zoomSpeed=5;
		
		public bool enableMouseZoom=true;
		public bool enableMouseRotate=true;
		public bool enableMousePanning=false;
		public bool enableKeyPanning=true;
		
		public int mousePanningZoneWidth=10;
		
		//for mobile/touch input 
		public bool enableTouchPan=true;
		public bool enableTouchZoom=true;
		public bool enableTouchRotate=false;
		public float rotateSpeed=1;
		
		
		public float minPosX=-10;
		public float maxPosX=10;
		
		public float minPosZ=-10;
		public float maxPosZ=10;
		
		public float minZoomDistance=8;
		public float maxZoomDistance=30;
		
		public float minRotateAngle=10;
		public float maxRotateAngle=89;


		//calculated deltaTime based on timeScale so camera movement speed always remain constant
		private float deltaT;
		
		
		
		private float currentZoom=0;
		
		private Transform thisT;
		public static CameraControl instance;
		
		public static void Disable(){ if(instance!=null) instance.enabled=false; }
		public static void Enable(){ if(instance!=null) instance.enabled=true; }

		void Awake(){
			thisT=transform;
			
			instance=this;
			
			//cam=Camera.main;
			camT=Camera.main.transform;
		}
		
		// Use this for initialization
		void Start () {
			minRotateAngle=Mathf.Max(10, minRotateAngle);
			maxRotateAngle=Mathf.Min(89, maxRotateAngle);
			
			minZoomDistance=Mathf.Max(1, minZoomDistance);
			
			currentZoom=camT.localPosition.z;
		}
		
		
		void OnEnable(){
			TBTK.onUnitSelectedE += _OnUnitSelected;
			
			Unit.onActionCamE += OnActionCam;
		}
		void OnDisable(){
			TBTK.onUnitSelectedE -= _OnUnitSelected;
			
			Unit.onActionCamE -= OnActionCam;
		}
		
		
		
		
		
		
		
		private bool actionCamInProgress=false;
		private Transform actionCamDummyT;
		void OnActionCam(Unit srcUnit, Vector3 tgtPos, float chance=-1){
			if(chance==-1) chance=0.5f;
			
			if(Random.Range(0f, 1f)<chance) StartCoroutine(ActionCamRoutine(srcUnit, tgtPos));
		}
		IEnumerator ActionCamRoutine(Unit srcUnit, Vector3 tgtPos){
			TurnControl.ActionCommenced();
			
			actionCamInProgress=true;
			
			Vector3 rootPosOrigin=thisT.position; 
			Quaternion rootRotOrigin=thisT.rotation;
			
			Vector3 posOrigin=camT.localPosition;
			float zoomOrigin=camT.localPosition.z;
			
			Vector3 centerPos=srcUnit.GetTargetT().position+(tgtPos-srcUnit.GetTargetT().position)*0.35f;
			centerPos.y=thisT.position.y;
			
			Quaternion wantedRot=Quaternion.LookRotation(centerPos-srcUnit.GetTargetT().position);
			wantedRot*=Quaternion.Euler(10, 15, 0);
			wantedRot=Quaternion.Euler(wantedRot.eulerAngles.x, wantedRot.eulerAngles.y, 0);
			
			float dist=Vector3.Distance(centerPos, srcUnit.GetTargetT().position);
			
			float duration=0;
			while(duration<1){
				thisT.rotation=Quaternion.Euler(thisT.rotation.eulerAngles.x, thisT.rotation.eulerAngles.y, 0);
				
				camT.localPosition=Vector3.Lerp(new Vector3(0, 0, zoomOrigin), new Vector3(0, 0, -dist-GridManager.GetTileSize()*2), duration);				
				thisT.position=Vector3.Lerp(rootPosOrigin, centerPos, duration);
				thisT.rotation=Quaternion.Lerp(rootRotOrigin, wantedRot, duration);
				
				duration+=Time.deltaTime*1f;
				yield return null;
			}
			thisT.rotation=wantedRot;
			
			TurnControl.ActionCompleted();
			
			yield return null;
			while(!TurnControl.ClearToProceed()) yield return null;
			
			TurnControl.ActionCommenced();
			
			duration=0;
			while(duration<1){
				thisT.rotation=Quaternion.Euler(thisT.rotation.eulerAngles.x, thisT.rotation.eulerAngles.y, 0);
				
				camT.localPosition=Vector3.Lerp(new Vector3(0, 0, -dist-GridManager.GetTileSize()*2), new Vector3(0, 0, zoomOrigin), duration);				
				thisT.position=Vector3.Lerp(centerPos, rootPosOrigin, duration);
				thisT.rotation=Quaternion.Lerp(wantedRot, rootRotOrigin, duration);
				
				duration+=Time.deltaTime*1f;
				yield return null;
			}
			
			thisT.position=rootPosOrigin;
			thisT.rotation=rootRotOrigin;
			camT.localPosition=posOrigin;
			
			TurnControl.ActionCompleted();
			
			actionCamInProgress=false;
		}
		
		
		// Update is called once per frame
		void Update () {
			
			if(actionCamInProgress) return;
			
			if(Time.timeScale==1) deltaT=Time.deltaTime;
			else if(Time.timeScale>1) deltaT=Time.deltaTime/Time.timeScale;
			else deltaT=0.015f;

			
			#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
		
			if(enableTouchPan){
				Quaternion camDir=Quaternion.Euler(0, transform.eulerAngles.y, 0);
				if(Input.touchCount==1){
					Touch touch=Input.touches[0];
					if (touch.phase == TouchPhase.Moved){
						Vector3 deltaPos = touch.position;
						
						if(lastTouchPos!=new Vector3(9999, 9999, 9999)){
							deltaPos=deltaPos-lastTouchPos;
							moveMagnitude=new Vector3(deltaPos.x, 0, deltaPos.y).magnitude*0.1f;
							moveDir=new Vector3(deltaPos.x, 0, deltaPos.y).normalized*-1;
						}
		
						lastTouchPos=touch.position;
					}
				}
				else lastTouchPos=new Vector3(9999, 9999, 9999);

				Vector3 dir=thisT.InverseTransformDirection(camDir*moveDir)*moveMagnitude;
				thisT.Translate (dir * panSpeed * deltaT);
				
				moveMagnitude=moveMagnitude*(1-deltaT*10);
			}
			
			if(enableTouchZoom){
				if(Input.touchCount==2){
					Touch touch1 = Input.touches[0];
					Touch touch2 = Input.touches[1];
					
					//~ Vector3 zoomScreenPos=(touch1.position+touch2.position)/2;
					
					if(touch1.phase==TouchPhase.Moved && touch1.phase==TouchPhase.Moved){
						Vector3 dirDelta=(touch1.position-touch1.deltaPosition)-(touch2.position-touch2.deltaPosition);
						Vector3 dir=touch1.position-touch2.position;
						float dot=Vector3.Dot(dirDelta.normalized, dir.normalized);
						
						if(Mathf.Abs(dot)>0.7f){	
							touchZoomSpeed=dir.magnitude-dirDelta.magnitude;
						}	
					}
					
				}

				currentZoom+=Time.deltaTime*zoomSpeed*touchZoomSpeed;
				
				touchZoomSpeed=touchZoomSpeed*(1-Time.deltaTime*15);
			}
			
			if(enableTouchRotate){
				if(Input.touchCount==2){
					Touch touch1 = Input.touches[0];
					Touch touch2 = Input.touches[1];
					
					Vector2 delta1=touch1.deltaPosition.normalized;
					Vector2 delta2=touch2.deltaPosition.normalized;
					Vector2 delta=(delta1+delta2)/2;
					
					float rotX=thisT.rotation.eulerAngles.x-delta.y*rotateSpeed;
					float rotY=thisT.rotation.eulerAngles.y+delta.x*rotateSpeed;
					rotX=Mathf.Clamp(rotX, minRotateAngle, maxRotateAngle);
					
					thisT.rotation=Quaternion.Euler(rotX, rotY, 0);
				}
			}
			
			#endif
			
			
			
			#if UNITY_EDITOR || !(UNITY_IPHONE && UNITY_ANDROID && UNITY_WP8 && UNITY_BLACKBERRY)
			
			//mouse and keyboard
			if(enableMouseRotate){
				if(Input.GetMouseButtonDown(1)){
					initialMousePosX=Input.mousePosition.x;
					initialMousePosY=Input.mousePosition.y;
					initialRotX=thisT.eulerAngles.y;
					initialRotY=thisT.eulerAngles.x;
				}

				if(Input.GetMouseButton(1)){
					float deltaX=(Input.mousePosition.x-initialMousePosX)*rotateSpeed;
					float deltaRotX=(.1f*(initialRotX/Screen.width));
					float rotX=deltaX+deltaRotX;
					
					float deltaY=(initialMousePosY-Input.mousePosition.y)*rotateSpeed;
					float deltaRotY=-(.1f*(initialRotY/Screen.height));
					float rotY=deltaY+deltaRotY;
					float y=rotY+initialRotY;
					
					//limit the rotation
					if(y>maxRotateAngle){
						initialRotY-=(rotY+initialRotY)-maxRotateAngle;
						y=maxRotateAngle;
					}
					else if(y<minRotateAngle){
						initialRotY+=minRotateAngle-(rotY+initialRotY);
						y=minRotateAngle;
					}
					
					thisT.rotation=Quaternion.Euler(y, rotX+initialRotX, 0);
				}
			}	
			
			Quaternion direction=Quaternion.Euler(0, thisT.eulerAngles.y, 0);
			
			
			if(enableKeyPanning){
				if(Input.GetButton("Horizontal")) {
					Vector3 dir=transform.InverseTransformDirection(direction*Vector3.right);
					thisT.Translate (dir * panSpeed * deltaT * Input.GetAxisRaw("Horizontal"));
				}

				if(Input.GetButton("Vertical")) {
					Vector3 dir=transform.InverseTransformDirection(direction*Vector3.forward);
					thisT.Translate (dir * panSpeed * deltaT * Input.GetAxisRaw("Vertical"));
				}
			}
			if(enableMousePanning){
				Vector3 mousePos=Input.mousePosition;
				Vector3 dirHor=transform.InverseTransformDirection(direction*Vector3.right);
				if(mousePos.x<=0) thisT.Translate(dirHor * panSpeed * deltaT * -3);
				else if(mousePos.x<=mousePanningZoneWidth) thisT.Translate(dirHor * panSpeed * deltaT * -1);
				else if(mousePos.x>=Screen.width) thisT.Translate(dirHor * panSpeed * deltaT * 3);
				else if(mousePos.x>Screen.width-mousePanningZoneWidth) thisT.Translate(dirHor * panSpeed * deltaT * 1);
				
				Vector3 dirVer=transform.InverseTransformDirection(direction*Vector3.forward);
				if(mousePos.y<=0) thisT.Translate(dirVer * panSpeed * deltaT * -3);
				else if(mousePos.y<=mousePanningZoneWidth) thisT.Translate(dirVer * panSpeed * deltaT * -1);
				else if(mousePos.y>=Screen.height) thisT.Translate(dirVer * panSpeed * deltaT * 3);
				else if(mousePos.y>Screen.height-mousePanningZoneWidth) thisT.Translate(dirVer * panSpeed * deltaT * 1);
			}
			
			
			if(enableMouseZoom){
				float zoomInput=Input.GetAxis("Mouse ScrollWheel");
				if(zoomInput!=0){
					currentZoom+=zoomSpeed*zoomInput;
				}
			}
			
			
			#endif
			
			if(avoidClipping){
				currentZoom=Mathf.Clamp (currentZoom, minZoomDistance, maxZoomDistance);
				Vector3 aPos=thisT.TransformPoint(new Vector3(0, 0, currentZoom));
				Vector3 dirC=aPos-thisT.position;
				float dist=Vector3.Distance(aPos, thisT.position);
				RaycastHit hit;
				obstacle=Physics.Raycast (thisT.position, dirC, out hit, dist);
				
				if(!obstacle){
					float camZ=Mathf.Lerp(camT.localPosition.z, currentZoom, Time.deltaTime*4);
					camT.localPosition=new Vector3(camT.localPosition.x, camT.localPosition.y, camZ);
				}
				else{
					dist=Vector3.Distance(hit.point, thisT.position)*0.85f;
					float camZ=Mathf.Lerp(camT.localPosition.z, -dist, Time.deltaTime*50);
					camT.localPosition=new Vector3(camT.localPosition.x, camT.localPosition.y, camZ);
				}
			}
			else{
				currentZoom=Mathf.Clamp(currentZoom, -maxZoomDistance, -minZoomDistance);
				float camZ=Mathf.Lerp(camT.localPosition.z, currentZoom, Time.deltaTime*4);
				camT.localPosition=new Vector3(camT.localPosition.x, camT.localPosition.y, camZ);
			}
			
			
			float x=Mathf.Clamp(thisT.position.x, minPosX, maxPosX);
			float z=Mathf.Clamp(thisT.position.z, minPosZ, maxPosZ);
			
			thisT.position=new Vector3(x, thisT.position.y, z);
			
		}
		
		
		public bool avoidClipping=false;
		private bool obstacle=false;
		
		
		
		
		public bool centerOnSelectedUnit=false;
		public static bool CenterOnSelectedUnit(){ return instance!=null ? instance.centerOnSelectedUnit : false; }
		
		public bool smoothLerping=true;
		public float lerpDuration=.15f;
		public static void OnUnitSelected(Unit unit, bool ignoreAI=true){ instance._OnUnitSelected(unit, ignoreAI); }
		public void _OnUnitSelected(Unit unit){ instance._OnUnitSelected(unit, false); }
		public void _OnUnitSelected(Unit unit, bool ignoreAI){
			if(unit==null || instance==null) return;
			if(ignoreAI && unit.isAIUnit) return;
			if(!instance.centerOnSelectedUnit) return;
			
			if(!instance.smoothLerping) instance.SetPosition(unit.tile.GetPos());
			else instance.StartCoroutine(instance.LerpToPoint(unit.tile.GetPos()));
		}
		
		public void SetPosition(Vector3 targetPoint){	//not in used
			targetPoint.x=Mathf.Clamp(targetPoint.x, minPosX, maxPosX);
			targetPoint.z=Mathf.Clamp(targetPoint.z, minPosZ, maxPosZ);
			
			instance.thisT.position=targetPoint;
		}
		
		IEnumerator LerpToPoint(Vector3 targetPoint){
			lerpingToPoint=true;
			
			targetPoint.x=Mathf.Clamp(targetPoint.x, minPosX, maxPosX);
			targetPoint.z=Mathf.Clamp(targetPoint.z, minPosZ, maxPosZ);
			
			float dist=Vector3.Distance(thisT.position, targetPoint);
			
			float duration=0;
			float step=(1f/lerpDuration)/(dist*0.2f);
			Vector3 startPoint=thisT.position;
			while(true){
				thisT.position=Vector3.Lerp(startPoint, targetPoint, duration);
				duration+=Time.deltaTime*step;
				if(duration>=1) break;
				yield return null;
			}
			thisT.position=targetPoint;
			
			lerpingToPoint=false;
		}
		
		
		private bool lerpingToPoint=false;
		public static bool IsLerping(){ return instance.lerpingToPoint; }
		
		
		
		public bool showGizmo=true;
		void OnDrawGizmos(){
			if(showGizmo){
				Vector3 p1=new Vector3(minPosX, transform.position.y, maxPosZ);
				Vector3 p2=new Vector3(maxPosX, transform.position.y, maxPosZ);
				Vector3 p3=new Vector3(maxPosX, transform.position.y, minPosZ);
				Vector3 p4=new Vector3(minPosX, transform.position.y, minPosZ);
				
				Gizmos.color=Color.green;
				Gizmos.DrawLine(p1, p2);
				Gizmos.DrawLine(p2, p3);
				Gizmos.DrawLine(p3, p4);
				Gizmos.DrawLine(p4, p1);
			}
		}
		
	}

}