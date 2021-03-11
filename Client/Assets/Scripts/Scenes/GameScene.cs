using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        // TODO 패킷의 맵ID에 따라 불러주기
        // Managers.Map.LoadMap(1);

        Screen.SetResolution(640, 480, false);// 테스트용 빌드 해상도 설정

        /* 서버없이 테스트
        GameObject player = Managers.Resource.Instantiate("Creatures/Player");
        player.name = "Player";
        Managers.Object.Add(player);

        for (int i = 0; i < 5; i++)
        {
            GameObject monster = Managers.Resource.Instantiate("Creatures/Monster");
            monster.name = $"Monster_{i}";

            // 랜덤위치 스폰
            Vector3Int pos = new Vector3Int()
            {
                x = Random.Range(-10, 10),
                y = Random.Range(-10, 10)
            };

            MonsterController mc = monster.GetComponent<MonsterController>();
            mc.CellPos = pos;

            Managers.Object.Add(monster);

        }*/
        //Managers.UI.ShowSceneUI<UI_Inven>();
        //Dictionary<int, Data.Stat> dict = Managers.Data.StatDict;
        //gameObject.GetOrAddComponent<CursorController>();

        //GameObject player = Managers.Game.Spawn(Define.WorldObject.Player, "UnityChan");
        //Camera.main.gameObject.GetOrAddComponent<CameraController>().SetPlayer(player);

        ////Managers.Game.Spawn(Define.WorldObject.Monster, "Knight");
        //GameObject go = new GameObject { name = "SpawningPool" };
        //SpawningPool pool = go.GetOrAddComponent<SpawningPool>();
        //pool.SetKeepMonsterCount(2);
    }

    public override void Clear()
    {

    }
}
