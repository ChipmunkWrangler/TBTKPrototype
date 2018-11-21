using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using UnityStandardAssets.ImageEffects;

using TBTK;

namespace TBTK {

	public class UIMainControl : MonoBehaviour {

		[Tooltip("Check to show damage overlay of each attack when hitting a target")]
		public bool enableTextOverlay=true;
		public static bool EnableTextOverlay(){ return instance.enableTextOverlay; }
		
		[Tooltip("Check to enable perk button. Otherwise the button will be hidden, deny the player access to perk screen")]
		public bool enablePerkButton=true;
		public static bool EnablePerkButton(){ return instance.enablePerkButton; }
		
		[Space(5)]
		[Tooltip("Check to show continue button to the next level on game-over menu even when the level is lost")]
		public bool showContinueButtonWhenLost=false;
		public static bool ShowContinueButtonWhenLost(){ return instance.showContinueButtonWhenLost; }
		
		
		[Space(5)]
		[Tooltip("Check to enable touch mode (optional mode intend for touch input)\n\nwhen using touch mode, build and ability button wont be trigger immediately as soon as they are click.\n\nInstead the first click will only bring up the tooltip, a second click will then confirm the button click")]
		public bool touchMode=false;
		public static bool InTouchMode(){ return instance.touchMode; }
		
		[Space(10)]
		[Tooltip("The blur image effect component on the main ui camera (optional)")]
		public BlurOptimized uiBlurEffect;
		
		[Space(10)]
		[Tooltip("Check to disable auto scale up of UIElement when the screen resolution exceed reference resolution specified in CanvasScaler/nRecommended to have this set to false when building mobile")]
		public bool limitScale=true;
		
		[Tooltip("The CanvasScaler components of all the canvas. Required to have the floating UI elements appear in the right screen position")]
		public List<CanvasScaler> scalerList=new List<CanvasScaler>();
		public static float GetScaleFactor(){ 
			if(instance.scalerList.Count==0) return 1;
			
			if(instance.scalerList[0].uiScaleMode==CanvasScaler.ScaleMode.ConstantPixelSize) 
				return 1f/instance.scalerList[0].scaleFactor;
			if(instance.scalerList[0].uiScaleMode==CanvasScaler.ScaleMode.ScaleWithScreenSize) 
				return (float)instance.scalerList[0].referenceResolution.x/(float)Screen.width;
			
			return 1;
		}
		
		
		private static UIMainControl instance;
		private float holdStartedTime = -1f;
		private const float holdThreshhold = 1f;
		private Tile heldTile;

		void Awake(){
			instance=this;
		}
		
		void Start(){
			if(limitScale){
				for(int i=0; i<scalerList.Count; i++){
					if(Screen.width>=scalerList[i].referenceResolution.x) instance.scalerList[i].uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
					else instance.scalerList[i].uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
				}
			}
		}
		
		
		
		void Update(){
			if(Input.touchCount>1) return;
			//if(TBTK.UI.IsCursorOnUI(0) || TDTK.UI.IsCursorOnUI()) return;
			
			int pointerID=Input.touchCount>0 ? 0 : -1;
			
			//~ if(!touchMode){
				//~ if(Input.GetMouseButtonDown(0)) OnLeftCursorDown(pointerID);
				//~ else if(Input.GetMouseButtonDown(1)) OnRightCursorDown(pointerID);
			//~ }
			//~ else{
				if(Input.GetMouseButtonDown(0)) 
					StartCoroutine(OnLeftCursorDown(pointerID));
				
				if(Input.GetMouseButtonDown(1)){
					if(!touchMode){
						Tile tile=GridManager.GetHoveredTile();
						if(tile!=null && tile.unit!=null)	UIUnitInfo.Show(tile);
						else UIUnitInfo.Hide();
					}
					else{
						ClearLastClickTile();
					}
					
					//~ Tile hoveredTile=GridManager.GetHoveredTile();
					//~ Debug.Log("Distance: "+GridManager.GetDistance(hoveredTile, GameControl.GetSelectedUnit().tile)+"  "+hoveredTile+"  "+GameControl.GetSelectedUnit().tile);
					//~ FogOfWar.InLOS(hoveredTile, GameControl.GetSelectedUnit().tile, true);
				}

				bool showUnitInfo = false;
				if (Input.GetMouseButton(0))
				{
					showUnitInfo = OnLeftCursor(pointerID);
				} else { 
					heldTile = null;
					holdStartedTime = -1f;
				}
				if (!showUnitInfo)
				{
					UIUnitInfo.Hide();
				}
			//~ }
		}
		
