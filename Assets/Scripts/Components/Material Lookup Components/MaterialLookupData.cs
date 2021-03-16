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
using System;

//##################################################################################################
// Material Name
// Used for lookups for material properties based on name
// Meant to be extended or modified to be game-specific
//##################################################################################################
public enum MaterialName {
    // The lookup component will warn if there is no entry marked as fallback
    Fallback,

    Metal,
    Dirt,
    // ...
}

//##################################################################################################
// Material Spawnable Type
// Used to lookup multiple spawnables based on type
// Meant to be extended or modified to be game-specific
//##################################################################################################
public enum MaterialSpawnableType {
    Decal,
    ImpactEffects,
    // ...
}

//##################################################################################################
// Material Sound Type
// Used to lookup multiple sound effects based on type
// Meant to be extended or modified to be game-specific
//##################################################################################################
public enum MaterialSoundType {
    Impact,
    Footstep,
    // ...
}

//##################################################################################################
// Material Spawnable
//##################################################################################################
[Serializable]
public class MaterialSpawnable {
    public MaterialSpawnableType type;
    public GameObject spawnable;
}

//##################################################################################################
// Material Sound
//##################################################################################################
[Serializable]
public class MaterialSound {
    public MaterialSoundType type;
    public AudioClip sound;
}

//##################################################################################################
// Material Lookup Entry
// Contains a set of spawnable gameobjects, marked with type, for when they should be spawned
// Also contains the set of soundes, again, marked with type, for when they should be spawned
//##################################################################################################
[Serializable]
public class MaterialLookupEntry {
    public MaterialName materialName;

    [Header("Spawnables")]
    public MaterialSpawnable[] spawnables;

    [Header("Sound Effects")]
    public MaterialSound[] sounds;
}

//##################################################################################################
// Material Lookup Data
// Contains a set of lookups of Material Names and their associated properties
// Used for things like collisions, bullet decals, and sound effects of a given material
//##################################################################################################
[CreateAssetMenu(fileName = "MaterialLookupData", menuName = "Shooter/Material Lookup Data", order = 1)]
public class MaterialLookupData : ScriptableObject {
    public MaterialLookupEntry[] lookups;
}
