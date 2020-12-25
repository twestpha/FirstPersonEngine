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
using UnityEngine;

//##################################################################################################
// Wad Palette Data
// This is a list of all the Unity assets that need to correlate with WAD file data, whether by
// index, name, type, or id number.
///##################################################################################################
[CreateAssetMenu(fileName = "WadPaletteData", menuName = "Shooter/Wad Palette Data", order = 2)]
public class WadPaletteData : ScriptableObject {

    [System.Serializable]
    public class ThingData {
        public int type;
        public GameObject prefab;
    }

    [HeaderAttribute("Thing Lookup Data")]
    public ThingData[] thingsData;

    [System.Serializable]
    public class MaterialData {
        public string name;
        public Material material;
    }

    [HeaderAttribute("Material (Texture) Lookup Data")]
    public MaterialData[] materialData;
}
