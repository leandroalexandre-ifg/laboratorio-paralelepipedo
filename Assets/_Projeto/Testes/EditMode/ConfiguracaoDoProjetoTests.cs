using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace LaboratorioParalelepipedo.Testes
{
    /// <summary>Confere o perfil Meta Quest, o OpenXR e a fonte do app.</summary>
    public class ConfiguracaoDoProjetoTests
    {
        private const string PackageName = "br.edu.ifg.laboratorioparalelepipedo";
        private const string CaminhoDoPerfil = "Assets/Settings/Build Profiles/Meta Quest.asset";

        [Test]
        public void PerfilMetaQuestEstaAtivo_ComOPackageNameFixo()
        {
            var perfil = BuildProfile.GetBuildProfileAtPath(CaminhoDoPerfil);

            Assert.That(perfil, Is.Not.Null);
            Assert.That(BuildProfile.GetActiveBuildProfile(), Is.EqualTo(perfil));
            Assert.That(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android), Is.EqualTo(PackageName));
        }

        [Test]
        public void PlayerSettingsDoQuest2()
        {
            Assert.That(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android), Is.EqualTo(ScriptingImplementation.IL2CPP));
            Assert.That(PlayerSettings.Android.targetArchitectures, Is.EqualTo(AndroidArchitecture.ARM64));
            Assert.That((int)PlayerSettings.Android.minSdkVersion, Is.EqualTo(32));
            Assert.That(PlayerSettings.colorSpace, Is.EqualTo(ColorSpace.Linear));
        }

        [Test]
        public void OpenXRSoNoAndroid()
        {
            var android = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            var standalone = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);

            Assert.That(android.Manager.activeLoaders.Select(l => l.GetType()), Does.Contain(typeof(OpenXRLoader)));
            Assert.That(standalone == null || standalone.Manager == null || standalone.Manager.activeLoaders.Count == 0,
                "o Standalone fica sem loader, para testar no Editor sem o óculos");
        }

        [Test]
        public void OpenXRDoAndroidEmMultiview()
        {
            var openXr = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);

            Assert.That(openXr.renderMode, Is.EqualTo(OpenXRSettings.RenderMode.SinglePassInstanced));
        }

        [Test]
        public void FonteNunito_TemTodosOsCaracteresDosDados()
        {
            var fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Projeto/Fontes/Nunito SDF.asset");
            Assert.That(fonte, Is.Not.Null);
            Assert.That(fonte.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Dynamic));

            string caracteres = string.Concat(new[] { "textos.json", "desafios.json" }
                    .SelectMany(a => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Projeto/Dados/" + a).text)
                    .Concat("×≈√²·–−")
                    .Where(c => !char.IsControl(c)).Distinct());

            // Confere numa fonte temporária, para não sujar o asset.
            var temporaria = TMP_FontAsset.CreateFontAsset(fonte.sourceFontFile, 90, 9,
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
            try
            {
                temporaria.TryAddCharacters(caracteres, out string faltando);
                Assert.That(faltando, Is.Null.Or.Empty, "caracteres sem glifo na Nunito");
            }
            finally
            {
                Object.DestroyImmediate(temporaria.atlasTexture);
                Object.DestroyImmediate(temporaria.material);
                Object.DestroyImmediate(temporaria);
            }
        }
    }
}
