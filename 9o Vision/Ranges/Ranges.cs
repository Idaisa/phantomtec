using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision.Ranges
{
    using System.Drawing;
    using Aimtec;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Util.Cache;

    class Ranges : IFeature
    {
        private bool _enemyTowerRanges;
        private bool _allyTowerRanges;
        private bool _enemyAttackRangesRanged;
        private bool _enemyAttackRangesMelee;
        private bool _drawMyHitbox;

        private List<Obj_AI_Turret> _allyTurrets;
        private List<Obj_AI_Turret> _enemyTurrets;
        private Obj_AI_Hero _hero;

        private Func<Obj_AI_Turret, float, float> _rangeCalculator;

        public void OnLoad(Menu rootMenu)
        {
            var menu = new Menu($"{nameof(_9o_Vision)}.ranges", "Ranges");
            rootMenu.Add(menu);

            menu.Add("Enemy Tower Ranges", false, val => _enemyTowerRanges = val);
            menu.Add("Ally Tower Ranges", false, val => _allyTowerRanges = val);
            menu.Add("Ranged Enemy Attack Ranges", false, val => _enemyAttackRangesRanged = val);
            menu.Add("Melee Enemy Attack Ranges", false, val => _enemyAttackRangesMelee = val);
            menu.Add("Only makes sense with evade drawings:");
            menu.Add("Draw My Hitbox", false, val => _drawMyHitbox = val);

            _allyTurrets = ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsAlly).ToList();
            _enemyTurrets = ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsEnemy).ToList();
            _hero = ObjectManager.GetLocalPlayer();

            switch (Game.MapId)
            {
                case GameMapId.TwistedTreeline:
                    _rangeCalculator = (turret, boundingBox) => 575f + turret.BoundingRadius + boundingBox; 
                    _allyTurrets.RemoveAll(turr => turr.Lane == LaneType.Middle);
                    _enemyTurrets.RemoveAll(turr => turr.Lane == LaneType.Middle);
                    break;
                default:
                    _rangeCalculator = (turret, boundingBox) => 775f + boundingBox; 
                    _allyTurrets.RemoveAll(turr => turr.Name.Contains("Shrine"));
                    _enemyTurrets.RemoveAll(turr => turr.Name.Contains("Shrine"));
                    break;
            }

            Render.OnRender += OnRender;
            GameObject.OnDestroy += GameObjectOnOnDestroy;
        }

        private void GameObjectOnOnDestroy(GameObject sender)
        {
            if (sender.Type == GameObjectType.obj_AI_Turret)
            {
                _enemyTurrets.RemoveAll(turr => turr.NetworkId == sender.NetworkId);
                _allyTurrets.RemoveAll(turr => turr.NetworkId == sender.NetworkId);
            }
        }

        private void OnRender()
        {
            if (_enemyTowerRanges)
            {
                foreach (var objAiTurret in _enemyTurrets)
                {
                    var range = _rangeCalculator(objAiTurret, _hero.BoundingRadius);
                    var hitRange = range + _hero.BoundingRadius;
                    Render.Circle(objAiTurret.Position, range, 32, objAiTurret.Position.DistanceSquared(_hero.Position) < hitRange * hitRange ? Color.OrangeRed : Color.Yellow);
                }
            }
            if (_allyTowerRanges)
            {
                foreach (var objAiTurret in _allyTurrets)
                {
                    var range = _rangeCalculator(objAiTurret, 65);
                    Render.Circle(objAiTurret.Position, range, 32, Color.Green);
                }
            }
            if (_enemyAttackRangesRanged || _enemyAttackRangesMelee)
            {
                foreach (var objAiHero in GameObjects.EnemyHeroes)
                {
                    if (!objAiHero.IsVisible || objAiHero.IsDead || objAiHero.IsRanged && !_enemyAttackRangesRanged || objAiHero.IsMelee && !_enemyAttackRangesMelee)
                        continue;

                    var range = objAiHero.GetFullAttackRange(_hero);
                    Render.Circle(objAiHero.Position, range, 32, _hero.Position.DistanceSquared(objAiHero.Position) < range * range ? Color.OrangeRed : Color.White);
                }
            }
            if (_drawMyHitbox)
            {
                Render.Circle(_hero.Position, _hero.BoundingRadius, 16, Color.White);
            }
        }

    }
}
