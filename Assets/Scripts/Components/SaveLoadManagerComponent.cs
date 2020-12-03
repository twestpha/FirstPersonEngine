using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
    private const string PREFAB_PATH = "Prefabs/";
    string assetName;

    public void FromGameObject(GameObject g){
        assetName = g.name;

        // Catch non-prefab gameobjects when saving
        #if UNITY_EDITOR
            ToGameObject();
        #endif
    }

    public GameObject ToGameObject(){
        GameObject foundAsset = Resources.Load<GameObject>(PREFAB_PATH + assetName);

        if(!foundAsset){
            Debug.LogError("Error finding asset '" + PREFAB_PATH + assetName + "' for SerializableSpawnedPrefab instantiation.");
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
        // LevelManagerComponent.Level owningLevel = (LevelManagerComponent.Level)(levelIndex);

        // if(!LevelManagerComponent.LevelCurrentlyLoaded(owningLevel)){
        //     return null;
        // }
        //
        // GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();
        // for(int i = 0; i < allGameObjects.Length; ++i){
        //     if(allGameObjects[i].name == name && allGameObjects[i].scene.buildIndex == levelIndex){
        //         return allGameObjects[i];
        //     }
        // }

        // Debug.LogError("Error referencing gameobject '" + name + "' (Level " + owningLevel + ") for SerializableGameObjectReference connection.");
        return null;
    }
}

[System.Serializable]
public class Save {
    public int version;

    // Enemy Combatants
    public int enemyIndex;
    public SerializableGameObjectReference[] enemyCombatants;
    public SerializableSpawnedPrefab[] enemyBodyPrefabs;
    public SerializableVector3[] enemyBodyPositions;
    public SerializableQuaternion[] enemyBodyOrientations;

    // Companion and Self Bodies
    public int demonIndex;
    public SerializableSpawnedPrefab[] demonBodyPrefabs;
    public SerializableVector3[] demonBodyPositions;
    public SerializableQuaternion[] demonBodyOrientations;

    // Player previous spawn and key count
    public int playerKeyCount;
    public int playerRuneCount;
    public SerializableGameObjectReference currentRespawn;

    // Key manager states, currently spawned keys
    public int completedKeyManagerCount;
    public SerializableGameObjectReference[] completedKeyManagers;
    public int spawnedKeyCount;
    public SerializableVector3[] spawnedKeyPositions;

    // Picked up Runes
    public int runesPickedUpCount;
    public SerializableGameObjectReference[] pickedUpRunes;

    // Opened Gates
    public int openedGateCount;
    public SerializableGameObjectReference[] openedGates;

    // Unlocked Spawn Points
    // This is just a decorative effect only, since the player unlocks them by
    // walking to them, which stomps and sets the latest
    public int unlockedSpawnPointCount;
    public SerializableGameObjectReference[] unlockedSpawnPoints;

    // Destroyed Tanks
    public int destroyedTankCount;
    public SerializableGameObjectReference[] destroyedTanks;

    public Save(){
        // Enemy Combatants
        enemyIndex = 0;
        enemyCombatants = new SerializableGameObjectReference[SaveLoadManagerComponent.MAX_ENEMIES];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_ENEMIES; ++i){ enemyCombatants[i] = new SerializableGameObjectReference(); }

