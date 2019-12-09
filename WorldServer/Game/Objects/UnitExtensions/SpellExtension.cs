using Common.Constants;
using Common.Database.DBC.Structures;
using Common.Helpers;
using Common.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Game.Managers;
using WorldServer.Game.Structs;

namespace WorldServer.Game.Objects.UnitExtensions
{
    public static class SpellExtension
    {
        public static SpellCheckCastResult PrepareSpell(this Unit u, SpellCast spell)
        {
            spell.Initialize();
            SpellCheckCastResult reason = u.CanCast(spell, true);

            if (u.SpellCast.ContainsKey(spell.SpellType) && !u.SpellCast[spell.SpellType].IsFinished)
                reason = SpellCheckCastResult.SPELL_FAILED_SPELL_IN_PROGRESS;

            if (reason != SpellCheckCastResult.SPELL_CAST_OK)
            {
                u.SendCastResult(reason, spell.Spell.Id);
                return reason;
            }

            spell.SendSpellStart();

            if (spell.Caster.IsTypeOf(ObjectTypes.TYPE_UNIT) && !spell.Caster.IsTypeOf(ObjectTypes.TYPE_PLAYER))
            {
                Creature unit = ((Creature)spell.Caster);
                if (spell.Targets.Target != null)
                    unit.TurnTo(spell.Targets.Target.Location);
                else if (spell.Targets.TargetMask.HasFlag((uint)SpellTargetType.TARGET_TYPE_LOCATION))
                    unit.TurnTo(spell.Targets.TargetLocation);
            }

            spell.State = SpellState.SPELL_STATE_CASTING;
            u.SpellCast[spell.SpellType] = spell;

            if (spell.Duration > 0)
            {
                u.ChannelSpell = spell.Spell.Id;
                u.ChannelObject = spell.Targets.Target.Guid;
                GridManager.Instance.SendSurrounding(u.BuildUpdate(), u);

                if (u.IsTypeOf(ObjectTypes.TYPE_PLAYER))
                {
                    PacketWriter channel = new PacketWriter(Opcodes.MSG_CHANNEL_START);
                    channel.WriteUInt32(spell.Spell.Id);
                    channel.WriteInt32(spell.Duration * 1000);
                    ((Player)u).Client.Send(channel);
                }
            }
            return SpellCheckCastResult.SPELL_CAST_OK;
        }

