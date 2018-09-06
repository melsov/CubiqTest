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
;

        //
        // At some point, this program might want to use transparent or semi transparent colors.
        // But if that's not the case, fully transparent colors can cause confusion
        // because its easy to forget to slide the alpha slider in Unity and
        // Cubiquity considers voxels with alpha = 0 to be empty / non-existent.
        //
        static bool IWantFullyTransparentColorForSomeReason {
            get {
                return false;
            }
        }

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
                            var qcolor = (QuantizedColor)perType.color;
                            if (!IWantFullyTransparentColorForSomeReason)
                            {
                                qcolor.alpha = qcolor.alpha == 0 ? (byte)255 : qcolor.alpha;
                            }
                            _lookup.Add(perType.type, qcolor);
                                
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
