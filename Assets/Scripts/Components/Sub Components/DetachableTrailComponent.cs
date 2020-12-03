using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachableTrailComponent : MonoBehaviour {

    public BulletComponent bulletParent;

    void Start(){
        bulletParent.RegisterOnBulletDestroyedDelegate(BulletDestroyed);
    }

    public void BulletDestroyed(){
        transform.parent = null;
    }
}
