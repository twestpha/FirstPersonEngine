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
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

//##################################################################################################
// Wad Map
// This is a helper struct to pass around data for a 'Map' as expressed in WAD files as several
// lumps.
//##################################################################################################
public class WadMap {
    public const string THINGS_NAME   = "THINGS";
    public const string LINEDEFS_NAME = "LINEDEFS";
    public const string SIDEDEFS_NAME = "SIDEDEFS";
    public const string VERTEXES_NAME = "VERTEXES";
    public const string SEGS_NAME     = "SEGS";
    public const string SSECTORS_NAME = "SSECTORS";
    public const string NODES_NAME    = "NODES";
    public const string SECTORS_NAME  = "SECTORS";
    public const string REJECT_NAME   = "REJECT";
    public const string BLOCKMAP_NAME = "BLOCKMAP";

    public bool valid = false;

    public int ThingsIndex, ThingsSize;
    public int LineDefsIndex, LineDefsSize;
    public int SideDefsIndex, SideDefsSize;
    public int VertexesIndex, VertexesSize;
    public int SegsIndex, SegsSize;
    public int SSectorsIndex, SSectorsSize;
    public int NodesIndex, NodesSize;
    public int SectorsIndex, SectorsSize;
    public int RejectIndex, RejectSize;
    public int BlockmapIndex, BlockmapSize;
}

//##################################################################################################
// Wad Thing
// This is a helper struct to pass around data for a 'Thing' as expressed in WAD files. This is
// a Doom/Heretic style thing, being 10 bytes long.
//
// Note that the attributes in this struct are native ints, not int16s like in the specification.
// They are converted from int16s to ints (preserving value) during parsing.
//##################################################################################################
public class WadThing {
    public const int THING_SIZE = 10;

    public const int X_POSITION_INDEX = 0;
    public const int Y_POSITION_INDEX = 2;
    public const int ANGLE_FACING_INDEX = 4;
    public const int TYPE_INDEX = 6;
    public const int FLAGS_INDEX = 8;

    public int xPosition;
    public int yPosition;
    public int angleFacing;
    public int type;
    public int flags;
}

//##################################################################################################
// Wad Builder
// TODO document me... a lot
//##################################################################################################
[ExecuteInEditMode]
public class WadBuilder : MonoBehaviour {
    private const int LUMPCOUNT_INDEX = 4;
    private const int INFOTABLE_INDEX = 8;

    private const int LUMP_FILEINDEX_OFFSET = 0;
    private const int LUMP_FILESIZE_OFFSET = 4;
    private const int LUMP_FILENAME_OFFSET = 8;

    private const int LUMP_INFO_SIZE = 16;

    public TextAsset wadAsset;
    public float mapScale = 0.1f;
    public string mapToBuildName;
    public WadPaletteData wadPalette;

    public bool run;

    public void Update(){
        if(run){
            run = false;

            ParseAndSetupMap();
        }
    }

    // https://doomwiki.org/wiki/WAD
    public void ParseAndSetupMap(){
        // Get the wad as a byte array
        byte[] wadBytes = wadAsset.bytes;

        // Find the map, and if valid, build it
        WadMap map = GetWadMap(wadBytes, mapToBuildName);

        if(map.valid){
            BuildMap(wadBytes, map);
        } else {
            Debug.LogError("Could not find map '" + mapToBuildName + "' in Wad.");
        }
    }

