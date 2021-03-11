using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    class Monster : GameObject
    {
        public int _ClassID {get; private set;} // 직업ID 1이면 근접 2면 화살
        int _searchCellDist = 10;   // 탐색범위
        int _chaseCellDist = 20;    // 얼마나 멀어질떄까지 쫒아가냐

        int _skillRange = 1;        // TEMP 스킬 사거리 데이터로 관리해줘야함

        public Monster() {
            ObjectType = GameObjectType.Monster;


            // TEMP 데이터 시트 만들기전 테스트코드
            Stat.Level = 1;
            Stat.MaxHp = 100;
            Stat.Hp = 100;
            Stat.Speed = 5.0f;

            //TODO 데이터 관리할때 몬스터 종류도 넣어주기
            _ClassID = 1;

            if(_ClassID == 1)
            {
                _searchCellDist = 10;
                _chaseCellDist = 20;
                _skillRange = 1;
            }
            else if(_ClassID == 2)
            {
                _searchCellDist = 20;
                _chaseCellDist = 25;
                _skillRange = 5;
            }

            State = CreatureState.Idle;
        }

        // 고정 상태 기계 AI
        public override void Update()
        {
            switch (State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();
                    break;
                case CreatureState.Skill:
                    UpdateSkill();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }
        }

        Player _target;
        long _nextSearchTick = 0;   // 서치 틱카운트
        long _nextMovehTick = 0;    // 무브 틱카운트


        protected virtual void UpdateIdle() {
            if (_nextSearchTick > Environment.TickCount64) return; // 서칭 쿨타임중
            _nextSearchTick = Environment.TickCount64 + 1000;

            // TODO  어떤 플레이어를 찾을까?
            Player target =  Room.FindPlayer((p) => {
                Vector2Int dir = p.CellPos - CellPos;
                return dir.cellDistFromZero < _searchCellDist;
            });

            if (target == null) return;
            _target = target;
            State = CreatureState.Moving;

        }
        // 움직일떄
        protected virtual void UpdateMoving() {
            if (_nextMovehTick > Environment.TickCount64) return;
            int moveTick = (int)(1000f / Speed);
            _nextMovehTick = Environment.TickCount64 + moveTick;

            // 유효성 체크
            if(_target == null || _target.Room != Room)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 나와 타겟의 거리
            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistFromZero;
            if (dist == 0 || dist > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }
            
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: false);

            // 쫒는거 포기
            if(path.Count < 2 || path.Count>_chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 스킬로 넘어갈지 체크
            if(dist <= _skillRange &&(dir.x == 0 || dir.y == 0))
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }

            // 이동
            Dir = GetDirFromVector(path[1] - CellPos);
            Room.Map.ApllyMove(this, path[1]);
            BroadcastMove();
        }



        long _coolTick = 0;
        // 스킬 사용 
        protected virtual void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                // 유효한 타겟인지
                if (_target == null || _target.Room != Room || _target.HP == 0)
                {
                    _target = null;
                    State = CreatureState.Moving;   // 타겟이 비정상적.. 다른타겟 찾게
                    BroadcastMove();                // 그걸 룸에 브로드케스팅
                    return;
                }

                // 스킬이 사용 가능한지
                Vector2Int dir = (_target.CellPos - CellPos);
                int dist = dir.cellDistFromZero;
                bool canUseSkill = (dist <= _skillRange && (dir.x == 0 || dir.y == 0));
                if (canUseSkill == false)
                {
                    State = CreatureState.Moving;   // 스킬을 못씀!
                    BroadcastMove();                // 그걸 룸에 브로드케스팅
                    return;
                }

                // 타겟팅 방향 주시
                MoveDir lookDir = GetDirFromVector(dir);
                if (Dir != lookDir)
                {
                    Dir = lookDir;                  // 스킬 쓸 방향이 다르면.. 방향 바라보게
                    BroadcastMove();
                }
                // 사용할 스킬정하기!
                Skill skillData = null;                              // 데이터 에서 몬스터 데이터 불러오기
                DataManager.SkillDict.TryGetValue(_ClassID, out skillData); // TEMP 몬스터도 플레이어 스킬 영향받음
                if (_ClassID == 1)
                {
                    // 데미지 판정

                    _target.OnDamaged(this, skillData.damage + Stat.Attack);
                }
                else if (_ClassID == 2)
                {
                    Room.SpawnProjectile(this, skillData);
                }

                // 사용했다! Breadcasting
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = skillData.id;
                Room.Broadcast(skill);

                // 스킬 쿨타임 적용
                int coolTick = (int)(1000 * skillData.cooldown);
                _coolTick = Environment.TickCount64 + coolTick;




            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }


        protected virtual void UpdateDead() {
        }

        void BroadcastMove()
        {
            // 다른플레이어 한테도 알려준다
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);
        }
    }
}
