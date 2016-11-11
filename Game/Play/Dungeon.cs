﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

using Regulus.CustomType;
using Regulus.Framework;
using Regulus.Project.ItIsNotAGame1.Data;
using Regulus.Utility;

namespace Regulus.Project.ItIsNotAGame1.Game.Play
{
    internal class Dungeon :Regulus.Utility.IUpdatable , IMapGate
    {
        private readonly RealmInfomation _RealmInfomation;

        private readonly Dictionary<ENTITY, int> _EntityEnteranceResource;
        private readonly Dictionary<ENTITY, int> _EntityFieldResource;
        private readonly Dictionary<LEVEL_UNIT, EntityGroupBuilder> _LevelUnitToGroupBuilder;
        private readonly TimesharingUpdater _Updater;

        private readonly List<Aboriginal> _Aboriginals;

        private readonly IRandom _Random;

        private readonly List<Entity> _WaitingRoom;

        private readonly List<Entity> _Inorganics;

        private readonly Map _Map;

        private bool _Ready;

        private readonly bool _Valid;

        public Dungeon(RealmInfomation realm_infomation)
        {
            _RealmInfomation = realm_infomation;
            _Valid = true;
            _Map = new Map();

            _EntityEnteranceResource = new Dictionary<ENTITY, int>
            {
                { ENTITY.ACTOR1, 50},
                { ENTITY.ACTOR2, 50},
                { ENTITY.ACTOR3, 50},
                { ENTITY.ACTOR4, 50},
                { ENTITY.ACTOR5, 50},
            };

            _EntityFieldResource = new Dictionary<ENTITY, int>
            {
                { ENTITY.ACTOR1, 50},
                { ENTITY.ACTOR2, 50},
                { ENTITY.ACTOR3, 50},
                { ENTITY.ACTOR4, 50},
                { ENTITY.ACTOR5, 50},
            };
            _Inorganics = new List<Entity>();
            _WaitingRoom = new List<Entity>();            

            _LevelUnitToGroupBuilder = new Dictionary<LEVEL_UNIT, EntityGroupBuilder>
            {
                {LEVEL_UNIT.WALL, new EntityGroupBuilder("wall", _AllInorganics )},
                {LEVEL_UNIT.ENTERANCE1, new EntityGroupBuilder("enterance", new EnterancePostProduction(new [] {ENTITY.ACTOR1}).ProcessEnterance )},
                {LEVEL_UNIT.ENTERANCE2, new EntityGroupBuilder("enterance", new EnterancePostProduction(new [] {ENTITY.ACTOR3, ENTITY.ACTOR2 }).ProcessStronghold )},
                {LEVEL_UNIT.ENTERANCE3, new EntityGroupBuilder("enterance", new EnterancePostProduction(new [] {ENTITY.ACTOR4, ENTITY.ACTOR2 }).ProcessStronghold )}, 
                {LEVEL_UNIT.ENTERANCE4, new EntityGroupBuilder("enterance", new EnterancePostProduction(new [] {ENTITY.ACTOR5, ENTITY.ACTOR2 }).ProcessStronghold )}, 
                {LEVEL_UNIT.CHEST, new EntityGroupBuilder("chest", new EnterancePostProduction(new [] {ENTITY.ACTOR3 , ENTITY.ACTOR4 , ENTITY.ACTOR5 }).ProcessChest ) },
                //{LEVEL_UNIT.CHEST, new EntityGroupBuilder("chest", _AllInorganics) },
                {LEVEL_UNIT.FIELD, new EntityGroupBuilder("field", new EnterancePostProduction(new [] {ENTITY.ACTOR3 , ENTITY.ACTOR4 , ENTITY.ACTOR5 }).ProcessField ) },
                {LEVEL_UNIT.GATE, new EntityGroupBuilder("thickwall", _AllInorganics )},
                {LEVEL_UNIT.POOL, new EntityGroupBuilder("pool", _ResourcePostProduction )},                
            };
            _Random = Regulus.Utility.Random.Instance;            
            _Updater = new TimesharingUpdater(1.0f / 30.0f);
            _Aboriginals = new List<Aboriginal>();

        }

        public IMapFinder Map { get {return _Map;} }

        private IUpdatable _AllInorganics(Entity[] entitys, IMapGate gate , IMapFinder finder)
        {
            _Inorganics.AddRange(entitys);
            return null;
        }

        private IUpdatable _ResourcePostProduction(Entity[] entitys, IMapGate gate , IMapFinder finder)
        {

            foreach (var entity in entitys)
            {
                entity.SetBag( new ResourceBag());
            }
            _Inorganics.AddRange(entitys);

            return null;
        }

        

        private Aboriginal _Create(Map map,ENTITY type, ItemProvider item_provider)
        {
            
            var entiry = EntityProvider.Create(type);
            var items = item_provider.FromStolen();
            foreach (var item in items)
            {
                entiry.Bag.Add(item);
            }            
            var wisdom = new StandardWisdom(entiry);
            var aboriginal = new Aboriginal(map, this, entiry, wisdom);            
            return aboriginal;
        }

        void IBootable.Launch()
        {
            
            var map = _Map;

            if (_RealmInfomation.IsMaze())
            {
                _BuildMaze(map);
            }
            else
            {
                _BuildTown();
            }
            foreach (var inorganic in _Inorganics)
            {
                map.JoinStaff(inorganic);
            }
            _Ready = true;
        }

