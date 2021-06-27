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
// Save
// This class contains all the data that will get saved to disk and loaded back in and restored.
// It is meant to be edited and extended for each game. Below are some simple examples of doing
// that.
//##################################################################################################
[System.Serializable]
public class Save {
    public int version = SaveLoadManagerComponent.SAVE_VERSION;

    // This is where you would fill out the data that needs to get saved for your
    // game. For illustrative purposes, we're going to save and load the player, the player's
    // position, and the current respawn point point.
    public string playerGameObject = string.Empty;
    public string playerPosition = string.Empty;
    public string currentRespawn = string.Empty;

    // This is for if defaults that need to be overridden.
    public Save(){

    }
}

//##################################################################################################
// Save Load Manager Component
// This is the main class responsible for handling saves and loads of game data, and then restoring
// that data to the currently loaded scenes.
//##################################################################################################
public class SaveLoadManagerComponent : MonoBehaviour {
    private static SaveLoadManagerComponent instance;

    // Edit this to be your games name
    private static string SAVE_NAME = "{0}_{1}.save";

    // This allows older, invalid saves to be rejected. Bump this whenever making major changes to
    // the Save class that cannot be reconciled on a load.
    public const int SAVE_VERSION = 0;

    // Set this before saving and loading to use different slots
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
        // Continuing the example save data, get the player's position and fill out the save data
        GameObject player = FirstPersonPlayerComponent.player.gameObject;
        save.playerGameObject = FromSceneGameObject(player);
        save.playerPosition = FromVector3(player.transform.position);

        PlayerRespawnVolumeComponent respawnComponent = PlayerRespawnVolumeComponent.GetCurrentRespawn();
        if(respawnComponent){
            save.currentRespawn = FromSceneGameObject(respawnComponent.gameObject);
        }
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
        Logger.Info("=== Applying Save " + mode + " ===");

        // Apply operations that happen regardless of mode go here

