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
        
        public Cannonselector(List<CannonTemplate> cannons)
        {
            var context = new ListenContext();
            context.Enable();
            Layout = new HorizontalLayout();
            Layout.LeftPadding = 5;
            Layout.RightPadding = 5;
            Layout.BottomPadding = 5;
            Layout.TopPadding = 5;

            Color = Color.Transparent;
            BorderColor = Color.Red;
            IsModal = false;
            
            foreach (var cannon in cannons)
            {
                Widget main = new Widget(40, 40);
                main.Color = Color.Transparent;
                main.BorderColor = Color.Black;
                Widget image = new Widget(30, 30);
                image.Image = GameManager.Images[cannon.Image];
                main.Add(image);
                Add(main);

                wcannons.Add(main);

                Game.Instance.Mouse.ListenOn(main, MouseButton.Left, ButtonState.Pressed, () => Select(main), null).InContext(context);
            }
            selected = wcannons.First();
            Game.Instance.Mouse.ListenOn(this, HoverState.On, MouseButton.None, ButtonState.Irrelevant,() => 
            {
                GameManager.DebugText = "Päällä";
                GameManager.ListenContext.Disable();
            }, null).InContext(context);
            Game.Instance.Mouse.ListenOn(this, HoverState.Off, MouseButton.None, ButtonState.Irrelevant, () => 
            {
                GameManager.DebugText = "Pois";
                GameManager.ListenContext.Enable();
            }, null).InContext(context);
        }

        private void Select(Widget w)
        {
            selected.BorderColor = Color.Black;
            w.BorderColor = Color.Red;
            selected = w;
        }
    }
}
