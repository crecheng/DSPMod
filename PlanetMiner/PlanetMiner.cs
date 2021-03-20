using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace PlanetMiner
{
    [BepInPlugin("crecheng.PlanetMiner", "PlanetMiner", PlanetMiner.Version)]
    public class PlanetMiner :BaseUnityPlugin
    {
        public const string Version = "2.0.3";
        public static bool isRun = false;
		const int uesEnergy = 20000*1000;
		const int waterSpeed = 100;
		static long frame = 0;
        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(PlanetMiner), null);
        }

		void Update()
        {
			frame++;
        }

        void Init()
        { 
            isRun = true;
        }

		static uint seed=100000;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(PlanetFactory), "GameTick")]
		static void Miner(PlanetFactory __instance)
		{
			GameHistoryData history = GameMain.history;
			float miningSpeedScale = history.miningSpeedScale;

			if (miningSpeedScale <= 0)
				return;
			int baseSpeed = (int)(120 / (miningSpeedScale));

			if (baseSpeed <= 0)
				baseSpeed = 1;

			if (frame % baseSpeed != 0)
				return;

			if (__instance.factorySystem.minerPool[0].seed == 0)
			{
				var rondom = new System.Random();
				__instance.factorySystem.minerPool[0].seed = (uint)(__instance.planet.id * 100000 + rondom.Next(1, 9999));
			}
			else
			{
				seed = __instance.factorySystem.minerPool[0].seed;
			}

			var veinPool = __instance.veinPool;

			Dictionary<int, List<int>> veins = new Dictionary<int, List<int>>();
			for (int i = 0; i < veinPool.Length; i++)
			{
				var d = veinPool[i];
				if (d.amount > 0 && d.productId > 0)
				{
					AddVeinData(veins, d.productId, i);
				}
			}

			float miningCostRate = history.miningCostRate;
			var transport = __instance.transport;
			GameStatData statistics = GameMain.statistics;
			FactoryProductionStat factoryProductionStat = statistics.production.factoryStatPool[__instance.index];
			int[] productRegister = factoryProductionStat.productRegister;
			var consumeRegister = factoryProductionStat.consumeRegister;
			for (int i = 1; i < transport.stationCursor; i++)
			{
				var sc = transport.stationPool[i];
				if (sc != null && sc.storage != null)
				{
					for (int j = 0; j < sc.storage.Length; j++)
					{
						var da = sc.storage[j];
						if (da.localLogic == ELogisticStorage.Demand && da.max > da.count)
						{
                            if (veins.ContainsKey(da.itemId)||da.itemId == __instance.planet.waterItemId)
                            {
								//当能量不足一半时
								if (sc.energyMax / 2 > sc.energy)
								{
									//获取倒数第二个物品栏
									var lastup = sc.storage[sc.storage.Length - 2];
									//如果物品数量大于0
									if (lastup.count > 0)
									{
										//获取物品的能量值
										long en = LDB.items.Select(lastup.itemId).HeatValue;
										//如果物品的能量大于0
										if (en > 0)
										{
											//获取需要充电的能量
											long needen = sc.energyMax - sc.energy;
											//计算需要的数量
											int needcount = (int)(needen / en);
											//如果需要是数量大于有的数量
											if (needcount > lastup.count)
											{
												//将需求数量改为当前数量
												needcount = sc.storage[sc.storage.Length - 2].count;
											}
											//消耗物品
											sc.storage[sc.storage.Length - 2].count -= needcount;
											//充能
											sc.energy += needcount * en;
										}
									}
								}
							}
							if (veins.ContainsKey(da.itemId))
							{
								if (sc.energy >= uesEnergy)
								{
									var vein = veins[da.itemId].First();
									if (veinPool[vein].type == EVeinType.Oil)
									{
										float count = 0;
										foreach (int index in veins[da.itemId])
										{
											if (veinPool.Length > index && veinPool[index].productId > 0)
											{
												count += veinPool[index].amount / 6000f;

											}
										}
										sc.storage[j].count += (int)count;
										productRegister[da.itemId] += (int)count;
										sc.energy -= uesEnergy;
									}
									else
									{
										int count = 0;
										foreach (int index in veins[da.itemId])
										{
											if (GetMine(veinPool, index, miningCostRate, __instance))
												count++;
										}
										sc.storage[j].count += count;
										productRegister[da.itemId]+=count;
										sc.energy -= uesEnergy;
									}
								}

							}
							else if (da.itemId == __instance.planet.waterItemId)
							{
								sc.storage[j].count += waterSpeed;
								productRegister[da.itemId] += waterSpeed;
								sc.energy -= uesEnergy;
							}
						}
					}
				}
			}
		}

		static void AddVeinData(Dictionary<int, List<int>> veins,int item,int index)
        {
			if (!veins.ContainsKey(item))
				veins.Add(item, new List<int>());
			veins[item].Add(index);
		}

		public static bool GetMine(VeinData[] veinDatas,int index, float miningRate,PlanetFactory factory)
        {
			if (veinDatas.Length > index && veinDatas[index].productId > 0 )
			{
				if (veinDatas[index].amount > 0)
				{
					bool flag = true;
					if (miningRate < 0.99999f)
					{
						seed = (uint)((ulong)(seed % 2147483646U + 1U) * 48271UL % 2147483647UL) - 1U;
						flag = (seed / 2147483646.0 < (double)miningRate);
					}
					if (flag)
					{
						veinDatas[index].amount -=  1;
						factory.planet.veinAmounts[(int)veinDatas[index].type] -= 1L;
						if (veinDatas[index].amount <= 0)
						{
							PlanetData.VeinGroup[] veinGroups2 = factory.planet.veinGroups;
							short groupIndex2 = veinDatas[index].groupIndex;
							veinGroups2[(int)groupIndex2].count = veinGroups2[(int)groupIndex2].count - 1;
							factory.RemoveVeinWithComponents(index);
						}
					}
					return true;
				}
                else
                {
					PlanetData.VeinGroup[] veinGroups2 = factory.planet.veinGroups;
					short groupIndex2 = veinDatas[index].groupIndex;
					veinGroups2[(int)groupIndex2].count = veinGroups2[(int)groupIndex2].count - 1;
					factory.RemoveVeinWithComponents(index);
					return false;
                }

			}
			return false;
		}



		//public uint MinerUpdate(PlanetFactory factory, VeinData[] veinPool, float power, float miningRate, float miningSpeed, int[] productRegister)
		//{
		//	if (power < 0.1f)
		//	{
		//		return 0U;
		//	}
		//	uint result = 0U;
		//	if (this.type == EMinerType.Vein)
		//	{
		//		if (this.veinCount > 0)
		//		{
		//			if (this.time <= this.period)
		//			{
		//				this.time += (int)(power * (float)this.speed * miningSpeed * (float)this.veinCount);
		//				result = 1U;
		//			}
		//			if (this.time >= this.period)
		//			{
		//				int num = this.veins[this.currentVeinIndex];
		//				Assert.Positive(num);
		//				if (veinPool[num].id == 0)
		//				{
		//					this.RemoveVeinFromArray(this.currentVeinIndex);
		//					this.GetMinimumVeinAmount(factory, veinPool);
		//					if (this.veinCount > 1)
		//					{
		//						this.currentVeinIndex %= this.veinCount;
		//					}
		//					else
		//					{
		//						this.currentVeinIndex = 0;
		//					}
		//					this.time += (int)(power * (float)this.speed * miningSpeed * (float)this.veinCount);
		//					return 0U;
		//				}
		//				if (this.productCount < 50 && (this.productId == 0 || this.productId == veinPool[num].productId))
		//				{
		//					this.productId = veinPool[num].productId;
		//					this.time -= this.period;
		//					if (veinPool[num].amount > 0)
		//					{
		//						this.productCount++;
		//						productRegister[this.productId]++;
		//						bool flag = true;
		//						if (miningRate < 0.99999f)
		//						{
		//							this.seed = (uint)((ulong)(this.seed % 2147483646U + 1U) * 48271UL % 2147483647UL) - 1U;
		//							flag = (this.seed / 2147483646.0 < (double)miningRate);
		//						}
		//						if (flag)
		//						{
		//							int num2 = num;
		//							veinPool[num2].amount = veinPool[num2].amount - 1;
		//							if (veinPool[num].amount < this.minimumVeinAmount)
		//							{
		//								this.minimumVeinAmount = veinPool[num].amount;
		//							}
		//							factory.planet.veinAmounts[(int)veinPool[num].type] -= 1L;
		//							PlanetData.VeinGroup[] veinGroups = factory.planet.veinGroups;
		//							short groupIndex = veinPool[num].groupIndex;
		//							veinGroups[(int)groupIndex].amount = veinGroups[(int)groupIndex].amount - 1L;
		//							factory.veinAnimPool[num].time = ((veinPool[num].amount < 20000) ? (1f - (float)veinPool[num].amount * 5E-05f) : 0f);
		//							if (veinPool[num].amount <= 0)
		//							{
		//								PlanetData.VeinGroup[] veinGroups2 = factory.planet.veinGroups;
		//								short groupIndex2 = veinPool[num].groupIndex;
		//								veinGroups2[(int)groupIndex2].count = veinGroups2[(int)groupIndex2].count - 1;
		//								factory.RemoveVeinWithComponents(num);
		//								this.RemoveVeinFromArray(this.currentVeinIndex);
		//								this.GetMinimumVeinAmount(factory, veinPool);
		//							}
		//							else
		//							{
		//								this.currentVeinIndex++;
		//							}
		//						}
		//					}
		//					else
		//					{
		//						this.RemoveVeinFromArray(this.currentVeinIndex);
		//						this.GetMinimumVeinAmount(factory, veinPool);
		//					}
		//					if (this.veinCount > 1)
		//					{
		//						this.currentVeinIndex %= this.veinCount;
		//					}
		//					else
		//					{
		//						this.currentVeinIndex = 0;
		//					}
		//				}
		//			}
		//		}
		//	}
		//	else if (this.type == EMinerType.Oil)
		//	{
		//		if (this.veinCount > 0)
		//		{
		//			int num3 = this.veins[0];
		//			float num4 = (float)veinPool[num3].amount * VeinData.oilSpeedMultiplier;
		//			if (this.time < this.period)
		//			{
		//				this.time += (int)(power * (float)this.speed * miningSpeed * num4 + 0.5f);
		//				result = 1U;
		//			}
		//			if (this.time >= this.period && this.productCount < 50)
		//			{
		//				this.productId = veinPool[num3].productId;
		//				this.productCount++;
		//				productRegister[this.productId]++;
		//				this.time -= this.period;
		//			}
		//		}
		//	}
		//	else if (this.type == EMinerType.Water)
		//	{
		//		if (this.time < this.period)
		//		{
		//			this.time += (int)(power * (float)this.speed * miningSpeed);
		//			result = 1U;
		//		}
		//		if (this.time >= this.period && this.productCount < 50)
		//		{
		//			this.productId = factory.planet.waterItemId;
		//			if (this.productId > 0)
		//			{
		//				this.productCount++;
		//				productRegister[this.productId]++;
		//			}
		//			else
		//			{
		//				this.productId = 0;
		//			}
		//			this.time -= this.period;
		//		}
		//	}
		//	return result;
		//}


	}
}
