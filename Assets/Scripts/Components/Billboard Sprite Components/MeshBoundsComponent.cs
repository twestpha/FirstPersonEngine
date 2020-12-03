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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//##################################################################################################
// Mesh Bounds Component
// This class is often placed on billboarded sprites. Unity's culling uses a pre-generated bounding
// box build from the model data, and since sprites have their vertices modified, sometimes it's
// culled and hidden from view, despite the sprite being able to be shown.
// Basically this just modifies the mesh filter bounds at init to be 3D and large.
//##################################################################################################
public class MeshBoundsComponent : MonoBehaviour {
    // This is the size that was most globally useful for me, but it's public so can be changed
    public float boundLength = 4.0f;

    //##############################################################################################
    // Basically just override the bounds with a box with boundLength on every dimension.
    //##############################################################################################
    void Start(){
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        meshFilter.mesh.bounds = new Bounds(
            Vector3.zero,
            new Vector3(boundLength, boundLength, boundLength)
        );
    }
}
