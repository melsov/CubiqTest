using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cubiquity;
using UnityEngine;

namespace VE.VoxelGen
{
    public enum VoxelType
    {
        Grass, Dirt
    }

    //
    // Hacky mechanism for providing UV offsets to 
    // the moded version of the color cubes shader
    //
    public class VoxelTypeToColor : MonoBehaviour
    {

        [System.Serializable]
        public struct TypeAndColor
        {
            public VoxelType type;
            public Color color;
        }

        //[SerializeField, Header("Number of columns and rows in the tile map")]
        //Vector2Int tileMapDimensions = new Vector2Int(4, 4);

        [SerializeField, Header("Put one entry per voxel type here")]
        TypeAndColor[] colorsPerType = new TypeAndColor[1];

        Dictionary<VoxelType, QuantizedColor> _lookup;

        Dictionary<VoxelType, QuantizedColor> lookup {
            get {
                if(_lookup == null)
                {
                    _lookup = new Dictionary<VoxelType, QuantizedColor>();
                    foreach (var perType in colorsPerType)
                    {
                        if (!_lookup.ContainsKey(perType.type))
                        {
                            _lookup.Add(perType.type, (QuantizedColor)perType.color);
                                //new QuantizedColor(
                                //(byte)(255 * (perType.offset.x / (float)tileMapDimensions.x)),
                                //(byte)(255 * (perType.offset.y / (float)tileMapDimensions.y)),
                                //0,
                                //255)); // z and w coords mean nothing at the moment (w (alpha) is 255 incase this is ever used as an actual color)

                        }
                    }
                }
                return _lookup;
            }
        }

        public QuantizedColor getQuantizedColor(VoxelType type)
        {
            return lookup[type];
        }


    }
}
