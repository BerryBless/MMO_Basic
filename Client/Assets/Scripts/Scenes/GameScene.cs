using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    UI_GameScene _sceneUI;
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        // TEMP Web테스트
        Managers.Web.BaseUrl = "https://localhost:5001/api";
        WebPacket.SendCreateAccount("asd", "123");

        Screen.SetResolution(640, 480, false);// 테스트용 빌드 해상도 설정

        _sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
    }

    public override void Clear()
    {

    }
}
