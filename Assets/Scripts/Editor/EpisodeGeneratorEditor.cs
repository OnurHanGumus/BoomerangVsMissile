using System.Collections.Generic;
using Data.UnityObject;
using Data.ValueObject;
using Enums;
using UnityEditor;
using UnityEngine;

namespace BoomerangVsMissile.EditorTools
{
    public static class EpisodeGeneratorEditor
    {
        private static readonly Dictionary<MissileEnums, int> Unlocks = new Dictionary<MissileEnums, int>
        {
            { MissileEnums.Missile_1_Default, 1 },
            { MissileEnums.Missile_2_Small, 4 },
            { MissileEnums.Missile_3_Exploder, 8 },
            { MissileEnums.Missile_4_Armored, 15 },
            { MissileEnums.Missile_5_Cluster, 23 },
            { MissileEnums.Missile_12_Default_v2, 35 },
            { MissileEnums.Missile_11_Zigzag, 45 },
            { MissileEnums.Missile_7_Armored_v2, 60 },
            { MissileEnums.Missile_15_Aegis, 80 },
            { MissileEnums.Missile_8_Cluster_v2, 110 },
            { MissileEnums.Missile_13_Default_v3, 160 },
            { MissileEnums.Missile_9_Armored_v3, 220 },
            { MissileEnums.Missile_16_Aegis_v2, 300 },
            { MissileEnums.Missile_14_Default_v4, 400 },
            { MissileEnums.Missile_10_Armored_v4, 500 },
            { MissileEnums.Missile_17_Reward, 1 }
        };

        [MenuItem("Tools/Missiles/Generate 1000 Episodes")]
        public static void Generate1000Episodes()
        {
            var asset = Resources.Load<CD_MissileCreator>("Data/CD_MissileCreator");
            if (asset == null)
            {
                Debug.LogError("CD_MissileCreator not found at Resources/Data/CD_MissileCreator");
                return;
            }

            var tutorial = asset.Data.MissileData != null && asset.Data.MissileData.Count > 0
                ? asset.Data.MissileData[0]
                : new MissileSpawnData
                {
                    MissileCount = 10,
                    MissileCreateOffset = 2f,
                    MissileTypeList = new List<MissileEnums> { MissileEnums.Missile_1_Default },
                    PercentageList = new List<float> { 1f }
                };

            var list = new List<MissileSpawnData>(1001) { tutorial };

            for (int lvl = 1; lvl <= 1000; lvl++)
            {
                list.Add(GenerateEpisode(lvl));
            }

            asset.Data.MissileData = list;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully generated {list.Count} episodes in CD_MissileCreator (Index 0 Tutorial + 1000 Episodes)!");
        }

