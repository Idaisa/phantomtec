using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _9o_Vision
{
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Util;

    static class MenuExtensions
    {
        public static Menu AddSubmenu(this Menu menu, string name)
        {
            var newMenu = new Menu(menu.InternalName + "." + name, name);
            menu.Add(newMenu);
            return newMenu;
        }

        public static MenuSeperator Add(this Menu menu, string name)
        {
            var component = new MenuSeperator(menu.InternalName + "." + name, name);
            menu.Add(component);
            return component;
        }

        public static MenuBool Add(this Menu menu, string name, bool defaultValue, Action<bool> valueChanged)
        {
            var component = new MenuBool(menu.InternalName + "." + name, name, defaultValue);
            menu.Add(component);
            component.OnValueChanged += (sender, args) => valueChanged(args.GetNewValue<MenuBool>().Value);
            valueChanged(component.Value);
            return component;
        }


        public static MenuSlider Add(this Menu menu, string name, int defaultValue, Action<int> valueChanged, int min = 0, int max = 100)
        {
            var component = new MenuSlider(menu.InternalName + "." + name, name, defaultValue, min, max);
            menu.Add(component);
            component.OnValueChanged += (sender, args) => valueChanged(args.GetNewValue<MenuSlider>().Value);
            valueChanged(component.Value);
            return component;
        }

        public static MenuKeyBind Add(this Menu menu, string name, KeyCode key, Action<bool> valueChanged, KeybindType type = KeybindType.Press)
        {
            var component = new MenuKeyBind(menu.InternalName + "." + name, name, key, type);
            menu.Add(component);
            component.OnValueChanged += (sender, args) => valueChanged(args.GetNewValue<MenuKeyBind>().Value);
            valueChanged(component.Value);
            return component;
        }
    }
}
