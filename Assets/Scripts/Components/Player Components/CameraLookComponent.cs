using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO add this to player component
public class CameraLookComponent : MonoBehaviour {
    private static CameraLookComponent instance;

    private const int DISTANCE_COUNT = 32;

    private const float SHAKE_DECAY_TIME = 0.75f;
    private const float SHAKE_NEAR_DISTANCE = 2.0f; // Distance at which shake is 1x
    private const float SHAKE_FAR_DISTANCE = 20.0f; // Distance at which shake is 0x
    private const float SHAKE_SCALE = 0.01f; // Distance at which shake is 0x

    public GameObject lookTarget;
    public float lookTime;
    public float layerCullClipDistance = 100.0f;
    private float cachedLookTime;
    private Vector3 lookVector;
    private Vector3 lookSpeed;

    public Timer shakeDecayTimer;
    public float shakeAmount;
    private Vector3 shakeOffset;

    void Start(){
        instance = this;

        cachedLookTime = lookTime;
        shakeDecayTimer = new Timer(SHAKE_DECAY_TIME);

        // Setup layer cull distances
        // So, by default, they all use the far clip plane
        // But if you manually set their distance, they clip earlier
        // So if I make the plane really far, then make everything *except* the layer
        // I want to keep really short
        // Camera camera = GetComponent<Camera>();
        //
        // float[] distances = new float[DISTANCE_COUNT];
        // for(int i = 0; i < DISTANCE_COUNT; ++i){
        //     distances[i] = layerCullClipDistance;
        // }
        //
        // distances[11 /* NoCameraCull */] = 0.0f;
        //
        // camera.layerCullDistances = distances;
    }

    void Update(){
        /*if(shakeAmount > 0.0f){
            float t = 1.0f - shakeDecayTimer.Parameterized();
            t = Mathf.Sin(t * Mathf.PI);

            shakeOffset = new Vector3(
                Mathf.Sin(Time.time * 20.0f) * t * shakeAmount * SHAKE_SCALE,
                Mathf.Sin(Time.time * 17.0f) * t * shakeAmount * SHAKE_SCALE,
                Mathf.Sin(Time.time * 18.5f) * t * shakeAmount * SHAKE_SCALE
            );

            if(shakeDecayTimer.Finished()){
                shakeAmount = 0.0f;
            }
        } else {
            shakeOffset = Vector3.zero;
        }

        lookVector = Vector3.SmoothDamp(lookVector, (lookTarget.transform.position - transform.position), ref lookSpeed, lookTime);
        lookVector.Normalize();

        transform.rotation = Quaternion.LookRotation(transform.rotation + shakeOffset);
        */
    }

    public void SetInstant(bool instant){
        lookTime = instant ? 0.0f : cachedLookTime;
    }

    #if UNITY_EDITOR
    [ContextMenu("Test Small Camera Shake")]
    void TestSmallCameraShake(){
        AddCameraShake(0.5f, transform.position);
    }

    [ContextMenu("Test Medium Camera Shake")]
    void TestMediumCameraShake(){
        AddCameraShake(1.5f, transform.position);
    }

    [ContextMenu("Test Large Camera Shake")]
    void TestLargeCameraShake(){
        AddCameraShake(5.0f, transform.position);
    }
    #endif // UNITY_EDITOR

    public static void AddCameraShake(float amount, Vector3 origin){
        float distance = (instance.transform.position - origin).magnitude;
        float t = Mathf.Abs(SHAKE_FAR_DISTANCE - distance) / (SHAKE_FAR_DISTANCE - SHAKE_NEAR_DISTANCE);
        t = Mathf.Clamp(t, 0.0f, 1.0f);

        if(t > 0.0f){
            instance.shakeAmount += (t * amount);
            instance.shakeDecayTimer.Start();
        }
    }
}
