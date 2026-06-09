namespace Tunetoon
{
    public class SosCard
    {
        public string Name    { get; set; }
        public int    Stars   { get; set; }
        public string Ability { get; set; }  // Heal, Attack, Restock, Toons Hit, Cogs Miss
        public string Track   { get; set; }  // Toon-Up, Trap, Lure, Sound, Drop, etc.
        public string Gag     { get; set; }
        public string Effect  { get; set; }
    }

    public static class SosCardData
    {
        public static readonly SosCard[] All = new SosCard[]
        {
            new SosCard { Name = "Madam Chuckle",   Stars = 3, Ability = "Heal",      Track = "Toon-Up", Gag = "Megaphone",      Effect = "+60 laff to one Toon / +30 to two / +20 to three / +15 to four" },
            new SosCard { Name = "Daffy Don",        Stars = 4, Ability = "Heal",      Track = "Toon-Up", Gag = "Bamboo Cane",    Effect = "+120 laff to one Toon / +60 to two / +40 to three / +30 to four" },
            new SosCard { Name = "Flippy",           Stars = 5, Ability = "Heal",      Track = "Toon-Up", Gag = "Juggling Cubes", Effect = "+180 laff to one Toon / +90 to two / +60 to three / +45 to four" },
            new SosCard { Name = "Clerk Will",       Stars = 3, Ability = "Attack",    Track = "Trap",    Gag = "Quicksand",      Effect = "Deals 60 damage to all lured Cogs" },
            new SosCard { Name = "Clerk Penny",      Stars = 4, Ability = "Attack",    Track = "Trap",    Gag = "Trapdoor",       Effect = "Deals 120 damage to all lured Cogs" },
            new SosCard { Name = "Clerk Clara",      Stars = 5, Ability = "Attack",    Track = "Trap",    Gag = "TNT",            Effect = "Deals 180 damage to all lured Cogs" },
            new SosCard { Name = "Stinky Ned",       Stars = 3, Ability = "Lure",      Track = "Lure",    Gag = "Small Magnet",   Effect = "Lures all Cogs for 4 rounds" },
            new SosCard { Name = "Nancy Gas",        Stars = 4, Ability = "Lure",      Track = "Lure",    Gag = "Big Magnet",     Effect = "Lures all Cogs for 5 rounds" },
            new SosCard { Name = "Lil Oldman",       Stars = 5, Ability = "Lure",      Track = "Lure",    Gag = "Hypno-goggles",  Effect = "Lures all Cogs for 6 rounds" },
            new SosCard { Name = "Barbara Seville",  Stars = 3, Ability = "Attack",    Track = "Sound",   Gag = "Aoogah",         Effect = "Deals 35 damage to all Cogs" },
            new SosCard { Name = "Sid Sonata",       Stars = 4, Ability = "Attack",    Track = "Sound",   Gag = "Elephant Trunk", Effect = "Deals 55 damage to all Cogs" },
            new SosCard { Name = "Moe Zart",         Stars = 5, Ability = "Attack",    Track = "Sound",   Gag = "Foghorn",        Effect = "Deals 75 damage to all Cogs" },
            new SosCard { Name = "Clumsy Ned",       Stars = 3, Ability = "Attack",    Track = "Drop",    Gag = "Big Weight",     Effect = "Deals 60 damage to all unlured Cogs" },
            new SosCard { Name = "Franz Neckvein",   Stars = 4, Ability = "Attack",    Track = "Drop",    Gag = "Safe",           Effect = "Deals 100 damage to all unlured Cogs" },
            new SosCard { Name = "Barnacle Bessie",  Stars = 5, Ability = "Attack",    Track = "Drop",    Gag = "Grand Piano",    Effect = "Deals 170 damage to all unlured Cogs" },
            new SosCard { Name = "Professor Guffaw", Stars = 3, Ability = "Restock",   Track = "Toon-Up", Gag = "Lipstick",       Effect = "Restocks 12 Toon-Up gags of the highest levels for all Toons" },
            new SosCard { Name = "Clerk Ray",        Stars = 3, Ability = "Restock",   Track = "Trap",    Gag = "Lipstick",       Effect = "Restocks 12 Trap gags of the highest levels for all Toons" },
            new SosCard { Name = "Doctor Drift",     Stars = 3, Ability = "Restock",   Track = "Lure",    Gag = "Lipstick",       Effect = "Restocks 12 Lure gags of the highest levels for all Toons" },
            new SosCard { Name = "Melody Wavers",    Stars = 3, Ability = "Restock",   Track = "Sound",   Gag = "Lipstick",       Effect = "Restocks 12 Sound gags of the highest levels for all Toons" },
            new SosCard { Name = "Baker Bridget",    Stars = 3, Ability = "Restock",   Track = "Throw",   Gag = "Lipstick",       Effect = "Restocks 12 Throw gags of the highest levels for all Toons" },
            new SosCard { Name = "Sofie Squirt",     Stars = 3, Ability = "Restock",   Track = "Squirt",  Gag = "Lipstick",       Effect = "Restocks 12 Squirt gags of the highest levels for all Toons" },
            new SosCard { Name = "Shelly Seaweed",   Stars = 3, Ability = "Restock",   Track = "Drop",    Gag = "Lipstick",       Effect = "Restocks 12 Drop gags of the highest levels for all Toons" },
            new SosCard { Name = "Professor Pete",   Stars = 5, Ability = "Restock",   Track = "All",     Gag = "Lipstick",       Effect = "Restocks 45 gags of the highest levels for all Toons" },
            new SosCard { Name = "Soggy Bottom",     Stars = 3, Ability = "Toons Hit",  Track = "--",     Gag = "Pixie Dust",     Effect = "Increases accuracy of gags by 75% for 3 rounds" },
            new SosCard { Name = "Soggy Nell",       Stars = 4, Ability = "Toons Hit",  Track = "--",     Gag = "Pixie Dust",     Effect = "Increases accuracy of gags by 75% for 4 rounds" },
            new SosCard { Name = "Sticky Lou",       Stars = 5, Ability = "Toons Hit",  Track = "--",     Gag = "Pixie Dust",     Effect = "Increases accuracy of gags by 75% for 5 rounds" },
            new SosCard { Name = "Flim Flam",        Stars = 3, Ability = "Cogs Miss",  Track = "--",     Gag = "Pixie Dust",     Effect = "Reduces accuracy of Cog attacks by 75% for 3 rounds" },
            new SosCard { Name = "Mr. Freeze",       Stars = 4, Ability = "Cogs Miss",  Track = "--",     Gag = "Pixie Dust",     Effect = "Reduces accuracy of Cog attacks by 75% for 4 rounds" },
            new SosCard { Name = "Julius Wheezer",   Stars = 5, Ability = "Cogs Miss",  Track = "--",     Gag = "Pixie Dust",     Effect = "Reduces accuracy of Cog attacks by 75% for 5 rounds" },
        };
    }
}
