//##################################################################################################
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//##################################################################################################

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

//##################################################################################################
// Serializable Classes
// The collection of following classes are helpers, since Unity doesn't natively support
// serializing these to a binary formatter for saving. These all implment a From setter, to convert
// from their native Unity counterpart type, and a To getter, to convert back. In same cases, this
// process is failable and will return null.
// Note that base types (float, int, bool) are already binary serializable, so those don't need
// implementation here.
//##################################################################################################

[System.Serializable]
public class SerializableVector3 {
    float x, y, z;

    public void FromVector3(Vector3 v){
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3(){
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class SerializableQuaternion {
    float w, x, y, z;

    public void FromQuaternion(Quaternion q){
        w = q.w;
        x = q.x;
        y = q.y;
        z = q.z;
    }

    public Quaternion ToQuaternion(){
        return new Quaternion(x, y, z, w);
    }
}

[System.Serializable]
public class SerializableSpawnedPrefab {
    // This requires the serializable spawned prefabs to be in the directory /Assets/Prefabs
    private const string PREFAB_PATH = "Prefabs/";
    string assetName;

    public void FromGameObject(GameObject g){
        assetName = g.name;

        // This triggers the below error for non-prefab gameobjects when saving
        #if UNITY_EDITOR
            ToGameObject();
        #endif
    }

    // Note that this does not instantiate the prefab, simply returning the loaded asset reference.
    // This also means no state of that gameobject in-game was stored or will get loaded. That must
    // happen separately.
    public GameObject ToGameObject(){
        GameObject foundAsset = Resources.Load<GameObject>(PREFAB_PATH + assetName);

        if(!foundAsset){
            Logger.Error("Error finding asset '" + PREFAB_PATH + assetName + "' for SerializableSpawnedPrefab instantiation.");
        }

        return foundAsset;
    }
}

[System.Serializable]
public class SerializableGameObjectReference {
    string name;
    int levelIndex;

    public void FromGameObject(GameObject g){
        name = g.name;
        levelIndex = g.scene.buildIndex;
    }

    public GameObject ToGameObject(){
        LevelManagerComponent.Level owningLevel = (LevelManagerComponent.Level)(levelIndex);

        if(!LevelManagerComponent.LevelCurrentlyLoaded(owningLevel)){
            return null;
        }

        // This is an expensive operation, because it requires searching through all gameobjects in
        // all scenes. But it will find that object, if it exists.
        GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();
        for(int i = 0; i < allGameObjects.Length; ++i){
            if(allGameObjects[i].name == name && allGameObjects[i].scene.buildIndex == levelIndex){
                return allGameObjects[i];
            }
        }

        Logger.Error("Error referencing gameobject '" + name + "' (Level " + owningLevel + ") for SerializableGameObjectReference connection.");
        return null;
    }
}

//##################################################################################################
// Save
// This class contains all the data that will get saved to disk and loaded back in and restored.
// It is meant to be edited and extended for each game. Below are some simple examples of doing
// that.
//##################################################################################################
[System.Serializable]
public class Save {
    public int version = SaveLoadManagerComponent.SAVE_VERSION;

    public SerializableGameObjectReference currentRespawn = new SerializableGameObjectReference();

    // This is where you would fill out the structs/arrays of data that needs to get saved for your
    // game. For illustrative purposes, we're going to save and load the player's position.
    public SerializableGameObjectReference playerGameObject = new SerializableGameObjectReference();
    public SerializableVector3 playerPosition = new SerializableVector3();

    // This is for if any serializable defaults need to be overridden.
    public Save(){

    }
}

//##################################################################################################
// Save Load Manager Component
// This is the main class responsible for handling saves and loads of game data, and then restoring
// that data to the currently loaded scenes.
// TODO save slots and slot management?
//##################################################################################################
public class SaveLoadManagerComponent : MonoBehaviour {
    private static SaveLoadManagerComponent instance;

    // Edit this to be your games name
    private static string SAVE_NAME = "{0}_{1}.save";

    // This allows older, invalid saves to be rejected. Bump this whenever making major changes to
    // the Save class that cannot be reconciled on a load.
    public const int SAVE_VERSION = 0;

    // Set this before saving and loading to get different slots
    public static int saveSlot = 0;

    // Serialized just for viewing in editor, don't edit this
    [SerializeField]
    private Save save;

    public enum SaveApplicationMode {
        Full,
        PerLevel,
    }

    //##############################################################################################
    // Setup the instance, and create a new empty save
    //##############################################################################################
    void Start(){
        instance = this;
        save = new Save();
    }

    //##############################################################################################
    // Only in unity editor, save on F5, and load on F6.
    //##############################################################################################
    #if UNITY_EDITOR
    void Update(){
        // Quicksave/Quickload for debugging
        if(Input.GetKeyDown(KeyCode.F5)){
            Save();
        }
        if(Input.GetKeyDown(KeyCode.F6)){
            Load();
        }
    }
    #endif // UNITY_EDITOR

    //##############################################################################################
    // Meant to be called externally to A) set up a new save, and then B) immediately save that to
    // disk, making it the 'current' saved game. This stomps any existing save.
    //##############################################################################################
    public void NewGame(){
        save = new Save();
        Save();
    }

    //##############################################################################################
    // Return whether or not a valid save game exists on disk.
    //##############################################################################################
    public bool SavedGameExists(){
      Save temp = new Save();

      try {
          BinaryFormatter bf = new BinaryFormatter();
          FileStream file = File.Open(Application.persistentDataPath + GetCurrentSaveName(), FileMode.Open);
          temp = (Save) bf.Deserialize(file);
          file.Close();
      } catch {
          return false;
      }

      return temp.version == SAVE_VERSION;
    }

    //##############################################################################################
    // First, update the save with data, then write that out to disk.
    //##############################################################################################
    public void Save(){
        Logger.Info("Saving Game...");

        UpdateSave();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + GetCurrentSaveName());
        bf.Serialize(file, save);
        file.Close();
    }

    //##############################################################################################
    // Load the data from disk if possible, then apply the save if it's valid.
    //##############################################################################################
    public void Load(){
        Logger.Info("Loading Game...");

        bool fileLoaded = true;

        try {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + GetCurrentSaveName(), FileMode.Open);
            save = (Save) bf.Deserialize(file);
            file.Close();
        } catch {
            fileLoaded = false;
        }

        if(fileLoaded && save.version == SAVE_VERSION){
            ApplySave(SaveApplicationMode.Full);
            Logger.Info("Game loaded successfully");
        } else {
            if(fileLoaded){
                Logger.Error("Invalid save version " + save.version + ", cannot apply save");
            } else {
                Logger.Error("Save file not found.");
            }
        }
    }

    //##############################################################################################
    // Before writing to disk, update the save's information with all the information about the
    // world.
    //##############################################################################################
    public void UpdateSave(){
        PlayerRespawnVolumeComponent respawnComponent = PlayerRespawnVolumeComponent.GetCurrentRespawn();
        if(respawnComponent){
            save.currentRespawn.FromGameObject(respawnComponent.gameObject);
        }

        // Continuing our example, get the player's position and fill out the save data
        GameObject player = FirstPersonPlayerComponent.player.gameObject;
        save.playerGameObject.FromGameObject(player);
        save.playerPosition.FromVector3(player.transform.position);
    }

    //##############################################################################################
    // After loading a valid save, apply it, while keeping in mind what mode we're in.
    //
    // The modes are important for keeping it clear what level of restoration we want. For example,
    // if we're loading from the main menu, we want a Full load, restoring the players position
    // and respawn point.
    // However, loading a level triggers a applySave, but on in PerLevel mode, so that the save runs
    // for the objects that just got loaded, only applying operations that are for level objects.
    // For example, open doors in that level that saved that they had been opened.
    //##############################################################################################
    public void ApplySave(SaveApplicationMode mode){

        // Apply operations that happen regardless of mode go here

        if(mode == SaveApplicationMode.Full){
            // Apply operations that happen only when fully loading the game state here

            if(save.currentRespawn != null){
                GameObject loadedRespawn = save.currentRespawn.ToGameObject();

                if(loadedRespawn != null){
                    PlayerRespawnVolumeComponent.SetCurrentRespawn(loadedRespawn);
                }
            }

            // Finishing our example, get the player's position back out of the savedata, and
            // apply it. Only do this for a full load, so we don't teleport the player around every
            // time a level loads.
            GameObject player = save.playerGameObject.ToGameObject();

            if(player != null){
                player.transform.position = save.playerPosition.ToVector3();
            }

        } else if(mode == SaveApplicationMode.PerLevel){
            // Apply operations that happen only after a level is loaded go here
        }
    }

    //##############################################################################################
    // Singleton operations
    //##############################################################################################
    public static void VerifySingleton(){
        if(instance == null){
            Logger.Error("No SaveLoadManagerComponent was found in the game. Consider adding a GameObject to your scene with a SaveLoadManagerComponent on it.");
        }
    }

    public static SaveLoadManagerComponent Instance(){
        VerifySingleton();
        return instance;
    }

    //##############################################################################################
    // Replace the placeholders in the save name with the product name and save slot
    //##############################################################################################
    public static string GetCurrentSaveName(){
        string saveName = SAVE_NAME;
        saveName = saveName.Replace("{0}", Application.productName).Replace("{1}", saveSlot.ToString());

        return "/" + saveName;

    }
}
