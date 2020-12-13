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
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

//##################################################################################################
// Tiled Builder Editor
// Pretty much just add a button to generate map
//##################################################################################################
#if UNITY_EDITOR
[CustomEditor(typeof(TiledBuilder))]
public class TiledBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TiledBuilder tiledBuilder = (TiledBuilder) target;
        if (GUILayout.Button("Generate Tiled Map"))
        {
            tiledBuilder.GenerateMap();
        }
    }
}
#endif // UNITY_EDITOR