        public static SpellCheckCastResult Cast(this Unit u, SpellCast spell)
        {
            if (u.SpellCast[spell.SpellType] != spell) //Check it is the right spell
            {
                u.SendCastResult(SpellCheckCastResult.SPELL_FAILED_NOT_KNOWN, spell.Spell.Id);
                return SpellCheckCastResult.SPELL_FAILED_NOT_KNOWN;
            }

            SpellCheckCastResult reason = u.CanCast(spell, false);
            if (reason != SpellCheckCastResult.SPELL_CAST_OK) //Final check we can cast this
            {
                u.SendCastResult(reason, spell.Spell.Id);
                return reason;
            }

            int SpellTime = 0;
            if (spell.Spell.speed > 0)
            {
                float SpellDistance = 0;
                if (spell.Targets.Target != null)
                    SpellDistance = u.Location.Distance(spell.Targets.Target.Location);
                else if (spell.Targets.TargetLocation != null)
                    SpellDistance = u.Location.Distance(spell.Targets.TargetLocation);

                if (SpellDistance > 0)
                    SpellTime = (int)Math.Floor(SpellDistance / spell.Spell.speed * 1000);
            }

            Dictionary<WorldObject, SpellMissInfo>[] TargetsInfected = new Dictionary<WorldObject, SpellMissInfo>[3];
            TargetsInfected[0] = spell.GetTargets(0);
            TargetsInfected[1] = spell.GetTargets(1);
            TargetsInfected[2] = spell.GetTargets(2);

            if (spell.Spell.Attributes.HasFlag((uint)SpellAttributes.SPELL_ATTR_ON_NEXT_SWING_1) || spell.Spell.Attributes.HasFlag((uint)SpellAttributes.SPELL_ATTR_ON_NEXT_SWING_2))
            {
                //TODO combat spell
            }

            if (spell.Caster.IsTypeOf(ObjectTypes.TYPE_PLAYER))
            {
                spell.SendCooldown();
                Player p = (Player)u;

                //Reagents
                for (uint i = 0; i < 7; i++)
                {
                    if (spell.Spell.Reagent[i] > 0 && spell.Spell.ReagentCount[i] > 0)
                        p.RemoveItem(spell.Spell.Reagent[i], spell.Spell.ReagentCount[i]);
                }

                //TODO ammo

                switch ((PowerTypes)spell.Spell.powerType)
                {
                    case PowerTypes.TYPE_MANA:
                        if (spell.Spell.Attributes.HasFlag((uint)SpellAttributesEx.SPELL_ATTR_EX_DRAIN_ALL_POWER))
                            p.Mana.Current = 0;
                        else
                            p.Mana.Current -= spell.GetManaCost(p.Mana.Current);
                        break;
                    case PowerTypes.TYPE_RAGE:
                        if (spell.Spell.Attributes.HasFlag((uint)SpellAttributesEx.SPELL_ATTR_EX_DRAIN_ALL_POWER))
                            p.Rage.Current = 0;
                        else
                            p.Rage.Current = (p.Rage.Current - spell.GetManaCost(p.Rage.Current)) / 10;
                        break;
                    case PowerTypes.POWER_HEALTH:
                        if (spell.Spell.Attributes.HasFlag((uint)SpellAttributesEx.SPELL_ATTR_EX_DRAIN_ALL_POWER))
                            p.Health.Current = 1;
                        else
                            p.Health.Current -= spell.GetManaCost(p.Health.Current);
                        break;
                    case PowerTypes.TYPE_FOCUS:
                        if (spell.Spell.Attributes.HasFlag((uint)SpellAttributesEx.SPELL_ATTR_EX_DRAIN_ALL_POWER))
                            p.Focus.Current = 0;
                        else
                            p.Focus.Current -= spell.GetManaCost(p.Focus.Current);
                        break;
                    case PowerTypes.TYPE_ENERGY:
                        if (spell.Spell.Attributes.HasFlag((uint)SpellAttributesEx.SPELL_ATTR_EX_DRAIN_ALL_POWER))
                            p.Energy.Current = 0;
                        else
                            p.Energy.Current -= spell.GetManaCost(p.Energy.Current);
                        break;
                }
            }
            else if (spell.Caster.IsTypeOf(ObjectTypes.TYPE_UNIT))
            {
                switch ((PowerTypes)spell.Spell.powerType)
                {
                    case PowerTypes.TYPE_MANA:
                        u.Mana.Current -= spell.GetManaCost(u.Mana.Current);
                        break;
                    case PowerTypes.POWER_HEALTH:
                        u.Health.Current -= spell.GetManaCost(u.Health.Current);
                        break;
                }
            }

            u.SpellCast[spell.SpellType].State = SpellState.SPELL_STATE_FINISHED;
            Dictionary<WorldObject, SpellMissInfo> TrueTargets = TargetsInfected[0].Concat(TargetsInfected[1]).Concat(TargetsInfected[2])
                                                                 .Where(w => w.Value == SpellMissInfo.MISS_NONE).GroupBy(d => d.Key)
                                                                 .ToDictionary(d => d.Key, d => d.First().Value);

            for (int i = 0; i < 3; i++)
            {
                List<WorldObject> targets = TargetsInfected[i].Where(x => x.Value == SpellMissInfo.MISS_NONE).Select(x => x.Key).ToList();
                reason = SpellEffect.InvokeHandler((SpellEffects)spell.Spell.Effect[i], spell, targets, i, null);

                if (reason != SpellCheckCastResult.SPELL_CAST_OK)
                    break;
            }

            if (reason == SpellCheckCastResult.SPELL_CAST_OK)
            {
                spell.SendSpellGo();
                spell.SendChannelUpdate(0);
                u.SendCastResult(SpellCheckCastResult.SPELL_CAST_OK, spell.Spell.Id);
            }
            else
                u.SendCastResult(reason, spell.Spell.Id);

            spell.State = SpellState.SPELL_STATE_FINISHED;
            GridManager.Instance.SendSurrounding(u.BuildUpdate(), u);
            return reason;
        }


        public static void SendCastResult(this Unit u, SpellCheckCastResult result, uint spellid)
        {
            if (u.IsTypeOf(ObjectTypes.TYPE_PLAYER))
            {
                PacketWriter pkt = new PacketWriter(Opcodes.SMSG_CAST_RESULT);
                pkt.WriteUInt32(spellid);

                if (result == SpellCheckCastResult.SPELL_CAST_OK)
                    pkt.WriteUInt8(0);
                else
                {
                    pkt.WriteUInt8(2);
                    pkt.WriteUInt8((byte)result);
                }

                GridManager.Instance.SendSurrounding(pkt, u);
                //((Player)u).Client.Send(pkt);
            }
        }

        public static SpellCheckCastResult CanCast(this Unit u, SpellCast spell, bool firstcheck)
        {
            if (u.IsSilenced)
                return SpellCheckCastResult.SPELL_FAILED_SILENCED;

            if (u.UnitFlags.HasFlag((uint)UnitFlags.UNIT_FLAG_FLYING))
                return SpellCheckCastResult.SPELL_FAILED_ERROR;

            if (spell.Spell.powerType < (uint)PowerTypes.POWER_HEALTH)
            {
                if ((u.GetPowerValue((PowerTypes)spell.Spell.powerType, false) < spell.PowerCost) &&
                // creatures can cast spells that require a power they do not have
                    !(u.IsCreature() && !u.HasPowerType((PowerTypes)spell.Spell.powerType))) 
                    return SpellCheckCastResult.SPELL_FAILED_NO_POWER;
            }

            return SpellCheckCastResult.SPELL_CAST_OK;
        }
    }
}
