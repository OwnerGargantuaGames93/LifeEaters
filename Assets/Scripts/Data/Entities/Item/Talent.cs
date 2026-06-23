// A talent is an ability that the player can learn and use
// Some talents require a certain level of characteristics to be learned
// The player can learn a talent by spending points

using System;
using System.Collections.Generic;
using Data.Entities.Player;

namespace Data.Entities.Item
{
    [Serializable]
    public class TalentItem: Item {
        public TalentId Id;
        
        // Learn cost
        public int PointsCost;

        // The characteristics that the player must have to learn the talent
        public PlayerCharacteristics NecessaryCharacteristics;
        
        // Required talents to learn this talent
        public List<TalentId> RequiredTalents;
    }

    public enum TalentId
    {
        Run,
        Jump,
        DoubleJump,
        Dash,
        WallJump,
        Swim,
        PillEater,
        Teleport,
        Headbutt,
        Climber,
        ExpertClimber,
        Grab,
        Throw,
        FlyingDash,
        Smash,
        ChargedThrow,
        Pull,
        PutDown,
        Invincibility,
        Invisibility,
        Stop,
        Mining,
        HookUp,
        Transporter,
        Juggling,
        ThrowDown,
        Shelter,
        Gourmet,
        HumanPray,
        AlienPray,
        HumanOratory,
        AlienOratory,
        Drain,
        Focus,
        Transfer,
        Metamorphosis,
        LifeEater,
        Pit,
    }
}