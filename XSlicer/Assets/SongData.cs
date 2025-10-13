using System;
using System.Collections.Generic;

[Serializable]
public class SongData
{
    public string id;
    public string title;
    public float tempo_bpm;
    public int num_beats;
    public List<float> beat_times_sec;
    public string localPath;
}
