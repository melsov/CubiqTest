using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cubiquity
{

    //
    // Make the VDBB
    //   Via menu item command or at start -- if it doesn't exist already
    //   Display the VDB.
    //     --make if it doesn't exist yet or load it
    //

    public class TestGenVolumeDatabase : MonoBehaviour
    {

        public Vector3i size = new Vector3i(60, 30, 60);
        [SerializeField]
        float noiseScale = 42f;

        private void Start()
        {
            CreateAVolumeDB();
        }

        private void CreateAVolumeDB()
        {
            System.Random randomIntGenerator = new System.Random();
            int randomInt = randomIntGenerator.Next();
            string saveLocation = Paths.voxelDatabases + "/Matt-test"+randomInt+".vdb";
            var volumeBounds = new Region(Vector3i.zero, size);

            ColoredCubesVolumeData data = VolumeData.CreateEmptyVolumeData<ColoredCubesVolumeData>(volumeBounds, null); // saveLocation);

            var coloredCubeVolume = GetComponent<ColoredCubesVolume>();

            coloredCubeVolume.data = data;

            float invRockScale = 1f / noiseScale;

            MaterialSet materialSet = new MaterialSet();

            // It's best to create these outside of the loop.
            QuantizedColor red = new QuantizedColor(255, 0, 0, 255);
            QuantizedColor blue = new QuantizedColor(122, 122, 255, 255);
            QuantizedColor gray = new QuantizedColor(127, 127, 127, 255);
            QuantizedColor white = new QuantizedColor(255, 255, 255, 255);

            // Iterate over every voxel of our volume
            for (int z = 0; z < size.x; z++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.z; x++)
                    {

                        // Simplex noise is quite high frequency. We scale the sample position to reduce this.
                        float sampleX = (float)x * invRockScale;
                        float sampleY = (float)y * invRockScale;
                        float sampleZ = (float)z * invRockScale;

                        // range -1 to +1
                        float simplexNoiseValue = SimplexNoise.Noise.Generate(sampleX, sampleY, sampleZ);

                        simplexNoiseValue -= y / size.y * .75f;
                        // mul by 5 and clamp?

                        //simplexNoiseValue *= 5f;
                        //simplexNoiseValue = Mathf.Clamp(simplexNoiseValue, -.5f, .5f);
                        //simplexNoiseValue += .5f;
                        //simplexNoiseValue *= 255;

                        if (simplexNoiseValue > 0f)
                        {
                            data.SetVoxel(x, y, z, blue);
                        }

                    }
                }
            }
            data.CommitChanges();

            Debug.Log("Voxel db saved to: " + saveLocation);
        }
    }
}
