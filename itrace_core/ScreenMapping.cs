﻿/********************************************************************************************************************************************************
* @file ScreenMapping.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

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
    class ScreenMapping
    {
        List<ScreenMappingRule> rules;
        Dictionary<SEWorldScreen, Screen> mapping;

        public ScreenMapping(SEWorldScreen[] seScreens, Screen[] osScreens)
        {
            rules = new List<ScreenMappingRule>();

            //Mapping rules in order of priority
            rules.Add(new ExactNameRule(seScreens, osScreens));
            rules.Add(new ExactResolutionRule(seScreens, osScreens));
            rules.Add(new NumberRule(seScreens, osScreens));

            mapping = new Dictionary<SEWorldScreen, Screen>();

            foreach (ScreenMappingRule rule in rules)
                if (rule.IsOneToOne())
                {
                    mapping = rule.GetMapping();
                    break;
                }
        }

        //Check that world model maps any screens
        public bool IsValid()
        {
            return mapping.Count > 0;
        }

        /// <summary>
        /// Get the screen whose name matches the name of the provided seScreenName, presumably obtained from an intersection
        /// Returns null if no screen has this name.
        /// </summary>
        /// <param name="seScreenName"></param>
        /// <returns></returns>
        public Screen GetSEToScreenMapping(String seScreenName)
        {
            foreach (KeyValuePair<SEWorldScreen, Screen> entry in mapping)
            {
                if (entry.Key.name == seScreenName)
                    return entry.Value;
            }

            return null;
        }
    }

    /// <summary>
    /// Represents a rule by which screens can be associated in the ScreensAssociater
    /// </summary>
    public abstract class ScreenMappingRule
    {
        private List<ScreenAssociation> associations;

        public ScreenMappingRule(SEWorldScreen[] seScreens, Screen[] osScreens)
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
        /// And crucially, has any associations
        /// </summary>
        /// <returns></returns>
        public bool IsOneToOne()
        {
            if (associations.Count == 0)
                return false;

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
    public class ExactNameRule : ScreenMappingRule
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
    public class ExactResolutionRule : ScreenMappingRule
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
    public class NumberRule : ScreenMappingRule
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
    public class SpatialWordRule : ScreenMappingRule
    {
        public SpatialWordRule(SEWorldScreen[] seScreens, Screen[] osScreens) : base(seScreens, osScreens) { }

        public override bool Matches(SEWorldScreen seScreen, Screen osScreen)
        {
            throw new NotImplementedException();
        }
    }
}
