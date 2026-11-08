using System.Collections.Generic;
using System.IO;
using LaboratorioParalelepipedo.Nucleo.Dados;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace LaboratorioParalelepipedo.Editor
{
    /// <summary>
    /// Configura o projeto por script, em passos que podem rodar no menu "Laboratório" ou em batchmode
    /// (cada passo numa execução, porque importar pacotes e criar o perfil de build terminam depois da chamada):
    /// <code>
    /// -importPackage "Library/PackageCache/com.unity.ugui@*/Package Resources/TMP Essential Resources.unitypackage" -quit
    /// -executeMethod LaboratorioParalelepipedo.Editor.ConfiguracaoDoProjeto.CriarPerfilMetaQuest   (sem -quit)
    /// -buildTarget Android -executeMethod LaboratorioParalelepipedo.Editor.ConfiguracaoDoProjeto.ConfigurarMetaQuest -quit
    /// -executeMethod LaboratorioParalelepipedo.Editor.ConfiguracaoDoProjeto.CriarFonte -quit
    /// </code>
    /// </summary>
    public static class ConfiguracaoDoProjeto
    {
        /// <summary>Package name fixo: depois do primeiro envio ao ALPHA, outro nome vira outro app na loja.</summary>
        public const string PackageName = "br.edu.ifg.laboratorioparalelepipedo";
        public const string NomeDoProduto = "Laboratório do Paralelepípedo";
        public const string NomeDaEmpresa = "NumbERS IFG";
        public const string Versao = "0.1.0";

        public const string CaminhoDoPerfil = "Assets/Settings/Build Profiles/Meta Quest.asset";
        public const string CaminhoDaFonte = "Assets/_Projeto/Fontes/Nunito-SemiBold.ttf";
        public const string CaminhoDaFonteTmp = "Assets/_Projeto/Fontes/Nunito SDF.asset";
        public const string CaminhoDosTextos = "Assets/_Projeto/Dados/textos.json";
        public const string CaminhoDosDesafios = "Assets/_Projeto/Dados/desafios.json";

        private const string IdDaPlataformaMetaQuest = "80657fe557de4d17822398b3a01b8c9e";
        private const string NomeDoPerfil = "Meta Quest";
        private const string ConfiguracoesDoTmp = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        private const string ConfiguracoesDoXr = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";
        private const string LoaderOpenXr = "UnityEngine.XR.OpenXR.OpenXRLoader";

        /// <summary>Símbolos que o código de exibição usa além dos JSON (dimensões, aproximações, rótulos).</summary>
        public const string SimbolosDoCodigo = "0123456789ABCDEFGH×≈√²³·,.+−-=()/: ";

        /// <summary>Features OpenXR do Android, as mesmas do projeto Missão Coração.</summary>
        private static readonly string[] FeaturesDoAndroid =
        {
            "com.unity.openxr.feature.metaquest",
            "com.meta.openxr.feature.metaxr",
            "com.meta.openxr.feature.foveation",
            "com.meta.openxr.feature.subsampledLayout",
            "com.meta.openxr.feature.spacewarp",
            "com.unity.openxr.feature.input.oculustouch",
            "com.meta.openxr.feature.input.oculustouch.proximity",
            "com.unity.openxr.feature.input.metaquestplus",
            "com.unity.openxr.feature.input.metaquestpro",
        };

        /// <summary>
        /// Importa os recursos essenciais do TextMeshPro (shaders e TMP Settings). A importação termina depois da
        /// chamada; em batchmode use -importPackage (ver o comentário da classe).
        /// </summary>
        [MenuItem("Laboratório/1. Importar recursos do TextMeshPro")]
        public static void ImportarRecursosDoTmp()
        {
            if (File.Exists(ConfiguracoesDoTmp))
            {
                Debug.Log("[Configuração] Os recursos do TextMeshPro já estão no projeto.");
                return;
            }

            TMP_PackageResourceImporter.ImportResources(importEssentials: true, importExamples: false, interactive: false);
            Debug.Log("[Configuração] Recursos essenciais do TextMeshPro importados.");
        }

        /// <summary>Cria o perfil "Meta Quest" (se não existir). Em batchmode, fecha o Unity quando o perfil fica pronto.</summary>
        [MenuItem("Laboratório/2. Criar perfil de build Meta Quest")]
        public static void CriarPerfilMetaQuest()
        {
            if (BuildProfile.GetBuildProfileAtPath(CaminhoDoPerfil) != null)
            {
                Debug.Log($"[Configuração] O perfil já existe em '{CaminhoDoPerfil}'.");
                SairSeBatch(0);
                return;
            }

            // O callback da API precisa ser método de um UnityEngine.Object; sem ele, o perfil é criado na hora
            // em Assets/Settings/Build Profiles.
            BuildProfile.CreateBuildProfile(new GUID(IdDaPlataformaMetaQuest), NomeDoPerfil, null);
            AssetDatabase.SaveAssets();
            bool criado = BuildProfile.GetBuildProfileAtPath(CaminhoDoPerfil) != null;
            if (criado) Debug.Log($"[Configuração] Perfil criado em '{CaminhoDoPerfil}'.");
            else Debug.LogError($"[Configuração] O perfil não apareceu em '{CaminhoDoPerfil}'.");
            SairSeBatch(criado ? 0 : 1);
        }

        /// <summary>
        /// Ativa o perfil Meta Quest e grava nele as Player Settings do Quest 2 (com o perfil ativo, a API de
        /// PlayerSettings grava no perfil). Liga o OpenXR no Android com as features do Missão Coração.
        /// </summary>
        [MenuItem("Laboratório/3. Configurar Meta Quest (perfil, Player e OpenXR)")]
        public static void ConfigurarMetaQuest()
        {
            BuildProfile perfil = BuildProfile.GetBuildProfileAtPath(CaminhoDoPerfil);
            if (perfil == null)
            {
                Debug.LogError($"[Configuração] Perfil não encontrado em '{CaminhoDoPerfil}'. Rode o passo 2 antes.");
                SairSeBatch(1);
                return;
            }

            if (BuildProfile.GetActiveBuildProfile() != perfil) BuildProfile.SetActiveBuildProfile(perfil);

            var android = NamedBuildTarget.Android;
            PlayerSettings.productName = NomeDoProduto;
            PlayerSettings.companyName = NomeDaEmpresa;
            PlayerSettings.bundleVersion = Versao;
            PlayerSettings.SetApplicationIdentifier(android, PackageName);
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorUtility.SetDirty(perfil);

            ConfigurarOpenXr();
            AssetDatabase.SaveAssets();
            Debug.Log("[Configuração] Perfil Meta Quest ativo e configurado.");
        }

        /// <summary>Cria (ou atualiza) a fonte TextMeshPro dinâmica da Nunito, já com os caracteres dos JSON.</summary>
        [MenuItem("Laboratório/4. Criar fonte Nunito SDF")]
        public static void CriarFonte()
        {
            var fonte = AssetDatabase.LoadAssetAtPath<Font>(CaminhoDaFonte);
            if (fonte == null)
            {
                Debug.LogError($"[Configuração] Fonte não encontrada em '{CaminhoDaFonte}'.");
                return;
            }

            var fonteTmp = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CaminhoDaFonteTmp);
            if (fonteTmp == null)
            {
                fonteTmp = TMP_FontAsset.CreateFontAsset(fonte, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                    AtlasPopulationMode.Dynamic, enableMultiAtlasSupport: true);
                fonteTmp.name = Path.GetFileNameWithoutExtension(CaminhoDaFonteTmp);
                AssetDatabase.CreateAsset(fonteTmp, CaminhoDaFonteTmp);
                AssetDatabase.AddObjectToAsset(fonteTmp.material, fonteTmp);
                AssetDatabase.AddObjectToAsset(fonteTmp.atlasTexture, fonteTmp);
            }

            string caracteres = CaracteresDoApp();
            if (!fonteTmp.TryAddCharacters(caracteres, out string faltando))
                Debug.LogWarning($"[Configuração] A Nunito não tem: {faltando}");

            EditorUtility.SetDirty(fonteTmp);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Configuração] Fonte '{CaminhoDaFonteTmp}' com {fonteTmp.characterTable.Count} caracteres.");
        }

        /// <summary>Todos os caracteres que o aluno pode ver: textos, desafios e símbolos do código.</summary>
        public static string CaracteresDoApp()
        {
            var conjunto = new SortedSet<char>(SimbolosDoCodigo);
            var textos = AssetDatabase.LoadAssetAtPath<TextAsset>(CaminhoDosTextos);
            var desafios = AssetDatabase.LoadAssetAtPath<TextAsset>(CaminhoDosDesafios);
            foreach (char c in textos.text) conjunto.Add(c);
            foreach (char c in desafios.text) conjunto.Add(c);
            conjunto.RemoveWhere(char.IsControl);
            return string.Concat(conjunto);
        }

        private static void ConfigurarOpenXr()
        {
            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,
                    out XRGeneralSettingsPerBuildTarget porPlataforma))
            {
                porPlataforma = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(ConfiguracoesDoXr);
                if (porPlataforma == null)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ConfiguracoesDoXr));
                    porPlataforma = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                    AssetDatabase.CreateAsset(porPlataforma, ConfiguracoesDoXr);
                }

                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, porPlataforma, true);
            }

            // Só o Android carrega o OpenXR. No Standalone (Play no Editor) fica sem loader: a câmera vira uma câmera
            // comum, e dá para testar a cena sem o óculos.
            if (!porPlataforma.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
                porPlataforma.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            XRGeneralSettings android = porPlataforma.SettingsForBuildTarget(BuildTargetGroup.Android);
            android.InitManagerOnStart = true;
            XRPackageMetadataStore.AssignLoader(android.Manager, LoaderOpenXr, BuildTargetGroup.Android);
            EditorUtility.SetDirty(android);
            EditorUtility.SetDirty(android.Manager);

            FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
            foreach (string id in FeaturesDoAndroid)
            {
                var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(BuildTargetGroup.Android, id);
                if (feature == null)
                {
                    Debug.LogWarning($"[Configuração] Feature OpenXR '{id}' não encontrada.");
                    continue;
                }

                feature.enabled = true;
                EditorUtility.SetDirty(feature);
            }

            var openXr = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            openXr.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
            EditorUtility.SetDirty(openXr);
        }

        private static void SairSeBatch(int codigo)
        {
            if (Application.isBatchMode) EditorApplication.Exit(codigo);
        }
    }
}
