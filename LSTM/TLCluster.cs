using System;
//using System.Text;
using System.Collections.Generic;

namespace LSTMMod
{
    public class TLCluster
    {
        public static bool IsSameRemoteCluster(StationComponent s1, StationComponent s2)
        {
            return IsSameCluster(s1, s2, "C");
        }
        public static bool IsSameLocalCluster(StationComponent s1, StationComponent s2)
        {
            return IsSameCluster(s1, s2, "c");
        }

        // all はすべての cluster と輸送可能 // [C:all,cluster] の扱い→そういう設定にすることなさそう
        public static bool IsSameCluster(StationComponent s1, StationComponent s2, string cmd)
        {
            string c1 = Util.GetCommandValue(s1.name, cmd);
            string c2 = Util.GetCommandValue(s2.name, cmd);
            if (string.IsNullOrEmpty(c1) && string.IsNullOrEmpty(c2))
            {
                return true;
            }

            char[] sep ={','};
            string[] c1s = c1.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] c2s = c2.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            c1s = Array.ConvertAll(c1s, s => s.Trim().ToLower());
            c2s = Array.ConvertAll(c2s, s => s.Trim().ToLower());

            if (c1s.Length == 1 && c2s.Length == 1)
            {
                return c1s[0] == c2s[0] || c1s[0] == "all" || c2s[0] == "all";
            }
            else if(c1s.Length == 0 && c2s.Length == 0)
            {
                return true;
            }
            else if (c1s.Length == 1 && c2s.Length == 0)
            {
                return c1s[0] == "all" || string.IsNullOrEmpty(c1s[0]);
            }
            else if (c2s.Length == 0 && c2s.Length == 1)
            {
                return c2s[0] == "all" || string.IsNullOrEmpty(c2s[0]);
            }

            string c2join = "," + string.Join(",", c2s) + ",";
            if (c2join.Contains(",all,"))
            {
                return true;
            }

            foreach (var item in c1s)
            {
                string c1sv = item;
                if (string.IsNullOrEmpty(c1sv))
                {
                    continue;
                }
                if (c1sv == "all" || c2join.Contains("," + c1sv + ","))
                {
                    return true;
                }
                // [C: ,  ,] みたいな場合falseになる？→書き方が悪いことにしておく
            }
            return false;
        }


    }
}
