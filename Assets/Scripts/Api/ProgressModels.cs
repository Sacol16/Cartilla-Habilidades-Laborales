using System;

[Serializable]
public class UpsertModuleProgressRequest
{
    public float score;          // 0..100
    public bool done;
    public ModuleDataDto data;
}

[Serializable]
public class UpsertModuleProgressResponse
{
    public bool ok;
    public YouthProgressDto progress;
    public string error;
}

[Serializable]
public class YouthProgressDto
{
    public string _id;
    public string youthId;
    public float score;
    public ModuleProgressDto[] modules;
}

[Serializable]
public class ModuleProgressDto
{
    public string moduleId;
    public float score;
    public bool done;
    public ModuleDataDto data;   // <-- útil para recargar actividades
}

[Serializable]
public class ModuleDataDto
{
    public Module1DataDto module1;
}

[Serializable]
public class Module1DataDto
{
    public SlotPlacementDto[] activity1;
    public string[] activity2Answers;
    public string activity3PngBase64;
    public string activity4SelectedOptionId;
    public string activity4AudioBase64;
}

[Serializable]
public class SlotPlacementDto
{
    public int slotIndex;
    public string itemObjectName;
}