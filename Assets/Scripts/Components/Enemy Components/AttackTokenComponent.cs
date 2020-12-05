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
// Attack Token Component
// This class is a helper for enemies to decide when to attack the player. To prevent overloading
// a player with being shot at by every enemy, always, this doles out an attack token to enemies
// so they can attack. However, these tokens are in limited amount, recharge over time, and each
// enemy can only get one until they're token expires.
//##################################################################################################
public class AttackTokenComponent : MonoBehaviour {
    private static AttackTokenComponent instance;

    public int currentTokens;
    public int maxTokens;

    public float tokenRespawnTime;

    private Timer tokenRespawnTimer;
    private Queue<GameObject> recentlyTakenTokens;

    //##############################################################################################
    // Cache this as the global instance, and setup the component
    //##############################################################################################
    void Start(){
        instance = this;

        currentTokens = maxTokens;

        tokenRespawnTimer = new Timer(tokenRespawnTime);
        recentlyTakenTokens = new Queue<GameObject>(maxTokens);
    }

    //##############################################################################################
    // If not all tokens have recharged, add one, and remove the requester, allowing them to take
    // a new token.
    //##############################################################################################
    void Update(){
        if(currentTokens < maxTokens && tokenRespawnTimer.Finished()){
            recentlyTakenTokens.Dequeue();
            currentTokens++;
            tokenRespawnTimer.Start();
        }
    }

    //##############################################################################################
    // Wrapper for requesting token. This returns whether a token request was successful.
    //##############################################################################################
    public static bool RequestToken(GameObject requester){
        return instance.RequestTokenInternal(requester);
    }

    //##############################################################################################
    // If there are no tokens, or the requester already has one, deny them. Otherwise track them,
    // subtrack the amount left, and notify the caller of the success.
    //##############################################################################################
    public bool RequestTokenInternal(GameObject requester){
        if(currentTokens <= 0){
            return false;
        }

        if(recentlyTakenTokens.Contains(requester)){
            return false;
        }

        recentlyTakenTokens.Enqueue(requester);

        currentTokens--;
        return true;
    }
}
