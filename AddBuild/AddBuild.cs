using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("crecheng.AddBuild", "AddBuild", "1.2.0.0")]
public class AddBuild : BaseUnityPlugin
{
    public void Start()
    {
        Harmony.CreateAndPatchAll(typeof(AddBuild), null);
        AddBuild.style.fontSize = 15;
        AddBuild.style.normal.textColor = new Color(255f, 255f, 255f);
    }

    public void OnGUI()
    {
        //GUI.Label(rect, BugInfo, style);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameData), "GameTick")]
    public static void addBuild(GameData __instance)
    {
        BugInfo = "isLoading";
        if (canUp)
        {
            UIGeneralTips.buildCursorText = $"<{ST.建造叠加}>" + showInfo;
            UIGeneralTips.buildCursorTextColor = 1;
        }
        canUp = false;
        Player player = __instance.mainPlayer;
        if (player.factory != null)
        {
            BugInfo += player.factory.index + "\n";
            if (__instance.mainPlayer.controller != null && __instance.mainPlayer.controller.cmd.raycast != null)
            {
                showInfo = "";
                //Info += "," + preDesc.isEjector;//太阳帆发射器
                int soilId = player.controller.cmd.raycast.castEntity.siloId;
                //Info += "\n" + preDesc.isSilo;//火箭发射器
                int assemblerId = player.controller.cmd.raycast.castEntity.assemblerId;

                int excId = player.controller.cmd.raycast.castEntity.powerExcId;
                int AccId = player.controller.cmd.raycast.castEntity.powerAccId;
                int ConId = player.controller.cmd.raycast.castEntity.powerConId;
                int GenId = player.controller.cmd.raycast.castEntity.powerGenId;
                int minerId = player.controller.cmd.raycast.castEntity.minerId;
                int insterId = player.controller.cmd.raycast.castEntity.inserterId;
                int storageId = player.controller.cmd.raycast.castEntity.storageId;
                int labId = player.controller.cmd.raycast.castEntity.labId;
                BugInfo += "\nassemblerId:" + assemblerId;
                //Info += "\nexcId:" + excId;
                //Info += "\nAccId:" + AccId;
                //Info += "\nConId:" + ConId;
                //Info += "\nGenId:" + GenId;
                //Info += "\nsoilId:" + soilId;
                BugInfo += "\nstorageId:" + storageId;
                BugInfo += "\nLabId:" + labId;
                BugInfo += "\ninhandItemId:" + player.inhandItemId;
                BugInfo += "\nItemCount:" + getItemCount(player, player.inhandItemId);
                if (assemblerId > 0)
                {
                    assemblerAdd(player, assemblerId);
                }
                if (minerId > 0)
                {
                    MinerAdd(player, minerId);
                }
                if (soilId > 0)
                {
                    //soilAdd(player, soilId);
                }
                if (GenId > 0)
                {
                    PowerGenAdd(player, GenId);
                }
                if (labId > 0)
                {
                    LabAdd(player, labId);
                }
                BugInfo += "\nCanUp:" + canUp;
            }
        }

    }

    //[HarmonyPrefix]
    //[HarmonyPatch(typeof(StorageComponent), "TakeTailItems", new Type[] { typeof(int), typeof(int), typeof(bool) }
    //                                      ,new ArgumentType[] { ArgumentType.Ref,ArgumentType.Ref,ArgumentType.Normal})]
    //static void other()
    //{
    //
    //}


    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameData), "GameTick")]
    public static void showText(GameData __instance) {
        if (canUp)
        {
            UIGeneralTips.buildCursorText = $"<{ST.建造叠加}>"+showInfo;
            UIGeneralTips.buildCursorTextColor = 1;
        }
    }

    //[HarmonyPostfix]
    //[HarmonyPatch(typeof(PlayerAction_Build), "GameTick")]
    //public static void modifyModle(PlayerAction_Build __instance)
    //{
    //    if (__instance.previewGizmoDesc != null&&canUp)
    //    {
    //        __instance.previewGizmoDesc.condition = EBuildCondition.Ok;
    //    }
    //}


    public static void PowerGenAdd(Player player, int genId)
    {
        var pf = player.factory;
        var fs = pf.powerSystem;
        var target = fs.genPool[genId];
        var entity = pf.entityPool[target.entityId];
        var entityitemId = pf.entityPool[target.entityId].protoId;
        if (CheckCanUp(player, entityitemId))
        {
            canUp = true;
            BugInfo += "\ngenEnergyPerTick:" + fs.genPool[genId].genEnergyPerTick;
            checkKey();
            if (key())
            {
                UIRealtimeTip.Popup(ST.建造叠加 + "+", false);
                var item = LDB.items.Select(player.inhandItemId);
                BugInfo += "\n" + player.inhandItemId;
                PrefabDesc desc = item.prefabDesc;
                fs.genPool[genId].genEnergyPerTick += (long)(desc.genEnergyPerTick*0.9);
                fs.genPool[genId].useFuelPerTick += desc.useFuelPerTick;
                useItem(player, player.inhandItemId);
            }
        }
    }
    public static void soilAdd(Player player, int soilId)
    {
        var pf = player.factory;
        var fs = pf.factorySystem;
        var target = fs.siloPool[soilId];
        var entity = pf.entityPool[target.entityId];
        var entityitemId = pf.entityPool[target.entityId].protoId;
        var pcid = target.pcId;
        BugInfo += "\n" + keyCount;
        BugInfo += "\nisSoil";
        checkKey();
        if (key())
        {
            fs.siloPool[soilId].chargeSpend /= 2;
            fs.siloPool[soilId].coldSpend /= 2;
            keyCount = 0;
        }
    }

    public static void MinerAdd(Player player, int minerId)
    {
        var pf = player.factory;
        var fs = pf.factorySystem;
        var target = fs.minerPool[minerId];
        var entity = pf.entityPool[target.entityId];
        var entityitemId = pf.entityPool[target.entityId].protoId;
        var pcid = target.pcId;

        BugInfo += "\nkeyCount;" + keyCount;
        BugInfo += "\nentityitemId;" + entityitemId;
        BugInfo += "\n";

        if (CheckCanUp(player, entityitemId))
        {
            canUp = true;
            BugInfo += "\nspeed:" + fs.minerPool[minerId].speed;
            string speedText = (fs.minerPool[minerId].speed / 10000.0).ToString();
            showInfo = $"\n\n< {ST.速度}" + speedText + " >";
            checkKey();
            if (key())
            {
                UIRealtimeTip.Popup(ST.建造叠加 + "+", false);
                var item = LDB.items.Select(player.inhandItemId);
                BugInfo += "\n" + player.inhandItemId;
                PrefabDesc desc = item.prefabDesc;
                fs.minerPool[minerId].speed += 10000;
                pf.powerSystem.consumerPool[pcid].workEnergyPerTick += (long)(desc.workEnergyPerTick * powerUp);
                useItem(player, player.inhandItemId);
            }
        }
    }

    public static void assemblerAdd(Player player, int assemblerId)
    {
        var pf = player.factory;
        var fs = pf.factorySystem;
        var target = fs.assemblerPool[assemblerId];
        var entity = pf.entityPool[target.entityId];
        var entityitemId = pf.entityPool[target.entityId].protoId;
        var pcid = target.pcId;

        BugInfo += "\nkeyCount;" + keyCount;
        BugInfo += "\nentityitemId;" + entityitemId;
        BugInfo += "\n" ;

        if (CheckCanUp(player, entityitemId))
        {
            canUp = true;
            BugInfo += "\nspeed:"+fs.assemblerPool[assemblerId].speed;
            string speedText = (fs.assemblerPool[assemblerId].speed / 10000.0).ToString();
            showInfo = $"\n\n< {ST.速度}" + speedText+" >";
            checkKey();
            if (key())
            {
                UIRealtimeTip.Popup(ST.建造叠加 + "+", false);
                var item = LDB.items.Select(player.inhandItemId);
                BugInfo += "\n" + player.inhandItemId;
                PrefabDesc desc = item.prefabDesc;
                fs.assemblerPool[assemblerId].speed += desc.assemblerSpeed;
                pf.powerSystem.consumerPool[pcid].workEnergyPerTick += (long)(desc.workEnergyPerTick * powerUp);
                useItem(player,player.inhandItemId);
            }
        }
    }

    public static void LabAdd(Player player, int labId)
    {
        var pf = player.factory;
        var fs = pf.factorySystem;
        var target = fs.labPool[labId];
        var entity = pf.entityPool[target.entityId];
        var entityitemId = pf.entityPool[target.entityId].protoId;
        var pcid = target.pcId;

        BugInfo += "\nkeyCount;" + keyCount;
        BugInfo += "\nentityitemId;" + entityitemId;
        BugInfo += "\n";

        if (CheckCanUp(player, entityitemId,true)&&!target.researchMode&& target.recipeId>0)
        {
            canUp = true;
            BugInfo += "\nspeed:" + target.timeSpend;
            BugInfo += "\ninhandItemId:" + player.inhandItemId;
            BugInfo += "\nworkEnergyPerTic:" + pf.powerSystem.consumerPool[pcid].workEnergyPerTick;
            int recipeId = target.recipeId;
            RecipeProto recipeProto = LDB.recipes.Select(recipeId);
            int oldSpeed = recipeProto.TimeSpend*10000;
            string speedText = ((int)(oldSpeed / (float)target.timeSpend)).ToString();
            showInfo = $"\n\n< {ST.速度}" + speedText + " >";
            checkKey();
            if (key())
            {
                UIRealtimeTip.Popup(ST.建造叠加+"+", false);
                BugInfo += "\n" + player.inhandItemId;
                int nowSpeed = (int)(oldSpeed / (float)target.timeSpend);
                fs.labPool[labId].timeSpend = oldSpeed / (nowSpeed + 1);
                pf.powerSystem.consumerPool[pcid].workEnergyPerTick += (long)(8000 * powerUp);
                useItem(player, 2901);
            }
        }
    }
    public static bool CheckCanUp(Player player,int entityitemId,bool isLab=false)
    {
        if (isLab)
        {
            return player.inhandItemId == 2302 && getItemCount(player, 2901) > 0;
        }
        else
        {
            return player.inhandItemId > 0 && entityitemId == player.inhandItemId && getItemCount(player, player.inhandItemId) > 0;
        }
    }

    public static void checkKey()
    {
        if (Input.GetKey(KeyCode.Mouse0))
            keyCount++;
        else
            keyCount = 0;
    }

    public static bool key()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            return false;
        return keyCount ==1||(keyCount>50&&keyCount%10==0)||(keyCount>100&&keyCount%5 == 0);
    }

    public static void useItem( Player player, int itemId)
    {
        if (player.inhandItemId == itemId && player.inhandItemCount > 0)
        {
            player.UseHandItems(1);
            return;
        }
        else
        {
            if (player.package.GetItemCount(itemId) > 0)
                player.package.TakeItem(itemId, 1);
        }
    }

    public static int getItemCount(Player player, int itemId)
    {
        int num = 0;
        if (player.inhandItemId == itemId)
        {
            num += player.inhandItemCount;
        }
        num += player.package.GetItemCount(itemId);
        return num;
    }


    private static readonly float powerUp = 1.5f;

    private static bool canUp = false;

    private static string BugInfo = "open";

    private static string showInfo = "";

    private static int keyCount = 0;

    private static Rect rect = new Rect(330f, 30f, 100f, 200f);

    private static GUIStyle style = new GUIStyle();

    private static Rect windowRect = new Rect(100, 100, 50, 50);
}