		private Tile lastClickTile;
		private void ClearLastClickTile(){
			lastClickTile=null;
			UIUnitInfo.Hide();
			UIInputOverlay.SetNewHoveredTile(null);
		}
		private void NewClickTile(Tile tile){	//only called in TouchMode
			if(lastClickTile!=tile) lastClickTile=tile;	//first click
			else GridManager.OnCursorDown();		//second click
			UIInputOverlay.SetNewHoveredTile(lastClickTile);
		}
		
		//~ void OnRightCursorDown(int pointer=-1){
			//~ if(GridManager.GetHoveredTile()!=null) GridManager.OnCursorDown();
			//~ ClearLastClickTile();
		//~ }
		bool OnLeftCursor(int pointer = -1) { // return true if we are showing the unit info
			//yield return null;
			
			if(UI.IsCursorOnUI(pointer)) return false;
			
			Tile tile=GridManager.GetHoveredTile();
			if(tile!=null && tile.unit!=null){
				// UIUnitInfo.Hide();
				if(touchMode && !GameControl.CanSelectUnit(tile.unit)) 
				{
					if (heldTile == tile)
					{
						if (holdStartedTime > 0 && Time.time - holdStartedTime > holdThreshhold)
						{
							UIUnitInfo.Show(tile);
							return true;
						}
					} else {
						heldTile = tile;
						holdStartedTime = Time.time;
					}
				}					
			}
			return false;
		}
		IEnumerator OnLeftCursorDown(int pointer=-1){
			yield return null;
			
			if(UI.IsCursorOnUI(pointer)) yield break;
			
			Tile tile=GridManager.GetHoveredTile();
			if(tile!=null){
				if(GameControl.GetGamePhase()==_GamePhase.UnitDeployment){
					GridManager.OnCursorDown();
					yield break;
				}
				
				if(AbilityManager.InTargetMode()){
					GridManager.OnCursorDown();
					yield break;
				}
				
				if(touchMode && GridManager.CanAttackTile(GridManager.GetHoveredTile())){
					NewClickTile(tile);
				}
				else if(touchMode && GridManager.CanMoveToTile(GridManager.GetHoveredTile())){
					NewClickTile(tile);
				}
				else{
					if(!GridManager.CanPerformAction(tile)){
						//ClearLastClickTile(); 	dont call last click tile, we dont want to hide unit info if there's any
						lastClickTile=null;
						UIInputOverlay.SetNewHoveredTile(null);
					}
					else GridManager.OnCursorDown();	//ClearLastClickTile() will be called as soon as unit move or attack or cast ability
				}
			}
			else ClearLastClickTile();
			
			//~ if(tile!=null){
				//~ if(tile.unit!=null){
					//~ if(!GameControl.CanSelectUnit(tile.unit)) UIUnitInfo.Show(tile);
					//~ else{
						//~ GridManager.OnCursorDown();
						//~ UIUnitInfo.Hide();
						//~ return;
					//~ }
				//~ }
				//~ else UIUnitInfo.Hide();
				
				//~ if(touchMode && GridManager.CanAttackTile(GridManager.GetHoveredTile())){
					//~ NewClickTile(tile);
				//~ }
				//~ else if(touchMode && GridManager.CanMoveToTile(GridManager.GetHoveredTile())){
					//~ NewClickTile(tile);
				//~ }
				//~ else{
					//~ if(!GridManager.CanPerFormAction(tile)){
						//~ //ClearLastClickTile(); 	dont call last click tile, we dont want to hide unit info if there's any
						//~ lastClickTile=null;
						//~ UIInputOverlay.SetNewHoveredTile(null);
					//~ }
					//~ else GridManager.OnCursorDown();	//ClearLastClickTile() will be called as soon as unit move or attack or cast ability
				//~ }
			//~ }
			//~ else ClearLastClickTile();
		}
		
		
		
