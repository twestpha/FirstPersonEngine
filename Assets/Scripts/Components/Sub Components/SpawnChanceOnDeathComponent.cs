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

//##################################################################################################
// Sub Components are a toolbox of simple, common building block behaviours, meant to do a single
// thing within the constraints of the other existing components.
//
// Spawn Chance On Death Component
// This component is used for spawning any prefab gameobject with a percentage chance. This is
// useful for item drops when killing enemies. Each spawnable has a change, between 0 and 1.
// These need to add up to one for the algorithm to work. Also, a null gameobject is a valid choice
// representing "spawn nothing".
//##################################################################################################
[RequireComponent(typeof(DamageableComponent))]
public class SpawnChanceOnDeathComponent : MonoBehaviour {

    [System.Serializable]
    public class ChanceSpawnable {
        public GameObject prefab;
        public float chance;
    }

    public Vector3 spawnOffset;
    public ChanceSpawnable[] spawnables;

    private void Start(){
        GetComponent<DamageableComponent>().RegisterOnKilledDelegate(Killed);

        #if UNITY_EDITOR
            float totalChance = 0.0f;

            for(int i = 0, count = spawnables.Length; i < count; ++i){
                totalChance += spawnables[i].chance;
            }

            float delta = Mathf.Abs(1.0f - totalChance);
            if(delta >= 0.01f){
                Logger.Error(gameObject.name + "'s SpawnChanceOnDeathComponent chances do not add up to 1.0 (actual value was " + totalChance + ")");
            }
        #endif
    }

    private void Killed(DamageableComponent damage){
        // Roll random
        float roll = Random.value;

        // Iterate over chances to see if that's ours
        float totalChance = 0.0f;
        int rolledIndex = 0;

        for(int i = 0, count = spawnables.Length; i < count; ++i){
            totalChance += spawnables[i].chance;

            if(roll <= totalChance){
                rolledIndex = i;
                break;
            }
        }

        // If prefab exists, spawn it. Null is valid, though, as 'spawn nothing'
        if(spawnables[rolledIndex].prefab != null){
            GameObject newObject = Object.Instantiate(spawnables[rolledIndex].prefab);
            newObject.transform.position = transform.position + spawnOffset;
        }
    }
}
