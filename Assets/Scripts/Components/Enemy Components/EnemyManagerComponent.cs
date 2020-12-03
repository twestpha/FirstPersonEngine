using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManagerComponent : MonoBehaviour {
    private static EnemyManagerComponent instance;

    #if UNITY_EDITOR
    public int debugUpdateCount;
    #endif // UNITY_EDITOR

    private bool updatingA;

    private List<EnemyBehavior> updateListA;
    private List<EnemyBehavior> updateListB;

    void Start(){
        instance = this;

        updateListA = new List<EnemyBehavior>();
        updateListB = new List<EnemyBehavior>();
    }

    void Update(){
        #if UNITY_EDITOR
        debugUpdateCount = Mathf.Max(updateListA.Count, updateListB.Count);
        #endif // UNITY_EDITOR

        updatingA = !updatingA;
        List<EnemyBehavior> updateList = updatingA ? updateListA : updateListB;

        for(int i = 0, count = updateList.Count; i < count; ++i){
            // Skip destroyed enemies
            if(updateList[i] != null){
                updateList[i].EnemyUpdate();
            }
        }

        updateList.Clear();
    }

    public static void RegisterUpdate(EnemyBehavior enemy){
        instance.RegisterUpdateInternal(enemy);
    }

    public void RegisterUpdateInternal(EnemyBehavior enemy){
        // Register with the opposite list that's updating
        if(updatingA){
            if(!updateListB.Contains(enemy)){
                updateListB.Add(enemy);
            }
        } else {
            if(!updateListA.Contains(enemy)){
                updateListA.Add(enemy);
            }
        }
    }
}
