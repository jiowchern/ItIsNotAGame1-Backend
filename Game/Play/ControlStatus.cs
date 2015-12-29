using System;
using System.Linq;

using Regulus.Framework;
using Regulus.Project.ItIsNotAGame1.Data;
using Regulus.Remoting;
using Regulus.Utility;

namespace Regulus.Project.ItIsNotAGame1.Game.Play
{
    internal class ControlStatus : Regulus.Utility.IUpdatable 
    {
        private readonly ISoulBinder _Binder;

        private readonly Entity _Player;

        private readonly Mover _Mover;

        private readonly Map _Map;

        private readonly StageMachine _Status;
        

        public ControlStatus(ISoulBinder binder, Entity player, Mover mover , Map map)
        {
            _Binder = binder;
            _Player = player;
            _Mover = mover;
            _Map = map;
            _Status = new StageMachine();
        }

        void IBootable.Launch()
        {            
            _ToNormal();
        }

        private void _ToNormal()
        {            
            var status = new NormalStatus(_Binder, _Player);
            status.ExploreEvent += _ToExplore;
            status.BattleEvent += _ToBattle;
            _SetStatus(status);
        }

        private void _ToBattle()
        {
            var status = new BattleStatus(_Binder, _Player, _Map);
            status.NormalEvent += _ToNormal;
            status.CasterEvent += _ToCast;
            _SetStatus(status);
        }

        private void _ToCast(SkillCaster caster)
        {
            var status = new BattleCasterStatus(_Binder, _Player, _Map , caster);
            //status.NextCastEvent += _ToCast;
            status.DoneEvent += _ToBattle;
            _SetStatus(status);
        }

        private void _SetStatus(IStage status)
        {            
            _Status.Push(status);
        }

        private void _ToExplore(Guid obj)
        {
            var status = new ExploreStatus(_Binder, _Player , _Map , obj);
            status.DoneEvent += _ToNormal;
            _SetStatus(status);
        }

        void IBootable.Shutdown()
        {
            _Status.Termination();            
        }

        bool IUpdatable.Update()
        {
            _Status.Update();
            _ProcessCaster();
            return true;
        }

        private void _ProcessCaster()
        {
            var casters = _Player.DequeueCaster();

            if (casters.Any())
                _ToDamage();
        }

        private void _ToDamage()
        {
            var stage = new DamageStage(_Binder, _Player);
            stage.DoneEvent += _ToBattle;

            _SetStatus(stage);
        }
    }
}