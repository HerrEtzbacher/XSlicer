using System;
using System.Collections.Generic;

[Serializable]
public class RhythmAnalysis
{
    public float tempo_bpm;
    public int num_beats;
    public List<float> beat_times_sec;
}

[Serializable]
public class SongData
{
    public string id;
    public string title;
    public string artist;
    public int duration;
    public string upload_date;
    public string link;
    public string analyzed_at;
    public RhythmAnalysis rhythm_analysis;
    public string localPath;
}