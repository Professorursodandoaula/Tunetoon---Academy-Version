using System.Collections.Generic;

namespace Tunetoon
{
    // ── Entrada de suit por toon ──────────────────────────────────────────────
    public class ToonSuitEntry
    {
        public string SuitType  { get; set; } = "";   // nome do cog type
        public int    SuitLevel { get; set; } = 1;    // nível selecionado
        public bool   IsV2      { get; set; } = false; // V2.0 flag
    }

    // ── Dados de lookup CEO ───────────────────────────────────────────────────
    public static class BossCalcData
    {
        // CEO — Bossbot suits
        // Chave: (CogType, Level) → Value (peso na média)
        public static readonly Dictionary<(string, int), int> CeoValues = BuildCeoValues();

        // CJ — Lawbot suits
        public static readonly Dictionary<(string, int), int> CjValues = BuildCjValues();

        // CEO: tipos e faixas de nível válidas
        public static readonly (string Name, int MinLv, int MaxLv)[] CeoSuits =
        {
            ("Flunky",           1,  5),
            ("Pencil Pusher",    2,  6),
            ("Yesman",           3,  7),
            ("Micromanager",     4,  8),
            ("Downsizer",        5,  9),
            ("Head Hunter",      6, 10),
            ("Corporate Raider", 7, 11),
            ("The Big Cheese",   8, 50),
        };

        // CJ: tipos e faixas de nível válidas
        public static readonly (string Name, int MinLv, int MaxLv)[] CjSuits =
        {
            ("Bottom Feeder",    1,  5),
            ("Bloodsucker",      2,  6),
            ("Double Talker",    3,  7),
            ("Ambulance Chaser", 4,  8),
            ("Back Stabber",     5,  9),
            ("Spin Doctor",      6, 10),
            ("Legal Eagle",      7, 11),
            ("Big Wig",          8, 50),
        };

        // ── CEO lookup table (CogType, Level) → Value ────────────────────────
        // Fonte: tabela da imagem. Value sobe 1 por nível dentro de cada tipo.
        // Faixas de value extraídas da tabela original:
        //   Flunky 1-5 → 0-4
        //   Pencil Pusher 2-6 → 5-9 (lv2=5..lv6=9... wait, tabela mostra lv2=5)
        // Reconstrução a partir das linhas da tabela:
        private static Dictionary<(string, int), int> BuildCeoValues()
        {
            var d = new Dictionary<(string, int), int>();

            // Flunky lv1-5 → value 0-4
            AddRange(d, "Flunky",           1,  5,  0);
            // Pencil Pusher lv2-6 → value 5-9
            AddRange(d, "Pencil Pusher",     2,  6,  5);
            // Yesman lv3-7 → value 10-14
            AddRange(d, "Yesman",            3,  7, 10);
            // Micromanager lv4-8 → value 15-19
            AddRange(d, "Micromanager",      4,  8, 15);
            // Downsizer lv5-9 → value 20-24
            AddRange(d, "Downsizer",         5,  9, 20);
            // Head Hunter lv6-10 → value 25-29
            AddRange(d, "Head Hunter",       6, 10, 25);
            // Corporate Raider lv7-11 → value 30-34 (tabela: lv7=30..lv11=34... mas tabela mostra lv7=30)
            // Nota: tabela original mostra Corporate Raider lv7=30 e The Big Cheese lv8=35
            AddRange(d, "Corporate Raider",  7, 11, 30);
            // The Big Cheese lv8-50 → value 35-77
            AddRange(d, "The Big Cheese",    8, 50, 35);

            return d;
        }