    //##############################################################################################
    // Using the raw bytes, find the metadata table, then iterate that to find a map that matches
    // the given name. Then, find all the lumps associated with that map, and build the map's
    // attributes.
    //##############################################################################################
    private WadMap GetWadMap(byte[] bytes, string mapName){
        WadMap map = new WadMap();

        // Get the WAD metadata: the number of lumps, and the start of the directory info table
        int lumpCount = Read4(bytes, LUMPCOUNT_INDEX);
        int infoTableIndex = Read4(bytes, INFOTABLE_INDEX);

        // Each infoTableIndex entry consists of 16 bytes.
        // 4 bytes for the file index, 4 bytes for the file size,
        // and 8 bytes of a null-padded character string of the lump's name
        bool foundMap = false;
        for(int i = 0; i < lumpCount; ++i){
            int lumpInfoIndex = infoTableIndex + (i * LUMP_INFO_SIZE);

            // Get lump metadata
            int lumpFileIndex = Read4(bytes, lumpInfoIndex + LUMP_FILEINDEX_OFFSET);
            int lumpFileSize = Read4(bytes, lumpInfoIndex + LUMP_FILESIZE_OFFSET);
            string lumpFileName = GetLumpName(bytes, lumpInfoIndex + LUMP_FILENAME_OFFSET);

            // If we're still looking for the map, check the name
            if(!foundMap){
                if(lumpFileName.Contains(mapName.ToUpper())){
                    Debug.Log("Found " + lumpFileName + "!!!");
                    map.valid = true;
                    foundMap = true;
                }
            // Once we've found the map, sort the following lumps by name into the map data
            } else {
                if(lumpFileName.Contains(WadMap.THINGS_NAME)){
                    map.ThingsIndex = lumpFileIndex;
                    map.ThingsSize  = lumpFileSize;
                } else if(lumpFileName.Contains(WadMap.LINEDEFS_NAME)){
                    map.LineDefsIndex = lumpFileIndex;
                    map.LineDefsSize  = lumpFileSize;
                } else if(lumpFileName.Contains(WadMap.SIDEDEFS_NAME)){
                    map.SideDefsIndex = lumpFileIndex;
                    map.SideDefsSize  = lumpFileSize;
                } else if(lumpFileName.Contains(WadMap.VERTEXES_NAME)){
                    map.VertexesIndex = lumpFileIndex;
                    map.VertexesSize  = lumpFileSize;
                } else if(lumpFileName.Contains(WadMap.SEGS_NAME)){
                    map.SegsIndex = lumpFileIndex;
                    map.SegsSize  = lumpFileSize;
                } else if(lumpFileName.Contains(WadMap.SSECTORS_NAME)){
                    map.SSectorsIndex = lumpFileIndex;
                    map.SSectorsSize  = lumpFileSize;
                } else if(lumpFileName.Contains(WadMap.NODES_NAME)){
                    map.NodesIndex = lumpFileIndex;
                    map.NodesSize  = lumpFileSize;
                } else if(lumpFileName.Contains(WadMap.SECTORS_NAME)){
                    map.SectorsIndex = lumpFileIndex;
                    map.SectorsSize  = lumpFileSize;
                } else if(lumpFileName.Contains(WadMap.REJECT_NAME)){
                    map.RejectIndex = lumpFileIndex;
                    map.RejectSize  = lumpFileSize;
                } else if(lumpFileName.Contains(WadMap.BLOCKMAP_NAME)){
                    map.BlockmapIndex = lumpFileIndex;
                    map.BlockmapSize  = lumpFileSize;
                } else if(lumpFileSize == 0){
                    // Break if we've found an un-parseable, 0-length lump (usually the next map)
                    break;
                }
            }
        }

        return map;
    }

