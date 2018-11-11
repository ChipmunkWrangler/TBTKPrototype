using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TBTK;

public class DemoUIMainControl : MonoBehaviour {
	
	public Text lbCurrency;
	
	public UIButton buttonLoadOut;
	public UIButton buttonUpgrade;
	public UIButton buttonResetProgress;
	
	
	private static DemoUIMainControl instance;
	
	void Awake(){
		instance=this;
		
		buttonLoadOut.Init();
		buttonUpgrade.Init();
		
		buttonLoadOut.button.interactable=false;
		
		buttonResetProgress.Init();
		buttonResetProgress.SetCallback(this.OnHoverResetButton, this.OnExitResetButton, null, null);
		resetWarningObj.SetActive(false);
	}
	
	void Start(){
		_OnLoadOutButton();
	}
	
	
	void Update(){
		lbCurrency.text="Credits: $"+PerkManager.GetPerkCurrency();
	}
	
	
	public GameObject resetWarningObj;
	public void OnHoverResetButton(GameObject butObj){ resetWarningObj.SetActive(true); }
	public void OnExitResetButton(GameObject butObj){ resetWarningObj.SetActive(false); }
	
	
	public void OnUpgradeButton(){ StartCoroutine(_OnUpgradeButton()); }
	public IEnumerator _OnUpgradeButton(){
		buttonLoadOut.button.interactable=true;
		buttonUpgrade.button.interactable=false;
		
		DemoUILoadOut.Hide();
		yield return new WaitForSeconds(0.25f);
		DemoUIUpgrade.Show();
	}
	public void OnLoadOutButton(){ StartCoroutine(_OnLoadOutButton()); }
	public IEnumerator _OnLoadOutButton(){
		buttonLoadOut.button.interactable=false;
		buttonUpgrade.button.interactable=true;
		
		DemoUIUpgrade.Hide();
		yield return new WaitForSeconds(0.25f);
		DemoUILoadOut.Show();
	}
	
	public void OnMainMenuButton(){
		DemoCampaign.MainMenu();
	}
	public void OnStartButton(){
		if(DemoCampaign.GetSelectedUnitCount()<=0){
			Debug.Log("No unit has been selected!");
			return;
		}
		DemoCampaign.StartBattle();
	}
	
	
	public void OnResetProgressButton(){
		DemoCampaign.ResetDemo();
		OnMainMenuButton();
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
	
}