        private void _BuildTown()
        {
            var data = Resource.Instance.FindTown(_RealmInfomation.Town);
            foreach (var staticEntity in data.Walls)
            {
                var entity = EntityProvider.CreateStatic(staticEntity.Mesh);                
                _Inorganics.Add(entity);
            }

            foreach (var portalData in data.Portals)
            {
                var entity = EntityProvider.Create(ENTITY.PORTAL);
                IIndividual individual = entity;
                individual.SetPosition(portalData.Position);

                _Updater.Add(new PortalWisdom(entity , portalData.TargetRealm , this , _Map));
                
            }
        }

        private void _BuildMaze(Map map)
        {
            var level = _CreateLevel(this, map);

            foreach (var unit in level)
            {
                var builder = _LevelUnitToGroupBuilder[unit.Type];
                var updater = builder.Create(unit.Direction, unit.Center, this, map);
                if (updater != null)
                {
                    _Updater.Add(updater);
                }
            }
        }

        private Level _CreateLevel(IMapGate gate, Map map)
        {            
            var builder = new LevelGenerator(_RealmInfomation.Width, _RealmInfomation.Height);
            var level = builder.Build(_RealmInfomation.Dimension);
            return level;
        }

        private void _Join(Aboriginal aboriginal)
        {
            _Updater.Add(aboriginal);
            _Aboriginals.Add(aboriginal);
        }

        void IBootable.Shutdown()
        {

            foreach (var inorganic in _Inorganics)
            {
                _Map.Left(inorganic);
            }

            _Updater.Shutdown();
        }

        bool IUpdatable.Update()
        {
            _Updater.Working();
            return true;
        }

        string IMapGate.Name { get { return _RealmInfomation.Name; } }
        

        void IMapGate.Left(Entity player)
        {
            _Map.Left(player);
            _WaitingRoom.RemoveAll((e) => e.Id == player.Id);
        }

        void IMapGate.Join(Entity player)
        {
            if (_InWaitingRoom(player))
            {
                _WaitingRoom.Add(player);
                if(_WaitEvent != null)
                    _WaitEvent(player.Id);
            }
                
            else
            {
                _Map.JoinStaff(player);
            }
        }

        private bool _InWaitingRoom(Entity player)
        {
            if (EntityData.IsActor(player.Type))
            {                
                return true;
            }
            return false;
        }

        void IMapGate.Pass(Vector2 position, ENTITY[] types)
        {
            var entity = (from e in _WaitingRoom where types.Any( t => t == e.Type) select e).FirstOrDefault();
            if (entity != null)
            {
                _Join(position, entity);
            }
                
        }

        

        void IMapGate.Pass(Vector2 position, Guid id)
        {
            var entity = (from e in _WaitingRoom where e.Id == id select e).FirstOrDefault();
            if (entity != null)
            {
                _Join(position, entity);
            }
        }

        private void _Join(Vector2 position, Entity entity)
        {
            IIndividual individual = entity;
            _WaitingRoom.RemoveAll((e) => e.Id == entity.Id);
            individual.SetPosition(position);
            _Map.JoinStaff(entity);
        }

        Guid IMapGate.SpawnEnterance(ENTITY type)
        {
            if (_EntityEnteranceResource[type] > 0)
            {
                var itemProvider = new ItemProvider();
                var aboriginal = _Create(_Map, type, itemProvider);
                _EntityEnteranceResource[type]--;
                _Join(aboriginal);                
                aboriginal.DoneEvent += () =>
                {
                    _EntityEnteranceResource[type]++;
                    _Left(aboriginal.Entity.Id);
                };
                return aboriginal.Entity.Id;
            }
            return Guid.Empty;
        }

        Guid[] IMapGate.SpawnField(ENTITY[] types)
        {
            foreach (var type in types)
            {
                if(_EntityFieldResource[type] <= 0)
                    return new Guid[0];
            }
            List<Guid> ids = new List<Guid>();
            foreach (var type in types)
            {
                if (_EntityFieldResource[type] > 0)
                {
                    var itemProvider = new ItemProvider();
                    var aboriginal = _Create(_Map, type, itemProvider);
                    _EntityFieldResource[type]--;
                    _Join(aboriginal);
                    aboriginal.DoneEvent += () =>
                    {
                        _EntityFieldResource[type]++;

                        _Left(aboriginal.Entity.Id);
                        
                    };
                    ids.Add(aboriginal.Entity.Id);
                }
            }
            return ids.ToArray();
        }

        Guid IMapGate.Spawn(ENTITY type)
        {
            var itemProvider = new ItemProvider();
            var aboriginal = _Create(_Map, type, itemProvider);
            aboriginal.DoneEvent += () =>
            {                
                _Left(aboriginal.Entity.Id);
            };
            _Join(aboriginal);
            return aboriginal.Entity.Id;
        }

        void IMapGate.Exit(Guid contestant)
        {
            _Left(contestant);
        }

        private void _Left(Guid contestant)
        {
            var aboriginals = (from a in _Aboriginals where a.Entity.Id == contestant select a);

            foreach (var aboriginal in aboriginals)
            {
                _Updater.Remove(aboriginal);
            }
            _Aboriginals.RemoveAll((a1) => aboriginals.Any((a2) => a2.Entity.Id == a1.Entity.Id));
        }

        private event Action<Guid> _WaitEvent;

        event Action<Guid> IMapGate.WaitEvent
        {
            add { this._WaitEvent += value; }
            remove { this._WaitEvent -= value; }
        }

        public bool IsReady()
        {

            return _Ready;
        }

        public bool IsValid()
        {
            return _Valid;
        }
    }
}
