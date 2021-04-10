using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)// "이몸등장"
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
    }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)// "이몸퇴장"
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)// ㅎㅇ
    {
        S_Spawn spawnPacket = packet as S_Spawn;
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)// ㅂㅇ
    {
        S_Despawn despawnPacket = packet as S_Despawn;
    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)// 무언가 이동
    {
        S_Move movePacket = packet as S_Move;
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet) // 무언가 스킬 사용
    {
        S_Skill skillPacket = packet as S_Skill;
    }
    public static void S_ChangeHpHandler(PacketSession session, IMessage packet) // 누군가의 HP 가 줄어듦
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;
    }
    public static void S_DieHandler(PacketSession session, IMessage packet) // 무언가 소멸됨
    {
        S_Die diePacket = packet as S_Die;
    }
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)// 연결
    {
    }

    public static void S_LoginHandler(PacketSession session, IMessage packet)// 로그인 + 캐릭터 목록
    {
        S_Login loginPacket = packet as S_Login;
    }
    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)// 플레이어 생성
    {
        S_CreatePlayer createOkPacket = packet as S_CreatePlayer;
    }
    public static void S_ItemListHandler(PacketSession session, IMessage packet)// 게임 처음 시작할떄 인벤토리 에 넣기
    {
        S_ItemList itemList = packet as S_ItemList;
    }

    public static void S_AddItemHandler(PacketSession session, IMessage packet)// 인게임중 아이템 획득
    {
        S_AddItem itemList = packet as S_AddItem;
    }
    public static void S_EquipItemHandler(PacketSession session, IMessage packet)// 아이템 장착 / 탈착 
    {
        S_EquipItem equipItemOk = packet as S_EquipItem;
    }
    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)// 캐릭터 스텟 바꾸기
    {
        S_ChangeStat changeStatPacket = packet as S_ChangeStat;
    }

    public static void S_PingHandler(PacketSession session, IMessage packet)// 서버랑 핑퐁~ (계속 연결중인가 확인)
    {
        C_Pong pongPacket = new C_Pong();
    }

    public static void S_ChangeMapHandler(PacketSession session, IMessage packet)// 맵로딩
    {
        S_ChangeMap changeMapPacket = packet as S_ChangeMap;
    }
}
