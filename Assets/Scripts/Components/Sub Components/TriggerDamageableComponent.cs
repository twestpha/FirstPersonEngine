using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerDamageableComponent : MonoBehaviour {
    public float damageAmount;
    public DamageType type = DamageType.Trigger;

    private void OnTriggerEnter(Collider other){
        if(enabled && other.gameObject.TryGetComponent(out DamageableComponent damageable)){
            damageable.DealDamage(damageAmount, type, transform.position, gameObject);
        }
    }
}
