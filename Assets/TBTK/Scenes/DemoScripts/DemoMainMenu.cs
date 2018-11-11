using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TBTK;

public class DemoMainMenu : MonoBehaviour {
	
	private string disclaimer="";
	
	public Text lbTooltip;
	
	public UIButton buttonCampaign;
	public UIButton buttonSimple;
	public UIButton buttonSimpleJRPG;
	public UIButton buttonFullFeature;
	
	[Space(15)]
	
	public Image imgPreview;
	private GameObject imgPreviewObj;
	
	public Sprite imgPreviewCampaign;
	public Sprite imgPreviewSimple;
	public Sprite imgPreviewJRPG;
	public Sprite imgPreviewFull;
	
	
	// Use this for initialization
	void Start () {
		lbTooltip.text="";
		lbTooltip.enabled=true;
		
		buttonCampaign.Init();
		buttonCampaign.SetCallback(this.OnHoverCampaign, this.OnExitButton, this.OnCampaignButton, null);
		
		buttonSimple.Init();
		buttonSimple.SetCallback(this.OnHoverSimple, this.OnExitButton, this.OnSimpleButton, null);
		
		buttonSimpleJRPG.Init();
		buttonSimpleJRPG.SetCallback(this.OnHoverJRPG, this.OnExitButton, this.OnJRPGButton, null);
		
		buttonFullFeature.Init();
		buttonFullFeature.SetCallback(this.OnHoverFull, this.OnExitButton, this.OnFullFeatureButton, null);
		
		disclaimer="\n\nThe goal of the demo is to showcase the potential of the framework.";
		disclaimer+="Please note that due to the difference of the setting used in each scene in the demo, not all units, abilities, perks design will make sense in the context of each individual scene.\n";
		disclaimer+="On top of that, the difficulty balance for each scene is not guaranteed.";
		
		disclaimer="";
		
		
		imgPreviewObj=imgPreview.gameObject;
		imgPreviewObj.SetActive(false);
	}
	
	
	
	
	public void OnHoverCampaign(GameObject butObj){
		string text="A repeating level to demonstrate the toolkit built in system that supports game progression. ";
		text+="You will be able to choose your starting lineup as well as purchasing upgrade before battle. ";
		text+="The purchased upgrades and surviving unit of a previous battle will be carried forth to next battle.";
		
		lbTooltip.text=text+disclaimer;
		
		imgPreview.sprite=imgPreviewCampaign;
		imgPreviewObj.SetActive(true);
	}
	public void OnHoverSimple(GameObject butObj){
		string text="A relatively small and simple self-contained level.";
		text+="All units in this level (both player's and AI) are procedurally generated upon loading of the level.";
		text+="Any perk progress (purchase/unlock) made in the level will be lost upon exiting the level";
		
		lbTooltip.text=text+disclaimer;
		
		imgPreview.sprite=imgPreviewSimple;
		imgPreviewObj.SetActive(true);
	}
	public void OnHoverJRPG(GameObject butObj){
		string text="A level setup to emulate a classic J-RPG stype turn based combat";
		text+="All units in this level (both player's and AI) are procedurally generated upon loading of the level.";
		
		lbTooltip.text=text+disclaimer;
		
		imgPreview.sprite=imgPreviewJRPG;
		imgPreviewObj.SetActive(true);
	}
	public void OnHoverFull(GameObject butObj){
		string text="A full featured self-contained level with all the advance gameplay mechanic such as cover system and fog-of-war enabled. ";
		text+="AI unit are procedurally generated in this level.";
		
		lbTooltip.text=text+disclaimer;
		
		imgPreview.sprite=imgPreviewFull;
		imgPreviewObj.SetActive(true);
	}
	
	public void OnExitButton(GameObject butObj){
		lbTooltip.text="";
		imgPreviewObj.SetActive(false);
	}
	
	
	public void OnCampaignButton(GameObject butObj, int pointerID=-1){
		DemoCampaign.LoadLevel("DemoCampaignMenu");
	}
	public void OnSimpleButton(GameObject butObj, int pointerID=-1){
		DemoCampaign.LoadLevel("DemoSimple");
	}
	public void OnJRPGButton(GameObject butObj, int pointerID=-1){
		DemoCampaign.LoadLevel("DemoJRPG");
	}
	public void OnFullFeatureButton(GameObject butObj, int pointerID=-1){
		DemoCampaign.LoadLevel("DemoFullFeature");
	}
	
	
}