        if(mode == SaveApplicationMode.Full){
            // Apply operations that happen only when fully loading the game state here

            GameObject loadedRespawn = ToSceneGameObject(save.currentRespawn);

            if(loadedRespawn != null){
                PlayerRespawnVolumeComponent.SetCurrentRespawn(loadedRespawn);
            }

            // Finishing our example, get the player's position back out of the savedata, and
            // apply it. Only do this for a full load, so we don't teleport the player around every
            // time a level loads.
            GameObject player = ToSceneGameObject(save.playerGameObject);

            if(player != null){
                player.transform.position = ToVector3(save.playerPosition);
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

    //##############################################################################################
    // Static serialization methods
    // https://forum.unity.com/threads/unity-binaryformatter-deserialization-problem.16281/
    // Because of the serialization issue described above, where deserialization of binary formatted
    // objects will fail (silently, thanks unity) if they are unrecognized types name-defined in
    // assembly, this class uses static, defined serializations to and from strings. Since strings
    // are built-in types, they are safe from the above issues.
    //
    // This does use a LOT of string operations, and is neither the most performant nor the most
    // space effecient. Could probably work on that.
    //
    // When you are saving and loading this to disk, simply contain the resulting strings in
    // variables or an array in the Save object, which will get serialized in turn in and out.
    //
    // Since this process is reversable and kind of important, it also includes unit testing, below.
    // This is triggerable from a context menu on a component instance.
    //##############################################################################################

    //##############################################################################################
    // Unit Tests
    //##############################################################################################
    #if UNITY_EDITOR
    [ContextMenu("Run Serialization Unit Tests")]
    public void UnitTestSerialization(){
        bool allTestsPassed = true;

        Vector2 originalVector2 = new Vector2(68.0f, 0.58858f);
        string serializedVector2 = FromVector2(originalVector2);
        Vector2 deserializedVector2 = ToVector2(serializedVector2);

        if(Vector2.Distance(originalVector2, deserializedVector2) > 0.01f){
            allTestsPassed = false;
            Debug.LogError("[SaveLoadManagerComponent Unit Test] Vector2 Serialization/Deserialization failed");
            Debug.Log(Vector2.Distance(originalVector2, deserializedVector2));
        }

        Vector3 originalVector3 = new Vector3(1.0f, 3.141592654f, 32453.324654f);
        string serializedVector3 = FromVector3(originalVector3);
        Vector3 deserializedVector3 = ToVector3(serializedVector3);

        if(Vector3.Distance(originalVector3, deserializedVector3) > 0.01f){
            allTestsPassed = false;
            Debug.LogError("[SaveLoadManagerComponent Unit Test] Vector3 Serialization/Deserialization failed");
            Debug.Log(Vector3.Distance(originalVector3, deserializedVector3));
        }

        Quaternion originalQuaternion = new Quaternion(0.462f, 0.191f, 0.462f, 0.733f);
        string serializedQuaternion = FromQuaternion(originalQuaternion);
        Quaternion deserializedQuaternion = ToQuaternion(serializedQuaternion);

        if(deserializedQuaternion.x != 0.462f){
            allTestsPassed = false;
            Debug.LogError("[SaveLoadManagerComponent Unit Test] Quaternion Serialization/Deserialization failed");
            Debug.Log(deserializedQuaternion);
        }

        GameObject originalPlayerPrefab = GameObject.Find("Player");
        string serializedPlayerPrefab = FromPrefabGameObject(originalPlayerPrefab);
        GameObject deserializedPlayerPrefab = ToPrefabGameObject(serializedPlayerPrefab);

        if(deserializedPlayerPrefab == null){
            allTestsPassed = false;
            Debug.LogError("[SaveLoadManagerComponent Unit Test] Player Prefab Serialization/Deserialization failed");
        }

        // Since ToSceneGameObject calls LevelCurrentlyLoaded, this only passes during runtime
        string serializedPlayerSceneGameObject = FromSceneGameObject(originalPlayerPrefab);
        GameObject deserializedPlayerSceneGameObject = ToSceneGameObject(serializedPlayerSceneGameObject);

        if(deserializedPlayerSceneGameObject == null){
            allTestsPassed = false;
            Debug.LogError("[SaveLoadManagerComponent Unit Test] Player Scene GameObject Serialization/Deserialization failed");
        }

        if(allTestsPassed){
            Debug.Log("All SaveLoad Serialization Unit Tests Passed!");
        }
    }
    #endif

    //##############################################################################################
    // Vectors and Quaternions
    //##############################################################################################
    private const char   DELIMETER_C = ',';
    private const string DELIMETER_S = ",";

    static string FromVector2(Vector2 v){
        return v.x.ToString() + DELIMETER_C + v.y.ToString();
    }

    static Vector2 ToVector2(string s){
        try {
            string[] tokens = s.Split(DELIMETER_C);
            return new Vector2(
                float.Parse(tokens[0]),
                float.Parse(tokens[1])
            );
        } catch {
            Logger.Error("SaveLoadManagerComponent could not parse Vector2 '" + s + "'");
            return new Vector2();
        }
    }

    static string FromVector3(Vector3 v){
        return v.x.ToString() + DELIMETER_C + v.y.ToString() + DELIMETER_C + v.z.ToString();
    }

    static Vector3 ToVector3(string s){
        try {
            string[] tokens = s.Split(DELIMETER_C);
            return new Vector3(
                float.Parse(tokens[0]),
                float.Parse(tokens[1]),
                float.Parse(tokens[2])
            );
        } catch {
            Logger.Error("SaveLoadManagerComponent could not parse Vector3 '" + s + "'");
            return new Vector3();
        }
    }

    static string FromQuaternion(Quaternion q){
        return q.x.ToString() + DELIMETER_C + q.y.ToString() + DELIMETER_C + q.z.ToString() + DELIMETER_C + q.w.ToString();
    }

    static Quaternion ToQuaternion(string s){
        try {
            string[] tokens = s.Split(DELIMETER_C);
            return new Quaternion(
                float.Parse(tokens[0]),
                float.Parse(tokens[1]),
                float.Parse(tokens[2]),
                float.Parse(tokens[3])
            );
        } catch {
            Logger.Error("SaveLoadManagerComponent could not parse Quaternion '" + s + "'");
            return new Quaternion();
        }
    }

    //##############################################################################################
    // Prefab GameObject
    // This requires the spawned prefabs to be in the directory /Assets/Resources/Prefabs and not
    // in a subdirectory. This can be changed below.
    // See https://docs.unity3d.com/ScriptReference/Resources.Load.html for more details.
    //##############################################################################################
    private const string PREFAB_PATH = "Prefabs/";

    static string FromPrefabGameObject(GameObject g){
        return PREFAB_PATH + g.name;
    }

    static GameObject ToPrefabGameObject(string s){
        GameObject foundAsset = s.Contains(PREFAB_PATH) ? Resources.Load<GameObject>(s) : null;

        if(foundAsset == null){
            Logger.Error("Error finding asset '" + s + "' for ToPrefabGameObject serialization.");
        }

        return foundAsset;
    }

    //##############################################################################################
    // GameObject in Scene
    // This makes the assumption that the saved object is the only one named that in the given
    // scene. Unity doesn't enforce that, but this will deliberately fail with an error if that's
    // the case.
    //##############################################################################################
    static string FromSceneGameObject(GameObject g){
        #if UNITY_EDITOR
            if(g.name.Contains(DELIMETER_S)){
                Logger.Error("GameObject '" + g.name + "' name cannot contain the reserved delimeter '" + DELIMETER_S + "' for FromSceneGameObject serialization");
                return null;
            }

            int countOfGameObjectsWithName = 0;
            GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();
            foreach(GameObject other in allGameObjects){
                if(other != g && other.name == g.name){
                    countOfGameObjectsWithName++;
                }
            }

            if(countOfGameObjectsWithName > 0){
                Logger.Error("Scene contains multiple gameobjects with name '" + g.name + "'. Skipping FromSceneGameObject serialization.");
                return null;
            }
        #endif

        return g.scene.buildIndex.ToString() + DELIMETER_C + g.name;
    }

    static GameObject ToSceneGameObject(string s){
        string[] tokens = s.Split(DELIMETER_C);
        int levelIndex = int.Parse(tokens[0]);
        string name = tokens[1];

        LevelManagerComponent.Level owningLevel = (LevelManagerComponent.Level)(levelIndex);

        if(!LevelManagerComponent.LevelCurrentlyLoaded(owningLevel)){
            return null;
        }

        GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();
        for(int i = 0; i < allGameObjects.Length; ++i){
            if(allGameObjects[i].name == name && allGameObjects[i].scene.buildIndex == levelIndex){
                return allGameObjects[i];
            }
        }

        Logger.Error("Error deserializing Scene Gameobject '" + name + "' (Level " + levelIndex + ") in ToSceneGameObject.");
        return null;
    }
}
