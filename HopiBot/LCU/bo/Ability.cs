using System.Collections;
using System.Collections.Generic;

namespace HopiBot.LCU.bo
{
    public class Ability
    {
        public int AbilityLevel { get; set; } = 0;
    }

    public class Abilities
    {
        public Ability E { get; set; }
        public Ability Passive { get; set; }
        public Ability Q { get; set; }
        public Ability R { get; set; }
        public Ability W { get; set; }
    }
}
