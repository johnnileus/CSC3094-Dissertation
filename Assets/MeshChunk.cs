using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshChunk : MonoBehaviour{
    private Mesh mesh;
    public int ID;
    private int detailLevel;
    public bool hasChildren;
    public MeshChunk[] children;

    public MeshChunk(Mesh m, int id){
        mesh = m;
        ID = id;
    }

    public void CreateChildren(){
        hasChildren = true;
        mesh = null;
    }
}
