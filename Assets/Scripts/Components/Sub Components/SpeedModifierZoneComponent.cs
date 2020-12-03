using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedModifierZoneComponent : MonoBehaviour {

    private const float SPEED_MULTIPLIER = 0.5f;

    private static GameObject modifierObject;
    private static List<GameObject> activeModifiers;

    void Start(){
        if(activeModifiers == null){
            modifierObject = gameObject;
            activeModifiers = new List<GameObject>();

            DamageableComponent playerDamageable = PlayerComponent.player.GetComponent<DamageableComponent>();
            playerDamageable.RegisterOnKilledDelegate(PlayerKilled);
        }
    }

    void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            AddActiveModifier(gameObject);
        }
    }

    void OnTriggerExit(Collider other){
        if(other.tag == "Player"){
            RemoveActiveModifier(gameObject);
        }
    }

    private static void AddActiveModifier(GameObject modifier){
        if(!activeModifiers.Contains(modifier)){
            activeModifiers.Add(modifier);
        } else {
            Debug.LogError("Already added");
        }

        if(activeModifiers.Count == 1){
            PlayerComponent.player.AddSpeedModifier(modifierObject, SPEED_MULTIPLIER);
        }
    }

    private static void RemoveActiveModifier(GameObject modifier){
        if(activeModifiers.Contains(modifier)){
            activeModifiers.Remove(modifier);
        } else {
            Debug.LogError("Already removed");
        }

        if(activeModifiers.Count == 0){
            PlayerComponent.player.RemoveSpeedModifier(modifierObject);
        }
    }

    void PlayerKilled(DamageableComponent damaged){
        activeModifiers.Clear();
    }
}
