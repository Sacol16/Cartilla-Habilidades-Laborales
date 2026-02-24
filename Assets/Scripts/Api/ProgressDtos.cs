using System;

[Serializable]
public class ProgressDto
{
    public string _id;
    public string youthId;
    public string moduleId;
    public float score;   // 0..100
    public bool done;
}

[Serializable]
public class GroupProgressResponse
{
    public bool ok;
    public ProgressDto[] progress;
}