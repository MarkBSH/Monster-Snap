using System.Collections.Generic;

[System.Serializable]
public class WhyScoreEntry
{
    public string monsterName;
    public float baseScore;
    public string screenFraction;
    public bool isFullyVisible;
    public float actionMultiplier;
    public string action;
}

[System.Serializable]
public class PictureData
{
    public string filename;
    public float score;
    public List<WhyScoreEntry> whyScore;
}