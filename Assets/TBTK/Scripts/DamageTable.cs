using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {
	
	
	[System.Serializable]
	public class DAType {
		public int ID=0;
		public string name="";
		public Sprite icon;
		
		public string desp="";
	}
	[System.Serializable]
	public class DamageType : DAType{
		
	}
	[System.Serializable]
	public class ArmorType : DAType{
		public List<float> modifiers=new List<float>();
	}
	
	
	public class DamageTable : MonoBehaviour {

		private static List<ArmorType> armorTypeList=new List<ArmorType>();
		private static List<DamageType> damageTypeList=new List<DamageType>();
		
		public static List<DamageType> GetAllDamageType(){ return damageTypeList; }
		public static List<ArmorType> GetAllArmorType(){ return armorTypeList; }
		
		
		// Use this for initialization
		void Awake() {
			LoadPrefab();
		}
		
		private static void LoadPrefab(){
			DamageTableDB prefab=DamageTableDB.LoadDB();
			
			armorTypeList=prefab.armorTypeList;
			damageTypeList=prefab.damageTypeList;
		}
		

		public static float GetModifier(int armorID=0, int dmgID=0){
			armorID=Mathf.Max(0, armorID);
			dmgID=Mathf.Max(0, dmgID);
			
			if(armorID<armorTypeList.Count && dmgID<damageTypeList.Count){
				return armorTypeList[armorID].modifiers[dmgID];
			}
			else{
				return 1f;
			}
		}
		
		public static ArmorType GetArmorTypeInfo(int ID){
			if(ID<0 || ID>=armorTypeList.Count){
				Debug.Log("ArmorType requested does not exist");
				return null;
			}
			return armorTypeList[ID];
		}
		
		public static DamageType GetDamageTypeInfo(int ID){
			if(ID<0 || ID>=damageTypeList.Count){
				Debug.Log("DamageType requested does not exist");
				return null;
			}
			return damageTypeList[ID];
		}
		
	}

}

