using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerRespawnVolumeComponent : MonoBehaviour {
    protected static PlayerRespawnVolumeComponent currentRespawn;

    public const float SWORD_DROP_TIME = 6.0f;
    public const float INDICATOR_PULSE_TIME = 2.0f;
    public const float INDICATOR_ROTATE_SPEED = 10.0f;

    private bool alreadyTriggered;
    private bool dropping;
    private bool finishedDrop;

    public GameObject sword;
    public GameObject swordEnd;
    public GameObject indicator;
    public GameObject indicator2;
    public GameObject dropEffectsPrefab;

    public GameObject respawnPosition;

    public bool gameStartRespawn;

    private Timer swordDropTimer;
    private Vector3 swordStartPos;

    // public LevelManagerComponent.Level primaryLevel;

    void Start(){
        alreadyTriggered = false;
        dropping = false;

        swordStartPos = sword.transform.position;

        swordDropTimer = new Timer(SWORD_DROP_TIME);
    }

    public static PlayerRespawnVolumeComponent GetCurrentRespawn(){
        return currentRespawn;
    }

    public static void SetCurrentRespawn(GameObject respawnObject){
        PlayerRespawnVolumeComponent respawnVolume = respawnObject.GetComponent<PlayerRespawnVolumeComponent>();

        if(respawnVolume){
            currentRespawn = respawnVolume;
        } else {
            Debug.Log("Invalid actor passed into SetCurrentRespawn: " + respawnObject);
        }
    }

    void Update(){
        if(!finishedDrop){
            indicator.transform.rotation = Quaternion.Euler(-90.0f, Time.time * INDICATOR_ROTATE_SPEED, 0.0f);
            indicator2.transform.rotation = Quaternion.Euler(-90.0f, Time.time * -INDICATOR_ROTATE_SPEED, 0.0f);
        }

        if(dropping){
            float t = swordDropTimer.Parameterized();
            t *= t;

            sword.transform.position = Vector3.Lerp(swordStartPos, swordEnd.transform.position, t);

            if(swordDropTimer.Finished()){
                GameObject fx = GameObject.Instantiate(dropEffectsPrefab);
                fx.transform.position = swordEnd.transform.position + (Vector3.up * 0.1f);

                CameraLookComponent.AddCameraShake(3.0f, swordEnd.transform.position);

                dropping = false;
                finishedDrop = true;

                Destroy(indicator);
                Destroy(indicator2);
            }
        }
    }

    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            Debug.Log("Player respawn set: " + gameObject  + ", position: " + respawnPosition.transform.position);
            currentRespawn = this;

            if(!alreadyTriggered){
                alreadyTriggered = true;
                dropping = true;
                swordDropTimer.Start();

                SaveLoadManagerComponent.RegisterSpawnPointUnlocked(gameObject);
            }
        }
    }

    public void SetDroppedImmediately(){
        alreadyTriggered = true;
        finishedDrop = true;
        dropping = false;

        sword.transform.position = swordEnd.transform.position;

        if(indicator != null){ Destroy(indicator); }
        if(indicator2 != null){ Destroy(indicator2); }
    }

    public bool ShouldSpawnCompanions(){
        return alreadyTriggered && currentRespawn == this;
    }
}
