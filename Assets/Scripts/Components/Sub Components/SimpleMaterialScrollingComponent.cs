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

//##################################################################################################
// Sub Components are a toolbox of simple, common building block behaviours, meant to do a single
// thing within the constraints of the other existing components.
//
// Simple Material Scrolling Component
// This script is for scrolling material UV offsets using a sin wave and a linear offset
// These values are all Vector2's for the x and y components
//##################################################################################################
public class SimpleMaterialScrollingComponent : MonoBehaviour {

    public Material material;

    public Vector2 linearVelocity;
    public Vector2 amplitude;
    public Vector2 frequency;

    void Update(){
        float xOffset = (Mathf.Sin(Time.time * frequency.x) * amplitude.x) + (Time.time * linearVelocity.x);
        float yOffset = (Mathf.Sin(Time.time * frequency.y) * amplitude.y) + (Time.time * linearVelocity.y);

        material.SetTextureOffset("_MainTex", new Vector2(xOffset, yOffset));
    }

    //##############################################################################################
    // Reset on 'exit' in editor
    //##############################################################################################
    void OnDestroy(){
        material.SetTextureOffset("_MainTex", new Vector2(0.0f, 0.0f));
    }
}
