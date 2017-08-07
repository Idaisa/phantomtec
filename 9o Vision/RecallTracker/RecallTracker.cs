using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision.RecallTracker
{
    using System.Drawing;
    using Aimtec;
    using Aimtec.SDK.Menu;

    class RecallTracker : IFeature
    {
        private bool _draw;
        private int _startX = 50, _startY = 50;
        private readonly Color _bgColor = Color.FromArgb(150, Color.DodgerBlue);
        private readonly Color _bgColor2 = Color.Aqua;
        private readonly Color _fgColor = Color.Black;
        private readonly List<Recall> _recalls = new List<Recall>();
        private float _barWidth = 400f;
        private float _barHeight = 30f;
        private float _barCount = 3;
        private bool _isMenuVisible;

        public void OnLoad(Menu rootMenu)
        {
            var menu = new Menu($"{nameof(_9o_Vision)}.recallTracker", "Recall Tracker");
            rootMenu.Add(menu);

            menu.Add("Draw Recalling ", true, val => _draw = val);
            menu.Add("Interface X", (int)(Render.Width / 2f - _barWidth / 2f), val => _startX = val, 0, (int)(Render.Width - _barWidth));
            menu.Add("Interface Y", (int)(Render.Height * (4 / 6f)), val => _startY = val, 0, (int)(Render.Height - _barHeight * _barCount));

            Render.OnPresent += OnPresent;
            Obj_AI_Hero.OnTeleport += OnTeleport;
            Game.OnWndProc += args => WndProc(args.Message, args.WParam, args.LParam);
        }

        private void WndProc(uint message, uint wparam, int lparam) // totally not stolen from sdk
        {
            if (message == (int)Aimtec.SDK.Util.WindowsMessages.WM_KEYDOWN && wparam == (ulong)Aimtec.SDK.Util.KeyCode.ShiftKey)
                _isMenuVisible = true;
            else if (message == (int)Aimtec.SDK.Util.WindowsMessages.WM_KEYUP && wparam == (ulong)Aimtec.SDK.Util.KeyCode.ShiftKey)
                _isMenuVisible = false;
        }

        private void OnTeleport(Obj_AI_Base sender, Obj_AI_BaseTeleportEventArgs args)
        {
            if (args.Name.Equals("recall", StringComparison.InvariantCultureIgnoreCase) && !sender.IsAlly)
            {
                _recalls.Add(new Recall(Game.ClockTime, Game.ClockTime + 8, sender as Obj_AI_Hero));
            }
            else if (args.Name.Equals("SuperRecall", StringComparison.InvariantCultureIgnoreCase) && !sender.IsAlly)
            {
                _recalls.Add(new Recall(Game.ClockTime, Game.ClockTime + 4, sender as Obj_AI_Hero));
            }
            else if (string.IsNullOrWhiteSpace(args.Name))
            {
                _recalls.RemoveAll(it => it?.Caster?.NetworkId == sender?.NetworkId);
            }
        }

        private void OnPresent()
        {
            if (_draw)
            {
                for (var index = 0; index < _recalls.Count; index++)
                {
                    var recall = _recalls[index];
                    var percent = (recall.EndTime - Game.ClockTime) / (recall.EndTime - recall.StartTime);

                    if (percent < 0) // should not be happening
                        continue;


                    Render.Line(_startX, _startY + index * _barHeight, _startX + (_barWidth * percent), _startY + index * _barHeight, 22f, false, _bgColor2);
                    Render.Line(_startX, _startY + index * _barHeight, _startX + _barWidth, _startY + index * _barHeight, _barHeight, false, _bgColor);

                    Render.Text(_startX + (_barWidth / 2f) - 15f, _startY + index * _barHeight - 5f, _fgColor, recall.Caster?.ChampionName ?? "Unknown");
                }
            }

            if (_isMenuVisible)
            {
                Render.Line((float)_startX, _startY + _barHeight * (_barCount / 2f) - _barHeight / 2f, _startX + _barWidth, _startY + _barHeight * (_barCount / 2f) - _barHeight / 2f, _barHeight * _barCount, false, Color.FromArgb(150, Color.OrangeRed));

                Render.Text(_startX + _barWidth / 2f - 64, _startY + _barHeight * (_barCount / 2f) - 28, Color.DarkRed, "RECALL TRACKER ZONE");
                Render.Text(_startX + _barWidth / 2f - 46, _startY + _barHeight * (_barCount / 2f) - 14, Color.DarkRed, "ADJUST IN MENU");
            }
        }
    }
}
