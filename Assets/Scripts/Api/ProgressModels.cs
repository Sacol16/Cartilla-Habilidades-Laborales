using System;

[Serializable]
public class UpsertModuleProgressRequest
{
    public int score;          // 0..100
    public bool done;          // true cuando terminas el módulo
    public ModuleDataDto data; // info del módulo
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
    public int score;
    public ModuleProgressDto[] modules;
}

[Serializable]
public class ModuleProgressDto
{
    public string moduleId;
    public int score;
    public bool done;
    public string updatedAt;
    // data lo dejamos como string/object si no lo vas a leer ahora
}

[Serializable]
public class ModuleDataDto
{
    public Module1DataDto module1;
}

[Serializable]
public class Module1DataDto
{
    public SlotPlacementDto[] activity1;      // slots
    public string[] activity2Answers;         // 5 inputs
    public string activity3PngBase64;         // png base64 (sin data:)
    public string activity4SelectedOptionId;  // selección
    public string activity4AudioBase64;       // webm base64 (sin data:)
}

[Serializable]
public class SlotPlacementDto
{
    public int slotIndex;
    public string itemObjectName;
}