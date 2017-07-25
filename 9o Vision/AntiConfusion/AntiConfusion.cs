using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision.AntiConfusion
{
    using System.Drawing;
    using Aimtec;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Util.Cache;

    class AntiConfusion : IFeature
    {
        private bool _drawFlash;
        private bool _drawClones;
        private readonly List<Tuple<Vector3, float, string>> _recentFlashes = new List<Tuple<Vector3, float, string>>();
        private readonly HashSet<string> _cloneSet = new HashSet<string>();
        private readonly List<Obj_AI_Base> _clones = new List<Obj_AI_Base>();

        private float _flashDrawDuration = 4f;
        private float _lastTick;

        public void OnLoad(Menu rootMenu)
        {
            var menu = new Menu($"{nameof(_9o_Vision)}.antiConfusion", "Anti Confusion");
            rootMenu.Add(menu);

            menu.Add("Draw Flash Destination", true, val => _drawFlash = val);
            menu.Add("Mark Clones", true, val => _drawClones = val);

            foreach (var objAiHero in GameObjects.EnemyHeroes)
            {
                _cloneSet.Add(objAiHero.Name);
            }

            Render.OnRender += OnRender;
            Obj_AI_Hero.OnProcessSpellCast += OnSpellCast;
            Game.OnUpdate += OnUpdate;
            GameObject.OnCreate += OnGameobjectCreated;
        }

        private void OnGameobjectCreated(GameObject sender)
        {
            if (sender.Type == GameObjectType.obj_AI_Minion && _cloneSet.Contains(sender.Name))
            {
                _clones.Add((Obj_AI_Base)sender);
            }
        }

        private void OnUpdate()
        {
            var clockTime = Game.ClockTime;
            if (clockTime - _lastTick > 0.25f)
            {
                _lastTick = clockTime;

                _recentFlashes.RemoveAll(item => item.Item2 + _flashDrawDuration < clockTime);
                _clones.RemoveAll(item => !item.IsValid || item.IsDead);
            }
        }

        private void OnSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (args.Sender.Type == GameObjectType.obj_AI_Hero && args.Sender.IsEnemy && args.SpellData.Name == "SummonerFlash")
            {
                var dst = Math.Min(args.End.Distance(args.Sender.Position), 425);
                _recentFlashes.Add(new Tuple<Vector3, float, string>(args.Sender.Position.Extend(args.End, dst), Game.ClockTime, args.Sender.Name));
            }
        }

        private void OnRender()
        {
            if (_drawFlash)
                foreach (var recentFlash in _recentFlashes)
                {
                    if (Render.WorldToScreen(recentFlash.Item1, out Vector2 screenCoord))
                    {
                        Render.Circle(recentFlash.Item1, 50, 16, Color.Yellow);
                        Render.Text(screenCoord, Color.Orange, recentFlash.Item3);
                    }

                }

            if (_drawClones)
                foreach (var clone in _clones)
                {
                    if (Render.WorldToScreen(clone.Position, out Vector2 screenCoord))
                    {
                        if (clone.IsValid && !clone.IsDead)
                        {
                            Render.Circle(clone.Position, clone.BoundingRadius, 16, Color.OrangeRed);
                            Render.Text(screenCoord, Color.Red, "CLONE");
                        }
                    }
                }
        }
    }
}
