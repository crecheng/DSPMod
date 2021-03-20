using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChangeSun
{
    public static class ST
    {
        public static string Tip
        {
            get
            {
                if (Localization.language == Language.zhCN)
                {
                    return "输入需要更换太阳的名字(可以部分)";
                }
                else
                {
                    return "Enter change sun name(can part)";
                }
            }
        }

        public static string 查找
        {
            get
            {
                if (Localization.language == Language.zhCN)
                {
                    return "查找";
                }
                else
                {
                    return "Find";
                }
            }
        }
        public static string 更换
        {
            get
            {
                if (Localization.language == Language.zhCN)
                {
                    return "更换";
                }
                else
                {
                    return "Change";
                }
            }
        }
    }
}
