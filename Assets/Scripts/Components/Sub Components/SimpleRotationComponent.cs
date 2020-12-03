using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotationComponent : MonoBehaviour {

    public Vector3 rotationDirection;
    public float rotationSpeed;

    void Start(){

    }

    void Update(){
        transform.Rotate(rotationDirection * Time.deltaTime * rotationSpeed, Space.World);
    }
}
