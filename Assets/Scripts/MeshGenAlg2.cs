using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MeshGenAlg2 : MonoBehaviour{
    [SerializeField] private float RootMeshWidth;
    [SerializeField] private int MeshCellCount;

    private float CellSize;
    private MeshChunk RootChunk;

    private GameObject player;

    private Dictionary<int, float> detailDistances = new Dictionary<int, float>() {
        { 0, 256f },
        { 1, 128f },
        { 2, 64f },
        { 3, 32f },
        { 4, 16f },
        { 5, 8f },
        { 6, 4f },
        { 7, 2f }
    };
    
    private void CheckChunkDistance(MeshChunk chunk){
        
        float offset = RootMeshWidth / MathF.Pow(2, chunk.DetailLevel + 1);
        Vector3 chunkCenter = chunk.Pos + new Vector3(offset, 0, offset);
        float dist = Vector2.Distance(new Vector2(chunkCenter.x, chunkCenter.z), new Vector2(player.transform.position.x, player.transform.position.z));
        
        if (chunk.HasChildren) {
            if (dist > detailDistances[chunk.DetailLevel]*32) {
                MergeMesh(chunk);
            } else {
                for (int i = 0; i < 4; i++) {
                    CheckChunkDistance(chunk.Children[i]);
                }
            }
        }
        else {
            if (chunk.DetailLevel < detailDistances.Count && dist < detailDistances[chunk.DetailLevel]*32) {
                SplitMesh(chunk);
            }
        }
    }
    
    private void Update(){
        CheckChunkDistance(RootChunk);
    }
    
    void Start(){

        player = GameObject.FindWithTag("Player");
        
        RootChunk = new MeshChunk(0, new Vector3(0,0,0), CellSize);
        CellSize = RootMeshWidth / MeshCellCount;

        GameObject meshObj = new GameObject("root");
        meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
        meshObj.AddComponent<MeshFilter>().sharedMesh = GenMesh(0, Vector2.zero);
        
        meshObj.transform.parent = transform;
        RootChunk.MeshGO = meshObj;
        
        // SplitMesh(RootChunk);
        // SplitMesh(RootChunk.Children[0]);
        // SplitMesh(RootChunk.Children[0].Children[0]);
        
        //MergeMesh(RootChunk.Children[0]);

    }
    //splits chunk into 4 child meshes
    private void SplitMesh(MeshChunk chunk){
        if (!chunk.HasChildren) {
            chunk.HasChildren = true;
            chunk.Children = new MeshChunk[4];
            chunk.MeshGO.GetComponent<MeshRenderer>().enabled = false;
        
            float offset = RootMeshWidth / MathF.Pow(2, chunk.DetailLevel + 1);

            Dictionary<int, Vector3> cellOffsets = new Dictionary<int, Vector3>() {
                {0, Vector3.zero},
                {1, new Vector3(offset, 0, 0)},
                {2, new Vector3(0, 0, offset)},
                {3, new Vector3(offset, 0, offset)},
            };

            for (int i = 0; i < 4; i++) {
                GameObject newObj = new GameObject($"mesh {i}");
                
                Vector3 newPos = chunk.Pos + cellOffsets[i];
                
                newObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                newObj.AddComponent<MeshFilter>().sharedMesh = GenMesh(chunk.DetailLevel + 1, new Vector2(newPos.x, newPos.z));

                newObj.transform.parent = chunk.MeshGO.transform;

                
                newObj.transform.position = chunk.Pos + cellOffsets[i];
                
                MeshChunk newChunk = new MeshChunk(chunk.DetailLevel + 1, newPos, CellSize);
            
                newChunk.MeshGO = newObj;
                newChunk.ParentChunk = chunk;
                chunk.Children[i] = newChunk;
            
            }
            // testing
            // if (chunk.DetailLevel < 4) {
            //     for (int i = 0; i < 4; i++) {
            //         if (Random.value < 0.6f) {
            //             SplitMesh(chunk.Children[i]);
            //         }
            //     }
            // }


        }
        
    }
    //merges all children meshes into parent mesh
    private void MergeMesh(MeshChunk chunk){
        if (chunk.HasChildren) {
            chunk.HasChildren = false;
            for (int i = 0; i < 4; i++) {
                MergeMesh(chunk.Children[i]);
                Destroy(chunk.Children[i].MeshGO);
                chunk.Children[i] = null;
            }

            chunk.Children = null;
            chunk.MeshGO.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    private float GetMeshHeight(float x, float y){
        return Mathf.PerlinNoise(x/30, y/30)*5 + Mathf.PerlinNoise(x/300, y/300)*50 + Mathf.PerlinNoise(x/5, y/5);
    }

    private Mesh GenMesh(int detailLevel, Vector2 globalPos){
        Mesh m = new Mesh();
        
        
        Vector3[] vertices = new Vector3[(MeshCellCount + 1) * (MeshCellCount + 1)];
        for (int y = 0; y < MeshCellCount + 1; y++) {
            for (int x = 0; x < MeshCellCount + 1; x++) {
                float scale = MathF.Pow(2, detailLevel);


                float xPos = x * CellSize / scale;
                float yPos = y * CellSize / scale;
                float height = GetMeshHeight(globalPos.x + xPos, globalPos.y + yPos);
                
                vertices[y * (MeshCellCount + 1) + x] = new Vector3(xPos, height, yPos);
            }
        }

        
        int[] triangles = new int[MeshCellCount * MeshCellCount * 6];
        for (int y = 0; y < MeshCellCount; y++) {
            for (int x = 0; x < MeshCellCount; x++) {

                int tileNum = (MeshCellCount * y + x) * 6;
                int rootVert = (MeshCellCount + 1) * y + x;
                triangles[tileNum] = rootVert + MeshCellCount + 1;
                triangles[tileNum + 1] = rootVert + 1;
                triangles[tileNum + 2] = rootVert;

                triangles[tileNum + 3] = rootVert + MeshCellCount + 1;
                triangles[tileNum + 4] = rootVert + MeshCellCount + 2;
                triangles[tileNum + 5] = rootVert + 1;
            }
        }

        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateNormals();
        return m;
    }
}