        // ── CJ lookup table ───────────────────────────────────────────────────
        // Bottom Feeder lv1-5 → value 0-4
        // Bloodsucker lv2-6 → 5-9 (lv6=8 na tabela... ajustar)
        // Reconstrução a partir da tabela CJ:
        //   Bottom Feeder 1-5 → 0-4
        //   Bloodsucker 2-6 → 5-9 (lv6=8)  ← tabela mostra lv6=8 não 9
        // Tabela CJ exata (Value coluna):
        //   BF1=0,BF2=1,BF3=2,BF4=3,BF5=4
        //   BS2=5,BS3=6,BS4=7,BS5=8,BS6=9 (wait tabela mostra BS6=8... re-check)
        // Usando a tabela da imagem linha a linha:
        private static Dictionary<(string, int), int> BuildCjValues()
        {
            var d = new Dictionary<(string, int), int>();

            // Bottom Feeder lv1-5 → value 0-4
            AddRange(d, "Bottom Feeder",    1,  5,  0);
            // Bloodsucker lv2-6 → value 5-9
            AddRange(d, "Bloodsucker",      2,  6,  5);
            // Double Talker lv3-7 → value 10-14
            AddRange(d, "Double Talker",    3,  7, 10);
            // Ambulance Chaser lv4-8 → value 15-19
            AddRange(d, "Ambulance Chaser", 4,  8, 15);
            // Back Stabber lv5-9 → value 20-24
            AddRange(d, "Back Stabber",     5,  9, 20);
            // Spin Doctor lv6-10 → value 25-29
            AddRange(d, "Spin Doctor",      6, 10, 25);
            // Legal Eagle lv7-11 → value 30-34
            AddRange(d, "Legal Eagle",      7, 11, 30);
            // Big Wig lv8-50 → value 35-77
            AddRange(d, "Big Wig",          8, 50, 35);

            return d;
        }

        private static void AddRange(Dictionary<(string,int),int> d,
            string name, int minLv, int maxLv, int startValue)
        {
            for (int lv = minLv; lv <= maxLv; lv++)
                d[(name, lv)] = startValue + (lv - minLv);
        }

        // Retorna o value para um ToonSuitEntry (CEO)
        public static int GetCeoValue(ToonSuitEntry e)
        {
            if (e.IsV2)
            {
                // V2.0 do Big Cheese: mesmo peso que lv50 = value 77
                return CeoValues.TryGetValue(("The Big Cheese", 50), out int v) ? v : 77;
            }
            return CeoValues.TryGetValue((e.SuitType, e.SuitLevel), out int val) ? val : 0;
        }

        // Retorna o value para um ToonSuitEntry (CJ)
        public static int GetCjValue(ToonSuitEntry e)
        {
            if (e.IsV2)
            {
                return CjValues.TryGetValue(("Big Wig", 50), out int v) ? v : 77;
            }
            return CjValues.TryGetValue((e.SuitType, e.SuitLevel), out int val) ? val : 0;
        }

        // ── CEO result lookup ─────────────────────────────────────────────────
        // Calcula resultado CEO a partir do value médio (soma/count, arredondado para baixo)
        public static CeoResult LookupCeo(int avgValue)
        {
            // Tabela CEO completa
            var table = GetCeoTable();
            CeoResult best = null;
            foreach (var row in table)
            {
                if (row.Value <= avgValue)
                    best = row;
                else
                    break;
            }
            return best ?? table[0];
        }

        // ── CJ result lookup ──────────────────────────────────────────────────
        public static CjResult LookupCj(int avgValue)
        {
            var table = GetCjTable();
            CjResult best = null;
            foreach (var row in table)
            {
                if (row.Value <= avgValue)
                    best = row;
                else
                    break;
            }
            return best ?? table[0];
        }

        // ── CEO full table ────────────────────────────────────────────────────
        public static List<CeoResult> GetCeoTable()
        {
            // (Value, Tables, CogsPerTable, CogTypes, V2s, BanquetTimer, Reward, Tier)
            var t = new List<CeoResult>();

            // Tier 1: value 0-15, Tables=10, CPT=4, HH, V2=13, 270s, 2PS
            for (int v = 0; v <= 15; v++)
                t.Add(new CeoResult { Value=v, Tables=10, CogsPerTable=4,
                    CogTypes="Head Hunters", V2s=13,
                    BanquetSeconds=270, Reward="2 Pink Slips", Tier=1 });

            // Tier 2: value 16-29, Tables=10, CPT=5, HH & CR, V2=17, 285s, 3PS
            for (int v = 16; v <= 29; v++)
                t.Add(new CeoResult { Value=v, Tables=10, CogsPerTable=5,
                    CogTypes="Head Hunters & Corporate Raiders", V2s=17,
                    BanquetSeconds=285, Reward="3 Pink Slips", Tier=2 });

            // Tier 3: value 30-46, Tables=11, CPT=5, CR, V2=17, 300s, 4PS
            for (int v = 30; v <= 46; v++)
                t.Add(new CeoResult { Value=v, Tables=11, CogsPerTable=5,
                    CogTypes="Corporate Raiders", V2s=17,
                    BanquetSeconds=300, Reward="4 Pink Slips", Tier=3 });

            // Tier 4: value 47-61, Tables=12, CPT=5, CR/TBC, V2=20, 315s, 5PS
            for (int v = 47; v <= 61; v++)
                t.Add(new CeoResult { Value=v, Tables=12, CogsPerTable=5,
                    CogTypes="Corporate Raiders / The Big Cheese", V2s=20,
                    BanquetSeconds=315, Reward="5 Pink Slips", Tier=4 });

            // Tier 5: value 62-77, Tables=13, CPT=5, TBC, V2=22, 330s, 6PS
            for (int v = 62; v <= 77; v++)
                t.Add(new CeoResult { Value=v, Tables=13, CogsPerTable=5,
                    CogTypes="The Big Cheese", V2s=22,
                    BanquetSeconds=330, Reward="6 Pink Slips", Tier=5 });

            return t;
        }

