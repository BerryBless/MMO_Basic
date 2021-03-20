﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    UI_GameScene _sceneUI;
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        // TODO 패킷의 맵ID에 따라 불러주기
        // Managers.Map.LoadMap(1);

        Screen.SetResolution(640, 480, false);// 테스트용 빌드 해상도 설정

        _sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
    }

    public override void Clear()
    {

    }
}
