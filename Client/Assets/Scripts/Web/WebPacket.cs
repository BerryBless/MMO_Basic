using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 서버정보
public class ServerInfo
{
    // 서버이름
    public string Name;
    // 서버주소
    public string Ip;
    // 혼잡도, 높을 수 록 혼잡
    public int CrowdedLevel;
}

// 클라에서 서버
#region CLIENT => SERVER
// 계정생성 시도 
public class CreateAccountPacketReq
{
    public string AccountName;
    public string Password;
}

// login
public class LoginAccountPacketReq
{
    public string AccountName;
    public string Password;
}
#endregion


// 서버에서 클라
#region SERVER => CLIENT
// 계정생성 성공여부
public class CreateAccountPacketRes
{
    public bool CreateOk;
}

// login
public class LoginAccountPacketRes
{
    public bool LoginOk;
    public List<ServerInfo> ServerList = new List<ServerInfo>();
}

#endregion

public class WebPacket
{
    public static void SendCreateAccount(string account, string password)
    {
        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, (res) =>
        {
            Debug.Log($"Account Cteate : {res.CreateOk}");
        });
    }

    public static void SendLoginAccount(string account, string password)
    {
        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, (res) =>
        {
            Debug.Log($"Account Login : {res.LoginOk}");
        });
    }
}