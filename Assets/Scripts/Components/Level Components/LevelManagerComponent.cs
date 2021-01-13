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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//##################################################################################################
// Level Load Entry
// This class is for holding information about a given level (self) and that level's neighbor
// levels, that should be loaded if self is loaded.
//##################################################################################################
[System.Serializable]
public class LevelLoadEntry {
    [HeaderAttribute("Level Load Entry")]
    public LevelManagerComponent.Level selfLevelLabel;
    public LevelManagerComponent.Level[] neighborLevels;
}

//##################################################################################################
// Level Manager Component
// This class is responsible for receiving trigger notifications from LevelTriggerComponents, and
// loading the appropriate neighbor levels as per data.
//
// For example, in a uniform grid of levels:
// A | B | C | D
//---+---+--+---
// E | F | G | H
//---+---+--+---
// I | J | K | L
//
// The entry marked self level as 'A' might want to load neighbors 'B', 'E', and 'F', so that when
// the player walks there, they're already loaded. In turn, walking to level 'E' might in turn load
// 'A', 'B', 'F', 'I', and 'J'.
// This allows for the continuous streaming in and out of scenes based on the players location.
//##################################################################################################
public class LevelManagerComponent : MonoBehaviour {
    public static LevelManagerComponent instance;

    public const int WAIT_FRAMES_FOR_SAVELOAD = 2;

    // Rename these the names of your scenes
    public enum Level {
        Global,
        BuildLevel1,
        BuildLevel2,
        BuildLevel3,
        BuildLevel4,
        BuildLevel5,
        BuildLevel6,
        BuildLevel7,
        BuildLevel8,
        BuildLevel9,
        // ...

        Invalid,
    }

    public Level startLevel;
    public LevelLoadEntry[] levelLoads;
    private Level primaryLevel = Level.Invalid;
    public bool[] levelIsLoaded;

    private Queue<Level> loadQueue;
    private Queue<Level> unloadQueue;

    private bool finishedLoading;
    private bool currentlyWorking;
    private int saveLoadQueued;

    //##############################################################################################
    // Setup the various queues, and error check the level loads entries. They should be ordered
    // in the same order as their build index.
    //##############################################################################################
    void Start(){
        instance = this;

        saveLoadQueued = 0;

        loadQueue = new Queue<Level>();
        unloadQueue = new Queue<Level>();

        levelIsLoaded = new bool[levelLoads.Length];
        levelIsLoaded[0] = true; // Since we're in global

        for(int i = 0; i < levelLoads.Length; ++i){
            if(levelLoads[i].selfLevelLabel != (Level)(i)){
                Logger.Error("Level load at index " + i + " is out of order. Is " + levelLoads[i].selfLevelLabel + ", but should be " + (Level)(i));
            }
        }
    }

    //##############################################################################################
    // Return whether or not a level is currently loaded.
    // we cache this information as levels load and unload, because unity doesn't really handle it
    // in an accessible way.
    //##############################################################################################
    public static bool LevelCurrentlyLoaded(Level level){
        if(level == Level.Invalid){
            return false;
        }

        return instance.IsLevelLoaded(level);
    }

    //##############################################################################################
    // Getter for if level is loaded
    //##############################################################################################
    public bool IsLevelLoaded(Level level){
        return levelIsLoaded[(int) level];
    }

    //##############################################################################################
    // Handle the sequential asynchronous loading of neighbor levels, and the unloading of old
    // levels. Also, if needed, perform a saveload.
    //##############################################################################################
    void Update(){
        if(!finishedLoading){
            // If there are any levels left to load, mark ourselves as working, and load the scene
            // asynchronously. We do this async, so framerate isn't interrupted for the further
            // distant neighbor levels. Make sure to mark it as loaded, as well.
            if(loadQueue.Count > 0){
                Level nextLevelToLoad = loadQueue.Peek();

                if(!currentlyWorking){
                    currentlyWorking = true;
                    SceneManager.LoadSceneAsync((int)(nextLevelToLoad), LoadSceneMode.Additive);
                    levelIsLoaded[(int)(nextLevelToLoad)] = true;
                } else {
                    if(LevelCurrentlyLoaded(nextLevelToLoad)){
                        loadQueue.Dequeue();
                        currentlyWorking = false;
                    }
                }
            // Once we've finished loading, begin unloading the previous, unneeded levels, much the
            // same way as above.
            } else if(unloadQueue.Count > 0){
                Level nextLevelToUnload = unloadQueue.Peek();

                if(!currentlyWorking){
                    currentlyWorking = true;
                    SceneManager.UnloadSceneAsync((int)(nextLevelToUnload));
                    levelIsLoaded[(int)(nextLevelToUnload)] = false;
                } else {
                    if(!LevelCurrentlyLoaded(nextLevelToUnload)){
                        unloadQueue.Dequeue();
                        currentlyWorking = false;
                    }
                }
            }

            // Mark ourselves as all done
            finishedLoading = loadQueue.Count == 0 && unloadQueue.Count == 0;

            // Kick off queued saveload
            saveLoadQueued = WAIT_FRAMES_FOR_SAVELOAD;
        }

        // Wait a few frames to let level loading settle down
        if(saveLoadQueued > 0 && finishedLoading){
            saveLoadQueued--;

            if(saveLoadQueued == 0){
                // Only partially apply save
                SaveLoadManagerComponent.Instance().ApplySave(SaveLoadManagerComponent.SaveApplicationMode.PerLevel);
            }
        }
    }

