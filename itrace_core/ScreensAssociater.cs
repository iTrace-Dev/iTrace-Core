using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iTrace_Core
{
    /// <summary>
    /// An association between SmartEye WorldModel screens and Screen objects in .NET (Screens as understood by the host OS)
    /// </summary>
    class ScreensAssociater
    {
        List<ScreenAssociationRule> rules;
        Dictionary<SEWorldScreen, Screen> mapping;

        public ScreensAssociater(SEWorldScreen[] seScreens, Screen[] osScreens)
        {
            rules = new List<ScreenAssociationRule>();

            //Mapping rules in order of priority
            rules.Add(new ExactNameRule(seScreens, osScreens));
            rules.Add(new ExactResolutionRule(seScreens, osScreens));
            rules.Add(new NumberRule(seScreens, osScreens));

            mapping = new Dictionary<SEWorldScreen, Screen>();

            foreach (ScreenAssociationRule rule in rules)
                if (rule.IsOneToOne())
                {
                    mapping = rule.GetMapping();
                    break;
                }

            if (mapping.Count == 0)
                throw new ArgumentException("No reasonable mapping between world model screens and device screens!");
        }

        public Screen GetSEToScreenMapping(String seScreenName)
        {
            foreach (KeyValuePair<SEWorldScreen, Screen> entry in mapping)
            {
                if (entry.Key.name == seScreenName)
                    return entry.Value;
            }

            throw new ArgumentException($"No screen maps to the world model name {seScreenName}");
        }
    }

    /// <summary>
    /// Represents a rule by which screens can be associated in the ScreensAssociater
    /// </summary>
    public abstract class ScreenAssociationRule
    {
        private List<ScreenAssociation> associations;

        public ScreenAssociationRule(SEWorldScreen[] seScreens, Screen[] osScreens)
        {
            this.associations = new List<ScreenAssociation>();

            for (int i = 0; i < seScreens.Length; i++)
                for (int j = 0; j < osScreens.Length; j++)
                    if (Matches(seScreens[i], osScreens[j]))
                        associations.Add(new ScreenAssociation(seScreens[i], osScreens[j]));
        }

        public abstract bool Matches(SEWorldScreen seScreen, Screen osScreen);

        public Dictionary<SEWorldScreen, Screen> GetMapping()
        {
            Dictionary<SEWorldScreen, Screen> map = new Dictionary<SEWorldScreen, Screen>();

            foreach (ScreenAssociation assoc in associations)
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

            foreach (ScreenAssociation a in associations)
                s += a.ToString() + ", ";

            return s;
        }

        private class ScreenAssociation
        {
            public SEWorldScreen seScreen { get; private set; }
            public Screen osScreen { get; private set; }

            public ScreenAssociation(SEWorldScreen seScreen, Screen osScreen)
            {
                this.seScreen = seScreen;
                this.osScreen = osScreen;
            }

            public bool Overlaps(ScreenAssociation b)
            {
                return this.seScreen.Equals(b.seScreen) || this.osScreen.Equals(b.osScreen);
            }

            public override string ToString()
            {
                return seScreen.name + " -> " + osScreen.DeviceName;
            }
        }
    }

    /// <summary>
    /// Check if screens have exactly the same name
    /// </summary>
    public class ExactNameRule : ScreenAssociationRule
    {
        public ExactNameRule(SEWorldScreen[] seScreens, Screen[] osScreens) : base(seScreens, osScreens) { }

        public override bool Matches(SEWorldScreen seScreen, Screen osScreen)
        {
            return seScreen.name.Equals(osScreen.DeviceName, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    /// <summary>
    /// Check if screens share an exact resolution 
    /// </summary>
    public class ExactResolutionRule : ScreenAssociationRule
    {
        public ExactResolutionRule(SEWorldScreen[] seScreens, Screen[] osScreens) : base(seScreens, osScreens) { }

        public override bool Matches(SEWorldScreen seScreen, Screen osScreen)
        {
            return seScreen.resolution[0] == osScreen.Bounds.Size.Width && seScreen.resolution[1] == osScreen.Bounds.Size.Height;
        }
    }

    /// <summary>
    /// Check if screens share a common number somewhere in their name
    /// </summary>
    public class NumberRule : ScreenAssociationRule
    {
        public NumberRule(SEWorldScreen[] seScreens, Screen[] osScreens) : base(seScreens, osScreens) { }

        public override bool Matches(SEWorldScreen seScreen, Screen osScreen)
        {
            int seNumber;
            int osNumber;

            if (Int32.TryParse(Regex.Match(seScreen.name, @"\d+").Value, out seNumber) && 
                Int32.TryParse(Regex.Match(osScreen.DeviceName, @"\d+").Value, out osNumber))

                return seNumber == osNumber;

            return false;
        }
    }

    /// <summary>
    /// Look for spatial words like Left Center and Right and match them to Screen bounds
    /// </summary>
    public class SpatialWordRule : ScreenAssociationRule
    {
        public SpatialWordRule(SEWorldScreen[] seScreens, Screen[] osScreens) : base(seScreens, osScreens) { }

        public override bool Matches(SEWorldScreen seScreen, Screen osScreen)
        {
            throw new NotImplementedException();
        }
    }
}
