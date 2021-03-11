public class ST
{
    public static string 货物
    {
        get
        {
            if (Localization.language == Language.zhCN)
            {
                return "货物";
            }
            else
            {
                return "Cargos";
            }
        }
    }

    public static string 传送带
    {
        get
        {
            if (Localization.language == Language.zhCN)
            {
                return "传送带";
            }
            else
            {
                return "belt";
            }
        }
    }
    public static string 小飞机
    {
        get
        {
            if (Localization.language == Language.zhCN)
            {
                return "小飞机";
            }
            else
            {
                return "Drones";
            }
        }
    }

    public static string 全部实体
    {
        get
        {
            if (Localization.language == Language.zhCN)
            {
                return "全部实体";
            }
            else
            {
                return "All Entities";
            }
        }
    }


}
