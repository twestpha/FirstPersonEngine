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

using System.Collections.Generic;
using UnityEngine;

//##################################################################################################
// Material Lookup Source Component
// This is the endpoint for material lookups to find the data they're asking for. This class is a
// go-between for the game and a MaterialLookupData asset, with helpful functions to get the exact
// data lists needed.
//
// The lookups filter based on type, and return a list, for data that might need more than one.
// For example, you may return 4 sound effects for dirt for the footstep sound, and randomly choose
// which one to play.
//
// The data returned from the lookups should be cached for better performance, and only re-evaluated
// when the underlying material changes.
//##################################################################################################
public class MaterialLookupSourceComponent : MonoBehaviour {
    private static MaterialLookupSourceComponent instance;

    public MaterialLookupData lookupData;

    private MaterialLookupEntry fallback = null;
    private Dictionary<MaterialName, MaterialLookupEntry> lookupDataDictionary = new Dictionary<MaterialName, MaterialLookupEntry>();

    //##############################################################################################
    // Build the lookup dictionary and warn if it's configured without a fallback
    //##############################################################################################
    void Start(){
        if(instance == null){
            instance = this;
        }

        // Build the lookup dictionary, and warn if no fallback is found
        for(int i = 0, count = lookupData.lookups.Length; i < count; ++i){
            if(lookupData.lookups[i].materialName == MaterialName.Fallback){
                fallback = lookupData.lookups[i];
            } else {
                lookupDataDictionary.Add(lookupData.lookups[i].materialName, lookupData.lookups[i]);
            }
        }

        if(fallback == null){
            Debug.LogWarning("In MaterialLookupSourceComponent, there is no fallback defined, and therefore nothing can be returned during lookup if no matching material is found.");
        }
    }

    //##############################################################################################
    // Get the lookup data for the given material, then build a list of the spawnables based on the
    // type. Use the fallback if indicated.
    //
    // This value should be cached by the callee, and only re-evaluated when materials change.
    //##############################################################################################
    public static List<GameObject> GetMaterialSpawnables(MaterialName materialName, MaterialSpawnableType spawnableType, bool useFallbackIfNotFound = false){
         List<GameObject> data = new List<GameObject>();

        if(instance.lookupDataDictionary.ContainsKey(materialName)){
            MaterialLookupEntry lookup = instance.lookupDataDictionary[materialName];

            foreach(MaterialSpawnable materialSpawnable in lookup.spawnables){
                if(materialSpawnable.type == spawnableType){
                    data.Add(materialSpawnable.spawnable);
                }
            }
        }

        if((materialName == MaterialName.Fallback || (data.Count == 0 && useFallbackIfNotFound)) && instance.fallback != null){
            foreach(MaterialSpawnable materialSpawnable in instance.fallback.spawnables){
                if(materialSpawnable.type == spawnableType){
                    data.Add(materialSpawnable.spawnable);
                }
            }
        }

        return data;
    }

    //##############################################################################################
    // Get the lookup data for the given material, then build a list of the sounds based on the
    // type. Use the fallback if indicated.
    //
    // This value should be cached by the callee, and only re-evaluated when materials change.
    //##############################################################################################
    public static List<AudioClip> GetMaterialSounds(MaterialName materialName, MaterialSoundType soundType, bool useFallbackIfNotFound = false){
         List<AudioClip> data = new List<AudioClip>();

        if(instance.lookupDataDictionary.ContainsKey(materialName)){
            MaterialLookupEntry lookup = instance.lookupDataDictionary[materialName];

            foreach(MaterialSound materialSound in lookup.sounds){
                if(materialSound.type == soundType){
                    data.Add(materialSound.sound);
                }
            }
        }

        if((materialName == MaterialName.Fallback || (data.Count == 0 && useFallbackIfNotFound)) && instance.fallback != null){
            foreach(MaterialSound materialSound in instance.fallback.sounds){
                if(materialSound.type == soundType){
                    data.Add(materialSound.sound);
                }
            }
        }

        return data;
    }
}
