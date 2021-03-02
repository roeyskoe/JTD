using System;
using System.Collections.Generic;
using Jypeli;
using System.Text.Json;
using System.IO;
using System.Reflection;

namespace JTD
{
    /// <summary>
    /// Reads cannon definitions from a file
    /// </summary>
    public static class CannonReader
    {
        public static List<Cannon> Read()
        {
            List<Cannon> cannons = new List<Cannon>();
            JsonDocument j;
            using (var sr = File.OpenRead("Content/CannonDefinitions.json"))
            {
                j = JsonDocument.Parse(sr);
            }
            foreach (var t in j.RootElement.EnumerateObject())
            {
                JsonElement je = t.Value;
                // TODO: Error handling
                // TODO: Burst firing/Different firemodes
                int price = je.GetProperty("Price").GetInt32();
                int damage = je.GetProperty("Damage").GetInt32();
                double speed = je.GetProperty("Interval").GetDouble();
                Image image = JTD.LoadImage(je.GetProperty("Image").GetString()); // TODO: "ImageLoader" so same image is not potentially loaded multiple times
                Cannon c = new Cannon(price, damage, speed, image);

                string colorstr = je.GetProperty("AmmoColor").GetString();
                c.AmmoColor = (Color) typeof(Color).GetField(colorstr).GetValue(null);
                    
                cannons.Add(c);
            }
            

            
            return cannons;
        }
    }
}