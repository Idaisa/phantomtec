using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision.GankAlerter
{
    using System.Drawing;
    using Aimtec;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Util.Cache;

    class GankAlerter : IFeature
    {
        private bool _alertOnEnemy;
        private bool _alertOnAlly;
        private readonly Dictionary<int, float> _lastVisible = new Dictionary<int, float>();
        private readonly Dictionary<int, float> _alertUntil = new Dictionary<int, float>();
        private readonly Dictionary<int, Color> _heroColor = new Dictionary<int, Color>();
        private readonly Dictionary<int, Color> _heroTextColor = new Dictionary<int, Color>();
        private float _lastTick;
        private float _leastTimeInShadowForGank = 3f;
        private float _alertFor = 8f;
        private int _lineWidth = 20;
        private float _maxDistance = 6000f;
        private float _junglerAlertDistance = 2500f;
        private Obj_AI_Hero _allyJungler;
        private float _lastJunglerDistance;
        private MenuBool _allyJunglerMenu;
        private int _junglerMenuIndex;
        private Obj_AI_Hero _me;

        public void OnLoad(Menu rootMenu)
        {
            var menu = new Menu($"{nameof(_9o_Vision)}.gankAlerter", "Gank Alerter");
            rootMenu.Add(menu);

            menu.Add("Enemy", true, val => _alertOnEnemy = val);
            menu.Add("Ally Jungler", true, val => _alertOnAlly = val);
            menu.Add("Settings");
            menu.Add("Ally Jungler Alert Distance", 2500, val => _junglerAlertDistance = val, 1000, 8000);
            menu.Add("Max Distance", 4000, val => _maxDistance = val, 2000, 20000);
            menu.Add("Alert Duration", 8, val => _alertFor = val, 1, 15);
            menu.Add("Line Width", 20, val => _lineWidth = val, 5, 30);
            menu.Add("Min FOW time", 5, val => _leastTimeInShadowForGank = val, 0, 30);


            foreach (var hero in GameObjects.Heroes)
            {
                _lastVisible[hero.NetworkId] = Game.ClockTime;
                _alertUntil[hero.NetworkId] = short.MinValue;

                _heroColor[hero.NetworkId] = hero.IsAlly ? Color.FromArgb(150, Color.Green) : Color.FromArgb(150, Color.Red);
                _heroTextColor[hero.NetworkId] = hero.IsAlly ? Color.LawnGreen : Color.Orange;
                if (hero.IsEnemy)
                {
                    if (hero.SpellBook.Spells.Any(sp => sp.Name == "SummonerSmite"))
                    {
                        _heroColor[hero.NetworkId] = Color.FromArgb(150, Color.DarkMagenta);
                        _heroTextColor[hero.NetworkId] =  Color.Magenta;
                    }
                }
            }
            
            _allyJungler = GameObjects.AllyHeroes.FirstOrDefault(ally => ally.SpellBook.Spells.Any(sp => sp.Name == "SummonerSmite"));
            _allyJunglerMenu = new MenuBool($"{nameof(_9o_Vision)}.gankAlerter.jungler", $"Ally Jungler: {_allyJungler?.ChampionName ?? "None"}", false);
            _me = ObjectManager.GetLocalPlayer();

            menu.Add("(Click below to correct jungler)");
            menu.Add(_allyJunglerMenu);

            _allyJunglerMenu.OnValueChanged += AllyJunglerMenuChanged;

            Game.OnUpdate += OnUpdate;
            Render.OnRender += OnRender;
        }

        private void AllyJunglerMenuChanged(MenuComponent sender, ValueChangedArgs args)
        {
            if (!args.GetNewValue<MenuBool>().Value)
                return;

            _allyJunglerMenu.Value = false;

            var index = _junglerMenuIndex % GameObjects.AllyHeroes.Count();
            _allyJungler = GameObjects.AllyHeroes.Skip(index).FirstOrDefault();
            _allyJunglerMenu.DisplayName = $"Ally Jungler: {_allyJungler?.ChampionName ?? "None"}";
            _junglerMenuIndex++;
        }

        private void OnUpdate()
        {
            var clockTime = Game.ClockTime;
            if (clockTime - _lastTick > 0.2f)
            {
                _lastTick = clockTime;

                foreach (var hero in GameObjects.EnemyHeroes)
                {
                    if (hero == null || !hero.IsValid || hero.IsEnemy && !_alertOnEnemy || hero.IsDead)
                        continue;

                    if (hero.IsVisible)
                    {
                        if (clockTime - _lastVisible[hero.NetworkId] > _leastTimeInShadowForGank && hero.Position.DistanceSquared(ObjectManager.GetLocalPlayer().Position) < _maxDistance * _maxDistance)
                        {
                            _alertUntil[hero.NetworkId] = clockTime + _alertFor;

                        }
                        _lastVisible[hero.NetworkId] = clockTime;
                    }

                }

                if (_alertOnAlly && _allyJungler != null)
                {
                    var distanceNow = _allyJungler.Position.Distance(ObjectManager.GetLocalPlayer().Position);
                    if (distanceNow < _junglerAlertDistance && _lastJunglerDistance > _junglerAlertDistance)
                    {
                        _alertUntil[_allyJungler.NetworkId] = clockTime + _alertFor;
                    }
                    _lastJunglerDistance = distanceNow;
                }
            }
        }

        private void OnRender()
        {
            if (!_alertOnAlly && !_alertOnEnemy || _me.IsDead)
                return;

            var clockTime = Game.ClockTime;
            var player = ObjectManager.GetLocalPlayer();

            Render.WorldToScreen(player.Position, out Vector2 playerScreenPos);

            foreach (var hero in GameObjects.Heroes)
            {
                var alertTime = _alertUntil[hero.NetworkId];
                if (clockTime < alertTime)
                {
                    Render.WorldToScreen(hero.Position, out Vector2 heroScreenPos);
                    Render.Line(playerScreenPos.X, playerScreenPos.Y, heroScreenPos.X, heroScreenPos.Y, _lineWidth, false, _heroColor[hero.NetworkId]);
                    Render.Text(playerScreenPos.Extend(heroScreenPos, 200f), _heroTextColor[hero.NetworkId], hero.ChampionName);
                }
            }

        }
    }
}
