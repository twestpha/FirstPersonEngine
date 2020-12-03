using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                Debug.LogError(gameObject.name + "'s SpawnChanceOnDeathComponent chances do not add up to 1.0 (actual value was " + totalChance + ")");
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
