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

//##################################################################################################
// TODO document me... a lot
//##################################################################################################

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
// They are converted from int16s to ints (preserving value) during parsing. Also note that most
// following SIZEs an INDEXs are a count of bytes.
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
// Wad Line Def
// This is a helper struct for managing a 'linedef' as expressed in WAD files, in the Doom format,
// being 14 bytes long.
//##################################################################################################
public class WadLineDef {
    public const int LINEDEF_SIZE = 14;

    public const int START_VERTEX_INDEX = 0;
    public const int END_VERTEX_INDEX = 2;
    public const int FLAGS_INDEX = 4;
    public const int SPECIAL_TYPE_INDEX = 6;
    public const int SECTOR_TAG_INDEX = 8;
    public const int FRONT_SIDEDEF_INDEX = 10;
    public const int BACK_SIDEDEF_INDEX = 12;

    public int startVertex;
    public int endVertex;
    public int flags;
    public int specialType;
    public int sectorTag;
    public int frontSideDef;
    public int backSideDef;
}

//##################################################################################################
// Wad Side Def
// This is a helper struct for a 'sidedef' expressed in WAD files. It is 30 bytes long.
//##################################################################################################
public class WadSideDef {
    public const int SIDEDEF_SIZE = 30;

    public const int X_OFFSET_INDEX = 0;
    public const int Y_OFFSET_INDEX = 2;
    public const int UPPER_TEXTURE_NAME_INDEX = 4;
    public const int LOWER_TEXTURE_NAME_INDEX = 12;
    public const int MIDDLE_TEXTURE_NAME_INDEX = 20;
    public const int SECTOR_NUMBER_FACING_INDEX = 28;

    public int xOffset;
    public int yOffset;
    public string upperTextureName;
    public string lowerTextureName;
    public string middleTextureName;
    public int sectorNumberFacing;
}

//##################################################################################################
// Wad Vertex
// This is a helper struct for a 'vertex' expressed in WAD files. It is 4 bytes long.
//##################################################################################################
public class WadVertex {
    public const int VERTEX_SIZE = 4;

    public const int X_OFFSET_INDEX = 0;
    public const int Y_OFFSET_INDEX = 2;

    public int xPosition;
    public int yPosition;
}

//##################################################################################################
// Wad Seg
// This is a helper struct for a 'seg' line segment expressed in WAD files. It is 12 bytes long.
//##################################################################################################
public class WadSeg {
    public const int SEG_SIZE = 12;

    public const int START_VERTEX_NUMBER_INDEX = 0;
    public const int END_VERTEX_NUMBER_INDEX = 2;
    public const int ANGLE_INDEX = 4;
    public const int LINEDEF_NUMBER_INDEX = 6;
    public const int DIRECTION_INDEX = 8;
    public const int DISTANCE_INDEX = 10;

    public int startVertexNumber;
    public int endVertexNumber;
    public int angle;
    public int lineDefNumber;
    public bool direction;
    public int distance;
}

//##################################################################################################
// Wad SSector
// This is a helper struct for a 'ssector' (subsector) expressed in WAD files. It is 4 bytes long.
//##################################################################################################
public class WadSSector {
    public const int SSECTOR_SIZE = 4;

    public const int SEG_COUNT_INDEX = 0;
    public const int FIRST_SEG_NUMBER_INDEX = 2;

    public int segCount;
    public int firstSegNumber;
}

//##################################################################################################
// Wad Node
// This is a helper struct for a 'node' expressed in WAD files. It is 28 bytes long. It also
// contains a helper bounding box struct.
//##################################################################################################
public class WadNode {
    public class WadBoundingBox {
        public const int BOUNDING_BOX_SIZE = 8;

        public const int MAX_Y_INDEX = 0;
        public const int MIN_Y_INDEX = 2;
        public const int MIN_X_INDEX = 4;
        public const int MAX_X_INDEX = 6;

        public int maxY;
        public int minY;
        public int minX;
        public int maxX;
    }

    public const int NODE_SIZE = 28;

    public const int PARTITION_LINE_X_COORDINATE_INDEX = 0;
    public const int PARTITION_LINE_Y_COORDINATE_INDEX = 2;
    public const int PARTITION_DELTA_X_INDEX = 4;
    public const int PARTITION_DELTA_Y_INDEX = 6;
    public const int RIGHT_BOUNDING_BOX_INDEX = 8;
    public const int LEFT_BOUNDING_BOX_INDEX = 16;
    public const int RIGHT_CHILD_INDEX = 24;
    public const int LEFT_CHILD_INDEX = 26;

    public int partitionLineXCoordinate;
    public int partitionLineYCoordinate;
    public int partitionDeltaX;
    public int partitionDeltaY;
    public WadBoundingBox rightBoundingBox;
    public WadBoundingBox leftBoundingBox;
    public int rightChild;
    public int leftChild;
}

//##################################################################################################
// Wad Sector
// This is a helper struct for a 'sector' expressed in WAD files. It is 26 bytes long, and therefore
// the Doom format.
//##################################################################################################
public class WadSector {
    public const int SECTOR_SIZE = 26;

