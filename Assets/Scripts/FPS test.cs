using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;

public class FPStest : MonoBehaviour{

    [SerializeField] private TextMeshProUGUI fpstext;
    [SerializeField] private TextMeshProUGUI memtext;

    [SerializeField] private bool movePlayer;
    [SerializeField] private GameObject player;
    [SerializeField] private float playerSpeed;
    
    private float maxfps;


    private float fps;
    private float updateTimer;
    [SerializeField] private float updateInterval;

    private float[] fpsHist;
    private int histIndex;
    private int maxHistIndex;
    [SerializeField] private float avgTime;
    
    
    
    ProfilerRecorder totalReservedMemoryRecorder;
    ProfilerRecorder gcReservedMemoryRecorder;
    ProfilerRecorder systemUsedMemoryRecorder;

    void OnEnable()
    {
        totalReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Reserved Memory");
        gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
    }

    void OnDisable()
    {
        totalReservedMemoryRecorder.Dispose();
        gcReservedMemoryRecorder.Dispose();
        systemUsedMemoryRecorder.Dispose();
    }

    void Update(){


        if (movePlayer) {
            float time = Time.time % playerSpeed / playerSpeed *2 * Mathf.PI;
            float posX = (Mathf.Cos(time) + 1) * 2048;
            float posY = (Mathf.Sin(time) + 1) * 2048;
            player.transform.position = new Vector3(posX, 0, posY);
        }
        
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0) {
            fps = 1f / Time.unscaledDeltaTime;
            if (fps > maxfps) maxfps = fps;

            updateTimer = updateInterval;
            fpsHist[histIndex] = fps;

            histIndex++;
            if (histIndex > maxHistIndex) {
                histIndex = 0;
            }

            float total = 0;
            for (int i = 0; i < fpsHist.Length; i++) {
                total += fpsHist[i];
            }

            float avgfps = total / (maxHistIndex + 1);
            float TR = totalReservedMemoryRecorder.LastValue / 1024 / 1024;
            float GCR = gcReservedMemoryRecorder.LastValue / 1024 / 1024;
            float SU = systemUsedMemoryRecorder.LastValue / 1024 / 1024;
            memtext.text = $"TR: {TR}MB, GCR: {GCR}MB, SU: {SU}MB ";
            fpstext.text = $"FPS: {fps}, max: {maxfps}, avg: {avgfps}";
        }
    }

    
    private void Start(){
        fpsHist = new float[Mathf.RoundToInt(avgTime/updateInterval)];
        histIndex = 0;
        maxHistIndex = Mathf.RoundToInt(avgTime / updateInterval) - 1;
        print(maxHistIndex);
    }
    


}
