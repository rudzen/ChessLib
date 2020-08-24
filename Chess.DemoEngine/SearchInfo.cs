using System.Collections.Generic;

namespace Chess.DemoEngine
{
    public class SearchInfo
    {
        public ulong starttime { get; set; }

        public ulong stoptime { get; set; }

        public ulong opp_time { get; set; }

        public ulong nodes { get; set; }

        public int depth { get; set; }

        public int movestogo { get; set; }

        public int W_INFINITE { get; set; }

        public int null_cut { get; set; }

        public int game_mode { get; set; }

        public bool searching { get; set; }

        public bool pondering { get; set; }

        public bool post_thinking { get; set; }

        public bool depthset { get; set; }

        public bool timeset { get; set; }
        public bool quit { get; set; }
        public bool stopped { get; set; }
        public bool ph { get; set; }

        public float fail_high { get; set; }
        public float fail_high_first { get; set; }

        public List<int> ph2 { get; set; } = new List<int>(14);
    }
}