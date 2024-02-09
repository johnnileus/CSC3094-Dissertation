using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class meshgenOld : MonoBehaviour{

    private MeshChunkOld[] meshChunks;
    private GameObject[] rootMeshGOs;
    
    [SerializeField] private int meshNodeCount;
    [SerializeField] private int meshCount;
    [SerializeField] private float cellSize;

    
    //unit size of root mesh
    private float meshSize;
    
    // Start is called before the first frame update
    void Start(){
        meshSize = cellSize * meshNodeCount;
        rootMeshGOs = new GameObject[meshCount*meshCount];

        for (int y = 0; y < meshCount; y++) {
            for (int x = 0; x < meshCount; x++) {
                int chunkID = y * meshCount + x;
                print($"{x}, {y}, {chunkID}");
                
                Mesh mesh = GenMesh(0);
                rootMeshGOs[chunkID] = new GameObject($"mesh {chunkID}");
                rootMeshGOs[chunkID].transform.parent = transform;
                Vector3 newPos = new Vector3(x*(cellSize*meshNodeCount),0,y*(cellSize*meshNodeCount));
                rootMeshGOs[chunkID].transform.position = newPos;

                rootMeshGOs[chunkID].AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                MeshFilter meshFilter =  rootMeshGOs[chunkID].AddComponent<MeshFilter>();
                MeshChunkOld meshChunkOldScript =  rootMeshGOs[chunkID].AddComponent<MeshChunkOld>();
                meshChunkOldScript.worldPos = newPos;
                meshFilter.sharedMesh = mesh;
            }
        }
        
        SplitMesh(rootMeshGOs[0]);
        
    }

    private void SplitMesh(GameObject meshObj){
        MeshChunkOld meshChunkOldScript = meshObj.GetComponent<MeshChunkOld>();
        int detailLevel = meshChunkOldScript.detailLevel;
        float meshWidth = meshSize / (2 ^ detailLevel);
        meshObj.GetComponent<MeshRenderer>().enabled = false;

        Dictionary<int, Vector3> offsets = new Dictionary<int, Vector3>() {
            {0, Vector3.zero},
            {1, new Vector3(meshWidth/2, detailLevel, 0)},
            {2, new Vector3(0, detailLevel, meshWidth/2)},
            {3, new Vector3(meshWidth/2, detailLevel, meshWidth/2)},

        };

        for (int i = 0; i < 4; i++) {
            GameObject newObj = new GameObject($"{meshObj.name} {i}");
            newObj.transform.parent = meshObj.transform;
            newObj.transform.position = meshObj.transform.position + offsets[i];
            newObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
            MeshFilter meshFilter = newObj.AddComponent<MeshFilter>();
            MeshChunkOld chunkOldScript =  newObj.AddComponent<MeshChunkOld>();

            meshFilter.sharedMesh = GenMesh(detailLevel);
            chunkOldScript.detailLevel = detailLevel + 1;
            if (Random.Range(0, 9) < 2) {
                SplitMesh(newObj);
            }
        }
        
    }
    
    private Mesh GenMesh(int detailLevel){
        Mesh m = new Mesh();
        Vector3[] vertices;
        int[] triangles;
        vertices = new Vector3[(meshNodeCount + 1) * (meshNodeCount + 1)];

        //create vertices

        for (int y = 0; y < meshNodeCount + 1; y++) {
            for (int x = 0; x < meshNodeCount + 1; x++) {
                vertices[y * (meshNodeCount + 1) + x] = new Vector3(x*cellSize/MathF.Pow(2, detailLevel), 0, y*cellSize/MathF.Pow(2, detailLevel));
            }
        }

        triangles = new int[meshNodeCount * meshNodeCount * 6];

        for (int y = 0; y < meshNodeCount; y++) {
            for (int x = 0; x < meshNodeCount; x++) {

                int tileNum = (meshNodeCount * y + x) * 6;
                int rootVert = (meshNodeCount + 1) * y + x;
                triangles[tileNum] = rootVert;
                triangles[tileNum + 1] = rootVert  + meshNodeCount + 1;
                triangles[tileNum + 2] = rootVert + 1;

                triangles[tileNum + 3] = rootVert + 1;
                triangles[tileNum + 4] = rootVert + meshNodeCount + 1;
                triangles[tileNum + 5] = rootVert + meshNodeCount + 2;
            }
        }

        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateNormals();
        return m;
    }
}
