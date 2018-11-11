using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UIHUD : MonoBehaviour {

		public GameObject endTurnObj;
		private UIButton endTurnButton=new UIButton();
		
		public GameObject perkButtonObj;
		private UIButton perkButton=new UIButton();
		
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIHUD instance;
		public static UIHUD GetInstance(){ return instance; }
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			endTurnButton.rootObj=endTurnObj;
			endTurnButton.Init();
			endTurnButton.imgHighlight.gameObject.SetActive(false);
			endTurnButton.rootObj.SetActive(false);
			
			perkButton.rootObj=perkButtonObj;
			perkButton.Init();
			perkButton.rootObj.SetActive(false);
			
			rectT.localPosition=new Vector3(0, 0, 0);
		}
		
		
		
		void OnEnable(){
			TBTK.onAllUnitOutOfMoveE += HighlightEndTurnButton;
		}
		void OnDisable(){
			TBTK.onAllUnitOutOfMoveE -= HighlightEndTurnButton;
		}
		
		
		void HighlightEndTurnButton(){
			endTurnButton.imgHighlight.gameObject.SetActive(true);
		}
		
		
		public static void OnGameStarted(){
			instance.endTurnButton.rootObj.SetActive(true);
			instance.perkButton.rootObj.SetActive(UIMainControl.EnablePerkButton());
		}
		
		public static void OnGameInAction(bool flag){ instance._OnGameInAction(flag); }
		public void _OnGameInAction(bool flag){
			//if(!flag) Debug.Log("resume control   "+UIMainControl.IsPlayerTurn());
			//else Debug.Log("lock control   "+UIMainControl.IsPlayerTurn());
			endTurnButton.button.interactable=!flag & UIMainControl.IsPlayerTurn();
			perkButton.button.interactable=!flag & UIMainControl.IsPlayerTurn();
		}
		
		public static void OnNewTurn(bool flag){ instance._OnNewTurn(flag); }
		public void _OnNewTurn(bool flag){
			endTurnButton.button.interactable=flag;
			perkButton.button.interactable=flag;
		}
		
		
		
		public void OnEndTurnButton(){
			if(!TurnControl.ClearToProceed()) return;
			OnGameInAction(true);
			
			endTurnButton.imgHighlight.gameObject.SetActive(false);
			
			//GameControl.EndTurn();
			UIMainControl.EndTurn();
		}
		
		
		public void OnPauseMenuButton(){
			UIMainControl.PauseGame();
		}
		
		public void OnPerkMenuButton(){
			UIMainControl.OnPerkMenu();
		}
		
		
		//this is call during unit deployment phase only
		public static void Show(){ instance._Show(); }
		public void _Show(){
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			canvasGroup.alpha=0;
			thisObj.SetActive(false);
			//UIMainControl.FadeOut(canvasGroup, 0.25f);
		}
		
		public static void HideInstant(){
			instance.canvasGroup.alpha=0;
			instance.thisObj.SetActive(false);
		}
		
	}

}