		void OnEnable(){
			TBTK.onUnitDeploymentE += OnUnitDeployment;
			
			TBTK.onNewTurnE += OnNewTurn;
			TBTK.onGameInActionE += OnGameInAction;
			TBTK.onGameOverE += OnGameOver;
			
			TBTK.onUnitSelectedE += OnUnitSelected;
		}
		void OnDisable(){
			TBTK.onUnitDeploymentE -= OnUnitDeployment;
			
			TBTK.onNewTurnE -= OnNewTurn;
			TBTK.onGameInActionE -= OnGameInAction;
			TBTK.onGameOverE -= OnGameOver;
			
			TBTK.onUnitSelectedE -= OnUnitSelected;
		}
		
		
		void OnUnitDeployment(bool flag){
			if(flag){
				UIUnitDeployment.Show();
				UIHUD.HideInstant();
				UISelectedUnitInfo.HideInstant();
			}
			else{
				//UIUnitDeployment.Hide();
				UIHUD.Show();
				UISelectedUnitInfo.Show();
			}
		}
		
		
		void OnNewTurn(bool flag){
			if(!isGameStarted){
				isGameStarted=true;
				UIHUD.OnGameStarted();
			}
			
			isPlayerTurn=flag;
			UIHUD.OnNewTurn(flag);
			UIUnitAbilityButton.OnNewTurn(flag);
			UIFactionAbilityButton.OnNewTurn(flag);
			
			ClearLastClickTile();
		}
		
		void OnGameInAction(bool flag){
			UIHUD.OnGameInAction(flag);
			UIUnitAbilityButton.OnGameInAction(flag);
			UIFactionAbilityButton.OnGameInAction(flag);
			
			ClearLastClickTile();
		}
		
		void OnGameOver(int winningFactionID){
			FadeBlur(uiBlurEffect, 0, 2);
			UIGameOverMenu.Show(winningFactionID);
		}
		
		void OnUnitSelected(Unit unit){
			ClearLastClickTile();
		}
		
		
		private bool isGameStarted=false;
		public static bool IsGameStarted(){ return instance!=null ? instance.isGameStarted : false; }
		
		private bool isPlayerTurn=false;
		public static bool IsPlayerTurn(){ return instance!=null ? instance.isPlayerTurn : false; }
		
		
		
