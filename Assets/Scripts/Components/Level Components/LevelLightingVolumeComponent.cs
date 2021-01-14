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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//##################################################################################################
// Level Lighting Volume Component
// This class is responsible for triggering changes to the global dynamic sun, fading over time,
// to give different lighting aesthetics to different areas of the map. For example, one scene could
// be overcast and ambiently dark, while a nearby scene could be more brightly lit.
//##################################################################################################
[RequireComponent(typeof(Collider))]
public class LevelLightingVolumeComponent : MonoBehaviour {

    public float fadeTime;

    [Header("Sun Settings")]
    public bool changeSunColor;
    public Color newSunColor;
    private Color oldSunColor;

    [Space(10)]
    public bool changeSunIntensity;
    public float newSunIntensity;
    private float oldSunIntensity;

    [Space(10)]
    public bool changeSunShadowStrength;
    public float newSunShadowStrength;
    private float oldSunShadowStrength;

    [Space(10)]
    public bool changeAmbientColor;
    public Color newAmbientColor;
    private Color oldAmbientColor;

    [Space(10)]
    public bool changeSunDirection;
    public Vector3 newSunDirection;
    private Vector3 oldSunDirection;

    private bool fading;
    private Timer fadeTimer;

    private Light sun;
    private Camera mainCamera;

    //##############################################################################################
    // Setup the timer and state, and find necessary sun and main camera
    //##############################################################################################
    void Start(){
        fading = false;
        fadeTimer = new Timer(fadeTime);

        sun = GameObject.FindWithTag("Sun").GetComponent<Light>();
        mainCamera = Camera.main;

        if(!GetComponent<Collider>().isTrigger){
            Logger.Error("Collider on " + gameObject.name + "'s LevelLightingVolumeComponent must be a trigger");
        }
    }

    //##############################################################################################
    // Immediately change to the menu-preset settings
    //##############################################################################################
    public void SetToMenuPresetInstantly(){
        fading = false;

        sun.color = Color.black;
        sun.intensity = 0.0f;
        sun.shadowStrength = 0.0f;
        RenderSettings.ambientLight = Color.black;
        sun.transform.rotation = Quaternion.identity;
    }

    //##############################################################################################
    // If currently fading, lerp between the cached previous states and the target states specified
    // in the component data
    //##############################################################################################
    void Update(){
        if(fading){
            float t = CustomMath.EaseInOut(fadeTimer.Parameterized(), 0.5f);

            // If player is dead, fastforward and kill fading
            if(FirstPersonPlayerComponent.player.Dead()){
                t = 1.0f;

                fadeTimer.SetParameterized(1.0f);
                fading = false;
            }

            if(changeSunColor){
                sun.color = Color.Lerp(oldSunColor, newSunColor, t);
            }

            if(changeSunIntensity){
                sun.intensity = Mathf.Lerp(oldSunIntensity, newSunIntensity, t);
            }

            if(changeSunShadowStrength){
                sun.shadowStrength = Mathf.Lerp(oldSunShadowStrength, newSunShadowStrength, t);
            }

            if(changeAmbientColor){
                RenderSettings.ambientLight = Color.Lerp(oldAmbientColor, newAmbientColor, t);
            }

            if(changeSunDirection){
                sun.transform.rotation = Quaternion.Slerp(
                    Quaternion.Euler(oldSunDirection),
                    Quaternion.Euler(newSunDirection),
                    t
                );
            }

            if(fadeTimer.Finished()){
                fading = false;
            }
        }
    }

    //##############################################################################################
    // If the player enters this trigger volume, begin the fade process, and cache the current
    // settings for lerping.
    // This is debug logged for general information. It tends to be helpful.
    //##############################################################################################
    private void OnTriggerEnter(Collider other){
        if(other.tag == "Player"){
            fading = true;
            fadeTimer.Start();

            Logger.Info("Applying Level Lighting Volume " + gameObject.name);

            oldSunColor = sun.color;
            oldSunIntensity = sun.intensity;
            oldSunShadowStrength = sun.shadowStrength;
            oldAmbientColor = RenderSettings.ambientLight;
            oldSunDirection = sun.transform.rotation.eulerAngles;
        }
    }
}
