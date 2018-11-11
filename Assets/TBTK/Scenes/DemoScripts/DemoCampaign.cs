using UnityEngine;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using System.Collections.Generic;

using TBTK;

public class DemoCampaign : MonoBehaviour {
	
	public int startingCurrency=50;
	
	public int loadOutUnitLimit=10;
	public static int GetLoadOutUnitLimit(){ return instance.loadOutUnitLimit; }
	
	List<Unit> availableUnitList=new List<Unit>();	//the unit available to unit to be added to the starting lineup (once loaded, they stay the same)
	List<Unit> selectedUnitList=new List<Unit>();		//the unit selected for the starting lineup	(player can add/remove unit to/from this list)
	List<int> selectedUnitMapList=new List<int>();	//the index of each selected unit mapped to availableUnitList, this is for saving the selected unit list as int
	
	public static List<Unit> GetAvailableUnitList(){ return instance.availableUnitList; }
	public static List<Unit> GetSelectedUnitList(){ return instance.selectedUnitList; }
	
	public static int GetAvailableUnitCount(){ return instance.availableUnitList.Count; }
	public static int GetSelectedUnitCount(){ return instance.selectedUnitList.Count; }
	
	public static Unit GetAvailableUnit(int index){ return instance.availableUnitList[index]; }
	public static Unit GetSelectedUnit(int index){ return instance.selectedUnitList[index]; }
	
	private static DemoCampaign instance;
	
	void Awake(){
		instance=this;
		
		PerkManager perkManager = (PerkManager)FindObjectOfType(typeof(PerkManager));
		if(perkManager!=null) perkManager.Init();
		
		AbilityManagerUnit abManagerUnit = (AbilityManagerUnit)FindObjectOfType(typeof(AbilityManagerUnit));
		if(abManagerUnit!=null) abManagerUnit.Init();
		
		
		availableUnitList=new List<Unit>( UnitDB.Load() );	//load the unit from DB, you manually assign this if you want
		
		//since we are loading from DB, we dont want the AI unit, which started from 6 (so we remove them)
		while(availableUnitList.Count>9) availableUnitList.RemoveAt(availableUnitList.Count-1);
		
		//if this there isn't any save
		if(!PlayerPrefs.HasKey("TBTK_Demo")){
			//for demo purpose, we are using perk currency as the universal currency
			//so we set the perk currency to our starting value and let PerkManager keep track of it
			PerkManager.SetPerkCurrency(startingCurrency);
			
			PlayerPrefs.SetInt("TBTK_Demo", 1);
		}
		else{
			//check with data to see if the scene is loaded from a battle 
			if(TBData.BattleEndDataExist()){
				List<TBDataUnit> dataList=TBData.GetEndData(0);	//get the data, the ID is zero, since we start the battle with 0
																						//note that the faction in the battle scene is configured to load data from 0 too
				
				if(dataList!=null){	//add the unit based on the dataList
					for(int i=0; i<dataList.Count; i++) selectedUnitList.Add(dataList[i].unit);
				}
				
				for(int i=0; i<selectedUnitList.Count; i++){		//fill up the availableUnitMapList (for saving)
					bool match=false;
					for(int n=0; n<availableUnitList.Count; n++){		//find the corresponding unit in availableList and add the index to selectedUnitMapList
						if(selectedUnitList[i]==availableUnitList[n]){
							selectedUnitMapList.Add(n);
							match=true;
							break;
						}
					}
					//if there's no match in availableUnitList, unit no longer available in game, remove it from selected list
					if(!match){
						selectedUnitList.RemoveAt(i);
						i-=1;
					}
				}
				
				_SaveLoadOut();
			}
			else{	//if we are not loading the scene from a battle, load the selectedUnitList from previous save in stead of getting it from data
				_LoadLoadOut();
			}
		}
	}
	
	
	
	//add unit to selectedUnitList
	public static void AddUnit(int index){
		if(instance.selectedUnitList.Count>=instance.loadOutUnitLimit){
			UIMessage.DisplayMessage("Unit Capacity Reached");
			return;
		}
		
		//using perk currency as the main currency for the demo, feel free to use your own
		int cost=instance.availableUnitList[index].value;
		if(PerkManager.GetPerkCurrency()<cost){
			UIMessage.DisplayMessage("Insufficient Credits");
			return;
		}
		
		PerkManager.SpendCurrency(cost);
		
		instance.selectedUnitList.Add(instance.availableUnitList[index]);
		instance.selectedUnitMapList.Add(index);
		
		SaveLoadOut();
	}
	//remove unit from selectedUnitList
	public static void RemoveUnit(int index){
		PerkManager.GainCurrency(instance.selectedUnitList[index].value);
		instance.selectedUnitList.RemoveAt(index);
		instance.selectedUnitMapList.RemoveAt(index);
		
		SaveLoadOut();
	}
	
	
	//save what we have in the selectedUnitList
	//in this instance, we will just save the selectedUnitMapList
	//we can use that information to fillup selectedUnitList again given we always know what is in availableUnitList
	public static void SaveLoadOut(){ instance._SaveLoadOut(); }
	public void _SaveLoadOut(){
		PlayerPrefs.SetInt("TBTK_LoadOut_Count", instance.selectedUnitList.Count);
		for(int i=0; i<instance.selectedUnitList.Count; i++)
			PlayerPrefs.SetInt("TBTK_LoadOut_"+i, instance.selectedUnitMapList[i]);
	}
	//load the selectedUnitMapList and filled up selectedUnitList based on the info loaded
	public void _LoadLoadOut(){
		int count=PlayerPrefs.GetInt("TBTK_LoadOut_Count");
		for(int i=0; i<count; i++){
			int index=PlayerPrefs.GetInt("TBTK_LoadOut_"+i, -1);
			if(index>=0){
				selectedUnitMapList.Add(index);
				selectedUnitList.Add(availableUnitList[index]);
			}
		}
	}
	//Delete all saved data
	public static void DeleteLoadOut(){ instance._DeleteLoadOut(); }
	public void _DeleteLoadOut(){
		for(int i=0; i<loadOutUnitLimit; i++) PlayerPrefs.DeleteKey("TBTK_LoadOut_"+i);
		PlayerPrefs.DeleteKey("TBTK_LoadOut_Count");
	}
	
	
	//reset demo, clear all saved data
	public static void ResetDemo(){
		DeleteLoadOut();
		PlayerPrefs.DeleteKey("TBTK_Demo");
		PerkManager.ClearPerkProgress();
	}
	
	
	//start battle
	public static void StartBattle(){
		//create a data list and fill it up with selectedUnitList
		List<TBDataUnit> dataList=new List<TBDataUnit>();
		for(int i=0; i<instance.selectedUnitList.Count; i++){
			dataList.Add(new TBDataUnit(instance.selectedUnitList[i]));
		}
		//set the data list as the start data, use 0 as the data ID, the faction in demo has been set to load data with ID-0
		TBData.SetStartData(0, dataList);
		
		//load the battle scene
		LoadLevel("DemoCampaignBattle");
		//LoadLevel("Battle_Scene_"+Random.Range(0, 1000).ToString());
	}
	
	
	public static void MainMenu(){
		LoadLevel("DemoMenu");
	}
	
	
	public static void LoadLevel(string sceneName){
		#if UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(sceneName);
		#else
			Application.LoadLevel(sceneName);
		#endif
	}
	
}
