using Jypeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTD;
using Jypeli.Controls;

namespace JTD.GUI
{
    /// <summary>
    /// Select cannons to build
    /// </summary>
    class Cannonselector : Window
    {
        Widget selected;
        List<Widget> wcannons = new List<Widget>();
        
        Color defaultColor = new Color(63, 84, 191, 120);
        Color selectColor = new Color(167, 234, 36, 120);

        public Cannonselector(List<CannonTemplate> cannons)
        {
            var context = new ListenContext();
            context.Enable();
            Layout = new HorizontalLayout();
            Layout.LeftPadding = 5;
            Layout.RightPadding = 5;
            Layout.BottomPadding = 5;
            Layout.TopPadding = 5;

            Color = new Color(193, 66, 66, 120);
            BorderColor = Color.Red;
            IsModal = false;

            // Need to update Jypeli so that this works and disables window dragging.
            // IsCapturingMouse = false;
            
            foreach (var cannon in cannons)
            {
                Widget main = new Widget(40, 40);
                main.Color = defaultColor;
                main.BorderColor = Color.Black;
                main.Tag = cannon.Name;
                Widget image = new Widget(30, 30);
                image.Image = GameManager.Images[cannon.Image];
                main.Add(image);
                Add(main);

                wcannons.Add(main);

                Game.Instance.Mouse.ListenOn(main, MouseButton.Left, ButtonState.Pressed, () => Select(main), null).InContext(context);
            }

            selected = wcannons.First();
            Select(wcannons.First());

            Game.Instance.Mouse.ListenOn(this, HoverState.Enter, MouseButton.None, ButtonState.Irrelevant,() => 
            {
                GameManager.ListenContext.Disable();
            }, null).InContext(context);
            Game.Instance.Mouse.ListenOn(this, HoverState.Exit, MouseButton.None, ButtonState.Irrelevant, () => 
            {
                GameManager.ListenContext.Enable();
            }, null).InContext(context);
        }

        private void Select(Widget w)
        {
            selected.BorderColor = Color.Black;
            selected.Color = defaultColor;
            w.BorderColor = Color.Red;
            selected = w;
            selected.Color = selectColor;
            GameManager.CannonSelected = w.Tag.ToString();
        }
    }
}