    public const int FLOOR_HEIGHT_INDEX = 0;
    public const int CEILING_HEIGHT_INDEX = 2;
    public const int FLOOR_TEXTURE_NAME_INDEX = 4;
    public const int CEILING_TEXTURE_NAME_INDEX = 12;
    public const int LIGHT_LEVEL_INDEX = 20;
    public const int SPECIAL_TYPE_INDEX = 22;
    public const int TAG_NUMBER_INDEX = 24;

    public int floorHeight;
    public int ceilingHeight;
    public string floorTextureName;
    public string ceilingTextureName;
    public int lightLevel;
    public int specialType;
    public int tagNumber;
}

//##################################################################################################
// Wad Triangle
// This is a helper struct for a triangular polygon. This is used as an intermediate to store data
// before building actual mesh data in the unity scene.
//##################################################################################################
public class WadTriangle {
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;

    public string textureName;

    // TODO what even are these? Offsets...?
    public Vector2 U;
    public Vector2 V;
}

//##################################################################################################
// Wad Builder
// https://doomwiki.org/wiki/WAD
//##################################################################################################
[ExecuteInEditMode]
public class WadBuilder : MonoBehaviour {
    private const int LUMPCOUNT_INDEX = 4;
    private const int INFOTABLE_INDEX = 8;

    private const int LUMP_FILEINDEX_OFFSET = 0;
    private const int LUMP_FILESIZE_OFFSET = 4;
    private const int LUMP_FILENAME_OFFSET = 8;

    private const int INVALID_SIDEDEF_INDEX = -1;
    private const int LUMP_INFO_SIZE = 16;

    // each wall has 3 quads (upper, middle, lower) and 2 tris per quad.
    private const int WALL_TRIANGLE_COUNT = 6; // TODO unused...?

    public TextAsset wadAsset;
    public float mapScale = 0.05f; // Using Doom maps, this looks about right
    public string mapToBuildName;
    public WadPaletteData wadPalette;

