using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PacketHandler
{
    public static object Room { get; private set; }
    public static GameRoom GameRoom { get; private set; }

    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        //Console.WriteLine($"{clientSession.MyPlayer.Info.ObjectId} : C_MOVE ({movePacket.PosInfo.PosX}, {movePacket.PosInfo.PosY}) Dir : {clientSession.MyPlayer.Info.PosInfo.MoveDir}");

        Player player = clientSession.MyPlayer;
        if (player == null) return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null) return;

        //room.HandleMove(player, movePacket);
        room.Push(room.HandleMove, player, movePacket);
    }

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
        C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        //Console.WriteLine($"{clientSession.MyPlayer.Info.ObjectId} : C_SKILL ({skillPacket.Info.SkillId})");

        Player player = clientSession.MyPlayer;
        if (player == null) return;

        GameRoom room = clientSession.MyPlayer.Room;
        if (room == null) return;
        // 스킬처리
        //room.HandleSkill(player, skillPacket);
        room.Push(room.HandleSkill, player, skillPacket);

    }

    public static void C_LoginHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleLogin(loginPacket);
    }

    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        C_EnterGame enterGamePacket = packet as C_EnterGame;
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleEnterGame(enterGamePacket);
    }

    public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        C_CreatePlayer createPlayerPacket = packet as C_CreatePlayer;
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleCreatePlayer(createPlayerPacket);
    }

}
