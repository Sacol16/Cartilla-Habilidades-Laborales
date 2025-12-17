using System;

[Serializable]
public class RegisterFacilitatorRequest
{
    public string email;
    public string name;
    public string password;
    public string code;
}

[Serializable]
public class RegisterFacilitatorResponse
{
    public bool ok;
    public string id;
}

[Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[Serializable]
public class LoginUser
{
    public string id;
    public string role;
    public string name;
    public string email;
    public string groupId;
}

[Serializable]
public class LoginResponse
{
    public bool ok;
    public string token;
    public LoginUser user;
}
