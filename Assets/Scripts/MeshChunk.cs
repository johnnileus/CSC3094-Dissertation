using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class MeshChunk{
    public GameObject MeshGO;
    public MeshChunk ParentChunk;

    
    public List<int> Path;

    
    public int DetailLevel;
    public Vector3 Pos; //bottom left
    
    public bool HasChildren;
    public MeshChunk[] Children;

    public MeshChunk(int detailLevel, Vector3 pos){
        DetailLevel = detailLevel;
        Pos = pos;
        HasChildren = false;
        Children = null;
    }
    
    public void PrintPath(string prefix) {
        string str = "";
        for (int i = 0; i < Path.Count; i++) {
            str += Path[i];
        }
        Debug.Log($"{prefix}, {str}");
        
    }
    
    
}
