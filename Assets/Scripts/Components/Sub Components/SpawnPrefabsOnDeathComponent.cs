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

//##################################################################################################
// Sub Components are a toolbox of simple, common building block behaviours, meant to do a single
// thing within the constraints of the other existing components.
//
// Spawn Prefabs On Death Component
// This script simply spawns many prefab gameobject, and if marked to, destroys the original
// gameObject. This is used for exploding damageables into particles on death.
//##################################################################################################
[RequireComponent(typeof(DamageableComponent))]
public class SpawnPrefabsOnDeathComponent : MonoBehaviour {

    public bool destroyOriginal;

    [System.Serializable]
    public class DeathSpawnPrefab {
        public GameObject effectsPrefab;
        public Vector3 effectsOffset;
    }

    public DeathSpawnPrefab[] prefabsToSpawnOnDeath;

    private void Start(){
        GetComponent<DamageableComponent>().RegisterOnKilledDelegate(Killed);
    }

    private void Killed(DamageableComponent damage){
        for(int i = 0, count = prefabsToSpawnOnDeath.Length; i < count; ++i){
            DeathSpawnPrefab deathSpawn = prefabsToSpawnOnDeath[i];

            GameObject newEffects = Object.Instantiate(deathSpawn.effectsPrefab);
            newEffects.transform.position = transform.position + deathSpawn.effectsOffset;
            newEffects.transform.rotation = transform.rotation;
        }

        if(destroyOriginal){
            Destroy(gameObject);
        }
    }
}
