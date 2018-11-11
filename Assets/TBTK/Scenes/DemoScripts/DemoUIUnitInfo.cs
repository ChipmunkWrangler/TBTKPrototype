using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

public class DemoUIUnitInfo : MonoBehaviour {

	//private GameObject thisObj;
	//private RectTransform rectT;
	private static DemoUIUnitInfo instance;
	
	public Text lbName;
	public Text lbDesp;
	
	public Transform statsListParent;
	public List<UIButton> statsItemList=new List<UIButton>();
	
	public List<UIButton> abilityItemList=new List<UIButton>();
	public List<UIButton> effectItemList=new List<UIButton>();
	
	public GameObject abilityTooltipObj;
	public Text lbAbilityName;
	public Text lbAbilityDesp;
	public Text lbAbilityCost;
	public Text lbAbilityCooldown;
	public Text lbAbilityUseCount;
	
	public UIButton buttonClose;
	
	public Tile tile;
	
	public void Awake(){
		instance=this;
		//thisObj=gameObject;
		//rectT=thisObj.GetComponent<RectTransform>();
		
		
		AbilityManagerUnit abManagerUnit = (AbilityManagerUnit)FindObjectOfType(typeof(AbilityManagerUnit));
		if(abManagerUnit!=null) abManagerUnit.Init();
		
		InitiateElement();
		
		abilityTooltipObj.SetActive(false);
	}
	
	
	void InitiateElement(){
		for(int i=0; i<10; i++){
			GameObject obj=statsListParent.GetChild(i).gameObject;
			statsItemList.Add(new UIButton(obj));
			
			if(i==0) statsItemList[i].label.text="Hit-Point:";
			else if(i==1) statsItemList[i].label.text="Action-Point:";
			else if(i==2) statsItemList[i].label.text="Damage:";
			else if(i==3) statsItemList[i].label.text="Hit Chance:";
			else if(i==4) statsItemList[i].label.text="Critical Chance:";
			
			else if(i==5) statsItemList[i].label.text="Move Range:";
			else if(i==6) statsItemList[i].label.text="Attack Range:";
			else if(i==7) statsItemList[i].label.text="Move Priority:";
			else if(i==8) statsItemList[i].label.text="Dodge Chance:";
			else if(i==9) statsItemList[i].label.text="Critical Avoidance:";
		}
		
		for(int i=0; i<6; i++){
			if(i==0) abilityItemList[0].Init();
			else if(i>0) abilityItemList.Add(UIButton.Clone(abilityItemList[0].rootObj, "AbilityItem"+(i+1)));
			abilityItemList[i].SetCallback(this.OnHoverAbilityItem, this.OnExitAbilityItem, null, null);
			abilityItemList[i].SetActive(false);
		}
	}
	
	
	
	void Update(){
		if(selectedUnit!=null) selectedUnit.transform.Rotate(Vector3.up * 10 * Time.deltaTime);
	}
	
	
	
	public void OnHoverAbilityItem(GameObject butObj){
		int ID=GetAbilityItemID(butObj);
		
		Ability ability=selectedUnit.GetUnitAbility(ID);
		
		lbAbilityName.text=ability.name;
		lbAbilityDesp.text=ability.desp;
		lbAbilityCost.text="Cost: "+ability.cost+"AP";
		lbAbilityCooldown.text="Cooldown: "+ability.cooldown;
		lbAbilityUseCount.text="UseCount: "+(ability.useLimit>0 ? ability.useLimit.ToString() : "∞");
		
		abilityTooltipObj.transform.localPosition=new Vector3(ID*50, 0, 0);
		
		abilityTooltipObj.SetActive(true);
	}
	public void OnExitAbilityItem(GameObject butObj){
		abilityTooltipObj.SetActive(false);
	}
	private int GetAbilityItemID(GameObject butObj){
		for(int i=0; i<abilityItemList.Count; i++){
			if(abilityItemList[i].rootObj==butObj) return i;
		}
		return 0;
	}
	
	
	
	private Unit selectedUnit;
	public static void UpdateDisplay(Unit unit){ instance._UpdateDisplay(unit); }
	public void _UpdateDisplay(Unit unit){
		if(unit==null){
			selectedUnit=null;
			
			lbName.text="";
			lbDesp.text="";
			
			statsItemList[0].labelAlt.text="-";
			statsItemList[1].labelAlt.text="-";
			statsItemList[2].labelAlt.text="-";
			statsItemList[3].labelAlt.text="-";
			statsItemList[4].labelAlt.text="-";
			
			statsItemList[5].labelAlt.text="-";
			statsItemList[6].labelAlt.text="-";
			statsItemList[7].labelAlt.text="-";
			statsItemList[8].labelAlt.text="-";
			statsItemList[9].labelAlt.text="-";
			
			for(int i=0; i<abilityItemList.Count; i++) abilityItemList[i].SetActive(false);
		
		}
		else{
			selectedUnit=CreateUnitInstance(unit);
			
			lbName.text=unit.name;
			lbDesp.text=unit.desp;
			
			statsItemList[0].labelAlt.text=selectedUnit.HP+"/"+selectedUnit.GetFullHP();//"Hit-Point:";
			statsItemList[1].labelAlt.text=selectedUnit.AP+"/"+selectedUnit.GetFullAP();;//"Action-Point:";
			statsItemList[2].labelAlt.text=selectedUnit.GetDamageMin()+"-"+selectedUnit.GetDamageMax();//"Damage:";
			statsItemList[3].labelAlt.text=(selectedUnit.GetHitChance()*100).ToString("f0")+"%";//"Hit Chance:";
			statsItemList[4].labelAlt.text=(selectedUnit.GetCritChance()*100).ToString("f0")+"%";//"Critical Chance:";
			
			statsItemList[5].labelAlt.text=selectedUnit.GetMoveRange().ToString("f0");//"Move Range:";
			statsItemList[6].labelAlt.text=selectedUnit.GetAttackRange().ToString("f0");//"Attack Range:";
			statsItemList[7].labelAlt.text=selectedUnit.GetTurnPriority().ToString("f0");//"Move Priority:";
			statsItemList[8].labelAlt.text=(selectedUnit.GetDodgeChance()*100).ToString("f0")+"%";//"Dodge Chance:";
			statsItemList[9].labelAlt.text=(selectedUnit.GetCritAvoidance()*100).ToString("f0")+"%";//"Critical Avoidance:";
			
			for(int i=0; i<abilityItemList.Count; i++){
				if(i<selectedUnit.abilityList.Count){
					abilityItemList[i].imgIcon.sprite=selectedUnit.abilityList[i].icon;
					abilityItemList[i].label.text="";//selectedUnit.abilityList[i].useLimit.ToString();
					abilityItemList[i].SetActive(true);
				}
				else abilityItemList[i].SetActive(false);
			}
		}
	}
	
	Unit CreateUnitInstance(Unit unit){
		if(selectedUnit!=null) Destroy(selectedUnit.gameObject);
		
		GameObject unitObj=(GameObject)Instantiate(unit.gameObject);
		Unit unitInstance=unitObj.GetComponent<Unit>();
		unitInstance.tile=tile;
		unitInstance.transform.position=tile.GetPos();
		unitInstance.transform.rotation=Quaternion.Euler(0, -25, 0);
		
		unitInstance.InitUnitAbility();
		
		return unitInstance;
	}
	
	void OnDisable(){
		if(selectedUnit!=null) Destroy(selectedUnit.gameObject);
		if(tile!=null) tile.gameObject.SetActive(false);
	}
	void OnEnable(){
		if(tile!=null) tile.gameObject.SetActive(true);
	}
	
}
