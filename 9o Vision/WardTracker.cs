using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision
{
    using System.Drawing;
    using Aimtec;
    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;

    class WardTracker
    {
        private readonly HashSet<string> _wardNames = new HashSet<string> { "SightWard", "VisionWard", "JammerDevice" };
        private readonly HashSet<string> _wardSpells = new HashSet<string> { "TrinketTotemLvl1", "ItemGhostWard", "JammerDevice" };
        private readonly Dictionary<string, Func<Obj_AI_Base, int>> _wardSpellToTimeResolveFunc = new Dictionary<string, Func<Obj_AI_Base, int>>
        {
            { "TrinketTotemLvl1", hero => (int)((hero.Level - 1) * 3.5f + 60.5f)},
            { "ItemGhostWard", hero => 150 },
            { "JammerDevice", hero => ushort.MaxValue }
        };

        private readonly List<Obj_AI_Minion> _wards = new List<Obj_AI_Minion>();
        private readonly List<CalculatedWard> _calculatedWards = new List<CalculatedWard>();

        private bool _drawWards = true;
        private bool _drawWardsMinimap = true;
        private bool _drawTimes = true;
        private float _lastTick = 0f;

        private readonly Color _greenWard = Color.LawnGreen;
        private readonly Color _pinkWard = Color.Magenta;
        private readonly Color _minimapUnderlay = Color.FromArgb(175, 0, 0, 0);

        public void OnLoad()
        {
            var menu = new Menu($"{nameof(_9o_Vision)}", "9o Vision", true)
            {
                { "Draw wards", true, val => _drawWards = val },
                { "Draw wards on Minimap", true, val => _drawWardsMinimap = val },
                { "Draw Times (where known)", true, val => _drawTimes = val }
            };
            menu.Attach();

            GameObject.OnCreate += OnGameObjectCreated;
            GameObject.OnDestroy += OnGameObjectDestroyed;
            Render.OnRender += OnRender;
            Render.OnPresent += OnPresent;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            Game.OnUpdate += OnUpdate;
        }

        private void OnPresent()
        {
            if (_drawWardsMinimap)
            {
                foreach (var ward in _wards)
                {
                    if (Render.WorldToMinimap(ward.Position, out Vector2 screenCoord))
                    {
                        Render.Line(screenCoord.X - 5, screenCoord.Y + 5, screenCoord.X + 5, screenCoord.Y + 5, 10, false, _minimapUnderlay);
                        Render.Text(screenCoord.X - 2, screenCoord.Y - 2, ward.Name == "JammerDevice" ? _pinkWard :_greenWard, "x");
                        
                    }
                }

                foreach (var ward in _calculatedWards)
                {
                    if (Render.WorldToMinimap(ward.Position, out Vector2 screenCoord))
                    {
                        Render.Line(screenCoord.X - 5, screenCoord.Y + 5, screenCoord.X + 5, screenCoord.Y + 5, 10, false, _minimapUnderlay);
                        Render.Text(screenCoord.X - 2, screenCoord.Y - 2, ward.Color, "x");
                    }
                }
            }
        }

        private void OnUpdate()
        {
            var clockTime = Game.ClockTime;
            if (clockTime - _lastTick > 1f)
            {
                _lastTick = clockTime;

                for (var i = 0; i < _calculatedWards.Count; i++)
                    if (_calculatedWards[i].EndTime < clockTime)
                        _calculatedWards.RemoveAt(i--);
            }
        }

        private void OnGameObjectDestroyed(GameObject sender)
        {
            if (!_wardNames.Contains(sender.Name))
                return;

            _wards.RemoveAll(item => item.NetworkId == sender.NetworkId);
            _calculatedWards.RemoveAll(item => item.Position.DistanceSquared(sender.Position) < 25f * 25f);
        }

        private void OnProcessSpell(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (args.Sender.IsAlly || args.Sender.Type != GameObjectType.obj_AI_Hero)
                return;

            if (_wardSpells.Contains(args.SpellData.Name))
            {
                var time = _wardSpellToTimeResolveFunc[args.SpellData.Name](args.Sender);
                _calculatedWards.Add(new CalculatedWard(Game.ClockTime + time, args.End, args.SpellData.Name == "JammerDevice" ? _pinkWard : _greenWard));
                EliminateDuplicates();
            }
        }

        private void OnRender()
        {
            if (_drawWards)
            {
                foreach (var ward in _wards)
                {
                    Render.Circle(ward.Position, 75, 32, ward.Name == "JammerDevice" ? _pinkWard : _greenWard);
                }
                foreach (var ward in _calculatedWards)
                {
                    Render.Circle(ward.Position, 75, 32, ward.Color);
                }
            }

            if (_drawTimes)
            {
                foreach (var calculatedWard in _calculatedWards)
                {
                    if (calculatedWard.EndTime - Game.ClockTime > 300) // Kappa
                        continue;

                    if (Render.WorldToScreen(calculatedWard.Position, out Vector2 screenCoord))
                    {
                        screenCoord.Y -= 5;
                        if (screenCoord.X > 0 && screenCoord.Y > 0 && screenCoord.X < Render.Width && screenCoord.Y < Render.Height)
                        {
                            Render.Text(screenCoord.X, screenCoord.Y, calculatedWard.Color, $"{calculatedWard.EndTime - Game.ClockTime:F0} sec");
                        }
                    }
                }
            }
        }

        private void EliminateDuplicates()
        {
            foreach (var calculatedWard in _calculatedWards)
            {
                for (var index = 0; index < _wards.Count; index++)
                {
                    if (_wards[index].Position.DistanceSquared(calculatedWard.Position) < 25 * 25)
                        _wards.RemoveAt(index--);
                }
            }
        }

        private void OnGameObjectCreated(GameObject sender)
        {
            if (_wardNames.Contains(sender.Name) && !sender.IsAlly)
            {
                _wards.Add((Obj_AI_Minion)sender);
                EliminateDuplicates();
            }
        }
    }
}
