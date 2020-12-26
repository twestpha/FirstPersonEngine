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

using UnityEngine;
using System.Collections.Generic;

//##################################################################################################
// Pooled Game Object Manager
// This is a generic class for setting up a pool of gameobjects that can then be drawn from without
// having to instantiate during runtime. The pools are managed using a poolIdentifier, so several
// different places can share a given pool.
//##################################################################################################
public class PooledGameObjectManager {
    private static Dictionary<string, Queue<GameObject>> poolLookup = new Dictionary<string, Queue<GameObject>>();

    //##############################################################################################
    // Returns whether or not a pool has been set up for a given identifier
    //##############################################################################################
    public static bool HasPoolForIdentifier(string poolIdentifier){
        return poolLookup.ContainsKey(poolIdentifier);
    }

    //##############################################################################################
    // This sets up a new pool of a given size, pre-instantiating the gameobjects ahead of time,
    // then setting them inactive and queueing them for use later.
    //##############################################################################################
    public static void SetupPool(string poolIdentifier, int poolSize, GameObject gameObject){
        if(!HasPoolForIdentifier(poolIdentifier)){
            Queue<GameObject> poolQueue = new Queue<GameObject>();

            for(int i = 0; i < poolSize; ++i){
                GameObject gameObjectInstance = GameObject.Instantiate(gameObject);
                gameObjectInstance.SetActive(false);

                poolQueue.Enqueue(gameObjectInstance);
            }

            poolLookup.Add(poolIdentifier, poolQueue);
        } else {
            Logger.Warning("The pool identifier '" + poolIdentifier + "' already has a queue setup.");
        }
    }

    //##############################################################################################
    // If the queue isn't empty, try to set an instance to active and return it for use
    // This is a rough equivalent for the Unity Instantiate() operation
    //##############################################################################################
    public static GameObject GetInstanceFromPool(string poolIdentifier){
        if(HasPoolForIdentifier(poolIdentifier)){
            Queue<GameObject> poolQueue = poolLookup[poolIdentifier];

            if(poolQueue.Count > 0){
                GameObject instance = poolLookup[poolIdentifier].Dequeue();
                instance.SetActive(true);

                return instance;
            } else {
                Logger.Error("The GameObject pool with identifier '" + poolIdentifier + "' ran out of instances. Consider bumping the maximum it was set up with.");
                return null;
            }
        } else {
            Logger.Warning("A pool with the identifier '" + poolIdentifier + "' doesn't exist.");
            return null;
        }
    }

    //##############################################################################################
    // Return the instance back to the pool and set it inactive
    // This is a rough equivalent for the Unity Destroy() operation
    //##############################################################################################
    public static void FreeInstanceToPool(string poolIdentifier, GameObject instance){
        if(HasPoolForIdentifier(poolIdentifier)){
            instance.SetActive(false);
            poolLookup[poolIdentifier].Enqueue(instance);
        } else {
            Logger.Warning("A pool with the identifier '" + poolIdentifier + "' doesn't exist.");
        }
    }
}