        // ── CJ full table ─────────────────────────────────────────────────────
        public static List<CjResult> GetCjTable()
        {
            var t = new List<CjResult>();

            // Tier 1: value 0-8, Gavels=4, Cogs=8, Evidence=38, ToonUp=1
            for (int v = 0; v <= 8; v++)
                t.Add(new CjResult { Value=v, Gavels=4, Cogs=8,
                    Evidence=38, ToonUp=1, Tier=1 });

            // Tier 2: value 9-17, Gavels=5, Cogs=8, Evidence=36, ToonUp=1
            for (int v = 9; v <= 17; v++)
                t.Add(new CjResult { Value=v, Gavels=5, Cogs=8,
                    Evidence=36, ToonUp=1, Tier=2 });

            // Tier 3: value 18-24, Gavels=6, Cogs=8, Evidence=34, ToonUp=1
            for (int v = 18; v <= 24; v++)
                t.Add(new CjResult { Value=v, Gavels=6, Cogs=8,
                    Evidence=34, ToonUp=1, Tier=3 });

            // Tier 4: value 25-29, Gavels=6, Cogs=8, Evidence=32, ToonUp=1
            for (int v = 25; v <= 29; v++)
                t.Add(new CjResult { Value=v, Gavels=6, Cogs=8,
                    Evidence=32, ToonUp=1, Tier=4 });

            // Tier 5: value 30-38, Gavels=6, Cogs=8, Evidence=30, ToonUp=2
            for (int v = 30; v <= 38; v++)
                t.Add(new CjResult { Value=v, Gavels=6, Cogs=8,
                    Evidence=30, ToonUp=2, Tier=5 });

            // Tier 6: value 39-51, Gavels=7, Cogs=8, Evidence=28, ToonUp=3
            for (int v = 39; v <= 51; v++)
                t.Add(new CjResult { Value=v, Gavels=7, Cogs=8,
                    Evidence=28, ToonUp=3, Tier=6 });

            // Tier 7: value 52-59, Gavels=7, Cogs=9, Evidence=26, ToonUp=3
            for (int v = 52; v <= 59; v++)
                t.Add(new CjResult { Value=v, Gavels=7, Cogs=9,
                    Evidence=26, ToonUp=3, Tier=7 });

            // Tier 8: value 60-68, Gavels=8, Cogs=9, Evidence=24, ToonUp=4
            for (int v = 60; v <= 68; v++)
                t.Add(new CjResult { Value=v, Gavels=8, Cogs=9,
                    Evidence=24, ToonUp=4, Tier=8 });

            // Tier 9: value 69-77, Gavels=8, Cogs=10, Evidence=22, ToonUp=4
            for (int v = 69; v <= 77; v++)
                t.Add(new CjResult { Value=v, Gavels=8, Cogs=10,
                    Evidence=22, ToonUp=4, Tier=9 });

            return t;
        }
    }

    // ── Result models ─────────────────────────────────────────────────────────
    public class CeoResult
    {
        public int    Value          { get; set; }
        public int    Tables         { get; set; }
        public int    CogsPerTable   { get; set; }
        public string CogTypes       { get; set; }
        public int    V2s            { get; set; }
        public int    BanquetSeconds { get; set; }
        public string Reward         { get; set; }
        public int    Tier           { get; set; }
        public string BanquetTime => $"{BanquetSeconds} Seconds";
    }

    public class CjResult
    {
        public int Value    { get; set; }
        public int Gavels   { get; set; }
        public int Cogs     { get; set; }
        public int Evidence { get; set; }
        public int ToonUp   { get; set; }
        public int Tier     { get; set; }
    }
}
