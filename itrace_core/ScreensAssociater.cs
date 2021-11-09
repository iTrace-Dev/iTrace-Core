using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iTrace_Core
{
    /// <summary>
    /// An association between SmartEye WorldModel screens and Screen objects in .NET (Screens as understood by the host OS)
    /// </summary>
    class ScreensAssociater
    {
        List<ScreenAssociationTerm> rules;

        public ScreensAssociater(SEWorldScreen[] seScreens, Screen[] osScreens)
        {
            rules = new List<ScreenAssociationTerm>();
            rules.Add(new ScreenAssociationTerm(seScreens, osScreens));
            

        }

        //public Screen GetSEToScreenMapping(String seScreenName)
        //{
        //    Screen screen;
        //    bool success = screenMapping.TryGetValue(seScreenName, out screen);

        //    if (!success)
        //        throw new ArgumentException($"No screen maps to the world model name {seScreenName}");

        //    return screen;
        //}
    }

    /// <summary>
    /// Represents a rule by which screens can be associated in the ScreensAssociater
    /// </summary>
    class ScreenAssociationTerm
    {
        private List<Association> associations;

        public ScreenAssociationTerm(SEWorldScreen[] seScreens, Screen[] osScreens)
        {
            this.associations = new List<Association>();

            for (int i = 0; i < seScreens.Length; i++)
                for (int j = 0; j < osScreens.Length; j++)
                    if (Matches(seScreens[i], osScreens[j]))
                        associations.Add(new Association(seScreens[i], osScreens[j]));
        }

        public bool Matches(SEWorldScreen seScreen, Screen osScreen)
        {
            //Hardcoded example
            return true;
        }

        public Dictionary<SEWorldScreen, Screen> GetMapping()
        {
            Dictionary<SEWorldScreen, Screen> map = new Dictionary<SEWorldScreen, Screen>();

            foreach (Association assoc in associations)
                map.Add(assoc.seScreen, assoc.osScreen);

            return map;
        }

        /// <summary>
        /// Returns true if this term contains exactly one entry per screen
        /// </summary>
        /// <returns></returns>
        public bool IsOneToOne()
        {
            for (int i = 0; i < associations.Count; i++)
                for (int j = 0; j < associations.Count; j++)
                    if (i != j && associations[i].Overlaps(associations[j]))
                        return false;

            return true;
        }

        public override string ToString()
        {
            string s = "";

            if (IsOneToOne())
                s += "One-to-One Association: ";
            else
                s += "Non One-to-One Association: ";

            foreach (Association a in associations)
                s += a.ToString() + ", ";

            return s;
        }

        private class Association
        {
            public SEWorldScreen seScreen { get; private set; }
            public Screen osScreen { get; private set; }

            public Association(SEWorldScreen seScreen, Screen osScreen)
            {
                this.seScreen = seScreen;
                this.osScreen = osScreen;
            }

            public bool Overlaps(Association b)
            {
                return this.seScreen.Equals(b.seScreen) || this.osScreen.Equals(b.osScreen);
            }

            public override string ToString()
            {
                return seScreen.name + " -> " + osScreen.DeviceName;
            }
        }
    }
}
