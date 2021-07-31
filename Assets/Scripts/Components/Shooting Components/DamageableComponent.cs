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
// Damage Type
// Used for resistances to certain types of damages
// Meant to be extended or modified to be game-specific
//##################################################################################################
public enum DamageType {
    None,
    Bullet,
    Explosion,
    Trigger,
    // ...
}

//##################################################################################################
// Team
// Used for determining when something is 'on the other team' and therefore an enemy
// This has no effect on damage dealt, therefore enabling 'friendly fire'
//##################################################################################################
public enum Team {
    Neutral,
    PlayerTeam,
    NonPlayerTeam,
    Hostile,
}

//##################################################################################################
// Damageable Component
//##################################################################################################
public class DamageableComponent : MonoBehaviour {

    [HeaderAttribute("Damageable Component")]

    [SerializeField]
    private bool invincible;

    public float maxHealth;
    [SerializeField]
    private float currentHealth;

    public Team team;

    [System.Serializable]
    public class DamageResistance {
        public DamageType type = DamageType.None;
        public float multiplier = 1.0f;
        public float offset = 0.0f;
    }

    public DamageResistance[] resistances;

    public delegate void OnDamageableDamaged(DamageableComponent damage);
    public delegate void OnDamageableKilled(DamageableComponent damage);
    public delegate void OnDamageableHealed(DamageableComponent damage);
    public delegate void OnDamageableRespawned(DamageableComponent damage);

    private List<OnDamageableDamaged> damagedDelegates;
    private List<OnDamageableKilled> killedDelegates;
    private List<OnDamageableHealed> healedDelegates;
    private List<OnDamageableRespawned> respawnedDelegates;

    private Vector3 damagerOrigin;
    private GameObject damager;

    //##############################################################################################
    // Set to full health by default after error checking
    //##############################################################################################
	void Start(){
        if(maxHealth <= 0.0f){
            Logger.Error("Max Health on " + gameObject.name + "'s DamageableComponent cannot be less than or equal zero");
        }

        currentHealth = maxHealth;
	}

    //##############################################################################################
    // These are static helper functions for determining if any two damageables are hostile or
    // friendly with each other.
    //
    // If they are the same team, then they are friendly
    // If they are opposite (non-neutral) teams, then they are hostile
    // If any damageable is 'hostile', then they are hostile and not friendly to anyone
    // If any damageable is 'neutral', then they are not hostile but not necessarily friendly
    //##############################################################################################
    public static bool Hostile(Team a, Team b){
        return (a == Team.PlayerTeam && b == Team.NonPlayerTeam) || (b == Team.PlayerTeam && a == Team.NonPlayerTeam) || a == Team.Hostile || b == Team.Hostile;
    }

    public static bool Friendly(Team a, Team b){
        return a == b && a != Team.Hostile && b != Team.Hostile;
    }

    //##############################################################################################
    // Deal damage to the damageable, after applying resistances. Notify the damage-delegates if
    // any damage was taken.
    // If this would kill the damageable, notify the killed-delegates of this too. Note that both
    // damaged and killed delegates can be sent, they are not exclusive.
    //##############################################################################################
    public void DealDamage(float damage, DamageType type, Vector3 position, GameObject damager_){

        // Total Damage = (multiplier * damage) + offset
        // This allows for multipliers like 0.5 (i.e. take half damage) and offsets of 0
        // Or constant changes, like an offset of -1 (i.e. always take one less damage)

        for(int i = 0, count = resistances.Length; i < count; ++i){
            if(type == resistances[i].type){
                damage = (resistances[i].multiplier * damage) + resistances[i].offset;
                Mathf.Max(damage, 0.0f);
                break;
            }
        }

        // If we won't do any damage, return early
        if(damage <= 0.0f || invincible || currentHealth <= 0){
            return;
        }

        currentHealth -= damage;
        damagerOrigin = position;
        damager = damager_;

        NotifyDamagedDelegates();

        if(currentHealth <= 0){
            NotifyKilledDelegates();
        }
    }

    //##############################################################################################
    // Attempt to heal the damageable, but only if it's alive. Return true if the heal actually
    // healed some amount.
    //##############################################################################################
    public bool Heal(float amount){
        if(currentHealth > 0.0f && currentHealth < maxHealth){
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            NotifyHealedDelegates();

            return true;
        }

        return false;
    }

    //##############################################################################################
    // Iterate the delegates and call them
    //##############################################################################################
    public void NotifyDamagedDelegates(){
        if(damagedDelegates != null){
            foreach(OnDamageableDamaged damageDelegate in damagedDelegates){
                damageDelegate(this);
            }
        }
    }

    public void NotifyKilledDelegates(){
        if(killedDelegates != null){
            foreach(OnDamageableKilled killedDelegate in killedDelegates){
                killedDelegate(this);
            }
        }
    }

    public void NotifyHealedDelegates(){
        if(healedDelegates != null){
            foreach(OnDamageableHealed healedDelegate in healedDelegates){
                healedDelegate(this);
            }
        }
    }

    public void NotifyRespawnedDelegates(){
        if(respawnedDelegates != null){
            foreach(OnDamageableRespawned respawnedDelegate in respawnedDelegates){
                respawnedDelegate(this);
            }
        }
    }

    //##############################################################################################
    // The endpoint for external scripts to register a delegate that gets called when this
    // damageable is damaged, killed, healed, or respawned
    //##############################################################################################
    public void RegisterOnDamagedDelegate(OnDamageableDamaged d){
        if(damagedDelegates == null){
            damagedDelegates = new List<OnDamageableDamaged>();
        }

        damagedDelegates.Add(d);
    }

    public void RegisterOnKilledDelegate(OnDamageableKilled d){
        if(killedDelegates == null){
            killedDelegates = new List<OnDamageableKilled>();
        }

        killedDelegates.Add(d);
    }

    public void RegisterOnHealedDelegate(OnDamageableHealed d){
        if(healedDelegates == null){
            healedDelegates = new List<OnDamageableHealed>();
        }

        healedDelegates.Add(d);
    }

    public void RegisterOnRespawnedDelegate(OnDamageableRespawned d){
        if(respawnedDelegates == null){
            respawnedDelegates = new List<OnDamageableRespawned>();
        }

        respawnedDelegates.Add(d);
    }

    //##############################################################################################
    // Override the resistance multiplier and offset for a given type
    // For safety, this function assumes that an entry for that type already exists, set up ahead
    // of time, so miscellaneous resistances can't be added spuriously.
    //##############################################################################################
    public void SetResistance(DamageType type, float multiplier, float offset){
        for(int i = 0, count = resistances.Length; i < count; ++i){
            if(type == resistances[i].type){
                resistances[i].multiplier = multiplier;
                resistances[i].offset = offset;

                return;
            }
        }

        Logger.Warning("No resistance exists for " + type + " on " + gameObject.name);
    }

    //##############################################################################################
    // Basic getters, setters, and utility functions
    //##############################################################################################
    public void SetInvincible(bool value){
        invincible = value;
    }

    public float CurrentHealth(){
        return currentHealth;
    }

    public bool Dead(){
        return currentHealth <= 0.0f;
    }

    public void Respawn(){
        currentHealth = maxHealth;
        NotifyRespawnedDelegates();
    }

    public GameObject GetDamager(){
        return damager;
    }

    public Vector3 GetDamagerOrigin(){
        return damagerOrigin;
    }
}
