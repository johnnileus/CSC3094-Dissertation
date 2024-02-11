using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshChunk{
    public GameObject MeshGO;
    public MeshChunk ParentChunk;
    
    public int DetailLevel;
    public Vector3 Pos; //bottom left
    
    public bool HasChildren;
    public MeshChunk[] Children;
    
    public int[] PathArray;

    public MeshChunk(int detailLevel, Vector3 pos){
        DetailLevel = detailLevel;
        Pos = pos;
        HasChildren = false;
        Children = null;
        
    }
}