    //##############################################################################################
    // Build the map using the lumps provided and the palette
    //##############################################################################################
    private void BuildMap(byte[] bytes, WadMap map){
        // Destroy all existing children
        while(transform.childCount > 0){
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Parse and instantiate things from palette
        List<WadThing> thingList = ParseThings(bytes, map);

        // Might have to instatiate things after map is built, to get y height for placement
        InstantiateThings(thingList);

        // Build raw geo as groups
        // apply materials, instantiate things
        // ???
        // Profit
    }

    //##############################################################################################
    // Parse the thing lump in the wad and return them as a list of things
    //##############################################################################################
    private List<WadThing> ParseThings(byte[] bytes, WadMap map){
        List<WadThing> wadThings = new List<WadThing>();

        for(int i = map.ThingsIndex; i < map.ThingsIndex + map.ThingsSize; i += WadThing.THING_SIZE){
            WadThing newThing = new WadThing();

            newThing.xPosition = Read2(bytes, i + WadThing.X_POSITION_INDEX);
            newThing.yPosition = Read2(bytes, i + WadThing.Y_POSITION_INDEX);
            newThing.angleFacing = Read2(bytes, i + WadThing.ANGLE_FACING_INDEX);
            newThing.type = Read2(bytes, i + WadThing.TYPE_INDEX);
            newThing.flags = Read2(bytes, i + WadThing.FLAGS_INDEX);

            wadThings.Add(newThing);
        }

        return wadThings;
    }

    //##############################################################################################
    // Instantiate a list of things using the palette and place them in the Unity scene
    //##############################################################################################
    private void InstantiateThings(List<WadThing> thingList){
        // Create a gameobject to parent the things under
        GameObject thingParentObject = new GameObject();
        thingParentObject.name = WadMap.THINGS_NAME;
        thingParentObject.transform.parent = transform;

        // Build a dictionary for looking up the things with the type
        Dictionary<int, GameObject> thingTypeToPrefabLookup = new Dictionary<int, GameObject>();

        for(int i = 0, count = wadPalette.thingsData.Length; i < count; ++i){
            WadPaletteData.ThingData thingData = wadPalette.thingsData[i];

            thingTypeToPrefabLookup[thingData.type] = thingData.prefab;
        }

        // Then, instantiate the thing prefabs with the properties in the wad thing
        for(int i = 0, count = thingList.Count; i < count; ++i){
            WadThing wadThingToInstantiate = thingList[i];

            if(thingTypeToPrefabLookup.ContainsKey(wadThingToInstantiate.type)){
                GameObject thingPrefab = thingTypeToPrefabLookup[wadThingToInstantiate.type];

                // Spawn as a prefab, so changes to it still reflect in editor
                GameObject newThingGameObject = (GameObject)(PrefabUtility.InstantiatePrefab(thingPrefab));

                // name it based on type and index in thing list
                newThingGameObject.name =  "type_" + wadThingToInstantiate.type + "_thing_" + i;

                // Move and orient to correct place
                newThingGameObject.transform.position = new Vector3(
                    mapScale * (float)(wadThingToInstantiate.xPosition),
                    0.0f /* how to get height...? */,
                    mapScale * (float)(wadThingToInstantiate.yPosition)
                );

                // TODO angle facing

                newThingGameObject.transform.parent = thingParentObject.transform;
            } else {
                Debug.LogWarning("WadThing with type " + wadThingToInstantiate.type + " was not found, skipping instantiation.");
            }
        }
    }













    //##############################################################################################
    // Read 2 bytes, interpreted as a signed int16 in little-endian order.
    // Conversion is necessary to return it as a signed native int.
    //##############################################################################################
    private int Read2(byte[] bytes, int index){
        int rawBits = ((int)(bytes[index + 0] << 0))
                    | ((int)(bytes[index + 1] << 8));

        // Detect if highest bit is 1
        int negative = rawBits & 0x8000;

        // Mask that, regardless
        int value    = rawBits & 0x7FFF;

        // If it was negative, offset the value as per 2's complement
        value = negative > 0 ? (value - 0x8000) : value;

        return value;
    }

    //##############################################################################################
    // Read 4 bytes, interpreted as a signed int32 in little-endian order.
    //##############################################################################################
    private int Read4(byte[] bytes, int index){
        return ((int)(bytes[index + 0] << 0))
             | ((int)(bytes[index + 1] << 8))
             | ((int)(bytes[index + 2] << 16))
             | ((int)(bytes[index + 3] << 24));
    }

    //##############################################################################################
    // Read 8 bytes, interpreted as a left-aligned string with null-padding.
    // The ""+ is necessary because a char + a char in c# is an int... thanks, c#.
    //##############################################################################################
    private string GetLumpName(byte[] bytes, int index){
        return "" + ((char)(bytes[index + 0]))
                  + ((char)(bytes[index + 1]))
                  + ((char)(bytes[index + 2]))
                  + ((char)(bytes[index + 3]))
                  + ((char)(bytes[index + 4]))
                  + ((char)(bytes[index + 5]))
                  + ((char)(bytes[index + 6]))
                  + ((char)(bytes[index + 7]));
    }
}
