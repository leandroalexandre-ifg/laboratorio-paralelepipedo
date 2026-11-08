using System.Linq;
using LaboratorioParalelepipedo.Nucleo.Dados;
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

        private const string CaminhoDaFonte = "Assets/_Projeto/Fontes/Nunito SDF.asset";

        private static string Ler(string caminho) => AssetDatabase.LoadAssetAtPath<TextAsset>(caminho).text;

        private static TMP_FontAsset Fonte()
        {
            var fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CaminhoDaFonte);
            Assert.That(fonte, Is.Not.Null, $"{CaminhoDaFonte} não existe. Rode Laboratório › Regenerar atlas da fonte.");
            return fonte;
        }

        private static string CaracteresDoAtlasGerado(TMP_FontAsset fonte) =>
            string.Concat(fonte.characterTable.Select(c => (char)c.unicode));

        [Test]
        public void FonteNunito_EhEstatica_ESemOTtfNoBuild()
        {
            var fonte = Fonte();

            Assert.That(fonte.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Static),
                "atlas dinâmico muda o asset ao dar Play");
            Assert.That(fonte.sourceFontFile, Is.Null, "fonte estática não leva o .ttf para o APK");
            Assert.That(fonte.atlasTexture.width, Is.GreaterThan(1));
        }

        [Test]
        public void FonteNunito_EhAPadraoDoTextMeshPro()
        {
            Assert.That(TMP_Settings.defaultFontAsset, Is.EqualTo(Fonte()), "a LiberationSans não deve ser usada");
        }

        [Test]
        public void AtlasDaFonte_TemTodosOsCaracteresDosTextosEDesafios()
        {
            string atlas = CaracteresDoAtlasGerado(Fonte());
            string esperados = CaracteresDoAtlas.Coletar(Ler("Assets/_Projeto/Fontes/caracteres-extras.txt"),
                Ler("Assets/_Projeto/Dados/textos.json"), Ler("Assets/_Projeto/Dados/desafios.json"));

            string faltando = CaracteresDoAtlas.QueFaltam(esperados, atlas);

            Assert.That(faltando, Is.Empty, MensagemDeFaltantes(faltando));
        }

        [Test]
        public void MensagemDeFaltantes_DizOQueFazer()
        {
            string mensagem = MensagemDeFaltantes(CaracteresDoAtlas.QueFaltam("φ = 1,618", CaracteresDoAtlasGerado(Fonte())));

            Assert.That(mensagem, Does.Contain("\"φ\" (U+03C6)").And.Contain("Laboratório › Regenerar atlas da fonte"));
        }

        /// <summary>Mensagem do teste quando um texto novo usa caractere fora do atlas.</summary>
        private static string MensagemDeFaltantes(string faltando) =>
            "Caracteres dos JSON fora do atlas da Nunito SDF: " +
            string.Join(", ", faltando.Select(c => $"\"{c}\" (U+{(int)c:X4})")) +
            ". Rode Laboratório › Regenerar atlas da fonte e commite o Nunito SDF.asset. Se a Nunito não tiver o " +
            "caractere, confira com Tools/verificar_glifos.py.";
    }
}