        enemyBodyPrefabs = new SerializableSpawnedPrefab[SaveLoadManagerComponent.MAX_ENEMIES];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_ENEMIES; ++i){ enemyBodyPrefabs[i] = new SerializableSpawnedPrefab(); }

        enemyBodyPositions = new SerializableVector3[SaveLoadManagerComponent.MAX_ENEMIES];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_ENEMIES; ++i){ enemyBodyPositions[i] = new SerializableVector3(); }

        enemyBodyOrientations = new SerializableQuaternion[SaveLoadManagerComponent.MAX_ENEMIES];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_ENEMIES; ++i){ enemyBodyOrientations[i] = new SerializableQuaternion(); }

        // Companion and Self Bodies
        demonIndex = 0;

        demonBodyPrefabs = new SerializableSpawnedPrefab[SaveLoadManagerComponent.MAX_COMPANIONS];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_COMPANIONS; ++i){ demonBodyPrefabs[i] = new SerializableSpawnedPrefab(); }

        demonBodyPositions = new SerializableVector3[SaveLoadManagerComponent.MAX_COMPANIONS];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_COMPANIONS; ++i){ demonBodyPositions[i] = new SerializableVector3(); }

        demonBodyOrientations = new SerializableQuaternion[SaveLoadManagerComponent.MAX_COMPANIONS];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_COMPANIONS; ++i){ demonBodyOrientations[i] = new SerializableQuaternion(); }

        // Player
        playerKeyCount = 0;
        playerRuneCount = 0;
        currentRespawn = new SerializableGameObjectReference();

        completedKeyManagerCount = 0;
        completedKeyManagers = new SerializableGameObjectReference[SaveLoadManagerComponent.MAX_KEY_MANAGERS];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_KEY_MANAGERS; ++i){
            completedKeyManagers[i] = new SerializableGameObjectReference();
        }

        spawnedKeyCount = 0;
        spawnedKeyPositions = new SerializableVector3[SaveLoadManagerComponent.MAX_KEYS];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_KEYS; ++i){
            spawnedKeyPositions[i] = new SerializableVector3();
        }

        runesPickedUpCount = 0;
        pickedUpRunes = new SerializableGameObjectReference[SaveLoadManagerComponent.MAX_RUNES];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_RUNES; ++i){
            pickedUpRunes[i] = new SerializableGameObjectReference();
        }

        openedGateCount = 0;
        openedGates = new SerializableGameObjectReference[SaveLoadManagerComponent.MAX_GATES];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_GATES; ++i){
            openedGates[i] = new SerializableGameObjectReference();
        }

        unlockedSpawnPointCount = 0;
        unlockedSpawnPoints = new SerializableGameObjectReference[SaveLoadManagerComponent.MAX_SPAWN_POINTS];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_SPAWN_POINTS; ++i){
            unlockedSpawnPoints[i] = new SerializableGameObjectReference();
        }

        destroyedTankCount = 0;
        destroyedTanks = new SerializableGameObjectReference[SaveLoadManagerComponent.MAX_TANKS];
        for(int i = 0; i < SaveLoadManagerComponent.MAX_TANKS; ++i){
            destroyedTanks[i] = new SerializableGameObjectReference();
        }
    }
}

public class SaveLoadManagerComponent : MonoBehaviour {
    private static SaveLoadManagerComponent instance;

    private const string SAVE_NAME = "/anopek.save";
    private const int SAVE_VERSION = 7;

    public const int MAX_ENEMIES = 1024;
    public const int MAX_COMPANIONS = 1024;

    public const int MAX_GATES = 64;
    public const int MAX_SPAWN_POINTS = 6;

    public const int MAX_KEY_MANAGERS = 6;
    public const int MAX_KEYS = 5;
    public const int MAX_RUNES = 3;

    public const int MAX_TANKS = 3;

    [SerializeField]
    private Save save;

    public GameObject globalKeyPrefab;

    public enum SaveApplicationMode {
        Full,
        PerLevel,
    }

    void Start(){
        save = new Save();
    }

    void Update(){
        #if UNITY_EDITOR
            // Quicksave/Quickload for debugging
            if(Input.GetKeyDown(KeyCode.F5)){
                Save();
            }
            if(Input.GetKeyDown(KeyCode.F6)){
                Load();
            }
        #endif
    }

    public void NewGame(){
        save = new Save();
        Save();
    }

    public bool SavedGameExists(){
      Save temp = new Save();

      try {
          BinaryFormatter bf = new BinaryFormatter();
          FileStream file = File.Open(Application.persistentDataPath + SAVE_NAME, FileMode.Open);
          temp = (Save) bf.Deserialize(file);
          file.Close();
      } catch {
          return false;
      }

      return temp.version == SAVE_VERSION;
    }

