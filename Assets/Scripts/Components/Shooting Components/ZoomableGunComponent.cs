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
//
//##################################################################################################
public class ZoomableGunComponent : GunComponent {
    private const float ZOOM_IN = 1.0f;
    private const float ZOOM_OUT = 0.0f;

    [HeaderAttribute("Zoomable Gun Component (Only needed if gun uses Zoom)")]
    public Camera playerCamera;
    public Image reticleOverlayImage;

    private float defaultFieldOfView;
    private Color tintColor;

    // Used for smooth damping the transition
    private float zoomParameter;
    private float zoomVelocity;

    //##############################################################################################
    // Check if the gun uses zoom, then set it up if it does
    //##############################################################################################
    protected new void Start(){
        base.Start();

        if(currentGunData.useZoom){
            if(playerCamera == null){
                Debug.LogError("Player Camera on " + gameObject.name + "'s ZoomableGunComponent cannot be null");
            }

            if(reticleOverlayImage == null){
                Debug.LogError("Reticle Overlay Image on " + gameObject.name + "'s ZoomableGunComponent cannot be null");
            }

            defaultFieldOfView = playerCamera.fieldOfView;

            tintColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            reticleOverlayImage.color = tintColor;
        }
    }

    //##############################################################################################
    // Update the zoom
    //##############################################################################################
    protected new void Update(){
        base.Update();

        if(currentGunData.useZoom){
            // TODO add a setting for toggling versus hold-to-zoom
            bool zooming = Input.GetMouseButton(1);

            // Smooth damp towards the target zoom
            zoomParameter = Mathf.SmoothDamp(zoomParameter, zooming ? ZOOM_IN : ZOOM_OUT, ref zoomVelocity, currentGunData.zoomTime);

            // Square the result; this gives the fading and zooming a nice ramp-up feel
            float parameterSquared = zoomParameter * zoomParameter;

            // Drive the field of view and reticle overlay alpha from that
            playerCamera.fieldOfView = Mathf.Lerp(defaultFieldOfView, currentGunData.zoomedFieldOfView, parameterSquared);

            tintColor.a = parameterSquared;
            reticleOverlayImage.color = tintColor;
        }
    }

    //##############################################################################################
    // Reset if the gun is getting disabled
    //##############################################################################################
    void OnDisable(){
        if(currentGunData.useZoom){
            zoomParameter = 0.0f;

            playerCamera.fieldOfView = defaultFieldOfView;

            tintColor.a = 0.0f;
            reticleOverlayImage.color = tintColor;
        }
    }
}
