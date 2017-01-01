using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Karthus.Modes
{
    public sealed class Combo : ModeBase
    {
        private Dictionary<SpellSlot, CheckBox> SpellUsage { get; set; }
        private Stopwatch sw { get; set; } // keeps track of how long you have been dead
        private bool swStarted { get; set; }
        private KeyBind UltWhileDeadKey { get; set; }

        public Combo(Karthus instance) : base(instance)
        {
            // Initialize properties
            SpellUsage = new Dictionary<SpellSlot, CheckBox>();

            // Setup menu
            Menu.AddGroupLabel("Spell usage");
            SpellUsage[SpellSlot.Q] = Menu.Add("Q", new CheckBox("Use Q"));
            SpellUsage[SpellSlot.W] = Menu.Add("W", new CheckBox("Use W"));
            SpellUsage[SpellSlot.E] = Menu.Add("E", new CheckBox("Use E"));
            SpellUsage[SpellSlot.R] = Menu.Add("R", new CheckBox("Use R", false));
            Menu.AddGroupLabel("Ulting while dead");
            Menu.AddLabel("Choose a key you would not normally press while playing the game like f1");
            UltWhileDeadKey = Menu.Add("key", new KeyBind("Toggle ult while dead", false, KeyBind.BindTypes.PressToggle, 24));
        }

        public override bool ShouldBeExecuted(Orbwalker.ActiveModes activeModes)
        {
            if (activeModes.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                return true;
            }
            var deadCombo = Instance.GetGlobal<CheckBox>("ComboWhileDead");
            return deadCombo != null && deadCombo.CurrentValue && Instance.IsDead;
        }

        public override bool Execute()
        {
            
            // Start counting how long you have been dead
            if (Instance.IsDead && !swStarted)
            {
                sw = new Stopwatch();
                sw.Start();
                swStarted = true;
            }
            // When alive again we have to stop counting
            if (!Instance.IsDead && swStarted)
            {
                sw.Reset();
                swStarted = false;   
            }
            // cast ult if all conditions are met
            if (UltWhileDeadKey.CurrentValue && Player.GetSpell(R.Slot).IsReady && Instance.IsDead && sw.Elapsed > TimeSpan.FromSeconds(3.1))
            {
                UltWhileDeadKey.CurrentValue = false;
                while (!R.Cast()) ;
            }
            // cast spells
            return SpellUsage.Keys.Any(slot => SpellUsage[slot].CurrentValue && Player.GetSpell(slot).IsReady && Instance.SpellHandler.CastOnBestTarget(slot));
        }
    }
}
