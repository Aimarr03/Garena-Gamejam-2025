using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public float minInterval;
    public float maxInterval;
    public float intervalToSpawn;
    public int totalPassengers;
    public List<WaveData> waves = new List<WaveData>();
    public List<int> spawns = new List<int>();
}
[Serializable]
public class WaveData
{
    public int requirementToSpawn;
    public int totalPassengers;
    public int currentDeliveredPassengers;
}
