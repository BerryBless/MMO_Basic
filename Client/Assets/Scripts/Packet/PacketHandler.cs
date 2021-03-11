using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{

    public static void S_EnterGameHandler(PacketSession session, IMessage packet)// "이몸등장"
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
    }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)// "이몸퇴장"
    {
        S_LeaveGame leaveGamePacket = packet as S_LeaveGame;

        Managers.Object.Clear();
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)// ㅎㅇ
    {
        S_Spawn spawnPacket = packet as S_Spawn;

        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Debug.Log($"S_MoveHandler ::\n\t{obj.ObjectId} :: CellPos ({obj.PosInfo.PosX},{obj.PosInfo.PosY}) | Dir : {obj.PosInfo.MoveDir}");
            Managers.Object.Add(obj, myPlayer: false);
        }
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)// ㅂㅇ
    {
        S_Despawn despawnPacket = packet as S_Despawn;

        foreach (int obj in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(obj);
        }
    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;

        GameObject go = Managers.Object.Find(movePacket.ObjectId);
        if (go == null) return;


        if (movePacket.ObjectId == Managers.Object.MyPlayer.Id) return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null) return;



        //Debug.Log($"S_MoveHandler ::\n\t{cc.gameObject.name} :: CellPos ({cc.PosInfo.PosX},{cc.PosInfo.PosY}) | Dir : {cc.PosInfo.MoveDir}");
        bc.PosInfo = movePacket.PosInfo;
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill skillPacket = packet as S_Skill;
        
        GameObject go = Managers.Object.Find(skillPacket.ObjectId);
        if (go == null) return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            cc.UseSkill(skillPacket.Info.SkillId);
        }

    }
    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;
        
        GameObject go = Managers.Object.Find(changePacket.ObjectId);
        if (go == null) return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            cc.Hp = changePacket.Hp;
        }

    }
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;
        
        GameObject go = Managers.Object.Find(diePacket.ObjectId);
        if (go == null) return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            // TODO 죽음 이펙트
            cc.Hp = 0;
            cc.OnDead();
        }

    }
}
