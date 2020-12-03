using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DamageableComponent))]
public class SpawnEffectsOnDeathComponent : MonoBehaviour {

    public bool destroyOriginal;

    public GameObject effectsPrefab;
    public Vector3 effectsOffset;

    private void Start(){
        GetComponent<DamageableComponent>().RegisterOnKilledDelegate(Killed);
    }

    private void Killed(DamageableComponent damage){
        GameObject newEffects = Object.Instantiate(effectsPrefab);
        newEffects.transform.position = transform.position + effectsOffset;

        if(destroyOriginal){
            Destroy(gameObject);
        }
    }
}