        public static MissileSpawnData GenerateEpisode(int levelId)
        {
            int cycle = (levelId - 1) % 10;
            int cycleNum = (levelId - 1) / 10;

            // 1. Base Missile Count
            float baseCount;
            if (levelId <= 50)
                baseCount = 8f + (20f - 8f) * (levelId - 1) / 49f;
            else if (levelId <= 200)
                baseCount = 20f + (32f - 20f) * (levelId - 50) / 150f;
            else if (levelId <= 500)
                baseCount = 32f + (42f - 32f) * (levelId - 200) / 300f;
            else
                baseCount = 42f + (52f - 42f) * (levelId - 500) / 500f;

            float countAdj;
            if (cycle == 0)
                countAdj = levelId > 10 ? -3f : -1f;
            else if (cycle == 9)
                countAdj = 4f;
            else if (cycle == 4 || cycle == 5)
                countAdj = 2f;
            else
                countAdj = (cycle - 4) * 0.5f;

            int missileCount = Mathf.Max(8, Mathf.RoundToInt(baseCount + countAdj));

            // 2. Base Spawn Offset
            float baseOffset;
            if (levelId <= 50)
                baseOffset = 2.00f - (2.00f - 1.45f) * (levelId - 1) / 49f;
            else if (levelId <= 200)
                baseOffset = 1.45f - (1.45f - 1.15f) * (levelId - 50) / 150f;
            else if (levelId <= 500)
                baseOffset = 1.15f - (1.15f - 0.88f) * (levelId - 200) / 300f;
            else
                baseOffset = 0.88f - (0.88f - 0.68f) * (levelId - 500) / 500f;

            float offsetAdj = 0f;
            if (cycle == 0) offsetAdj = 0.08f;
            else if (cycle == 9) offsetAdj = -0.04f;
            else if (cycle == 4) offsetAdj = -0.05f;

            float missileOffset = (float)System.Math.Round(Mathf.Clamp(baseOffset + offsetAdj, 0.65f, 2.10f), 2);

            // 3. Reward Missile Weight
            float rewardWeight;
            if (levelId <= 30)
                rewardWeight = 0.05f;
            else if (levelId <= 100)
                rewardWeight = 0.05f + (0.15f - 0.05f) * (levelId - 30) / 70f;
            else if (levelId <= 300)
                rewardWeight = 0.15f + (0.45f - 0.15f) * (levelId - 100) / 200f;
            else if (levelId <= 600)
                rewardWeight = 0.45f + (0.90f - 0.45f) * (levelId - 300) / 300f;
            else
                rewardWeight = 0.90f + (1.50f - 0.90f) * (levelId - 600) / 400f;

            rewardWeight = (float)System.Math.Round(rewardWeight, 3);

            // 4. Unlocked set
            var unlocked = new HashSet<MissileEnums>();
            foreach (var kvp in Unlocks)
            {
                if (levelId >= kvp.Value)
                    unlocked.Add(kvp.Key);
            }

            // 5. Archetype Theme
            string theme = "standard";
            if (cycle == 0) theme = "breather";
            else if (cycle == 9) theme = "climax";
            else if (cycle == 4 || cycle == 5)
            {
                int flavor = (cycleNum + cycle) % 4;
                if (flavor == 0 && unlocked.Contains(MissileEnums.Missile_11_Zigzag)) theme = "swarm";
                else if (flavor == 1 && unlocked.Contains(MissileEnums.Missile_4_Armored)) theme = "siege";
                else if (flavor == 2 && unlocked.Contains(MissileEnums.Missile_15_Aegis)) theme = "aegis";
                else if (flavor == 3 && unlocked.Contains(MissileEnums.Missile_5_Cluster)) theme = "cluster";
            }

            // 6. Category Target Budgets
            float targetPool = 100f - rewardWeight;
            bool hasArmored = unlocked.Contains(MissileEnums.Missile_4_Armored) || unlocked.Contains(MissileEnums.Missile_7_Armored_v2) || unlocked.Contains(MissileEnums.Missile_9_Armored_v3) || unlocked.Contains(MissileEnums.Missile_10_Armored_v4);
            bool hasCluster = unlocked.Contains(MissileEnums.Missile_5_Cluster) || unlocked.Contains(MissileEnums.Missile_8_Cluster_v2);
            bool hasMisc = unlocked.Contains(MissileEnums.Missile_3_Exploder) || unlocked.Contains(MissileEnums.Missile_11_Zigzag) || unlocked.Contains(MissileEnums.Missile_15_Aegis) || unlocked.Contains(MissileEnums.Missile_16_Aegis_v2);

            float wDefault, wArmored = 0f, wCluster = 0f, wMisc = 0f;
            if (theme == "breather")
            {
                wDefault = 0.45f;
                wMisc = unlocked.Contains(MissileEnums.Missile_3_Exploder) ? 0.30f : 0.15f;
                if (hasArmored) wArmored = 0.15f;
                if (hasCluster) wCluster = 0.10f;
            }
            else if (theme == "climax")
            {
                if (hasArmored) wArmored = 0.30f;
                if (hasCluster) wCluster = 0.25f;
                wMisc = hasMisc ? 0.25f : 0.15f;
                wDefault = 0.20f;
            }
            else if (theme == "swarm")
            {
                wDefault = 0.50f;
                wMisc = 0.30f;
                if (hasArmored) wArmored = 0.10f;
                if (hasCluster) wCluster = 0.10f;
            }
            else if (theme == "siege")
            {
                if (hasArmored) wArmored = 0.45f;
                wMisc = unlocked.Contains(MissileEnums.Missile_3_Exploder) ? 0.25f : 0.10f;
                if (hasCluster) wCluster = 0.10f;
                wDefault = 0.20f;
            }
            else if (theme == "aegis")
            {
                wMisc = 0.30f;
                if (hasArmored) wArmored = 0.25f;
                if (hasCluster) wCluster = 0.25f;
                wDefault = 0.20f;
            }
            else if (theme == "cluster")
            {
                if (hasCluster) wCluster = 0.45f;
                wMisc = hasMisc ? 0.20f : 0.10f;
                if (hasArmored) wArmored = 0.15f;
                wDefault = 0.20f;
            }
            else
            {
                if (hasArmored) wArmored = 0.25f;
                if (hasCluster) wCluster = 0.20f;
                wMisc = hasMisc ? 0.20f : 0.10f;
                wDefault = 0.35f;
            }

            float totalFrac = wDefault + wArmored + wCluster + wMisc;
            wDefault = (wDefault / totalFrac) * targetPool;
            wArmored = (wArmored / totalFrac) * targetPool;
            wCluster = (wCluster / totalFrac) * targetPool;
            wMisc = (wMisc / totalFrac) * targetPool;

            var typeWeights = new Dictionary<MissileEnums, float>();

            // Sub-distribute Default family
            var defCandidates = new List<MissileEnums>();
            foreach (var m in new[] { MissileEnums.Missile_1_Default, MissileEnums.Missile_2_Small, MissileEnums.Missile_12_Default_v2, MissileEnums.Missile_13_Default_v3, MissileEnums.Missile_14_Default_v4 })
            {
                if (unlocked.Contains(m)) defCandidates.Add(m);
            }
            if (defCandidates.Count > 0)
            {
                var subW = new Dictionary<MissileEnums, float>();
                if (theme == "swarm" && defCandidates.Contains(MissileEnums.Missile_2_Small))
                {
                    subW[MissileEnums.Missile_2_Small] = 0.60f;
                    float rem = defCandidates.Count > 1 ? 0.40f / (defCandidates.Count - 1) : 0f;
                    foreach (var m in defCandidates)
                    {
                        if (m != MissileEnums.Missile_2_Small) subW[m] = rem;
                    }
                }
                else
                {
                    foreach (var m in defCandidates)
                    {
                        if (m == MissileEnums.Missile_1_Default) subW[m] = Mathf.Max(0.08f, 1f - (levelId / 150f));
                        else if (m == MissileEnums.Missile_2_Small) subW[m] = 0.25f;
                        else if (m == MissileEnums.Missile_12_Default_v2) subW[m] = levelId < 250 ? 0.40f : 0.20f;
                        else if (m == MissileEnums.Missile_13_Default_v3) subW[m] = levelId < 500 ? 0.45f : 0.25f;
                        else if (m == MissileEnums.Missile_14_Default_v4) subW[m] = 0.50f;
                    }
                    float tot = 0f;
                    foreach (var v in subW.Values) tot += v;
                    var keys = new List<MissileEnums>(subW.Keys);
                    foreach (var k in keys) subW[k] /= tot;
                }

                foreach (var m in defCandidates)
                {
                    typeWeights[m] = (float)System.Math.Round(wDefault * subW[m], 2);
                }
            }

            // Sub-distribute Armored family
            var armCandidates = new List<MissileEnums>();
            foreach (var m in new[] { MissileEnums.Missile_4_Armored, MissileEnums.Missile_7_Armored_v2, MissileEnums.Missile_9_Armored_v3, MissileEnums.Missile_10_Armored_v4 })
            {
                if (unlocked.Contains(m)) armCandidates.Add(m);
            }
            if (armCandidates.Count > 0)
            {
                var subW = new Dictionary<MissileEnums, float>();
                foreach (var m in armCandidates)
                {
                    if (m == MissileEnums.Missile_4_Armored) subW[m] = Mathf.Max(0.10f, 1f - (levelId / 200f));
                    else if (m == MissileEnums.Missile_7_Armored_v2) subW[m] = levelId < 300 ? 0.45f : 0.20f;
                    else if (m == MissileEnums.Missile_9_Armored_v3) subW[m] = levelId < 600 ? 0.50f : 0.30f;
                    else if (m == MissileEnums.Missile_10_Armored_v4) subW[m] = 0.55f;
                }
                float tot = 0f;
                foreach (var v in subW.Values) tot += v;
                var keys = new List<MissileEnums>(subW.Keys);
                foreach (var k in keys) subW[k] /= tot;

                foreach (var m in armCandidates)
                {
                    typeWeights[m] = (float)System.Math.Round(wArmored * subW[m], 2);
                }
            }

            // Sub-distribute Cluster family
            var cluCandidates = new List<MissileEnums>();
            foreach (var m in new[] { MissileEnums.Missile_5_Cluster, MissileEnums.Missile_8_Cluster_v2 })
            {
                if (unlocked.Contains(m)) cluCandidates.Add(m);
            }
            if (cluCandidates.Count > 0)
            {
                var subW = new Dictionary<MissileEnums, float>();
                foreach (var m in cluCandidates)
                {
                    if (m == MissileEnums.Missile_5_Cluster) subW[m] = unlocked.Contains(MissileEnums.Missile_8_Cluster_v2) ? 0.40f : 1.0f;
                    else if (m == MissileEnums.Missile_8_Cluster_v2) subW[m] = 0.60f;
                }
                float tot = 0f;
                foreach (var v in subW.Values) tot += v;
                var keys = new List<MissileEnums>(subW.Keys);
                foreach (var k in keys) subW[k] /= tot;

                foreach (var m in cluCandidates)
                {
                    typeWeights[m] = (float)System.Math.Round(wCluster * subW[m], 2);
                }
            }

            // Sub-distribute Miscellaneous family
            var miscCandidates = new List<MissileEnums>();
            foreach (var m in new[] { MissileEnums.Missile_3_Exploder, MissileEnums.Missile_11_Zigzag, MissileEnums.Missile_15_Aegis, MissileEnums.Missile_16_Aegis_v2 })
            {
                if (unlocked.Contains(m)) miscCandidates.Add(m);
            }
            if (miscCandidates.Count > 0)
            {
                var subW = new Dictionary<MissileEnums, float>();
                foreach (var m in miscCandidates)
                {
                    if (m == MissileEnums.Missile_3_Exploder) subW[m] = (theme == "breather" || theme == "siege") ? 0.45f : 0.25f;
                    else if (m == MissileEnums.Missile_11_Zigzag) subW[m] = theme == "swarm" ? 0.50f : 0.30f;
                    else if (m == MissileEnums.Missile_15_Aegis) subW[m] = unlocked.Contains(MissileEnums.Missile_16_Aegis_v2) ? 0.15f : (theme == "aegis" ? 0.45f : 0.25f);
                    else if (m == MissileEnums.Missile_16_Aegis_v2) subW[m] = theme == "aegis" ? 0.50f : 0.30f;
                }
                float tot = 0f;
                foreach (var v in subW.Values) tot += v;
                var keys = new List<MissileEnums>(subW.Keys);
                foreach (var k in keys) subW[k] /= tot;

                foreach (var m in miscCandidates)
                {
                    typeWeights[m] = (float)System.Math.Round(wMisc * subW[m], 2);
                }
            }

            // Reward Missile (always added with its designated rare weight)
            typeWeights[MissileEnums.Missile_17_Reward] = rewardWeight;

            var types = new List<MissileEnums>();
            var weights = new List<float>();

            foreach (var kvp in typeWeights)
            {
                if (kvp.Value > 0f)
                {
                    types.Add(kvp.Key);
                    weights.Add(kvp.Value);
                }
            }

            return new MissileSpawnData
            {
                MissileCount = missileCount,
                MissileCreateOffset = missileOffset,
                MissileTypeList = types,
                PercentageList = weights
            };
        }
    }
}
