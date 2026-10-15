using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BoomerangEditor
{
    public static class BoomerangMaterialDesigner
    {
        private const string ModelsFolder = "Assets/Art/Models";
        private const string MaterialsFolder = "Assets/Art/Models/Materials";
        private const string TexturesFolder = "Assets/Art/Models/Materials/Textures";

        private class BoomerangConfig
        {
            public string Id;
            public string AlbedoPath;
            public string MaterialName;
            public float Metallic;
            public float Smoothness;
            public float BumpScale;
            public float ClearCoat;
            public float ClearCoatSmoothness;
            public bool HasEmission;
            public Color EmissionColor;
            public EmissionType EmissionStyle;
            public string[] ModelPaths;
        }

        private enum EmissionType
        {
            None,
            LightningYellow,
            CyberNeon,
            CyanWave,
            GlacialCore,
            EmeraldMystic
        }

        [MenuItem("Tools/Boomerang/Design Boomerang Materials")]
        public static void DesignMaterials()
        {
            EditorPrefs.SetBool("BoomerangMaterialsDesigned_v1", true);
            ExecuteDesign();
        }

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorPrefs.GetBool("BoomerangMaterialsDesigned_v1", false))
                {
                    EditorPrefs.SetBool("BoomerangMaterialsDesigned_v1", true);
                    ExecuteDesign();
                }
            };
        }

        private static void ExecuteDesign()
        {
            Debug.Log("<color=#00FF99><b>[BoomerangMaterialDesigner] Starting Material Design for all Boomerang Models...</b></color>");

            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
            {
                AssetDatabase.CreateFolder("Assets/Art/Models", "Materials");
            }
            if (!AssetDatabase.IsValidFolder(TexturesFolder))
            {
                AssetDatabase.CreateFolder(MaterialsFolder, "Textures");
            }

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
            {
                Debug.LogError("[BoomerangMaterialDesigner] Universal Render Pipeline/Lit shader not found!");
                return;
            }

            var configs = GetConfigs();

            int createdCount = 0;
            foreach (var cfg in configs)
            {
                try
                {
                    ProcessBoomerang(cfg, litShader);
                    createdCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[BoomerangMaterialDesigner] Failed processing {cfg.Id}: {ex}");
                }
            }

            // Also create specialized FrozenShard sub-materials if needed
            CreateFrozenShardSubMaterials(litShader);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF99><b>[BoomerangMaterialDesigner] Successfully designed and applied materials for {createdCount} boomerangs!</b></color>");
        }

        private static List<BoomerangConfig> GetConfigs()
        {
            return new List<BoomerangConfig>
            {
                // 1. Simple / Outback
                new BoomerangConfig
                {
                    Id = "Boomerang_Simple",
                    AlbedoPath = "Assets/Art/Models/Boomerangs/1/Boomerang_Simple_Albedo.png",
                    MaterialName = "Boomerang_Simple_Albedo",
                    Metallic = 0.04f,
                    Smoothness = 0.58f,
                    BumpScale = 1.0f,
                    ClearCoat = 0.35f,
                    ClearCoatSmoothness = 0.65f,
                    HasEmission = false,
                    ModelPaths = new[] { "Assets/Art/Models/Boomerangs/1/Boomerang_Simple.fbx" }
                },
                // 2. Thunderbolt
                new BoomerangConfig
                {
                    Id = "Boomerang_12_Thunderbolt",
                    AlbedoPath = "Assets/Art/Models/Boomerang_12_Thunderbolt_Albedo.png",
                    MaterialName = "Boomerang_12_Thunderbolt",
                    Metallic = 0.75f,
                    Smoothness = 0.82f,
                    BumpScale = 1.2f,
                    ClearCoat = 0.40f,
                    ClearCoatSmoothness = 0.85f,
                    HasEmission = true,
                    EmissionColor = new Color(1.0f, 0.85f, 0.15f) * 2.5f,
                    EmissionStyle = EmissionType.LightningYellow,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_12_Thunderbolt.fbx",
                        "Assets/Art/Models/Boomerang_12_Thunderbolt.obj"
                    }
                },
                // 3. Needle
                new BoomerangConfig
                {
                    Id = "Boomerang_16_Needle",
                    AlbedoPath = "Assets/Art/Models/Boomerang_16_Needle_Albedo.png",
                    MaterialName = "Boomerang_16_Needle",
                    Metallic = 0.86f,
                    Smoothness = 0.84f,
                    BumpScale = 1.1f,
                    ClearCoat = 0.30f,
                    ClearCoatSmoothness = 0.80f,
                    HasEmission = false,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_16_Needle.fbx",
                        "Assets/Art/Models/Boomerang_16_Needle.obj"
                    }
                },
                // 4. Bulldog
                new BoomerangConfig
                {
                    Id = "Boomerang_17_Bulldog",
                    AlbedoPath = "Assets/Art/Models/Boomerang_17_Bulldog_Albedo.png",
                    MaterialName = "Boomerang_17_Bulldog",
                    Metallic = 0.68f,
                    Smoothness = 0.54f,
                    BumpScale = 1.35f,
                    ClearCoat = 0.10f,
                    ClearCoatSmoothness = 0.50f,
                    HasEmission = false,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_17_Bulldog.fbx",
                        "Assets/Art/Models/Boomerang_17_Bulldog.obj"
                    }
                },
                // 5. Omega
                new BoomerangConfig
                {
                    Id = "Boomerang_18_Omega",
                    AlbedoPath = "Assets/Art/Models/Boomerang_18_Omega_Albedo.png",
                    MaterialName = "Boomerang_18_Omega",
                    Metallic = 0.76f,
                    Smoothness = 0.80f,
                    BumpScale = 1.15f,
                    ClearCoat = 0.25f,
                    ClearCoatSmoothness = 0.75f,
                    HasEmission = true,
                    EmissionColor = new Color(0.05f, 0.5f, 1.0f) * 1.5f,
                    EmissionStyle = EmissionType.CyanWave,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_18_Omega.fbx",
                        "Assets/Art/Models/Boomerang_18_Omega.obj"
                    }
                },
                // 6. Razor
                new BoomerangConfig
                {
                    Id = "Boomerang_19_Razor",
                    AlbedoPath = "Assets/Art/Models/Boomerang_19_Razor_Albedo.png",
                    MaterialName = "Boomerang_19_Razor",
                    Metallic = 0.92f,
                    Smoothness = 0.90f,
                    BumpScale = 1.25f,
                    ClearCoat = 0.50f,
                    ClearCoatSmoothness = 0.95f,
                    HasEmission = false,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_19_Razor.fbx",
                        "Assets/Art/Models/Boomerang_19_Razor.obj"
                    }
                },
                // 7. SWave
                new BoomerangConfig
                {
                    Id = "Boomerang_20_SWave",
                    AlbedoPath = "Assets/Art/Models/Boomerang_20_SWave_Albedo.png",
                    MaterialName = "Boomerang_20_SWave",
                    Metallic = 0.58f,
                    Smoothness = 0.86f,
                    BumpScale = 1.1f,
                    ClearCoat = 0.40f,
                    ClearCoatSmoothness = 0.85f,
                    HasEmission = true,
                    EmissionColor = new Color(0.0f, 0.95f, 1.0f) * 2.0f,
                    EmissionStyle = EmissionType.CyanWave,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_20_SWave.fbx",
                        "Assets/Art/Models/Boomerang_20_SWave.obj"
                    }
                },
                // 8. Aerosplit
                new BoomerangConfig
                {
                    Id = "Boomerang_24_Aerosplit",
                    AlbedoPath = "Assets/Art/Models/Boomerang_24_Aerosplit_Albedo.png",
                    MaterialName = "Boomerang_24_Aerosplit",
                    Metallic = 0.52f,
                    Smoothness = 0.78f,
                    BumpScale = 1.2f,
                    ClearCoat = 0.50f,
                    ClearCoatSmoothness = 0.80f,
                    HasEmission = false,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_24_Aerosplit.fbx",
                        "Assets/Art/Models/Boomerang_24_Aerosplit.obj"
                    }
                },
                // 9. CyberShuriken
                new BoomerangConfig
                {
                    Id = "Boomerang_26_CyberShuriken",
                    AlbedoPath = "Assets/Art/Models/Boomerang_26_CyberShuriken_Albedo.png",
                    MaterialName = "Boomerang_26_CyberShuriken",
                    Metallic = 0.84f,
                    Smoothness = 0.76f,
                    BumpScale = 1.3f,
                    ClearCoat = 0.30f,
                    ClearCoatSmoothness = 0.80f,
                    HasEmission = true,
                    EmissionColor = new Color(0.1f, 1.0f, 1.0f) * 3.2f,
                    EmissionStyle = EmissionType.CyberNeon,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_26_CyberShuriken.fbx",
                        "Assets/Art/Models/Boomerang_26_CyberShuriken.obj"
                    }
                },
                // 10. DesertMulga
                new BoomerangConfig
                {
                    Id = "Boomerang_31_DesertMulga",
                    AlbedoPath = "Assets/Art/Models/Boomerang_31_DesertMulga_Albedo.png",
                    MaterialName = "Boomerang_31_DesertMulga",
                    Metallic = 0.02f,
                    Smoothness = 0.50f,
                    BumpScale = 1.15f,
                    ClearCoat = 0.28f,
                    ClearCoatSmoothness = 0.60f,
                    HasEmission = false,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_31_DesertMulga.fbx",
                        "Assets/Art/Models/Boomerang_31_DesertMulga.obj"
                    }
                },
                // 11. KylieHunter
                new BoomerangConfig
                {
                    Id = "Boomerang_33_KylieHunter",
                    AlbedoPath = "Assets/Art/Models/Boomerang_33_KylieHunter_Albedo.png",
                    MaterialName = "Boomerang_33_KylieHunter",
                    Metallic = 0.03f,
                    Smoothness = 0.52f,
                    BumpScale = 1.25f,
                    ClearCoat = 0.22f,
                    ClearCoatSmoothness = 0.55f,
                    HasEmission = false,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_33_KylieHunter.fbx",
                        "Assets/Art/Models/Boomerang_33_KylieHunter.obj"
                    }
                },
                // 12. VikingAxe
                new BoomerangConfig
                {
                    Id = "Boomerang_36_VikingAxe",
                    AlbedoPath = "Assets/Art/Models/Boomerang_36_VikingAxe_Albedo.png",
                    MaterialName = "Boomerang_36_VikingAxe",
                    Metallic = 0.85f,
                    Smoothness = 0.74f,
                    BumpScale = 1.3f,
                    ClearCoat = 0.20f,
                    ClearCoatSmoothness = 0.65f,
                    HasEmission = false,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_36_VikingAxe.fbx",
                        "Assets/Art/Models/Boomerang_36_VikingAxe.obj"
                    }
                },
                // 13. CrusaderSword
                new BoomerangConfig
                {
                    Id = "Boomerang_37_CrusaderSword",
                    AlbedoPath = "Assets/Art/Models/Boomerang_37_CrusaderSword_Albedo.png",
                    MaterialName = "Boomerang_37_CrusaderSword",
                    Metallic = 0.90f,
                    Smoothness = 0.86f,
                    BumpScale = 1.2f,
                    ClearCoat = 0.40f,
                    ClearCoatSmoothness = 0.90f,
                    HasEmission = false,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_37_CrusaderSword.fbx",
                        "Assets/Art/Models/Boomerang_37_CrusaderSword.obj"
                    }
                },
                // 14. SaracenScimitar
                new BoomerangConfig
                {
                    Id = "Boomerang_43_SaracenScimitar",
                    AlbedoPath = "Assets/Art/Models/Boomerang_43_SaracenScimitar_Albedo.png",
                    MaterialName = "Boomerang_43_SaracenScimitar",
                    Metallic = 0.88f,
                    Smoothness = 0.84f,
                    BumpScale = 1.25f,
                    ClearCoat = 0.35f,
                    ClearCoatSmoothness = 0.85f,
                    HasEmission = true,
                    EmissionColor = new Color(0.1f, 0.9f, 0.7f) * 1.6f,
                    EmissionStyle = EmissionType.EmeraldMystic,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerang_43_SaracenScimitar.fbx",
                        "Assets/Art/Models/Boomerang_43_SaracenScimitar.obj"
                    }
                },
                // 15. FrozenShard
                new BoomerangConfig
                {
                    Id = "Boomerang_FrozenShard",
                    AlbedoPath = "Assets/Art/Models/Boomerangs/2/Boomerang_FrozenShard_Albedo.png",
                    MaterialName = "Boomerang_FrozenShard",
                    Metallic = 0.18f,
                    Smoothness = 0.94f,
                    BumpScale = 1.35f,
                    ClearCoat = 0.60f,
                    ClearCoatSmoothness = 0.95f,
                    HasEmission = true,
                    EmissionColor = new Color(0.2f, 0.75f, 1.0f) * 2.4f,
                    EmissionStyle = EmissionType.GlacialCore,
                    ModelPaths = new[]
                    {
                        "Assets/Art/Models/Boomerangs/2/Boomerang_FrozenShard.fbx",
                        "Assets/Art/Models/Boomerangs/2/Boomerang_FrozenShard.obj"
                    }
                }
            };
        }

        private static void ProcessBoomerang(BoomerangConfig cfg, Shader litShader)
        {
            Texture2D albedoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(cfg.AlbedoPath);
            if (albedoTex == null)
            {
                Debug.LogWarning($"[BoomerangMaterialDesigner] Albedo texture not found at {cfg.AlbedoPath}");
                return;
            }

            // Ensure albedo is readable to generate normal/emission maps
            EnsureTextureReadable(cfg.AlbedoPath);

            // 1. Generate Normal Map
            string normalPath = $"{TexturesFolder}/{cfg.Id}_Normal.png";
            GenerateNormalMap(albedoTex, normalPath, cfg.BumpScale);

            // 2. Generate Emission Map if applicable
            string emissionPath = null;
            if (cfg.HasEmission && cfg.EmissionStyle != EmissionType.None)
            {
                emissionPath = $"{TexturesFolder}/{cfg.Id}_Emission.png";
                GenerateEmissionMap(albedoTex, emissionPath, cfg.EmissionStyle);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Texture2D normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            Texture2D emissionTex = !string.IsNullOrEmpty(emissionPath) ? AssetDatabase.LoadAssetAtPath<Texture2D>(emissionPath) : null;

            // 3. Create or Update Material
            string matPath = $"{MaterialsFolder}/{cfg.MaterialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(litShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = litShader;
            }

            mat.SetTexture("_BaseMap", albedoTex);
            mat.SetColor("_BaseColor", Color.white);

            if (normalTex != null)
            {
                mat.SetTexture("_BumpMap", normalTex);
                mat.SetFloat("_BumpScale", cfg.BumpScale);
                mat.EnableKeyword("_NORMALMAP");
            }

            mat.SetFloat("_Metallic", cfg.Metallic);
            mat.SetFloat("_Smoothness", cfg.Smoothness);
            mat.SetFloat("_SpecularHighlights", 1f);
            mat.SetFloat("_EnvironmentReflections", 1f);

            if (cfg.ClearCoat > 0f)
            {
                mat.SetFloat("_ClearCoatMask", cfg.ClearCoat);
                mat.SetFloat("_ClearCoatSmoothness", cfg.ClearCoatSmoothness);
                mat.EnableKeyword("_CLEARCOAT");
            }

            if (cfg.HasEmission)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", cfg.EmissionColor);
                if (emissionTex != null)
                {
                    mat.SetTexture("_EmissionMap", emissionTex);
                }
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
                mat.SetTexture("_EmissionMap", null);
            }

            EditorUtility.SetDirty(mat);

            // Also create an alias material if the model uses M_<Name> as material slot name
            string aliasName = GetMtlMaterialName(cfg.Id);
            if (!string.IsNullOrEmpty(aliasName) && aliasName != cfg.MaterialName)
            {
                string aliasPath = $"{MaterialsFolder}/{aliasName}.mat";
                Material aliasMat = AssetDatabase.LoadAssetAtPath<Material>(aliasPath);
                if (aliasMat == null)
                {
                    aliasMat = new Material(mat);
                    AssetDatabase.CreateAsset(aliasMat, aliasPath);
                }
                else
                {
                    aliasMat.CopyPropertiesFromMaterial(mat);
                    EditorUtility.SetDirty(aliasMat);
                }
            }

            // 4. Remap to 3D Models
            foreach (var modelPath in cfg.ModelPaths)
            {
                RemapModelMaterial(modelPath, mat, aliasName);
            }
        }

        private static string GetMtlMaterialName(string id)
        {
            if (id.StartsWith("Boomerang_"))
            {
                string suffix = id.Substring("Boomerang_".Length);
                // e.g. 12_Thunderbolt -> M_Thunderbolt
                int underscoreIdx = suffix.IndexOf('_');
                if (underscoreIdx >= 0)
                {
                    return "M_" + suffix.Substring(underscoreIdx + 1);
                }
                return "M_" + suffix;
            }
            return null;
        }

        private static void CreateFrozenShardSubMaterials(Shader litShader)
        {
            // Ice Material
            string iceMatPath = $"{MaterialsFolder}/M_FrozenShard_Ice.mat";
            Material iceMat = AssetDatabase.LoadAssetAtPath<Material>(iceMatPath);
            if (iceMat == null)
            {
                iceMat = new Material(litShader);
                AssetDatabase.CreateAsset(iceMat, iceMatPath);
            }
            iceMat.SetColor("_BaseColor", new Color(0.24f, 0.84f, 0.98f, 0.9f));
            iceMat.SetFloat("_Metallic", 0.15f);
            iceMat.SetFloat("_Smoothness", 0.95f);
            iceMat.SetFloat("_ClearCoatMask", 0.8f);
            iceMat.SetFloat("_ClearCoatSmoothness", 0.95f);
            EditorUtility.SetDirty(iceMat);

            // Frost Material
            string frostMatPath = $"{MaterialsFolder}/M_FrozenShard_Frost.mat";
            Material frostMat = AssetDatabase.LoadAssetAtPath<Material>(frostMatPath);
            if (frostMat == null)
            {
                frostMat = new Material(litShader);
                AssetDatabase.CreateAsset(frostMat, frostMatPath);
            }
            frostMat.SetColor("_BaseColor", new Color(0.96f, 0.98f, 1.0f, 1.0f));
            frostMat.SetFloat("_Metallic", 0.05f);
            frostMat.SetFloat("_Smoothness", 0.72f);
            EditorUtility.SetDirty(frostMat);

            // Core Material (Glowing)
            string coreMatPath = $"{MaterialsFolder}/M_FrozenShard_Core.mat";
            Material coreMat = AssetDatabase.LoadAssetAtPath<Material>(coreMatPath);
            if (coreMat == null)
            {
                coreMat = new Material(litShader);
                AssetDatabase.CreateAsset(coreMat, coreMatPath);
            }
            coreMat.SetColor("_BaseColor", new Color(0.7f, 0.95f, 1.0f, 1.0f));
            coreMat.SetFloat("_Metallic", 0.1f);
            coreMat.SetFloat("_Smoothness", 0.95f);
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_EmissionColor", new Color(0.5f, 0.9f, 1.0f) * 3.5f);
            coreMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(coreMat);

            // Remap multi-material models
            string[] fzPaths = new[]
            {
                "Assets/Art/Models/Boomerangs/2/Boomerang_FrozenShard.fbx",
                "Assets/Art/Models/Boomerangs/2/Boomerang_FrozenShard.obj"
            };
            foreach (var path in fzPaths)
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null)
                {
                    importer.materialLocation = ModelImporterMaterialLocation.External;
                    importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
                    importer.materialSearch = ModelImporterMaterialSearch.Everywhere;

                    var idIce = new AssetImporter.SourceAssetIdentifier(typeof(Material), "M_FrozenShard_Ice");
                    var idFrost = new AssetImporter.SourceAssetIdentifier(typeof(Material), "M_FrozenShard_Frost");
                    var idCore = new AssetImporter.SourceAssetIdentifier(typeof(Material), "M_FrozenShard_Core");

                    importer.AddRemap(idIce, iceMat);
                    importer.AddRemap(idFrost, frostMat);
                    importer.AddRemap(idCore, coreMat);

                    AssetDatabase.WriteImportSettingsIfDirty(path);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        private static void RemapModelMaterial(string modelPath, Material mat, string aliasName)
        {
            ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) return;

            importer.materialLocation = ModelImporterMaterialLocation.External;
            importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
            importer.materialSearch = ModelImporterMaterialSearch.Everywhere;

            // Try remapping both the mat.name and aliasName (M_<Name>)
            var id1 = new AssetImporter.SourceAssetIdentifier(typeof(Material), mat.name);
            importer.AddRemap(id1, mat);

            if (!string.IsNullOrEmpty(aliasName))
            {
                var id2 = new AssetImporter.SourceAssetIdentifier(typeof(Material), aliasName);
                importer.AddRemap(id2, mat);
            }

            // Also remap default/fallback names like "Material" or "default"
            var idDef = new AssetImporter.SourceAssetIdentifier(typeof(Material), "default");
            importer.AddRemap(idDef, mat);

            AssetDatabase.WriteImportSettingsIfDirty(modelPath);
            AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);
        }

        private static void EnsureTextureReadable(string texturePath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                AssetDatabase.WriteImportSettingsIfDirty(texturePath);
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            }
        }

        private static void GenerateNormalMap(Texture2D src, string outputPath, float bumpScale)
        {
            int w = src.width;
            int h = src.height;

            Color[] srcPixels = src.GetPixels();
            Color[] normalPixels = new Color[srcPixels.Length];

            float strength = 2.5f * bumpScale;

            // Fast Sobel Normal generation
            for (int y = 0; y < h; y++)
            {
                int y0 = (y > 0) ? y - 1 : y;
                int y1 = (y < h - 1) ? y + 1 : y;

                for (int x = 0; x < w; x++)
                {
                    int x0 = (x > 0) ? x - 1 : x;
                    int x1 = (x < w - 1) ? x + 1 : x;

                    float tl = srcPixels[y0 * w + x0].grayscale;
                    float t  = srcPixels[y0 * w + x ].grayscale;
                    float tr = srcPixels[y0 * w + x1].grayscale;

                    float l  = srcPixels[y  * w + x0].grayscale;
                    float r  = srcPixels[y  * w + x1].grayscale;

                    float bl = srcPixels[y1 * w + x0].grayscale;
                    float b  = srcPixels[y1 * w + x ].grayscale;
                    float br = srcPixels[y1 * w + x1].grayscale;

                    float dX = (tr + 2.0f * r + br) - (tl + 2.0f * l + bl);
                    float dY = (bl + 2.0f * b + br) - (tl + 2.0f * t + tr);

                    Vector3 normal = new Vector3(-dX * strength, -dY * strength, 1.0f).normalized;

                    // Encode into normal map color [0..1]
                    normalPixels[y * w + x] = new Color(
                        normal.x * 0.5f + 0.5f,
                        normal.y * 0.5f + 0.5f,
                        normal.z * 0.5f + 0.5f,
                        1.0f
                    );
                }
            }

            Texture2D normalTex = new Texture2D(w, h, TextureFormat.RGB24, false);
            normalTex.SetPixels(normalPixels);
            normalTex.Apply();

            byte[] bytes = normalTex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(normalTex);

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), outputPath);
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllBytes(fullPath, bytes);

            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.wrapMode = TextureWrapMode.Repeat;
                AssetDatabase.WriteImportSettingsIfDirty(outputPath);
                AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            }
        }

        private static void GenerateEmissionMap(Texture2D src, string outputPath, EmissionType type)
        {
            int w = src.width;
            int h = src.height;

            Color[] srcPixels = src.GetPixels();
            Color[] emissionPixels = new Color[srcPixels.Length];

            for (int i = 0; i < srcPixels.Length; i++)
            {
                Color c = srcPixels[i];
                float r = c.r;
                float g = c.g;
                float b = c.b;

                bool isEmissive = false;
                Color emitColor = Color.black;

                switch (type)
                {
                    case EmissionType.LightningYellow:
                        // Bright yellow/gold lightning streaks (high R, high G, lower B)
                        if (r > 0.65f && g > 0.55f && b < 0.45f)
                        {
                            isEmissive = true;
                            emitColor = new Color(r * 1.2f, g * 1.1f, b * 0.5f, 1f);
                        }
                        break;

                    case EmissionType.CyberNeon:
                        // Saturated cyan or vibrant magenta/pink circuits
                        bool isCyan = (b > 0.55f && g > 0.55f && r < 0.45f);
                        bool isMagenta = (r > 0.65f && b > 0.55f && g < 0.45f);
                        if (isCyan || isMagenta)
                        {
                            isEmissive = true;
                            emitColor = new Color(r, g, b, 1f);
                        }
                        break;

                    case EmissionType.CyanWave:
                        // Vibrant cyan wave crests
                        if (b > 0.58f && g > 0.55f && (b + g) > (r * 2.2f))
                        {
                            isEmissive = true;
                            emitColor = new Color(0.1f, g, b, 1f);
                        }
                        break;

                    case EmissionType.GlacialCore:
                        // Radiant ice core (bright azure/white highlights)
                        float lum = 0.299f * r + 0.587f * g + 0.114f * b;
                        if (lum > 0.72f && b > 0.65f)
                        {
                            isEmissive = true;
                            emitColor = new Color(r * 0.8f, g * 0.95f, b, 1f);
                        }
                        break;

                    case EmissionType.EmeraldMystic:
                        // Glowing emerald inlays
                        if (g > 0.58f && r < 0.45f && b > 0.35f)
                        {
                            isEmissive = true;
                            emitColor = new Color(r * 0.3f, g, b * 0.7f, 1f);
                        }
                        break;
                }

                emissionPixels[i] = isEmissive ? emitColor : Color.black;
            }

            Texture2D emissionTex = new Texture2D(w, h, TextureFormat.RGB24, false);
            emissionTex.SetPixels(emissionPixels);
            emissionTex.Apply();

            byte[] bytes = emissionTex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(emissionTex);

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), outputPath);
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllBytes(fullPath, bytes);

            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                AssetDatabase.WriteImportSettingsIfDirty(outputPath);
                AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
