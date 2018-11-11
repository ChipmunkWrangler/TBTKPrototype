using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;


public class DemoUIUnitSelect : MonoBehaviour {
	
	public List<UIButton> avaiItemList=new List<UIButton>();
	public List<UIButton> selectedItemList=new List<UIButton>();
	
	
	public UIButton buttonAdd;
	public UIButton buttonRemove;
	
	
	IEnumerator Start(){
		List<Unit> availableUnitList=DemoCampaign.GetAvailableUnitList();
		for(int i=0; i<availableUnitList.Count; i++){
			if(i==0) avaiItemList[0].Init();
			else if(i>0) avaiItemList.Add(UIButton.Clone(avaiItemList[0].rootObj, "AvaiItem"+(i+1)));
			
			avaiItemList[i].imgIcon.sprite=availableUnitList[i].iconSprite;
			avaiItemList[i].label.text="$"+availableUnitList[i].value.ToString();
			avaiItemList[i].SetCallback(null, null, this.OnAvailableItem, null);
		}
		
		
		List<Unit> selectedUnitList=DemoCampaign.GetSelectedUnitList();
		for(int i=0; i<DemoCampaign.GetLoadOutUnitLimit(); i++){
			if(i==0) selectedItemList[0].Init();
			else if(i>0) selectedItemList.Add(UIButton.Clone(selectedItemList[0].rootObj, "SelectedItem"+(i+1)));
			
			if(i>=selectedUnitList.Count) selectedItemList[i].SetActive(false);
			else{
				selectedItemList[i].imgIcon.sprite=selectedUnitList[i].iconSprite;
				selectedItemList[i].label.text="$"+selectedUnitList[i].value.ToString();
				selectedItemList[i].SetActive(true);
			}
			
			selectedItemList[i].SetCallback(null, null, this.OnSelectedItem, null);
		}
		
		//UpdateAvailableContentRectSize();
		//UpdateSelectedContentRectSize();
		
		buttonAdd.Init();
		buttonRemove.Init();
		
		buttonAdd.button.interactable=true;
		buttonRemove.button.interactable=false;
		
		yield return null;
	}
	
	
	void OnEnable(){
		StartCoroutine(InitSelect());
	}
	IEnumerator InitSelect(){
		yield return null;
		ClearSelected();
		selectedID=-1;
		OnAvailableItem(avaiItemList[0].rootObj);
	}
	
	
	
	
	private enum _SelectedTab{ Available, Selected }
	private _SelectedTab selectedTab=_SelectedTab.Available;
	private int selectedID=-1;
	public void OnAvailableItem(GameObject butObj, int pointerID=-1){
		int newID=GetAvailableItemID(butObj);
		if(selectedTab==_SelectedTab.Available && selectedID==newID) return;
		
		ClearSelected();
		
		selectedTab=_SelectedTab.Available;
		selectedID=newID;
		avaiItemList[selectedID].imgHighlight.gameObject.SetActive(true);
		
		buttonAdd.button.interactable=true;
		buttonRemove.button.interactable=false;
		
		DemoUIUnitInfo.UpdateDisplay(DemoCampaign.GetAvailableUnit(selectedID));
	}
	public void OnSelectedItem(GameObject butObj, int pointerID=-1){
		int newID=GetSelectedItemID(butObj);
		if(selectedTab==_SelectedTab.Selected && selectedID==newID) return;
		
		ClearSelected();
		
		selectedTab=_SelectedTab.Selected;
		selectedID=newID;
		selectedItemList[selectedID].imgHighlight.gameObject.SetActive(true);
		
		buttonAdd.button.interactable=false;
		buttonRemove.button.interactable=true;
		
		DemoUIUnitInfo.UpdateDisplay(DemoCampaign.GetSelectedUnit(selectedID));
	}
	
	public int GetAvailableItemID(GameObject butObj){
		for(int i=0; i<avaiItemList.Count; i++){
			if(avaiItemList[i].rootObj==butObj) return i;
		}
		return 0;
	}
	public int GetSelectedItemID(GameObject butObj){
		for(int i=0; i<selectedItemList.Count; i++){
			if(selectedItemList[i].rootObj==butObj) return i;
		}
		return 0;
	}
	
	public void ClearSelected(){
		if(selectedID==-1) return;
		
		if(selectedTab==_SelectedTab.Available){
			avaiItemList[selectedID].imgHighlight.gameObject.SetActive(false);
		}
		if(selectedTab==_SelectedTab.Selected){
			selectedItemList[selectedID].imgHighlight.gameObject.SetActive(false);
		}
	}
	
	
	
	public void OnAddButton(){
		DemoCampaign.AddUnit(selectedID);
		
		UpdateSelectedDisplay();
	}
	public void OnRemoveButton(){
		DemoCampaign.RemoveUnit(selectedID);
		
		UpdateSelectedDisplay();
		
		if(DemoCampaign.GetSelectedUnitCount()==0){
			buttonRemove.button.interactable=false;
			OnAvailableItem(avaiItemList[0].rootObj);
			return;
		}
		
		if(DemoCampaign.GetSelectedUnitCount()<=selectedID){
			OnSelectedItem(selectedItemList[DemoCampaign.GetSelectedUnitCount()-1].rootObj);
		}
		else{
			selectedTab=_SelectedTab.Available;
			OnSelectedItem(selectedItemList[selectedID].rootObj);
		}
	}
	
	void UpdateSelectedDisplay(){
		List<Unit> selectedUnitList=DemoCampaign.GetSelectedUnitList();
		for(int i=0; i<selectedItemList.Count; i++){
			if(i>=selectedUnitList.Count) selectedItemList[i].SetActive(false);
			else{
				selectedItemList[i].imgIcon.sprite=selectedUnitList[i].iconSprite;
				selectedItemList[i].label.text="$"+selectedUnitList[i].value.ToString();
				selectedItemList[i].SetActive(true);
			}
		}
		
		//UpdateSelectedContentRectSize();
	}
	
	
	//no longer in use
	[HideInInspector] public GridLayoutGroup layoutAvai;
	private void UpdateAvailableContentRectSize(){
		int avaiCount=DemoCampaign.GetAvailableUnitCount();
		int rowCount=(int)Mathf.Ceil(avaiCount/(float)layoutAvai.constraintCount);
		float size=rowCount*layoutAvai.cellSize.y+rowCount*layoutAvai.spacing.y+layoutAvai.padding.top;
		
		RectTransform contentRect=layoutAvai.gameObject.GetComponent<RectTransform>();
		contentRect.sizeDelta=new Vector2(contentRect.sizeDelta.x, size);
	}
	[HideInInspector] public GridLayoutGroup layoutSelected;
	private void UpdateSelectedContentRectSize(){
		int selectedCount=DemoCampaign.GetSelectedUnitCount();
		int rowCount=(int)Mathf.Ceil(selectedCount/(float)layoutSelected.constraintCount);
		float size=rowCount*layoutSelected.cellSize.y+rowCount*layoutSelected.spacing.y+layoutSelected.padding.top;
		
		RectTransform contentRect=layoutSelected.gameObject.GetComponent<RectTransform>();
		contentRect.sizeDelta=new Vector2(contentRect.sizeDelta.x, size);
	}
	
}
