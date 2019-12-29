using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Game.Structs;
using WorldServer.Storage;
using WorldServer.Game.Managers;
using WorldServer.Game.Objects.PlayerExtensions.Quests;
using WorldServer.Game.Objects;

namespace WorldServer.Game.Managers
{
    static class ScriptMgr
    {
        enum ScriptTarget
        {
            TARGET_T_PROVIDED_TARGET                = 0,            //Object that was provided to the command.

            TARGET_T_HOSTILE                        = 1,            //Our current target (ie: highest aggro).
            TARGET_T_HOSTILE_SECOND_AGGRO           = 2,            //Second highest aggro (generaly used for cleaves and some special attacks).
            TARGET_T_HOSTILE_LAST_AGGRO             = 3,            //Dead last on aggro (no idea what this could be used for).
            TARGET_T_HOSTILE_RANDOM                 = 4,            //Just any random target on our threat list.
            TARGET_T_HOSTILE_RANDOM_NOT_TOP         = 5,            //Any random target except top threat.

            TARGET_T_OWNER_OR_SELF                  = 6,            //Either self or owner if pet or controlled.
            TARGET_T_OWNER                          = 7,            //The owner of the source.
    

            TARGET_T_CREATURE_WITH_ENTRY            = 8,            //Searches for nearby creature with the given entry.
                                                                    //Param1 = creature_entry
                                                                    //Param2 = search_radius

            TARGET_T_CREATURE_WITH_GUID             = 9,            //The creature with this database guid.
                                                                    //Param1 = db_guid

            TARGET_T_CREATURE_FROM_INSTANCE_DATA    = 10,           //Find creature by guid stored in instance data.
                                                                    //Param1 = instance_data_field

            TARGET_T_GAMEOBJECT_WITH_ENTRY          = 11,           //Searches for nearby gameobject with the given entry.
                                                                    //Param1 = gameobject_entry
                                                                    //Param2 = search_radius

            TARGET_T_GAMEOBJECT_WITH_GUID           = 12,           //The gameobject with this database guid.
                                                                    //Param1 = db_guid

            TARGET_T_GAMEOBJECT_FROM_INSTANCE_DATA  = 13,           //Find gameobject by guid stored in instance data.
                                                                    //Param1 = instance_data_field

            TARGET_T_FRIENDLY                       = 14,           //Random friendly unit.
                                                                    //Param1 = search_radius
                                                                    //Param2 = (bool) exclude_target
            TARGET_T_FRIENDLY_INJURED               = 15,           //Friendly unit missing the most health.
                                                                    //Param1 = search_radius
                                                                    //Param2 = hp_percent
            TARGET_T_FRIENDLY_INJURED_EXCEPT        = 16,           //Friendly unit missing the most health but not provided target.
                                                                    //Param1 = search_radius
                                                                    //Param2 = hp_percent
            TARGET_T_FRIENDLY_MISSING_BUFF          = 17,           //Friendly unit without aura.
                                                                    //Param1 = search_radius
                                                                    //Param2 = spell_id
            TARGET_T_FRIENDLY_MISSING_BUFF_EXCEPT   = 18,           //Friendly unit without aura but not provided target.
                                                                    //Param1 = search_radius
                                                                    //Param2 = spell_id
            TARGET_T_FRIENDLY_CC                    = 19,           //Friendly unit under crowd control.
                                                                    //Param1 = search_radius
            TARGET_T_MAP_EVENT_SOURCE               = 20,           //The source WorldObject of a scripted map event.
                                                                    //Param1 = eventId
            TARGET_T_MAP_EVENT_TARGET               = 21,           //The target WorldObject of a scripted map event.
                                                                    //Param1 = eventId
            TARGET_T_MAP_EVENT_EXTRA_TARGET         = 22,           //An additional WorldObject target from a scripted map event.
                                                                    //Param1 = eventId
                                                                    //Param2 = creature_entry or gameobject_entry
            TARGET_T_NEAREST_PLAYER                 = 23,           //Nearest player within range.
                                                                    //Param1 = search-radius
            TARGET_T_NEAREST_HOSTILE_PLAYER         = 24,           //Nearest hostile player within range.
                                                                    //Param1 = search-radius
            TARGET_T_NEAREST_FRIENDLY_PLAYER        = 25,           //Nearest friendly player within range.
                                                                    //Param1 = search-radius
            TARGET_T_END
        };

