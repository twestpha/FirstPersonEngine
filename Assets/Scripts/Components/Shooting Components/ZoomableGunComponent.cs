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

using UnityEngine;
using UnityEngine.UI;

//##################################################################################################
// Zoomable Gun Component
// This is an inheritable class for player guns that zoom in and have a reticle. This gun zooms
// using the camera field-of-view (fov). Also, it can have an overlay image that will be made more
// or less opaque when zooming.
// This class descends from GunComponent
//
// TODO modify player look sensitivity while zoomed
//##################################################################################################
public class ZoomableGunComponent : GunComponent {
    private const float ZOOM_IN = 1.0f;
    private const float ZOOM_OUT = 0.0f;

    [HeaderAttribute("Zoomable Gun Component (Only needed if gun uses Zoom)")]
    public Camera playerCamera;
    public Image reticleOverlayImage;

    public Transform zoomedMuzzleTransform;
    private Transform originalMuzzleTransform;

    private float defaultFieldOfView;
    private Color tintColor;

    // Used for smooth damping the transition
    private bool currentlyZooming;
    private float zoomParameter;
    private float zoomVelocity;

    //##############################################################################################
    // Check if the gun uses zoom, then set it up if it does
    //##############################################################################################
    protected new void Start(){
        base.Start();

        if(currentGunData.useZoom){
            if(playerCamera == null){
                Logger.Error("Player Camera on " + gameObject.name + "'s ZoomableGunComponent cannot be null");
            }

            defaultFieldOfView = playerCamera.fieldOfView;

            if(reticleOverlayImage != null){
                tintColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                reticleOverlayImage.color = tintColor;
            }

            if(zoomedMuzzleTransform != null){
                originalMuzzleTransform = muzzleTransform;
            }
        }
    }

    //##############################################################################################
    // Update the zoom
    //##############################################################################################
    protected new void Update(){
        base.Update();

        if(currentGunData.useZoom){
            // TODO add a player setting for toggling versus hold-to-zoom
            currentlyZooming = Input.GetMouseButton(1);

            // If available, use the zoomed muzzle transform when zooming
            if(zoomedMuzzleTransform != null){
                muzzleTransform = currentlyZooming ? zoomedMuzzleTransform : originalMuzzleTransform;
            }

            // Smooth damp towards the target zoom
            zoomParameter = Mathf.SmoothDamp(zoomParameter, currentlyZooming ? ZOOM_IN : ZOOM_OUT, ref zoomVelocity, currentGunData.zoomTime);

            // Square the result; this gives the fading and zooming a nice ramp-up feel
            float parameterSquared = zoomParameter * zoomParameter;

            // Drive the field of view and reticle overlay alpha from that
            playerCamera.fieldOfView = Mathf.Lerp(defaultFieldOfView, currentGunData.zoomedFieldOfView, parameterSquared);

            // Drive the player movement speed and look multiplier, lerp from 1 to the modifier
            float speedModifierParameter = Mathf.Lerp(1.0f, currentGunData.zoomMovementModifier, zoomParameter);
            FirstPersonPlayerComponent.player.AddSpeedModifier(gameObject, speedModifierParameter);

            float lookModifierParameter = Mathf.Lerp(1.0f, currentGunData.zoomLookModifier, zoomParameter);
            FirstPersonPlayerComponent.player.AddLookModifier(gameObject, lookModifierParameter);

            if(reticleOverlayImage != null){
                tintColor.a = parameterSquared;
                reticleOverlayImage.color = tintColor;
            }
        }
    }

    //##############################################################################################
    // Reset if the gun is getting disabled
    //##############################################################################################
    void OnDisable(){
        if(currentGunData.useZoom){
            zoomParameter = 0.0f;

            playerCamera.fieldOfView = defaultFieldOfView;

            if(reticleOverlayImage != null){
                tintColor.a = 0.0f;
                reticleOverlayImage.color = tintColor;
            }
        }
    }

    //##############################################################################################
    // Return if the zoomable gun is currently zooming
    //##############################################################################################
    protected bool Zooming(){
        return currentlyZooming;
    }
}
