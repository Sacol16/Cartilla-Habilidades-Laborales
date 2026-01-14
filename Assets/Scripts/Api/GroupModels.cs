using System;

[Serializable]
public class GroupDto
{
    public string _id;
    public string name;
    public int memberCount;
}

[Serializable]
public class GetMyGroupsResponse
{
    public bool ok;
    public GroupDto[] groups;
}
