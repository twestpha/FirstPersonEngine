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
// Simple Rotation Component
// Rotates the transform of the gameObject around the rotationAxis over time, with rotation speed
// of degrees per second.
//##################################################################################################
public class SimpleRotationComponent : MonoBehaviour {

    public Vector3 rotationAxis;
    public float rotationSpeed;

    void Update(){
        transform.Rotate(rotationAxis * Time.deltaTime * rotationSpeed, Space.World);
    }
}
