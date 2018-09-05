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
    public class VEGenerateVolume : MonoBehaviour
    {
        

        [MenuItem("VE/Generate and save volume")]
        static void GenerateVolume()
        {
            var selected = Selection.activeGameObject;
            VEGenerateVolume genVolume = null;
            if (selected)
            {
                genVolume = selected.GetComponent<VEGenerateVolume>();
            }

            if(!genVolume)
            {
                genVolume = FindObjectOfType<VEGenerateVolume>();
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
        float noiseScale = 42f;

        [SerializeField, Range(-2f, 2f)]
        private float isSolidThreshhold;

        [SerializeField]
        Vector3i size = new Vector3i(64, 64, 64);

        [SerializeField, Header("Unset this if you haven't changed your generation code. (Saves time.)")]
        bool alwaysRegenerateVolume = true;

        string saveLocation {
            get { return Paths.voxelDatabases + "/" + volumeName + ".vdb"; }
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

            float invRockScale = 1f / noiseScale;

            var uvOffsetAsColor = FindObjectOfType<VoxelTypeToColor>();
            if(!uvOffsetAsColor)
            {
                Debug.LogError("I need a 'UVOffsetAsColor' gameobject");
                return null;
            }

            // It's best to create these outside of the loop.
            QuantizedColor grass = new QuantizedColor(122, 122, 255, 255);
            QuantizedColor red = new QuantizedColor(255, 0, 0, 255);
            QuantizedColor gray = new QuantizedColor(127, 127, 127, 255);
            QuantizedColor white = new QuantizedColor(255, 255, 255, 255);

            grass = uvOffsetAsColor.getQuantizedColor(VoxelType.Grass);

            // Iterate over every voxel of our volume
            for (int z = 0; z < size.x; z++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.z; x++)
                    {

                        // Simplex noise is quite high frequency. We scale the sample position to reduce this.
                        float sampleX = x * invRockScale;
                        float sampleY = y * invRockScale;
                        float sampleZ = z * invRockScale;

                        // range -1 to +1
                        float simplexNoiseValue = SimplexNoise.Noise.Generate(sampleX, sampleY, sampleZ);

                        float altitude = y / size.y;

                        altitude -= .5f;
                        altitude *= 2f;

                        simplexNoiseValue += altitude;
                        // mul by 5 and clamp?

                        //simplexNoiseValue *= 5f;
                        //simplexNoiseValue = Mathf.Clamp(simplexNoiseValue, -.5f, .5f);
                        //simplexNoiseValue += .5f;
                        //simplexNoiseValue *= 255;

                        if (simplexNoiseValue > isSolidThreshhold)
                        {
                            data.SetVoxel(x, y, z, grass);
                        }

                    }
                }
            }
            data.CommitChanges();

            
            return data;
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