		public static void EndTurn(){ instance.StartCoroutine(instance._EndTurn()); }
		public IEnumerator _EndTurn(){
			UIUnitAbilityButton.Hide();
			UIFactionAbilityButton.Hide();
			
			yield return new WaitForSeconds(0.25f);
			GameControl.EndTurn();
		}
		
		
		public static void PauseGame(){ instance._PauseGame(); }
		public void _PauseGame(){
			Debug.Log("_PauseGame");
			FadeBlur(uiBlurEffect, 0, 2);
			//CameraControl.TurnBlurOn();
			UIPauseMenu.Show();
		}
		public static void ResumeGame(){ instance.StartCoroutine(instance._ResumeGame()); }
		IEnumerator _ResumeGame(){
			Debug.Log("_ResumeGame");
			FadeBlur(uiBlurEffect, 2, 0);
			//CameraControl.TurnBlurOff();
			UIPauseMenu.Hide();
			yield return StartCoroutine(WaitForRealSeconds(0.25f));
		}
		
		
		public static void OnShowUnitInfoScreen(Unit unit){ instance._OnShowUnitInfoScreen(unit); }
		public void _OnShowUnitInfoScreen(Unit unit){
			FadeBlur(uiBlurEffect, 0, 2);
			UIUnitInfoScreen.Show(unit);
		}
		public static void HideUnitInfoScreen(){ instance.StartCoroutine(instance._HideUnitInfoScreen()); }
		IEnumerator _HideUnitInfoScreen(){
			FadeBlur(uiBlurEffect, 2, 0);
			UIUnitInfoScreen.Hide();
			yield return StartCoroutine(WaitForRealSeconds(0.25f));
		}
		
		
		public static void OnPerkMenu(){ instance._OnPerkMenu(); }
		public void _OnPerkMenu(){
			FadeBlur(uiBlurEffect, 0, 2);
			UIPerkMenu.Show();
		}
		public static void OnClosePerkMenu(){ instance.StartCoroutine(instance._OnClosePerkMenu()); }
		IEnumerator _OnClosePerkMenu(){
			FadeBlur(uiBlurEffect, 2, 0);
			UIPerkMenu.Hide();
			yield return StartCoroutine(WaitForRealSeconds(0.25f));
		}
		
		
		public static IEnumerator WaitForRealSeconds(float time){
			float start = Time.realtimeSinceStartup;
			while (Time.realtimeSinceStartup < start + time) yield return null;
		}
		
		
		public static void FadeOut(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){ 
			instance.StartCoroutine(instance._FadeOut(canvasGroup, 1f/duration, obj));
		}
		IEnumerator _FadeOut(CanvasGroup canvasGroup, float timeMul, GameObject obj){
			float duration=0;
			while(duration<1){
				canvasGroup.alpha=Mathf.Lerp(1f, 0f, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			canvasGroup.alpha=0f;
			
			if(obj!=null) obj.SetActive(false);
		}
		public static void FadeIn(CanvasGroup canvasGroup, float duration=0.25f, GameObject obj=null){ 
			instance.StartCoroutine(instance._FadeIn(canvasGroup, 1f/duration, obj)); 
		}
		IEnumerator _FadeIn(CanvasGroup canvasGroup, float timeMul, GameObject obj){
			if(obj!=null) obj.SetActive(true);
			
			float duration=0;
			while(duration<1){
				canvasGroup.alpha=Mathf.Lerp(0f, 1f, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			canvasGroup.alpha=1f;
		}
		public static void Fade(CanvasGroup canvasGroup, float duration=0.25f, float startValue=0.5f, float endValue=0.5f){ 
			instance.StartCoroutine(instance._Fade(canvasGroup, 1f/duration, startValue, endValue));
		}
		IEnumerator _Fade(CanvasGroup canvasGroup, float timeMul, float startValue, float endValue){
			float duration=0;
			while(duration<1){
				canvasGroup.alpha=Mathf.Lerp(startValue, endValue, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
			canvasGroup.alpha=endValue;
		}
		
		
		
		public static void FadeBlur(BlurOptimized blurEff, float startValue=0, float targetValue=0){
			if(blurEff==null || instance==null) return;
			instance.StartCoroutine(instance.FadeBlurRoutine(blurEff, startValue, targetValue));
		}
		//change the blur component blur size from startValue to targetValue over 0.25 second
		IEnumerator FadeBlurRoutine(BlurOptimized blurEff, float startValue=0, float targetValue=0){
			blurEff.enabled=true;
			
			float duration=0;
			while(duration<1){
				float value=Mathf.Lerp(startValue, targetValue, duration);
				blurEff.blurSize=value;
				duration+=Time.unscaledDeltaTime*4f;	//multiply by 4 so it only take 1/4 of a second
				yield return null;
			}
			blurEff.blurSize=targetValue;
			
			if(targetValue==0) blurEff.enabled=false;
			if(targetValue==1) blurEff.enabled=true;
		}
		
	}

}