    //##############################################################################################
    // Set the primary level, queueing up the necessary loads and unloads of neighbors
    //##############################################################################################
    public void SetPrimaryLevel(Level newPrimary){
        // Early out if we're re-triggering ourselves
        if(!enabled || primaryLevel == newPrimary){
            return;
        }

        primaryLevel = newPrimary;

        bool newPrimaryInCurrentLevels = LevelCurrentlyLoaded(newPrimary);

        loadQueue.Clear();
        unloadQueue.Clear();

        // queue load of surrounding levels
        Level[] neighborLevels = levelLoads[(int) primaryLevel].neighborLevels;
        for(int i = 0; i < neighborLevels.Length; ++i){
            if(!LevelCurrentlyLoaded(neighborLevels[i])){
                loadQueue.Enqueue(neighborLevels[i]);
            }
        }

        // queue unload of not-surrounding levels
        for(int j = 1; j < SceneManager.sceneCount; ++j){
            bool shouldUnload = true;
            Level currentlyLoadedLevel = (Level)(SceneManager.GetSceneAt(j).buildIndex);

            for(int i = 0; i < neighborLevels.Length; ++i){
                if(currentlyLoadedLevel == neighborLevels[i]){
                    shouldUnload = false;
                    break;
                }
            }

            if(currentlyLoadedLevel != newPrimary && shouldUnload){
                unloadQueue.Enqueue(currentlyLoadedLevel);
            }
        }

        // Log everything to the console - this ends up being extremely helpful, not just for
        // debugging level loading itself, but figuring out the location of the player.

        // If current scene not loaded, do a 1-frame emergency load, as fast as possible
        // Ideally, this should rarely be necessary, as levels are progressively loaded in first
        // as neighbors, then as primaries as the player reaches them.
        // This is most useful from starting new games, or loading from saves.
        if(!newPrimaryInCurrentLevels){
            Logger.Info("=== Current Level Emergency Loading ===");
            Logger.Info(primaryLevel.ToString());
            SceneManager.LoadScene((int)(primaryLevel), LoadSceneMode.Additive);
        }

        Logger.Info("=== Levels Loading ===");
        Level[] load = loadQueue.ToArray();
        for(int i = 0; i < load.Length; ++i){
            Logger.Info(load[i].ToString());
        }

        Logger.Info("=== Levels Unloading ===");
        Level[] unload = unloadQueue.ToArray();
        for(int i = 0; i < unload.Length; ++i){
            Logger.Info(unload[i].ToString());
        }

        finishedLoading = false;
        currentlyWorking = false;
    }

    //##############################################################################################
    // Unload all levels... except global. This is usually called when returning to a main menu.
    //##############################################################################################
    public void UnloadAllLevelsExceptGlobal(){
        // This happens immediately because the game isn't running, therefore framerate doesn't matter
        primaryLevel = Level.Invalid;

        Logger.Info("=== Levels Unloading ===");
        for(int i = 1; i < SceneManager.sceneCount; ++i){
            Level currentlyLoadedLevel = (Level)(SceneManager.GetSceneAt(i).buildIndex);

            if(currentlyLoadedLevel != Level.Global){
                Logger.Info(currentlyLoadedLevel.ToString());
                SceneManager.UnloadSceneAsync((int)(currentlyLoadedLevel));
            }
        }

        // Destroy all objects marked with a 'spawned' tags. This cleans up wayward effects,
        // bullets, etc.
        Logger.Info("=== Spawned Objects Deleted ===");
        GameObject[] spawnedObjects = GameObject.FindGameObjectsWithTag("Spawned");
        int spawnedCount = spawnedObjects.Length;
        for(int i = 0; i < spawnedCount; ++i){
            Destroy(spawnedObjects[i]);
        }
        Logger.Info("Deleted " + spawnedCount + " spawned objects");
    }
}
