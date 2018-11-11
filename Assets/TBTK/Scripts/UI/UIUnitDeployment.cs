using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UIUnitDeployment : MonoBehaviour {
		
		public Text lbStatus;
		public List<UIButton> buttonList=new List<UIButton>();
		
		public UIButton butAutoNCom;
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIUnitDeployment instance;
		public static UIUnitDeployment GetInstance(){ return instance; }
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			butAutoNCom.Init();
			
			for(int i=0; i<6; i++){
				if(i==0) buttonList[0].Init();
				else if(i>0) buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "Button"+(i+1)));
				
				buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnButton, null);
				
				buttonList[i].rootObj.SetActive(false);
			}
			
			rectT.localPosition=new Vector3(0, 0, 0);
			
			thisObj.SetActive(false);
		}
		
		
		void OnEnable(){
			TBTK.onUnitDeployedE += UpdateDisplay;
		}
		void OnDisable(){
			TBTK.onUnitDeployedE -= UpdateDisplay;
		}
		
		
		public void OnUnitDeployed(Unit unit){
			UpdateDisplay();
		}
		
		public void OnAutoDeploy(){
			FactionManager.AutoDeployCurrentFaction();
			UpdateDisplay();
		}
		
		public void OnAutoNCompleteButton(){
			if(FactionManager.IsDeploymentComplete()) _Hide();
			else FactionManager.AutoDeployCurrentFaction();
		}
		
		
		
		public void UpdateDisplay(Unit unit=null){
			List<Unit> unitList=FactionManager.GetDeployingUnitList();
			int unitID=FactionManager.GetDeployingUnitID();
			
			if(buttonList.Count<unitList.Count){
				buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "Button"+(buttonList.Count+1)));
				buttonList[buttonList.Count-1].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnButton, null);
			}
			
			for(int i=0; i<buttonList.Count; i++){
				if(i<unitList.Count){
					buttonList[i].imgIcon.sprite=unitList[i].iconSprite;
					buttonList[i].imgHighlight.enabled=(unitID==i);
					buttonList[i].rootObj.SetActive(true);
					
				}
				else buttonList[i].rootObj.SetActive(false);
			}
			
			butAutoNCom.label.text=FactionManager.IsDeploymentComplete() ? "Complete" : "Auto Deploy";
			butAutoNCom.imgHighlight.gameObject.SetActive(FactionManager.IsDeploymentComplete());
			
			lbStatus.text=unitList.Count>0 ? "Deploying Unit:" : "All Unit Deployed";
		}
		
		
		
		public void OnButton(GameObject butObj, int pointerID=-1){
			int prevID=FactionManager.GetDeployingUnitID();
			buttonList[prevID].imgHighlight.enabled=false;
			
			int ID=GetButtonID(butObj);
			buttonList[ID].imgHighlight.enabled=true;
			
			FactionManager.SetDeployingUnitID(ID);
		}
		
		public void OnHoverButton(GameObject butObj){
			
		}
		
		public void OnExitButton(GameObject butObj){
			
		}
		
		private int GetButtonID(GameObject butObj){
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		
		
		public void OnPauseMenuButton(){
			UIMainControl.PauseGame();
		}
		
		
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			
			UpdateDisplay();
			
			UIMainControl.FadeIn(canvasGroup, 0.35f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			UIMainControl.FadeOut(canvasGroup, 0.35f);
			StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			yield return new WaitForSeconds(0.35f);
			
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			
			yield return new WaitForSeconds(0.5f);
			
			thisObj.SetActive(false);
			
			FactionManager.CompleteDeployment();
		}
		
	}

}