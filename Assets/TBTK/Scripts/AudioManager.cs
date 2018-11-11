using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class AudioManager : MonoBehaviour {

		private List<AudioSource> audioSourceList=new List<AudioSource>();
		
		private static float musicVolume=.75f;
		private static float sfxVolume=.75f;
		
		public List<AudioClip> musicList;
		public bool playMusic=true;
		public bool shuffle=false;
		private int currentTrackID=0;
		private AudioSource musicSource;
		
		
		private static AudioManager instance;
		private GameObject thisObj;
		private Transform thisT;
		
		
		public static void Init(){
			if(instance!=null) return;
			GameObject obj=new GameObject();
			obj.name="AudioManager";
			obj.AddComponent<AudioManager>();
		}
		
		
		void Awake(){
			if(instance!=null){
				Destroy(gameObject);
				return;
			}
			
			instance=this;
			
			thisObj=gameObject;
			thisT=transform;
			
			DontDestroyOnLoad(thisObj);
			
			if(playMusic && musicList!=null && musicList.Count>0){
				musicSource=thisObj.AddComponent<AudioSource>();
				musicSource.loop=false;
				musicSource.playOnAwake=false;
				musicSource.volume=musicVolume;
				
				musicSource.ignoreListenerVolume=true;
			}
			
			audioSourceList=new List<AudioSource>();
			for(int i=0; i<5; i++){
				GameObject obj=new GameObject();
				obj.name="AudioSource"+(i+1);
				
				AudioSource src=obj.AddComponent<AudioSource>();
				src.playOnAwake=false;
				src.loop=false;
				
				obj.transform.parent=thisT;
				obj.transform.localPosition=Vector3.zero;
				
				audioSourceList.Add(src);
			}
			
			AudioListener.volume=sfxVolume;
		}
		
		
		void Update(){
			if(musicSource!=null && musicSource.isPlaying){
				if(shuffle) musicSource.clip=musicList[Random.Range(0, musicList.Count)];
				else{
					musicSource.clip=musicList[currentTrackID];
					currentTrackID+=1;
					if(currentTrackID==musicList.Count) currentTrackID=0;
				}
				
				musicSource.Play();
			}
		}
		
		
		void OnEnable(){
			TBTK.onGameOverE += OnGameOver;
			
			//AbilityManagerFaction.onAbilityActivatedE += OnAbilityActivated;
			//AbilityManagerUnit.onAbilityActivatedE += OnAbilityActivated;
			
			TBTK.onAbilityActivatedE += OnAbilityActivated;
		}
		
		void OnDisable(){
			TBTK.onGameOverE -= OnGameOver;
			
			//AbilityManagerFaction.onAbilityActivatedE -= OnAbilityActivated;
			//AbilityManagerUnit.onAbilityActivatedE -= OnAbilityActivated;
			
			TBTK.onAbilityActivatedE -= OnAbilityActivated;
		}
		
		
		
		void OnGameOver(int factionID){ 
			bool playerWon=FactionManager.IsPlayerFaction(factionID);
			if(playerWon && gameWonSound!=null) _PlaySound(gameWonSound);  
			else if(!playerWon && gameLostSound!=null) _PlaySound(gameLostSound);  
		}
		
		void OnAbilityActivated(){ if(abilityActivatedSound!=null) _PlaySound(abilityActivatedSound); }
		
		
		
		public AudioClip gameWonSound;
		public AudioClip gameLostSound;
		
		public AudioClip abilityActivatedSound;
		
		
		
		
		//check for the next free, unused audioObject
		private int GetUnusedAudioSourceID(){
			for(int i=0; i<audioSourceList.Count; i++){
				if(!audioSourceList[i].isPlaying) return i;
			}
			return 0;	//if everything is used up, use item number zero
		}
		
		
		//call to play a specific clip
		public static void PlaySound(AudioClip clip){ 
			if(instance==null) Init();
			instance._PlaySound(clip);
		}
		public void _PlaySound(AudioClip clip){ 
			int ID=GetUnusedAudioSourceID();
			
			audioSourceList[ID].clip=clip;
			audioSourceList[ID].Play();
		}
		
		
		
		
		
		
		public static void SetSFXVolume(float val){
			sfxVolume=val;
			AudioListener.volume=val;
		}
		
		public static void SetMusicVolume(float val){
			musicVolume=val;
			if(instance && instance.musicSource) instance.musicSource.volume=val;
		}
		
		public static float GetMusicVolume(){ return musicVolume; }
		public static float GetSFXVolume(){ return sfxVolume; }
	}




}