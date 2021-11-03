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

        // any はすべての cluster と輸送可能
        public static bool IsSameCluster(StationComponent s1, StationComponent s2, string cmd)
        {
            //GetCommandValue は null チェック不要
            string c1 = Util.GetCommandValue(s1.name, cmd).ToLower();
            string c2 = Util.GetCommandValue(s2.name, cmd).ToLower();

            if (c1 == "any" || c2 == "any")
            {
                return true;
            }

            return c1 == c2;

            /*
            //,区切りで複数のクラスター
            //いろいろめんどいので不採用
            char[] sep ={','};
            string[] c1s = c1.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] c2s = c2.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            c1s = Array.ConvertAll(c1s, s => s.Trim().ToLower());
            c2s = Array.ConvertAll(c2s, s => s.Trim().ToLower());

            if (c1s.Length == 1 && c2s.Length == 1)
            {
                return c1s[0] == c2s[0] || c1s[0] == "any" || c2s[0] == "any";
            }
            else if(c1s.Length == 0 && c2s.Length == 0)
            {
                return true;
            }
            else if (c1s.Length == 1 && c2s.Length == 0)
            {
                return c1s[0] == "any" || string.IsNullOrEmpty(c1s[0]);
            }
            else if (c2s.Length == 0 && c2s.Length == 1)
            {
                return c2s[0] == "any" || string.IsNullOrEmpty(c2s[0]);
            }

            string c2join = "," + string.Join(",", c2s) + ",";
            if (c2join.Contains(",any,"))
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
                if (c1sv == "any" || c2join.Contains("," + c1sv + ","))
                {
                    return true;
                }
                // [C: ,  ,] みたいな場合falseになる？→書き方が悪いことにしておく
            }
            return false;
            */
        }


    }
}