    public void LoadAndBuildMap(){
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
    // the given name. Then, find all the lumps associated with that map, and fill out the map's
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
            string lumpFileName = Read8Name(bytes, lumpInfoIndex + LUMP_FILENAME_OFFSET);

            // If we're still looking for the map, check the name
            if(!foundMap){
                if(lumpFileName.Contains(mapName.ToUpper())){
                    Debug.Log("Found map in WAD as " + lumpFileName);
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
    // Build the map using the lumps provided and the palette. This constructs the geometry, applies
    // materials, and places the 'things' as prefabs.
    //##############################################################################################
    private void BuildMap(byte[] bytes, WadMap map){
        // Destroy all existing children
        while(transform.childCount > 0){
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Mark this as dirty so the save asterisk appears
        EditorUtility.SetDirty(gameObject);

        // Parse the maps lumps into more workable data
        List<WadThing> thingList = ParseThings(bytes, map);
        List<WadLineDef> lineDefList = ParseLineDefs(bytes, map);
        List<WadSideDef> sideDefList = ParseSideDefs(bytes, map);
        List<WadVertex> vertexList = ParseVertexes(bytes, map);
        List<WadSeg> segList = ParseSegs(bytes, map);
        List<WadSSector> sSectorList = ParseSSectors(bytes, map);
        List<WadNode> nodeList = ParseNodes(bytes, map);
        List<WadSector> sectorList = ParseSectors(bytes, map);

        // Debug print all the stats from parsing for verifying things loaded correctly
        Debug.Log("Parsed " + thingList.Count + " things (THINGS)");
        Debug.Log("Parsed " + lineDefList.Count + " line definitions (LINEDEFS)");
        Debug.Log("Parsed " + sideDefList.Count + " side definitions (SIDEDEFS)");
        Debug.Log("Parsed " + vertexList.Count + " vertexes (VERTEXES)");
        Debug.Log("Parsed " + segList.Count + " line segments (SEGS)");
        Debug.Log("Parsed " + sSectorList.Count + " sub-sectors (SSECTORS)");
        Debug.Log("Parsed " + nodeList.Count + " nodes (NODES)");
        Debug.Log("Parsed " + sectorList.Count + " sectors (SECTORS)");

        List<WadTriangle> wallTriangles      = new List<WadTriangle>();
        List<WadTriangle> floorTriangles     = new List<WadTriangle>();
        List<WadTriangle> ceilingTriangles   = new List<WadTriangle>();

        BuildTriangles(lineDefList, sideDefList, vertexList, sectorList,
                       ref wallTriangles, ref floorTriangles, ref ceilingTriangles);

        // Then we'll make those polygons into unity shit later

        // Build raw geo while sorting into material groups
        // Also add them to floor, ceiling, and wall groups
        // How to save that model data? Is there writable files?

        // Apply materials to those once they're on game objects (separate and static)

        // Apply collision meshes (just floors)

        // Might have to instatiate things after map is built, to get y height for placement
        InstantiateThings(thingList);

        InstantiateWalls(wallTriangles);

        // Apply collision meshes (walls and ceilings)
    }

    //##############################################################################################
    // Parse the thing lump in the wad and return them as a list of things
    //##############################################################################################
    private List<WadThing> ParseThings(byte[] bytes, WadMap map){
        List<WadThing> wadThings = new List<WadThing>();

        for(int i = map.ThingsIndex; i < map.ThingsIndex + map.ThingsSize; i += WadThing.THING_SIZE){
            WadThing newThing = new WadThing();

            newThing.xPosition   = Read2(bytes, i + WadThing.X_POSITION_INDEX);
            newThing.yPosition   = Read2(bytes, i + WadThing.Y_POSITION_INDEX);
            newThing.angleFacing = Read2(bytes, i + WadThing.ANGLE_FACING_INDEX);
            newThing.type        = Read2(bytes, i + WadThing.TYPE_INDEX);
            newThing.flags       = Read2(bytes, i + WadThing.FLAGS_INDEX);

            wadThings.Add(newThing);
        }

        return wadThings;
    }

    //##############################################################################################
    // Parse the linedef lump in the wad, and return the constructed list of linedefs
    //##############################################################################################
    private List<WadLineDef> ParseLineDefs(byte[] bytes, WadMap map){
        List<WadLineDef> wadLineDefs = new List<WadLineDef>();

        for(int i = map.LineDefsIndex; i < map.LineDefsIndex + map.LineDefsSize; i += WadLineDef.LINEDEF_SIZE){
            WadLineDef newLineDef = new WadLineDef();

            newLineDef.startVertex  = Read2(bytes, i + WadLineDef.START_VERTEX_INDEX);
            newLineDef.endVertex    = Read2(bytes, i + WadLineDef.END_VERTEX_INDEX);
            newLineDef.flags        = Read2(bytes, i + WadLineDef.FLAGS_INDEX);
            newLineDef.specialType  = Read2(bytes, i + WadLineDef.SPECIAL_TYPE_INDEX);
            newLineDef.sectorTag    = Read2(bytes, i + WadLineDef.SECTOR_TAG_INDEX);
            newLineDef.frontSideDef = Read2(bytes, i + WadLineDef.FRONT_SIDEDEF_INDEX);
            newLineDef.backSideDef  = Read2(bytes, i + WadLineDef.BACK_SIDEDEF_INDEX);

            wadLineDefs.Add(newLineDef);
        }

        return wadLineDefs;
    }

    //##############################################################################################
    // Parse the sidedef lump in the wad, and return the constructed list of sidedefs
    //##############################################################################################
    private List<WadSideDef> ParseSideDefs(byte[] bytes, WadMap map){
        List<WadSideDef> wadSideDefs = new List<WadSideDef>();

        for(int i = map.SideDefsIndex; i < map.SideDefsIndex + map.SideDefsSize; i += WadSideDef.SIDEDEF_SIZE){
            WadSideDef newSideDef = new WadSideDef();

            newSideDef.xOffset            = Read2(bytes, i + WadSideDef.X_OFFSET_INDEX);
            newSideDef.yOffset            = Read2(bytes, i + WadSideDef.Y_OFFSET_INDEX);
            newSideDef.upperTextureName   = Read8Name(bytes, i + WadSideDef.UPPER_TEXTURE_NAME_INDEX);
            newSideDef.lowerTextureName   = Read8Name(bytes, i + WadSideDef.LOWER_TEXTURE_NAME_INDEX);
            newSideDef.middleTextureName  = Read8Name(bytes, i + WadSideDef.MIDDLE_TEXTURE_NAME_INDEX);
            newSideDef.sectorNumberFacing = Read2(bytes, i + WadSideDef.SECTOR_NUMBER_FACING_INDEX);

            wadSideDefs.Add(newSideDef);
        }

        return wadSideDefs;
    }

    //##############################################################################################
    // Parse the vertex lump in the wad, and return the constructed list of vertexes
    //##############################################################################################
    private List<WadVertex> ParseVertexes(byte[] bytes, WadMap map){
        List<WadVertex> wadVertexes = new List<WadVertex>();

        for(int i = map.VertexesIndex; i < map.VertexesIndex + map.VertexesSize; i += WadVertex.VERTEX_SIZE){
            WadVertex newVertex = new WadVertex();

            newVertex.xPosition = Read2(bytes, i + WadVertex.X_OFFSET_INDEX);
            newVertex.yPosition = Read2(bytes, i + WadVertex.Y_OFFSET_INDEX);

            wadVertexes.Add(newVertex);
        }

        return wadVertexes;
    }

    //##############################################################################################
    // Parse the seg lump in the wad, and return the constructed list of segs
    //##############################################################################################
    private List<WadSeg> ParseSegs(byte[] bytes, WadMap map){
        List<WadSeg> wadSegs = new List<WadSeg>();

        for(int i = map.SegsIndex; i < map.SegsIndex + map.SegsSize; i += WadSeg.SEG_SIZE){
            WadSeg newSeg = new WadSeg();

            newSeg.startVertexNumber = Read2(bytes, i + WadSeg.START_VERTEX_NUMBER_INDEX);
            newSeg.endVertexNumber   = Read2(bytes, i + WadSeg.END_VERTEX_NUMBER_INDEX);
            newSeg.angle             = Read2(bytes, i + WadSeg.ANGLE_INDEX);
            newSeg.lineDefNumber     = Read2(bytes, i + WadSeg.LINEDEF_NUMBER_INDEX);
            newSeg.direction         = Read2(bytes, i + WadSeg.DIRECTION_INDEX) > 0; // Simple bool cast
            newSeg.distance          = Read2(bytes, i + WadSeg.DISTANCE_INDEX);

            wadSegs.Add(newSeg);
        }

        return wadSegs;
    }

    //##############################################################################################
    // Parse the ssector lump in the wad, and return the constructed list of ssectors
    //##############################################################################################
    private List<WadSSector> ParseSSectors(byte[] bytes, WadMap map){
        List<WadSSector> wadSSectors = new List<WadSSector>();

        for(int i = map.SSectorsIndex; i < map.SSectorsIndex + map.SSectorsSize; i += WadSSector.SSECTOR_SIZE){
            WadSSector newSSector = new WadSSector();

            newSSector.segCount       = Read2(bytes, i + WadSSector.SEG_COUNT_INDEX);
            newSSector.firstSegNumber = Read2(bytes, i + WadSSector.FIRST_SEG_NUMBER_INDEX);

            wadSSectors.Add(newSSector);
        }

        return wadSSectors;
    }

    //##############################################################################################
    // Parse the node lump in the wad, and return the constructed list of nodes
    //##############################################################################################
    private List<WadNode> ParseNodes(byte[] bytes, WadMap map){
        List<WadNode> wadNodes = new List<WadNode>();

        for(int i = map.NodesIndex; i < map.NodesIndex + map.NodesSize; i += WadNode.NODE_SIZE){
            WadNode newNode = new WadNode();

            newNode.partitionLineXCoordinate = Read2(bytes, i + WadNode.PARTITION_LINE_X_COORDINATE_INDEX);
            newNode.partitionLineYCoordinate = Read2(bytes, i + WadNode.PARTITION_LINE_Y_COORDINATE_INDEX);
            newNode.partitionDeltaX          = Read2(bytes, i + WadNode.PARTITION_DELTA_X_INDEX);
            newNode.partitionDeltaY          = Read2(bytes, i + WadNode.PARTITION_DELTA_Y_INDEX);
            newNode.rightBoundingBox         = ReadBoundingBox(bytes, i + WadNode.RIGHT_BOUNDING_BOX_INDEX);
            newNode.leftBoundingBox          = ReadBoundingBox(bytes, i + WadNode.LEFT_BOUNDING_BOX_INDEX);
            newNode.rightChild               = Read2(bytes, i + WadNode.RIGHT_CHILD_INDEX);
            newNode.leftChild                = Read2(bytes, i + WadNode.LEFT_CHILD_INDEX);

            wadNodes.Add(newNode);
        }

        return wadNodes;
    }

    //##############################################################################################
    // Parse the sector lump in the wad, and return the constructed list of sectors
    //##############################################################################################
    private List<WadSector> ParseSectors(byte[] bytes, WadMap map){
        List<WadSector> wadSectors = new List<WadSector>();

        for(int i = map.SectorsIndex; i < map.SectorsIndex + map.SectorsSize; i += WadSector.SECTOR_SIZE){
            WadSector newSector = new WadSector();

            newSector.floorHeight        = Read2(bytes, i + WadSector.FLOOR_HEIGHT_INDEX);
            newSector.ceilingHeight      = Read2(bytes, i + WadSector.CEILING_HEIGHT_INDEX);
            newSector.floorTextureName   = Read8Name(bytes, i + WadSector.FLOOR_TEXTURE_NAME_INDEX);
            newSector.ceilingTextureName = Read8Name(bytes, i + WadSector.CEILING_TEXTURE_NAME_INDEX);
            newSector.lightLevel         = Read2(bytes, i + WadSector.LIGHT_LEVEL_INDEX);
            newSector.specialType        = Read2(bytes, i + WadSector.SPECIAL_TYPE_INDEX);
            newSector.tagNumber          = Read2(bytes, i + WadSector.TAG_NUMBER_INDEX);

            wadSectors.Add(newSector);
        }

        return wadSectors;
    }

    //##############################################################################################
    // Built the triangles based on line, side, vertex, and sector definitions, and output them
    // into the appropriate buffers
    //##############################################################################################
    private void BuildTriangles(List<WadLineDef> lineDefs, List<WadSideDef> sideDefs, List<WadVertex> vertexList, List<WadSector> sectorList,
                                ref List<WadTriangle> wallTriangles, ref List<WadTriangle> floorTriangles, ref List<WadTriangle> ceilingTriangles){

        // Iterate over all the line definitions, building the walls
        for(int i = 0, count = lineDefs.Count; i < count; ++i){
            WadLineDef lineDef = lineDefs[i];

            WadSideDef frontSideDef = sideDefs[lineDef.frontSideDef];
            WadSideDef backSideDef  = lineDef.backSideDef == INVALID_SIDEDEF_INDEX ? null : sideDefs[lineDef.backSideDef];

            WadSector frontSideSector = sectorList[frontSideDef.sectorNumberFacing];
            WadSector backSideSector  = backSideDef == null ? null : sectorList[backSideDef.sectorNumberFacing];

            Vector3 startVertexFlat = VertexInScaled3D(vertexList[lineDef.startVertex]);
            Vector3 endVertexFlat   = VertexInScaled3D(vertexList[lineDef.endVertex]);

            // Each wall can have up to 3 quads associated with it; lower, middle, and upper
            // Each of these has four vertexes, in the pattern below:
            // B-----C
            // |     |
            // A-----D
            // Where ABD and DAC form the two triangles of that quad, and A and B correspond vertically
            // with the start vertex, and C and D with the end vertex

            // Simple case, we are a one-sided sidedef
            if(backSideSector == null){
                // Don't add triangles if there's no texture
                if(!string.IsNullOrEmpty(frontSideDef.middleTextureName)){
                    Vector3 wallA = startVertexFlat + (Vector3.up * ConvertToScaled(frontSideSector.floorHeight));
                    Vector3 wallB = startVertexFlat + (Vector3.up * ConvertToScaled(frontSideSector.ceilingHeight));
                    Vector3 wallC = endVertexFlat + (Vector3.up * ConvertToScaled(frontSideSector.ceilingHeight));
                    Vector3 wallD = endVertexFlat + (Vector3.up * ConvertToScaled(frontSideSector.floorHeight));

                    WadTriangle simpleWallTriangleA = new WadTriangle();
                    simpleWallTriangleA.A = wallA;
                    simpleWallTriangleA.B = wallB;
                    simpleWallTriangleA.C = wallC;
                    simpleWallTriangleA.textureName = frontSideDef.middleTextureName;
                    // TODO UVs

                    WadTriangle simpleWallTriangleB = new WadTriangle();
                    simpleWallTriangleB.A = wallD;
                    simpleWallTriangleB.B = wallA;
                    simpleWallTriangleB.C = wallC;
                    simpleWallTriangleB.textureName = frontSideDef.middleTextureName;
                    // TODO UVs

                    wallTriangles.Add(simpleWallTriangleA);
                    wallTriangles.Add(simpleWallTriangleB);

                    // Debug.DrawLine(wallA, wallB, Color.red, 10.0f);
                    // Debug.DrawLine(wallB, wallC, Color.red, 10.0f);
                    // Debug.DrawLine(wallC, wallD, Color.red, 10.0f);
                    // Debug.DrawLine(wallD, wallA, Color.red, 10.0f);
                }
            } else {
                // Get the lower of the back/front side floor heights
                int lowFloor = 0;
                int middleFloor = 0;
                bool frontSideLow = false;
                bool floorDeltaZero = false;

                if(frontSideSector.floorHeight > backSideSector.floorHeight){
                    lowFloor = backSideSector.floorHeight;
                    middleFloor = frontSideSector.floorHeight;
                    frontSideLow = false;
                } else {
                    lowFloor = frontSideSector.floorHeight;
                    middleFloor = backSideSector.floorHeight;
                    frontSideLow = true;
                }

                floorDeltaZero = (frontSideSector.floorHeight - backSideSector.floorHeight) == 0;

                // Get the higher of the back/front side ceiling heights
                int middleCeiling = 0;
                int highCeiling = 0;
                bool frontSideHigh = false;
                bool ceilingDeltaZero = false;

                if(frontSideSector.ceilingHeight > backSideSector.ceilingHeight){
                    middleCeiling = backSideSector.ceilingHeight;
                    highCeiling = frontSideSector.ceilingHeight;
                    frontSideHigh = false;
                } else {
                    middleCeiling = frontSideSector.ceilingHeight;
                    highCeiling = backSideSector.ceilingHeight;
                    frontSideHigh = true;
                }

                ceilingDeltaZero = (frontSideSector.ceilingHeight - backSideSector.ceilingHeight) == 0;

                bool middleDeltaZero = (middleCeiling - middleFloor) == 0;

                // Get all the vertices set up based on lower, middle, and upper quads
                Vector3 lowerA = startVertexFlat + (Vector3.up * ConvertToScaled(lowFloor));
                Vector3 lowerB = startVertexFlat + (Vector3.up * ConvertToScaled(middleFloor));
                Vector3 lowerC = endVertexFlat + (Vector3.up * ConvertToScaled(middleFloor));
                Vector3 lowerD = endVertexFlat + (Vector3.up * ConvertToScaled(lowFloor));

                // Debug.DrawLine(lowerA, lowerB, Color.blue, 10.0f);
                // Debug.DrawLine(lowerB, lowerC, Color.blue, 10.0f);
                // Debug.DrawLine(lowerC, lowerD, Color.blue, 10.0f);
                // Debug.DrawLine(lowerD, lowerA, Color.blue, 10.0f);

                Vector3 middleA = startVertexFlat + (Vector3.up * ConvertToScaled(middleFloor));
                Vector3 middleB = startVertexFlat + (Vector3.up * ConvertToScaled(middleCeiling));
                Vector3 middleC = endVertexFlat + (Vector3.up * ConvertToScaled(middleCeiling));
                Vector3 middleD = endVertexFlat + (Vector3.up * ConvertToScaled(middleFloor));

                // Debug.DrawLine(middleA, middleB, Color.green, 10.0f);
                // Debug.DrawLine(middleB, middleC, Color.green, 10.0f);
                // Debug.DrawLine(middleC, middleD, Color.green, 10.0f);
                // Debug.DrawLine(middleD, middleA, Color.green, 10.0f);

                Vector3 upperA = startVertexFlat + (Vector3.up * ConvertToScaled(middleCeiling));
                Vector3 upperB = startVertexFlat + (Vector3.up * ConvertToScaled(highCeiling));
                Vector3 upperC = endVertexFlat + (Vector3.up * ConvertToScaled(highCeiling));
                Vector3 upperD = endVertexFlat + (Vector3.up * ConvertToScaled(middleCeiling));

                // Debug.DrawLine(upperA, upperB, Color.white, 10.0f);
                // Debug.DrawLine(upperB, upperC, Color.white, 10.0f);
                // Debug.DrawLine(upperC, upperD, Color.white, 10.0f);
                // Debug.DrawLine(upperD, upperA, Color.white, 10.0f);

                // Something is fucky here...

                if(!floorDeltaZero){
                    // figure out which wall (front or back) is the low wall, then create the quad for that
                    WadTriangle lowerWallTriangleA = new WadTriangle();
                    WadTriangle lowerWallTriangleB = new WadTriangle();

                    if(frontSideLow){
                        lowerWallTriangleA.A = lowerA;
                        lowerWallTriangleA.B = lowerB;
                        lowerWallTriangleA.C = lowerC;
                        lowerWallTriangleA.textureName = frontSideDef.lowerTextureName;
                        // TODO UVs

                        lowerWallTriangleB.A = lowerD;
                        lowerWallTriangleB.B = lowerA;
                        lowerWallTriangleB.C = lowerC;
                        lowerWallTriangleB.textureName = frontSideDef.lowerTextureName;
                        // TODO UVs
                    } else {
                        lowerWallTriangleA.A = lowerD;
                        lowerWallTriangleA.B = lowerC;
                        lowerWallTriangleA.C = lowerB;
                        lowerWallTriangleA.textureName = backSideDef.lowerTextureName;
                        // TODO UVs

                        lowerWallTriangleB.A = lowerB;
                        lowerWallTriangleB.B = lowerA;
                        lowerWallTriangleB.C = lowerD;
                        lowerWallTriangleB.textureName = backSideDef.lowerTextureName;
                        // TODO UVs
                    }

                    if(!string.IsNullOrEmpty(lowerWallTriangleA.textureName)){
                        wallTriangles.Add(lowerWallTriangleA);
                    }

                    if(!string.IsNullOrEmpty(lowerWallTriangleB.textureName)){
                        wallTriangles.Add(lowerWallTriangleB);
                    }
                }

                /*if(!ceilingDeltaZero){
                    // figure out which wall (front or back) is the low wall, then create the quad for that
                    WadTriangle middleWallTriangleA = new WadTriangle();
                    WadTriangle middleWallTriangleB = new WadTriangle();

                    if(frontSideLow){
                        middleWallTriangleA.A = middleA;
                        middleWallTriangleA.B = middleB;
                        middleWallTriangleA.C = middleC;
                        middleWallTriangleA.textureName = frontSideDef.middleTextureName;
                        // TODO UVs

                        middleWallTriangleB.A = middleD;
                        middleWallTriangleB.B = middleA;
                        middleWallTriangleB.C = middleC;
                        middleWallTriangleB.textureName = frontSideDef.middleTextureName;
                        // TODO UVs
                    } else {
                        middleWallTriangleA.A = middleD;
                        middleWallTriangleA.B = middleC;
                        middleWallTriangleA.C = middleB;
                        middleWallTriangleA.textureName = backSideDef.middleTextureName;
                        // TODO UVs

                        middleWallTriangleB.A = middleB;
                        middleWallTriangleB.B = middleA;
                        middleWallTriangleB.C = middleD;
                        middleWallTriangleB.textureName = backSideDef.middleTextureName;
                        // TODO UVs
                    }

                    if(!string.IsNullOrEmpty(middleWallTriangleA.textureName)){
                        wallTriangles.Add(middleWallTriangleA);
                    }

                    if(!string.IsNullOrEmpty(middleWallTriangleB.textureName)){
                        wallTriangles.Add(middleWallTriangleB);
                    }
                }*/

                if(!ceilingDeltaZero){
                    // figure out which wall (front or back) is the low wall, then create the quad for that
                    WadTriangle upperWallTriangleA = new WadTriangle();
                    WadTriangle upperWallTriangleB = new WadTriangle();

                    if(frontSideHigh){
                        upperWallTriangleA.A = upperA;
                        upperWallTriangleA.B = upperB;
                        upperWallTriangleA.C = upperC;
                        upperWallTriangleA.textureName = frontSideDef.upperTextureName;
                        // TODO UVs

                        upperWallTriangleB.A = upperD;
                        upperWallTriangleB.B = upperA;
                        upperWallTriangleB.C = upperC;
                        upperWallTriangleB.textureName = frontSideDef.upperTextureName;
                        // TODO UVs
                    } else {
                        upperWallTriangleA.A = upperD;
                        upperWallTriangleA.B = upperC;
                        upperWallTriangleA.C = upperB;
                        upperWallTriangleA.textureName = backSideDef.upperTextureName;
                        // TODO UVs

                        upperWallTriangleB.A = upperB;
                        upperWallTriangleB.B = upperA;
                        upperWallTriangleB.C = upperD;
                        upperWallTriangleB.textureName = backSideDef.upperTextureName;
                        // TODO UVs
                    }

                    if(!string.IsNullOrEmpty(upperWallTriangleA.textureName)){
                        wallTriangles.Add(upperWallTriangleA);
                    }

                    if(!string.IsNullOrEmpty(upperWallTriangleB.textureName)){
                        wallTriangles.Add(upperWallTriangleB);
                    }
                }
            }
        }

        // For the sectors, we need to map sector to linedef to get the vertexes
        Dictionary<WadSector, List<WadLineDef>> sectorToLineLookup = new Dictionary<WadSector, List<WadLineDef>>();

        for(int i = 0, count = lineDefs.Count; i < count; ++i){
            WadLineDef lineDef = lineDefs[i];

            WadSideDef frontSideDef = sideDefs[lineDef.frontSideDef];
            WadSideDef backSideDef  = lineDef.backSideDef == INVALID_SIDEDEF_INDEX ? null : sideDefs[lineDef.backSideDef];

            WadSector frontSideSector = sectorList[frontSideDef.sectorNumberFacing];
            WadSector backSideSector  = backSideDef == null ? null : sectorList[backSideDef.sectorNumberFacing];

            // Add the line for the front (and back, if it exists) sector
            if(!sectorToLineLookup.ContainsKey(frontSideSector)){
                sectorToLineLookup[frontSideSector] = new List<WadLineDef>();
            }

            sectorToLineLookup[frontSideSector].Add(lineDef);

            if(backSideSector != null){
                if(!sectorToLineLookup.ContainsKey(backSideSector)){
                    sectorToLineLookup[backSideSector] = new List<WadLineDef>();
                }

                sectorToLineLookup[backSideSector].Add(lineDef);
            }
        }

        // sectorToLineLookup
        // I think we have to use a mix of algorithms
        // if we have one single continuous loop of lines (no inner islands), we can use the corner-walking algorithm (maybe?)
        // if we have inner islands, bridge them, then do the corner-walking?

        int k = 0;
        foreach(var sectorToLine in sectorToLineLookup){
            List<WadLineDef> sectorLineDefs = sectorToLine.Value;
            List<WadLineDef> exploredLineDefs = new List<WadLineDef>();

            // Test the shape of the vertex graph by iterating over as many lines as possible, starting
            // from a given vertex. If this iterates all of the lines of that graph, this indicates that
            // the sector has no separate islands of vertexes, and is a single-cycle graph.
            int loopStartVertexIndex = sectorLineDefs[0].startVertex;
            int nextVertexIndex = sectorLineDefs[0].endVertex;
            exploredLineDefs.Add(sectorLineDefs[0]);

            int loopCount = 1; // Starts at one because of the first link

            // Shit
            int iterations = 0;

            while(loopStartVertexIndex != nextVertexIndex){
                foreach(var line in sectorLineDefs){
                    if((line.startVertex == nextVertexIndex || line.endVertex == nextVertexIndex) && !exploredLineDefs.Contains(line)){
                        exploredLineDefs.Add(line);

                        // Use the opposite vertex as the next vertex
                        if(line.startVertex == nextVertexIndex){
                            nextVertexIndex = line.endVertex;
                        } else {
                            nextVertexIndex = line.startVertex;
                        }

                        break;
                    }
                }

                loopCount++;

                // Bad state
                iterations++;
                if(iterations > 2000){
                    Debug.LogError("ALSKJDKLAS");
                    return;
                }
            }

            // sector has disjoint islands if the count of any loop is not the same as the number of line defs in that sector
            bool sectorHasIslands = loopCount != sectorLineDefs.Count;
            // Debug.Log("loopCount: " + loopCount);
            // Debug.Log("sectorLineDefs.Count: " + sectorLineDefs.Count);
            Debug.Log("Sector (" + sectorToLine.Key.floorTextureName + ", " + sectorToLine.Key.ceilingTextureName + ") has islands: " + sectorHasIslands);
            Debug.Log(sectorHasIslands);
            k++;

            // Once we know if we have islands, we can do... something?

            // break;
        }
    }


    //##############################################################################################
    // Instantiate a list of things using the palette and place them in the Unity scene
    //##############################################################################################
    private void InstantiateThings(List<WadThing> thingList){
        // Create a gameobject to parent the things under
        GameObject thingParentObject = new GameObject();
        thingParentObject.name = WadMap.THINGS_NAME;
        thingParentObject.transform.parent = transform;
        thingParentObject.transform.localPosition = Vector3.zero;

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
                newThingGameObject.transform.localPosition = new Vector3(
                    mapScale * (float)(wadThingToInstantiate.xPosition),
                    0.0f /* how to get height...? */,
                    mapScale * (float)(wadThingToInstantiate.yPosition)
                );

                // TODO angle facing

                newThingGameObject.transform.parent = thingParentObject.transform;
            } else {
                Debug.LogWarning("WadThing with type '" + wadThingToInstantiate.type + "' was not found, skipping instantiation.");
            }
        }
    }

    //##############################################################################################
    // Instantiate a list of walls as geometry, using the palette and place them in the Unity scene
    //##############################################################################################
    private void InstantiateWalls(List<WadTriangle> wallTriangles){
        // Create a gameobject to parent the walls under
        GameObject thingParentObject = new GameObject();
        thingParentObject.name = "WALLS";
        thingParentObject.transform.parent = transform;
        thingParentObject.transform.localPosition = Vector3.zero;

        // Group the triangles by texture
        Dictionary<string, List<WadTriangle>> textureNameToTrianglesLookup = new Dictionary<string, List<WadTriangle>>();
        for(int i = 0, count = wallTriangles.Count; i < count; ++i){
            WadTriangle tri = wallTriangles[i];

            if(!textureNameToTrianglesLookup.ContainsKey(tri.textureName)){
                textureNameToTrianglesLookup[tri.textureName] = new List<WadTriangle>();
            }

            textureNameToTrianglesLookup[tri.textureName].Add(tri);
        }

        // Then, build vertice, uv, and triangle lists as per the unity format,
        // and then add them to instantiated meshes on a gameobject with an appropriate material
        // from the palette
        foreach(var textureNameAndTriangles in textureNameToTrianglesLookup){

            // First, format the wad triangles as unity verts and triangle indices
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals  = new List<Vector3>();
            List<Vector2> uvs      = new List<Vector2>();
            List<int> triangles    = new List<int>();

            string textureName = textureNameAndTriangles.Key.Trim((char)(0)).ToUpper();
            List<WadTriangle> trianglesOfThisTexture = textureNameAndTriangles.Value;

            int triangleIndex = 0;
            foreach(var triangle in trianglesOfThisTexture){
                vertices.Add(triangle.A);
                vertices.Add(triangle.B);
                vertices.Add(triangle.C);

                // Calculate a normal for the face based on winding order
                Vector3 flatNormal = Vector3.Cross(triangle.B - triangle.A, triangle.C - triangle.A);
                normals.Add(flatNormal);
                normals.Add(flatNormal);
                normals.Add(flatNormal);

                // TODO uvs
                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(1.0f, 0.0f));
                uvs.Add(new Vector2(1.0f, 1.0f));

                triangles.Add(triangleIndex + 0);
                triangles.Add(triangleIndex + 1);
                triangles.Add(triangleIndex + 2);
                triangleIndex += 3;
            }

            // Then, setup the gameObject and apply the triangles to it's mesh
            GameObject meshGameObject = new GameObject();
            meshGameObject.name = textureName;
            meshGameObject.transform.parent = thingParentObject.transform;
            meshGameObject.transform.localPosition = Vector3.zero;

            // It's possible some sector flags make this not true, for moving sectors, but we're not supporting that yet
            meshGameObject.isStatic = true;

            // Add all the necessary components
            MeshFilter meshFilter     = meshGameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
            MeshRenderer meshRenderer = meshGameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            MeshCollider collider     = meshGameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;

            // Add the mesh in the proper format
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);

            meshFilter.mesh = mesh;

            // Add the material looked up in the palette data. This isn't ideal, but we need to use
            // the contains() call for partial matches
            // for(int i = 0, count = wadPalette.materialData.Length; i < count; ++i){
            //     WadPaletteData.MaterialData materialData = wadPalette.materialData[i];
            //
            //     if(textureName == materialData.name){
            //         meshRenderer.sharedMaterial = materialData.material;
            //         break;
            //     }
            // }

            meshRenderer.sharedMaterial = wadPalette.materialData[0].material;

            if(meshRenderer.sharedMaterial == null){
                Debug.LogWarning("WadTriangle with texture '" + textureName + "' was not found, skipping applying material.");
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
    // Trim the null bytes for readability
    //##############################################################################################
    private string Read8Name(byte[] bytes, int index){
        return "" + ((char)(bytes[index + 0]))
                  + ((char)(bytes[index + 1]))
                  + ((char)(bytes[index + 2]))
                  + ((char)(bytes[index + 3]))
                  + ((char)(bytes[index + 4]))
                  + ((char)(bytes[index + 5]))
                  + ((char)(bytes[index + 6]))
                  + ((char)(bytes[index + 7]));
    }

    //##############################################################################################
    // Read a bounding box starting at index
    //##############################################################################################
    private WadNode.WadBoundingBox ReadBoundingBox(byte[] bytes, int index){
        WadNode.WadBoundingBox newBoundingBox = new WadNode.WadBoundingBox();

        newBoundingBox.maxY = Read2(bytes, index + WadNode.WadBoundingBox.MAX_Y_INDEX);
        newBoundingBox.minY = Read2(bytes, index + WadNode.WadBoundingBox.MIN_Y_INDEX);
        newBoundingBox.minX = Read2(bytes, index + WadNode.WadBoundingBox.MIN_X_INDEX);
        newBoundingBox.maxX = Read2(bytes, index + WadNode.WadBoundingBox.MAX_X_INDEX);

        return newBoundingBox;
    }

    //##############################################################################################
    // Small helper function to convert vertex to unity 3d
    //##############################################################################################
    private Vector3 VertexInScaled3D(WadVertex vertex){
        return new Vector3(ConvertToScaled(vertex.xPosition), 0.0f /* assume 0 */, ConvertToScaled(vertex.yPosition));
    }

    //##############################################################################################
    // Small helper function to convert integer representation to scaled value
    //##############################################################################################
    private float ConvertToScaled(int value){
        return mapScale * (float)(value);
    }
}

#endif // UNITY_EDITOR
