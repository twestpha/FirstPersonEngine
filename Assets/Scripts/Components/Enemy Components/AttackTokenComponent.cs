using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerComponent))]
public class AttackTokenComponent : MonoBehaviour {
    private static AttackTokenComponent instance;

    public int currentTokens;
    public int maxTokens;

    public float tokenRespawnTime;

    private Timer tokenRespawnTimer;
    private Queue<GameObject> recentlyTakenTokens;

    void Start(){
        instance = this;

        currentTokens = maxTokens;

        tokenRespawnTimer = new Timer(tokenRespawnTime);
        recentlyTakenTokens = new Queue<GameObject>(maxTokens);
    }

    void Update(){
        if(currentTokens < maxTokens && tokenRespawnTimer.Finished()){
            recentlyTakenTokens.Dequeue();
            currentTokens++;
            tokenRespawnTimer.Start();
        }
    }

    public static bool RequestToken(GameObject requester){
        return instance.RequestTokenInternal(requester);
    }

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
