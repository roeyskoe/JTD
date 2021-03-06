using Jypeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTD.GUI
{
    /// <summary>
    /// Select cannons to build
    /// </summary>
    class Cannonselector : Window
    {
        public Cannonselector(List<CannonTemplate> cannons)
        {
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
            }
        }
    }
}