        // Returns a target based on the type specified.
        public static WorldObject GetTargetByType(WorldObject source, WorldObject target, Byte targetType, UInt32 param1, UInt32 param2)
        {
            switch ((ScriptTarget)targetType)
            {
                case ScriptTarget.TARGET_T_PROVIDED_TARGET:
                    return target;
                case ScriptTarget.TARGET_T_HOSTILE:
                    if (source != null && source.IsUnit())
                        return ((Unit)source).GetVictim();
                    break;
                case ScriptTarget.TARGET_T_HOSTILE_SECOND_AGGRO:
                    // THREAT LIST NYI
                    if (source != null && source.IsUnit())
                        return ((Unit)source).GetVictim();
                    break;
                case ScriptTarget.TARGET_T_HOSTILE_LAST_AGGRO:
                    // THREAT LIST NYI
                    if (source != null && source.IsUnit())
                        return ((Unit)source).GetVictim();
                    break;
                case ScriptTarget.TARGET_T_HOSTILE_RANDOM:
                    if (source != null && source.IsUnit())
                    {
                        List<Unit> attackerList = ((Unit)source).Attackers.Values.ToList();
                        if (attackerList.Count > 0)
                        {
                            var random = new Random();
                            int index = random.Next(attackerList.Count);
                            return attackerList[index];
                        }
                    }
                    break;
                case ScriptTarget.TARGET_T_HOSTILE_RANDOM_NOT_TOP:
                    if (source != null && source.IsUnit())
                    {
                        foreach (Unit victim in ((Unit)source).Attackers.Values.ToList())
                        {
                            if (victim != ((Unit)source).GetVictim())
                                return victim;
                        }
                    }
                    break;
                case ScriptTarget.TARGET_T_OWNER_OR_SELF:
                    // PETS NYI
                    break;
                case ScriptTarget.TARGET_T_OWNER:
                    // PETS NYI
                    break;
                case ScriptTarget.TARGET_T_FRIENDLY:
                    if (source != null && source.IsUnit())
                        return ((Unit)source).FindFriendlyUnitInRange(param1 != 0 ? param1 : 30.0f);
                    break;
                case ScriptTarget.TARGET_T_FRIENDLY_INJURED:
                    if (source != null && source.IsUnit())
                        return ((Unit)source).FindInjuredFriendlyUnitInRange(param1 != 0 ? param1 : 30.0f, param2 != 0 ? param2 : 50, null);
                    break;
                case ScriptTarget.TARGET_T_FRIENDLY_INJURED_EXCEPT:
                    if (source != null && source.IsUnit())
                        return ((Unit)source).FindInjuredFriendlyUnitInRange(param1 != 0 ? param1 : 30.0f, param2 != 0 ? param2 : 50, target != null ? target.ToUnit() : null);
                    break;
                case ScriptTarget.TARGET_T_FRIENDLY_MISSING_BUFF:
                    // AURAS NYI
                    if (source != null && source.IsUnit())
                        return ((Unit)source).FindFriendlyUnitInRange(param1 != 0 ? param1 : 30.0f);
                    break;
                case ScriptTarget.TARGET_T_FRIENDLY_MISSING_BUFF_EXCEPT:
                    // AURAS NYI
                    if (source != null && source.IsUnit())
                        return ((Unit)source).FindFriendlyUnitInRange(param1 != 0 ? param1 : 30.0f);
                    break;
                case ScriptTarget.TARGET_T_FRIENDLY_CC:
                    // AURAS NYI
                    if (source != null && source.IsUnit())
                        return ((Unit)source).FindFriendlyUnitInRange(param1 != 0 ? param1 : 30.0f);
                    break;
                case ScriptTarget.TARGET_T_MAP_EVENT_SOURCE:
                    // SCRIPTED MAP EVENTS NYI
                    break;
                case ScriptTarget.TARGET_T_MAP_EVENT_TARGET:
                    // SCRIPTED MAP EVENTS NYI
                    break;
                case ScriptTarget.TARGET_T_MAP_EVENT_EXTRA_TARGET:
                    // SCRIPTED MAP EVENTS NYI
                    break;
                case ScriptTarget.TARGET_T_NEAREST_PLAYER:
                    if (source != null)
                        return source.FindNearestPlayer(param1 != 0 ? param1 : 30.0f);
                    break;
                case ScriptTarget.TARGET_T_NEAREST_HOSTILE_PLAYER:
                    if (source != null && source.IsUnit())
                        return ((Unit)source).FindNearestHostilePlayer(param1 != 0 ? param1 : 30.0f);
                    break;
                case ScriptTarget.TARGET_T_NEAREST_FRIENDLY_PLAYER:
                    if (source != null && source.IsUnit())
                        return ((Unit)source).FindNearestFriendlyPlayer(param1 != 0 ? param1 : 30.0f);
                    break;
            }
            return null;
        }
    }
}
