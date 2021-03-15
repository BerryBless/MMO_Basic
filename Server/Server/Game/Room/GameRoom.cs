using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameRoom : JobSerializer
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
            monster.CellPos = new Vector2Int(6, 6);
            this.Push(this.EnterGame,monster);

        }


        // 누군가 주기적으로 호출해야 겜돌아감
        public void Update()
        {
            foreach (Projectile projectile in _projectile.Values)
            {
                projectile.Update();
            }

            foreach (Monster monster in _monsters.Values)
            {
                monster.Update();
            }

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

                bool isApllyMove = Map.ApllyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

                if (isApllyMove == false)
                {
                    //Console.WriteLine($"isApllyMove :: {isApllyMove}");
                    
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
                // TODO 몬스터 룸 스폰
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;

                Map.ApllyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));

            }
            // 오브젝트 타입이 Projectile
            else if (type == GameObjectType.Projectile)
            {
                // TODO 투사체 룸 스폰
                Projectile projecttile = gameObject as Projectile;
                _projectile.Add(gameObject.Id, projecttile);
                projecttile.Room = this;
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

        // C_Move패킷핸들러에서 변경할 내용 
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null) return;


            // TODO : 클라가 거짓으로 보냈는지 검증

            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            // 다른 좌표로 이동할경우, 갈 수 있는지 체크
            if (movePosInfo.PosX != info.PosInfo.PosX ||
                movePosInfo.PosY != info.PosInfo.PosY)
            {
                // 갈수있냐
                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                {
                    //없다
                    return;
                }
            }
            // 상태변화
            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            // 플레이어 이동하고 맵에도 처리
            Map.ApllyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            // 다른 플레이어 한테도 알려준다
            S_Move resMovePaket = new S_Move();
            resMovePaket.ObjectId = player.Info.ObjectId;
            resMovePaket.PosInfo = movePacket.PosInfo;

            Broadcast(resMovePaket);

        }
        // C_Skill
        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null) return;

            ObjectInfo info = player.Info;
            if (info.PosInfo.State != CreatureState.Idle) return;

            // TODO : 스킬 사용가능한지 검증


            // 통과
            info.PosInfo.State = CreatureState.Skill;

            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId; // 유저가 보내준 스킬
            Broadcast(skill);

            Data.Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skill.Info.SkillId, out skillData) == false) return;

            // 스킬 종류에 따른 로직
            switch (skillData.skillType)
            {
                case SkillType.SkillAuto: // 평타
                                          // 데미지 판정
                    Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                    GameObject target = Map.Find(skillPos);
                    if (target != null)
                    {
                        this.Push(target.OnDamaged, player, player.Info.StatInfo.Attack);
                    }
                    break;
                case SkillType.SkillProjectile: // 투사체
                    SpawnProjectile(player, skillData);
                    break;
                case SkillType.SkillNone:
                    break;
                default:
                    return;
            }

        }

        public void SpawnProjectile(GameObject owner, Data.Skill skillData)
        {

            if (skillData.projectile.name == "Arrow") {
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


        // TODO // condition 에따른 플레이어 찾기
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
