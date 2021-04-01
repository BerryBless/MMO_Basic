using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();               // This.room 안의 플레이어
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();            // This.room 안의 몬스터
        Dictionary<int, Projectile> _projectile = new Dictionary<int, Projectile>();    // This.room 안의 투사체

        public Map Map { get; private set; } = new Map();
        public void Init(int mapId)
        {
            Map.LoadMap(mapId);

            // TEMP 테스트할몬스터 만들기
            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.Init(1);
            monster.CellPos = new Vector2Int(6, 6);
            this.Push(this.EnterGame, monster);

        }


        // 누군가 주기적으로 호출해야 겜돌아감
        // TODO : JobSerializer 에 넣기
        public void Update()
        {
            Flush();
        }
        // 오브젝트 룸에서 생성해욧
        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null) return;
            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            // 오브젝트 타입이 Player일때
            if (type == GameObjectType.Player)
            {
                // 플레이어 룸 스폰
                Player player = gameObject as Player;
                // 플레이어가 룸안에 들어옴
                _players.Add(gameObject.Id, player);
                player.Room = this;

                player.RefreshAdditionalStat();

                bool isApplyMove = Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

                if (isApplyMove == false)
                {
                    //Console.WriteLine($"isApplyMove :: {isApplyMove}");

                }
                // 클라이언트에게 Room 에서 처리하고 있는 맵을 로드하라!
                {
                    S_ChangeMap changeMapPacket = new S_ChangeMap();
                    changeMapPacket.MapId = this.Map.MapID;
                    player.Session.Send(changeMapPacket);
                }

                // 본인한테 Room 에 있던 player정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            spawnPacket.Objects.Add(p.Info);
                    }

                    foreach (Monster m in _monsters.Values)
                        spawnPacket.Objects.Add(m.Info);

                    foreach (Projectile p in _projectile.Values)
                        spawnPacket.Objects.Add(p.Info);

                    player.Session.Send(spawnPacket);
                }

            }
            // 오브젝트 타입이 Monster
            else if (type == GameObjectType.Monster)
            {
                // 몬스터 룸 스폰
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));
                // 업데이트 한번실행
                monster.Update();
            }
            // 오브젝트 타입이 Projectile
            else if (type == GameObjectType.Projectile)
            {
                // 투사체 룸 스폰
                Projectile projecttile = gameObject as Projectile;
                _projectile.Add(gameObject.Id, projecttile);
                projecttile.Room = this;
                // 업데이트 한번실행
                projecttile.Update();
            }

            // 타인에게 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach (Player p in _players.Values)
                {
                    if (gameObject.Id != p.Id)
                        p.Session.Send(spawnPacket);
                }
            }
        }
        // Id에 따른 오브젝트 룸에서 지워욧
        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {

                Player player;
                if (_players.Remove(objectId, out player) == false) return;

                player.OnLeaveGame();
                Map.ApllyLeave(player);
                player.Room = null;


                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                // 몬스터 룸에서 삭제
                Monster monster;
                if (_monsters.Remove(objectId, out monster) == false) return;

                Map.ApllyLeave(monster);
                monster.Room = null;
            }
            else if (type == GameObjectType.Projectile)
            {
                // 투사체 룸에서 삭제
                Projectile projectile;
                if (_projectile.Remove(objectId, out projectile) == false) return;

                projectile.Room = null;
            }
            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);

                foreach (Player p in _players.Values)
                {
                    if (objectId != p.Id)
                        p.Session.Send(despawnPacket);
                }

            }
        }

        public void SpawnProjectile(GameObject owner, Data.Skill skillData)
        {

            if (skillData.projectile.name == "Arrow")
            {
                Arrow arrow = ObjectManager.Instance.Add<Arrow>();

                if (arrow == null)
                    return;
                arrow.Data = skillData;                         // 해당스킬의 정보시트
                arrow.Owner = owner;                           // 주인은 플레이어
                arrow.PosInfo.State = CreatureState.Moving;     // 화살은 계속 움직임
                arrow.PosInfo.PosX = owner.PosInfo.PosX;       // 화살 생성위치
                arrow.PosInfo.PosY = owner.PosInfo.PosY;
                arrow.PosInfo.MoveDir = owner.PosInfo.MoveDir;
                arrow.Speed = skillData.projectile.speed;       // 화살정보 입력
                                                                // 화살 입갤
                this.Push(this.EnterGame, arrow);//EnterGame(arrow);
            }
            // else if (skillData.projectile.name == "FireBall") {} // TODO 다른 투사체
        }


        // condition 에따른 플레이어 찾기
        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player p in _players.Values)
            {
                if (condition.Invoke(p) == true)
                {
                    return p;
                }
            }
            return null;
        }


        // 룸내 모든 플레이어 한테 뿌리기
        public void Broadcast(IMessage packet)
        {

            foreach (Player p in _players.Values)
            {
                p.Session.Send(packet);
            }
        }
    }
}
