﻿using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; } // "이몸"   
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
    /* 기획의 영역 오브젝트 종류에따른 관리
    Dictionary<int, GameObject> _players = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> _enemys = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> _npcs = new Dictionary<int, GameObject>();
    */

    // ID에 따른 타입 반환
    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        GameObjectType type = GetObjectTypeById(info.ObjectId);
        if (type == GameObjectType.Player)
        {
            // 추가되는게 플레이어 일때
            GameObject go;
            if (myPlayer)
            {
                go = Managers.Resource.Instantiate("Creatures/MyPlayer");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                MyPlayer = go.GetComponent<MyPlayerController>();
                MyPlayer.Id = info.ObjectId;
                MyPlayer.PosInfo = info.PosInfo;
                MyPlayer.Stat = info.StatInfo;
                
                MyPlayer.SynkPos();
            }
            else
            {
                go = Managers.Resource.Instantiate("Creatures/Player");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Id = info.ObjectId;
                pc.PosInfo = info.PosInfo;
                pc.Stat = info.StatInfo;

                pc.SynkPos();
            }
        }
        else if (type == GameObjectType.Monster)
        {
            //  몬스터 ADD
            GameObject go;
            go = Managers.Resource.Instantiate("Creatures/Monster");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            MonsterController mc = go.GetComponent<MonsterController>();
            mc.Id = info.ObjectId;
            mc.PosInfo = info.PosInfo;
            mc.Stat = info.StatInfo;
            mc.SynkPos();

        }
        else if (type == GameObjectType.Projectile)
        {
            // TODO 투사체 ADD
            GameObject go;
            go = Managers.Resource.Instantiate("Creatures/Arrow");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            //TODO 화살 발싸!
            ArrowController ac = go.GetComponent<ArrowController>();
            ac.PosInfo = info.PosInfo;
            ac.Stat = info.StatInfo;
            ac.CellPos = new Vector3Int(info.PosInfo.PosX, info.PosInfo.PosY, 0);


            ac.SynkPos();
        }
    }
    public void Remove(int id)
    {
        GameObject go = Find(id);
        if (go == null) return;


        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    // 하나 하나 찾기
    public GameObject FindCreatur(Vector3Int cellPos)
    {
        foreach (GameObject obj in _objects.Values)
        {
            CreatureController cc = obj.GetComponent<CreatureController>();
            if (cc == null)
                continue;

            if (cc.CellPos == cellPos)
            {
                return obj;
            }
        }
        return null;
    }

    public GameObject Find(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }


    // Func 델리게이트 입력으로 게임오브젝트 받고 bool값 반환
    // conditon의 GameObject가 찾기 적합한 오브젝트인가
    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }
        return null;
    }


    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Managers.Resource.Destroy(obj);
        _objects.Clear();
        MyPlayer = null;
    }

    public void LoadMap(int mapId)
    {
        // TODO 맵 바뀔때 기존맵 지우고 로딩하기
        //Managers.Map.DestroyMap();

        Managers.Map.LoadMap(mapId);
    }
}
