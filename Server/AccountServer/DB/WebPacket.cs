﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


// 서버정보
public class ServerInfo
{
    // 서버이름
    public string Name { get; set; }
    // 서버주소
    public string Ip { get; set; }
    // 혼잡도, 높을 수 록 혼잡
    public int CrowdedLevel { get; set; }
}

// 클라에서 서버
#region CLIENT => SERVER
// 계정생성 시도 
public class CreateAccountPacketReq
{
    public string AccountName { get; set; }
    public String Password { get; set; }
}

// login
public class LoginAccountPacketReq
{
    public string AccountName { get; set; }
    public String Password { get; set; }
}
#endregion


// 서버에서 클라
#region SERVER => CLIENT
// 계정생성 성공여부
public class CreateAccountPacketRes
{
    public bool CreateOk { get; set; }
}

// login
public class LoginAccountPacketRes
{
    public bool LoginOk { get; set; }
    public List<ServerInfo> ServerList { get; set; } = new List<ServerInfo>();
}

#endregion