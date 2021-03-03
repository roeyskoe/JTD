using Jypeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTD
{
    /// <summary>
    /// Dictionary-like class to help cache commonly used images.
    /// </summary>
    public class Images : Dictionary<string, Image>
    {
        public new Image this[string key]
        {
            get
            {
                if (ContainsKey(key)) return base[key];
                else
                {
                    Image img = JTD.LoadImage(key);
                    base[key] = img;
                    return img;
                }
            }
            set
            {
                base[key] = value;
            }
        }
    }
}
