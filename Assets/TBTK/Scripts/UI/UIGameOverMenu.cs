using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TBTK;

namespace TBTK {

	public class UIGameOverMenu : MonoBehaviour {
		
		public Text lbStatus;
		
		public GameObject continueButton;
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIGameOverMenu instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			canvasGroup.alpha=0;
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			
			//thisObj.SetActive(false);
			rectT.anchoredPosition=new Vector3(0, 0, 0);
		}
		
		
		public void OnContinueButton(){
			GameControl.LoadNextScene();
		}
		public void OnRestartButton(){
			GameControl.RestartScene();
		}
		public void OnMenuButton(){
			GameControl.LoadMainMenu();
		}
		
		
		public static void Show(int winningFactionID){ instance._Show(winningFactionID); }
		public void _Show(int winningFactionID){
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			
			if(FactionManager.IsPlayerFaction(winningFactionID)){
				lbStatus.text="Victory!!";
			}
			else{
				if(!UIMainControl.ShowContinueButtonWhenLost() && continueButton!=null) continueButton.SetActive(false);
			}
			
			UIMainControl.FadeIn(canvasGroup, 0.25f);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			UIMainControl.FadeOut(canvasGroup, 0.25f);
			StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			yield return new WaitForSeconds(0.25f);
			
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
		}
		
	}

}