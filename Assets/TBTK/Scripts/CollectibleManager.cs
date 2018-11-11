using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using TBTK;

namespace TBTK{

	public class CollectibleManager : MonoBehaviour {
		
		public bool generateCollectibleOnStart=false;
		public int activeItemLimit=4;
		
		public bool generateInGame=false;
		public int spawnPerTurn=2;
		public float spawnChance=0.5f;
		
		public GameObject spawnEffect;
		public bool autoDestroySpawnEffect=true;
		public float spawnEffectDuration=2f;
		
		public List<int> unavailableIDList=new List<int>();
		private List<Collectible> itemList=new List<Collectible>();
		
		public List<Collectible> activeItemList=new List<Collectible>();
		
		private static CollectibleManager instance;
		public static CollectibleManager SetInstance(){ 	//called in Inspector Editor
			if(instance==null) instance=(CollectibleManager)FindObjectOfType(typeof(CollectibleManager));
			return instance;
		}
		public static CollectibleManager GetInstance(){ return instance; }
		
		void Awake(){
			
		}
		
		public void Init(){
			if(instance==null) instance=this;
			
			InitItem();
			
			if(generateCollectibleOnStart) GenerateCollectible();
			else{
				for(int i=0; i<activeItemList.Count; i++){
					if(activeItemList[i]==null){
						activeItemList.RemoveAt(i); i-=1;
					}
				}
			}
			
			if(spawnEffect!=null) ObjectPoolManager.New(spawnEffect);
		}
		
		
		public void InitItem(){
			itemList=new List<Collectible>();
			List<Collectible> dbList=CollectibleDB.Load();
			for(int i=0; i<dbList.Count; i++){
				if(!unavailableIDList.Contains(dbList[i].prefabID)){
					itemList.Add(dbList[i]);
				}
			}
		}
		
		
		public static void TriggerCollectible(Collectible item){
			instance.activeItemList.Remove(item);
		}
		
		
		public static float NewTurn(){ return instance._NewTurn(); }
		public float _NewTurn(){
			if(!generateInGame) return 0;
			if(activeItemList.Count>=activeItemLimit) return 0;
			
			bool spawned=false;
			for(int i=0; i<spawnPerTurn; i++){
				if(activeItemList.Count>=activeItemLimit) break;
				if(Random.value>spawnChance) continue;
				
				Tile tile=GetRandomTile();
				if(tile==null) continue;
				
				int rand=Random.Range(0, itemList.Count);
				GameObject itemObj=(GameObject)Instantiate(itemList[rand].gameObject);
				
				PlaceItemAtTile(itemObj, tile);
				
				spawned=true;
				
				if(spawnEffect!=null){
					if(!autoDestroySpawnEffect) ObjectPoolManager.Spawn(spawnEffect, tile.GetPos(), Quaternion.identity);
					else ObjectPoolManager.Spawn(spawnEffect, tile.GetPos(), Quaternion.identity, spawnEffectDuration);
				}
			}
			
			GameControl.DisplayMessage("New Collectible!");
			
			return spawned ? 2 : 0;
		}
		
		
		
		public static void GenerateCollectible(){
			if(instance==null) SetInstance();
			instance._GenerateCollectible();
		}
		public void _GenerateCollectible(){
			_ClearAllActiveItem();
			
			InitItem();
			
			int itemCount=Random.Range(activeItemLimit/2, activeItemLimit+1);
			int iterateCount=0;
			while(itemCount>0){
				iterateCount+=1;
				if(iterateCount>10) break;
				
				Tile tile=GetRandomTile();
				if(tile==null) break;
				
				int rand=Random.Range(0, itemList.Count);
				#if UNITY_EDITOR
					GameObject itemObj=(GameObject)PrefabUtility.InstantiatePrefab(itemList[rand].gameObject);
				#else
					GameObject itemObj=(GameObject)MonoBehaviour.Instantiate(itemList[rand].gameObject);
				#endif
				
				PlaceItemAtTile(itemObj, tile);
				
				itemCount-=1;
			}
		}
		
		
		public static void ClearAllActiveItem(){
			if(instance==null) SetInstance();
			instance._ClearAllActiveItem();
		}
		public void _ClearAllActiveItem(){
			for(int i=0; i<activeItemList.Count; i++){
				if(activeItemList[i]!=null) DestroyImmediate(activeItemList[i].gameObject);
			}
			
			activeItemList=new List<Collectible>();
		}
		
		
		
		
		private Tile GetRandomTile(){
			Tile tile=null;
			
			List<Tile> tileList=GridManager.GetTileList();
			
			int iterateCount=0;
			while(true){
				iterateCount+=1;
				if(iterateCount>10) break;
				
				tile=tileList[Random.Range(0, tileList.Count)];
				if(!tile.walkable) continue;
				if(tile.unit!=null) continue;
				if(tile.collectible!=null) continue;
				
				break;
			}
			
			return tile;
		}
		public void PlaceItemAtTile(GameObject itemObj, Tile tile){
			float rotUnit=tile.type==_TileType.Hex ? 60 : 90 ;
			Quaternion rotation=Quaternion.Euler(0, Random.Range(0, 6)*rotUnit, 0);
			
			itemObj.transform.position=tile.GetPos();
			itemObj.transform.rotation=rotation;
			itemObj.transform.parent=tile.transform;
			
			Collectible collectible=itemObj.GetComponent<Collectible>();
			tile.collectible=collectible;
			activeItemList.Add(collectible);
		}
		
		//called from editor to remove item
		public void RemoveItem(Collectible item){
			activeItemList.Remove(item);
		}
	}
	
}
