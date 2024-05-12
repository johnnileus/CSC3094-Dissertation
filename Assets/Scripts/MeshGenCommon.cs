using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenCommon
{
    public static Dictionary<int, float> detailDistances = new Dictionary<int, float>() {
        { 0, 256f },
        { 1, 128f },
        { 2, 64f },
        { 3, 32f },
        { 4, 16f },
        { 5, 8f },
        { 6, 4f },
        { 7, 2f }
    };
    
    public static float GetMeshHeight(float x, float z) {
        return 10*(Mathf.PerlinNoise(x/30, z/30)*5 + Mathf.PerlinNoise(x/300, z/300)*100 + Mathf.PerlinNoise(x/5, z/5));
    }
    
}
