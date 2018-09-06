using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cubiquity;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace VE.VoxelGen
{
    public class VEGenerateVolumeBase : MonoBehaviour
    {
        

        [MenuItem("VE/Generate and save volume")]
        static void GenerateVolume()
        {
            var selected = Selection.activeGameObject;
            VEGenerateVolumeBase genVolume = null;
            if (selected)
            {
                genVolume = selected.GetComponent<VEGenerateVolumeBase>();
            }

            if(!genVolume)
            {
                genVolume = FindObjectOfType<VEGenerateVolumeBase>();
            }

            if(!genVolume)
            {
                Debug.Log("Couldn't find a VEGenerateVolume game object in the hierarchy");
                return;
            }


            genVolume.CreateOrOverwriteVolumeDatabase();
            Debug.Log("Voxel db saved to: " + genVolume.saveLocation);

            //genVolume.SetVolume(data); // Don't do this! (Perma locks the SQLite database until you restart Unity.)
            // Probably, I'm missing a step--copying the data somewhere?--that would allow this
        }

        [SerializeField]
        string volumeName = "AwesomeCubeVolumeName";

        [SerializeField]
        protected float noiseScale = 42f;

        [SerializeField, Range(0f, 1f)]
        protected float isSolidThreshhold;

        [SerializeField]
        protected Vector3i size = new Vector3i(64, 64, 64);

        [SerializeField, Header("Unset this if you haven't changed your generation code. (Saves time.)")]
        bool alwaysRegenerateVolume = true;

        [SerializeField, Range(0f, 1.8f)]
        protected float snoise2DScale = .6f;

        string _saveLocation;
        string saveLocation {
            get {
                if (_saveLocation == null)
                {
                    _saveLocation = Paths.voxelDatabases + "/" + volumeName + ".vdb";
                }
                return _saveLocation;
            }
        }

        VoxelTypeToColor _voxelTypeToColor;
        protected VoxelTypeToColor voxelTypeToColor {
            get {
                if(!_voxelTypeToColor)
                {
                    _voxelTypeToColor = FindObjectOfType<VoxelTypeToColor>();
                }
                return _voxelTypeToColor;
            }
        }

        private void Start()
        {
            DisplayVolume();
        }

        private ColoredCubesVolumeData GenerateIfNone()
        {
            if(File.Exists(saveLocation))
            {
                return VolumeData.CreateFromVoxelDatabase<ColoredCubesVolumeData>(
                    saveLocation, 
                    VolumeData.WritePermissions.ReadWrite);
            }
            return CreateOrOverwriteVolumeDatabase();
        }

        private ColoredCubesVolumeData CreateOrOverwriteVolumeDatabase()
        {

            var volumeBounds = new Region(Vector3i.zero, size);

            ColoredCubesVolumeData data = null;
            if (!File.Exists(saveLocation))
            {
                data = VolumeData.CreateEmptyVolumeData<ColoredCubesVolumeData>(volumeBounds, saveLocation);
            } else
            {
                data = VolumeData.CreateFromVoxelDatabase<ColoredCubesVolumeData>(
                    saveLocation, 
                    VolumeData.WritePermissions.ReadWrite);
            }

            MakeChunk(data);


            data.CommitChanges();

            
            
            return data;
        }

        protected virtual void MakeChunk(ColoredCubesVolumeData data)
        {
            throw new Exception("please override MakeChunk() in a subclass of VEGenerateVolumeBase");
        }

        private void DebugCheckData(ColoredCubesVolumeData data)
        {
            for(int x = 0; x < 4; ++x)
            {
                var color = data.GetVoxel(x, 0, 0);
                Debug.Log(color.ToString());
            }
        }

        private void DisplayVolume()
        {
            ColoredCubesVolumeData data = null;
            if (alwaysRegenerateVolume)
            {
                data = CreateOrOverwriteVolumeDatabase();
            }
            else
            {
                data = GenerateIfNone();
            }

            SetVolume(data);
        }

        void SetVolume(ColoredCubesVolumeData data)
        {
            var coloredCubeVolume = GetComponent<ColoredCubesVolume>();
            coloredCubeVolume.data = data;
        }
    }
}