    public void Save(){
        // We should write to disk only when exiting the game (and maybe on a timer?)
        // However, the save values should be kept up to date with the different setters

        Debug.Log("Saving Game...");

        UpdateSave();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + SAVE_NAME);
        bf.Serialize(file, save);
        file.Close();
    }

    public void Load(){
        Debug.Log("Loading Game...");

        bool fileLoaded = true;

        try {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + SAVE_NAME, FileMode.Open);
            save = (Save) bf.Deserialize(file);
            file.Close();
        } catch {
            fileLoaded = false;
        }

        if(fileLoaded && save.version == SAVE_VERSION){
            ApplySave(SaveApplicationMode.Full);
            Debug.Log("Game loaded successfully");
        } else {
            if(fileLoaded){
                Debug.LogError("Invalid save version " + save.version + ", cannot apply save");
            } else {
                Debug.LogError("Save file not found.");
            }
        }
    }

    public void UpdateSave(){
        save.version = SAVE_VERSION;

        // Save player properties
        PlayerRespawnVolumeComponent respawnComponent = PlayerRespawnVolumeComponent.GetCurrentRespawn();
        if(respawnComponent){
            save.currentRespawn.FromGameObject(respawnComponent.gameObject);
        }
    }

    // Every level load calls this, and it'll run and apply partially on those levels
    public void ApplySave(SaveApplicationMode mode){
        for(int i = 0; i < save.enemyIndex; ++i){
            // Regardless of mode, always clean up the combatant
            GameObject deadCombatant = save.enemyCombatants[i].ToGameObject();

            if(deadCombatant != null){
                Debug.Log("Load-killing " + deadCombatant);
                Destroy(deadCombatant);
            } else {
                Debug.LogWarning("Unable to restore dead combatant");
            }
        }

        // Regardless of mode, try to destroy tanks
        for(int i = 0; i < save.destroyedTankCount; ++i){
            GameObject tank = save.destroyedTanks[i].ToGameObject();

            if(tank != null){
                Debug.Log("Load-destroying tank " + tank);
                Destroy(tank);
            }
        }

        if(mode == SaveApplicationMode.Full){
            // Only spawn bodies when fully applying, since they'll live in the global gamespace

            // Spawn enemy dead bodies
            for(int i = 0; i < save.enemyIndex; ++i){
                GameObject foundEnemyPrefab = save.enemyBodyPrefabs[i].ToGameObject();

                if(foundEnemyPrefab != null){
                    GameObject newBody = GameObject.Instantiate(foundEnemyPrefab);
                    newBody.transform.position = save.enemyBodyPositions[i].ToVector3();
                    newBody.transform.rotation = save.enemyBodyOrientations[i].ToQuaternion();
                } else {
                    Debug.LogWarning("Unable to restore body prefab");
                }
            }

            // Spawn companion dead bodies
            for(int i = 0; i < save.demonIndex; ++i){
                GameObject foundCompanionPrefab = save.demonBodyPrefabs[i].ToGameObject();

                if(foundCompanionPrefab != null){
                    GameObject newBody = GameObject.Instantiate(foundCompanionPrefab);
                    newBody.transform.position = save.demonBodyPositions[i].ToVector3();
                    newBody.transform.rotation = save.demonBodyOrientations[i].ToQuaternion();
                } else {
                    Debug.LogWarning("Unable to restore body prefab");
                }
            }
        }

        // Unlock all respawn volumes possible, regardless of mode
        for(int i = 0; i < save.unlockedSpawnPointCount; ++i){
            GameObject spawnPoint = save.unlockedSpawnPoints[i].ToGameObject();
            if(spawnPoint){
                PlayerRespawnVolumeComponent respawnComponent = spawnPoint.GetComponent<PlayerRespawnVolumeComponent>();

                if(respawnComponent){
                    respawnComponent.SetDroppedImmediately();
                } else {
                    Debug.LogError("GameObject " + spawnPoint + " was marked as saved, but doesn't have PlayerRespawnVolumeComponent");
                }
            }
        }

        if(mode == SaveApplicationMode.Full){
            // Only create keys when fully loading
            for(int i = 0; i < save.spawnedKeyCount; ++i){
                GameObject newKey = GameObject.Instantiate(globalKeyPrefab);
                newKey.transform.position = save.spawnedKeyPositions[i].ToVector3();
            }

            // Only delete runes when fully loading
            for(int i = 0; i < save.runesPickedUpCount; ++i){
                GameObject rune = save.pickedUpRunes[i].ToGameObject();
                if(rune != null){
                    Destroy(rune);
                }
            }
        }

        // Restore players properties, only in full apply mode
        if(mode == SaveApplicationMode.Full){
            if(save.currentRespawn != null){
                GameObject loadedRespawn = save.currentRespawn.ToGameObject();

                if(loadedRespawn != null){
                    PlayerRespawnVolumeComponent.SetCurrentRespawn(loadedRespawn);
                }
            }
        }
    }

    // #########################################################################
    // Static Methods
    // #########################################################################
    static void SetupInstance(){
        if(SaveLoadManagerComponent.instance == null){
            GameObject saveload = GameObject.FindWithTag("SaveLoad Manager");
            SaveLoadManagerComponent.instance = saveload.GetComponent<SaveLoadManagerComponent>();
        }
    }

    public static SaveLoadManagerComponent Instance(){
        SetupInstance();
        return instance;
    }

    public static void RegisterBody(GameObject deadCombatant, GameObject prefab, Vector3 position, Quaternion orientation){
        SetupInstance();
        instance.RegisterBodyInternal(deadCombatant, prefab, position, orientation);
    }

    public static void RegisterGateUnlocked(GameObject gate){
        SetupInstance();
        instance.RegisterGateUnlockedInternal(gate);
    }

    public static void RegisterSpawnPointUnlocked(GameObject spawnPoint){
        SetupInstance();
        instance.RegisterSpawnPointUnlockedInternal(spawnPoint);
    }

    public static void RegisterKeyManagerCompleted(GameObject keyManager){
        SetupInstance();
        instance.RegisterKeyManagerCompletedInternal(keyManager);
    }

    public static void RegisterRunePickedUp(GameObject rune){
        SetupInstance();
        instance.RegisterRunePickedUpInternal(rune);
    }

    public static void RegisterTankDestroyed(GameObject tank){
        SetupInstance();
        instance.RegisterTankDestroyedInternal(tank);
    }

    // #########################################################################
    // Instance Methods
    // #########################################################################
    void RegisterBodyInternal(GameObject deadCombatant, GameObject prefab, Vector3 position, Quaternion orientation){
        bool isPlayer = deadCombatant.GetComponent<PlayerComponent>();
        // bool isCompanion = deadCombatant.GetComponent<CompanionBehavior>();
        bool isCompanion = false;

        if(isPlayer || isCompanion){
            if(save.demonIndex >= MAX_COMPANIONS){
                Debug.LogError("Bump MAX_COMPANIONS in SaveLoadManagerComponent");

                // Emergency wraparound
                save.demonIndex = 0;
            }

            try {
                save.demonBodyPrefabs[save.demonIndex].FromGameObject(prefab);
                save.demonBodyPositions[save.demonIndex].FromVector3(position);
                save.demonBodyOrientations[save.demonIndex].FromQuaternion(orientation);
            } catch {
                // Emergency wraparound to fix existing bug in build
                // Where MAX_COMPANIONS is 128 and those arrays' size are already allocated
                save.demonIndex = 0;
            }

            save.demonIndex++;
        } else {
            if(save.enemyIndex >= MAX_ENEMIES){
                Debug.LogError("Bump MAX_ENEMIES in SaveLoadManagerComponent");

                // Emergency wraparound
                save.enemyIndex = 0;
            }

            try {
                save.enemyCombatants[save.enemyIndex].FromGameObject(deadCombatant);
                save.enemyBodyPrefabs[save.enemyIndex].FromGameObject(prefab);
                save.enemyBodyPositions[save.enemyIndex].FromVector3(position);
                save.enemyBodyOrientations[save.enemyIndex].FromQuaternion(orientation);
            } catch {
                // Emergency wraparound to fix existing bug in build
                // Where MAX_ENEMIES is 128 and those arrays' size are already allocated
                save.enemyIndex = 0;
            }

            save.enemyIndex++;
        }
    }

    void RegisterGateUnlockedInternal(GameObject gate){
        save.openedGates[save.openedGateCount].FromGameObject(gate);
        save.openedGateCount++;

        if(save.openedGateCount > MAX_GATES){
            Debug.LogError("Bump MAX_GATES in SaveLoadManagerComponent");
        }
    }

    void RegisterSpawnPointUnlockedInternal(GameObject spawnPoint){
        save.unlockedSpawnPoints[save.unlockedSpawnPointCount].FromGameObject(spawnPoint);
        save.unlockedSpawnPointCount++;

        if(save.unlockedSpawnPointCount > MAX_SPAWN_POINTS){
            Debug.LogError("Bump MAX_SPAWN_POINTS in SaveLoadManagerComponent");
        }
    }

    void RegisterKeyManagerCompletedInternal(GameObject keyManager){
        save.completedKeyManagers[save.completedKeyManagerCount].FromGameObject(keyManager);
        save.completedKeyManagerCount++;

        if(save.completedKeyManagerCount > MAX_KEY_MANAGERS){
            Debug.LogError("Bump MAX_KEY_MANAGERS in SaveLoadManagerComponent");
        }
    }

    void RegisterRunePickedUpInternal(GameObject rune){
        save.pickedUpRunes[save.runesPickedUpCount].FromGameObject(rune);
        save.runesPickedUpCount++;

        if(save.runesPickedUpCount > MAX_RUNES){
            Debug.LogError("Bump MAX_KEY_MANAGERS in SaveLoadManagerComponent");
        }
    }

    void RegisterTankDestroyedInternal(GameObject tank){
        save.destroyedTanks[save.destroyedTankCount].FromGameObject(tank);
        save.destroyedTankCount++;

        if(save.destroyedTankCount > MAX_TANKS){
            Debug.LogError("Bump MAX_TANKS in SaveLoadManagerComponent");
        }
    }
}
