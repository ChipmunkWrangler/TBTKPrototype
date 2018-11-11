using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TBTK;

namespace TBTK {

	public class UIUnitOverlay : MonoBehaviour {
		
		[HideInInspector] public Unit unit;
		
		public float posOffset=-20;
		
		public Slider sliderHP;
		public Slider sliderAP;
		
		public GameObject line;
		
		public Image imgCoverIcon;
		private int currentCoverStatus=-1;
		
		public Sprite spriteHalfCover;
		public Sprite spriteFullCover;
		
		
		//private GameObject thisObj;
		private RectTransform rectT;
		public CanvasGroup canvasGroup;
		
		void Awake() {
			//thisObj=gameObject;
			rectT=gameObject.GetComponent<RectTransform>();
			if(canvasGroup==null) canvasGroup=gameObject.AddComponent<CanvasGroup>();
		}
		
		// Update is called once per frame
		void LateUpdate () {
			if(unit==null){
				if(gameObject.activeInHierarchy) gameObject.SetActive(false);
				return;
			}
			
			Vector3 screenPos=Camera.main.WorldToScreenPoint(unit.thisT.position);
			screenPos.z=0;
			rectT.localPosition=(screenPos+new Vector3(0, posOffset))*UIMainControl.GetScaleFactor(); 
			
			sliderHP.value=unit.GetHPRatio();
			sliderAP.value=unit.GetAPRatio();
			
			canvasGroup.alpha=(unit.thisObj.layer==TBTK.GetLayerUnitInvisible() ? 0 :  1);
			
			//_CoverType{None, Half, Full}
			if((int)unit.coverStatus!=currentCoverStatus){
				currentCoverStatus=(int)unit.coverStatus;
				if(currentCoverStatus==1) imgCoverIcon.sprite=spriteHalfCover;
				if(currentCoverStatus==2) imgCoverIcon.sprite=spriteFullCover;
				imgCoverIcon.enabled=!(currentCoverStatus==0);
			}
		}
		
		
		public void SetUnit(Unit tgtUnit){
			unit=tgtUnit;
			
			gameObject.SetActive(true);
			
			if(unit.isAIUnit){
				sliderHP.fillRect.GetComponent<Image>().color=UIUnitOverlayManager.GetHostileHPColor();
				
				sliderAP.gameObject.SetActive(false);
				//line.SetActive(false);
			}
			else{
				sliderHP.fillRect.GetComponent<Image>().color=UIUnitOverlayManager.GetFriendlyHPColor();
				sliderAP.fillRect.GetComponent<Image>().color=UIUnitOverlayManager.GetFriendlyAPColor();
			}
		}
	
	}

}