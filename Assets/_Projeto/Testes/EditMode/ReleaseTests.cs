using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LaboratorioParalelepipedo.Editor.Release;
using NUnit.Framework;

namespace LaboratorioParalelepipedo.Testes
{
    /// <summary>Lista de verificação do APK, manifesto de privacidade, pacotes e configuração do envio.</summary>
    public class ReleaseTests
    {
        private const string Pacote = "br.edu.ifg.laboratorioparalelepipedo";
        private const int VersionCode = 263121530;

        private static string Badging(params string[] permissoes) =>
            $"package: name='{Pacote}' versionCode='{VersionCode}' versionName='0.1.0' platformBuildVersionName=''\n" +
            "targetSdkVersion:'34'\n" +
            string.Concat(permissoes.Select(p => $"uses-permission: name='{p}'\n"));

        private const string Assinatura = "Signer #1 certificate DN: CN=Laboratorio do Paralelepipedo, O=NumbERS, C=BR\n";

        private static readonly string[] ArquivosLimpos =
        {
            "lib/arm64-v8a/libil2cpp.so", "lib/arm64-v8a/libunity.so",
            "assets/bin/Data/Managed/Metadata/global-metadata.dat", "classes.dex",
        };

        private const string ManifestoAceito =
            "E: manifest\n  A: http://schemas.android.com/apk/res/android:installLocation(0x010102b7)=0\n" +
            "    E: activity\n      A: http://schemas.android.com/apk/res/android:screenOrientation(0x0101001e)=0\n";

        private static RelatorioDoApk Limpo() => RelatorioDoApk.Ler(
            Badging($"{Pacote}.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION"), Assinatura, ArquivosLimpos, ManifestoAceito);

        [TestCase("installLocation(0x010102b7)=2", "screenOrientation(0x0101001e)=0", "installLocation")]
        [TestCase("installLocation(0x010102b7)=0", "screenOrientation(0x0101001e)=13", "paisagem")]
        [TestCase("", "screenOrientation(0x0101001e)=0", "installLocation")]
        public void ExigenciasDaMetaNoManifesto(string local, string orientacao, string problema)
        {
            string arvore = $"A: http://schemas.android.com/apk/res/android:{local}\nA: http://schemas.android.com/apk/res/android:{orientacao}\n";
            var relatorio = RelatorioDoApk.Ler(Badging(), Assinatura, ArquivosLimpos, arvore);

            Assert.That(relatorio.Problemas(Pacote, VersionCode), Has.Some.Contains(problema));
        }

        [Test]
        public void PerfilMetaQuest_PaisagemEInstalacaoAuto()
        {
            Assert.That(UnityEditor.PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UnityEditor.UIOrientation.LandscapeLeft));
            Assert.That(UnityEditor.PlayerSettings.Android.preferredInstallLocation,
                Is.EqualTo(UnityEditor.AndroidPreferredInstallLocation.Auto));
        }

        [Test]
        public void ApkLimpo_NaoTemProblemas()
        {
            var relatorio = Limpo();

            Assert.That(relatorio.Problemas(Pacote, VersionCode), Is.Empty);
            Assert.That(relatorio.TargetSdk, Is.EqualTo(34));
            Assert.That(relatorio.Assinante, Does.StartWith("CN=Laboratorio"));
        }

        [Test]
        public void PermissaoDeInternet_Reprovada()
        {
            var relatorio = RelatorioDoApk.Ler(Badging("android.permission.INTERNET"), Assinatura, ArquivosLimpos);

            Assert.That(relatorio.Problemas(Pacote, VersionCode),
                Has.Some.Contains("Permissão proibida (privacidade): android.permission.INTERNET"));
        }

        [Test]
        public void PermissaoNova_PrecisaDeRevisao()
        {
            var relatorio = RelatorioDoApk.Ler(Badging("android.permission.VIBRATE"), Assinatura, ArquivosLimpos);

            Assert.That(relatorio.Problemas(Pacote, VersionCode), Has.Some.Contains("fora da lista aceita"));
        }

        [Test]
        public void AgentBridgeNoCodigo_Reprovado()
        {
            var relatorio = RelatorioDoApk.Ler(Badging(), Assinatura, ArquivosLimpos,
                codigoProibido: new[] { "Meta.XR.AI.AgentBridge" });

            Assert.That(relatorio.Problemas(Pacote, VersionCode), Has.Some.Contains("Meta.XR.AI.AgentBridge"));
        }

        [Test]
        public void TelemetriaE32Bits_Reprovados()
        {
            var arquivos = ArquivosLimpos.Concat(new[]
                { "lib/arm64-v8a/libISDKEngineTelemetry.so", "assets/lib/armeabi-v7a/libx.so" });
            var relatorio = RelatorioDoApk.Ler(Badging(), Assinatura, arquivos,
                "E: meta-data\n A: android:name=\"com.oculus.telemetry.project_guid\"", new[] { "Lcom/google/firebase/" });

            var problemas = relatorio.Problemas(Pacote, VersionCode);

            Assert.That(problemas, Has.Some.Contains("libISDKEngineTelemetry.so"));
            Assert.That(problemas, Has.Some.Contains("32 bits"));
            Assert.That(problemas, Has.Some.Contains("com.oculus.telemetry.project_guid"));
            Assert.That(problemas, Has.Some.Contains("Lcom/google/firebase/"));
        }

        [Test]
        public void AssinaturaDeDebug_PacoteErrado_EDepuravel_Reprovados()
        {
            string badging = Badging().Replace(Pacote, "com.exemplo") + "application-debuggable\n";
            var relatorio = RelatorioDoApk.Ler(badging, "Signer #1 certificate DN: C=US, O=Android, CN=Android Debug",
                ArquivosLimpos);

            var problemas = relatorio.Problemas(Pacote, VersionCode);

            Assert.That(problemas, Has.Some.Contains("certificado de debug"));
            Assert.That(problemas, Has.Some.Contains("Pacote 'com.exemplo'"));
            Assert.That(problemas, Has.Some.Contains("depurável"));
        }

        [Test]
        public void SemArm64_Reprovado()
        {
            var relatorio = RelatorioDoApk.Ler(Badging(), Assinatura, new[] { "classes.dex" });

            Assert.That(relatorio.Problemas(Pacote, VersionCode), Has.Some.Contains("arm64-v8a"));
        }

        [Test]
        public void Manifesto_RemoveTodasAsPermissoesProibidas()
        {
            string manifesto = File.ReadAllText("Assets/Plugins/Android/AndroidManifest.xml");

            foreach (string permissao in RegrasDePrivacidade.PermissoesProibidas)
                Assert.That(manifesto, Does.Match($"<uses-permission android:name=\"{Regex.Escape(permissao)}\" tools:node=\"remove\" />"),
                    $"o manifesto precisa remover {permissao}");
            Assert.That(manifesto, Does.Contain("\"com.oculus.telemetry.project_guid\" tools:node=\"remove\""));
        }

        [Test]
        public void Manifesto_NaoPedeNenhumaPermissao()
        {
            string manifesto = File.ReadAllText("Assets/Plugins/Android/AndroidManifest.xml");
            var pedidas = Regex.Matches(manifesto, "<uses-permission [^>]*>").Cast<Match>()
                .Where(m => !m.Value.Contains("tools:node=\"remove\"")).Select(m => m.Value);

            Assert.That(pedidas, Is.Empty);
        }

        [Test]
        public void ProjetoNaoTemPacotesProibidos_ESoOCoreDaMeta()
        {
            string manifesto = File.ReadAllText("Packages/manifest.json");

            foreach (string pacote in RegrasDePrivacidade.PacotesProibidos)
                Assert.That(manifesto, Does.Not.Contain($"\"{pacote}\""), $"pacote proibido: {pacote}");
            Assert.That(Regex.Matches(manifesto, "\"com\\.meta\\.").Count, Is.EqualTo(1), "só o com.meta.xr.sdk.core");
        }

        [TestCase("Library/Bee/meta.xr.ai.agentbridge.dll", true)]
        [TestCase("META.XR.AI.AGENTBRIDGE", true)]
        [TestCase("Meta.XR.ImmersiveDebugger.DevAgent.dll", true)]
        [TestCase("Oculus.VR.dll", false)]
        [TestCase("LaboratorioParalelepipedo.Apresentacao.dll", false)]
        public void FiltroDeAssemblies_TiraOAgentBridge(string assembly, bool foraDoBuild)
        {
            Assert.That(FiltroDeAssemblies.EhForaDoBuild(assembly), Is.EqualTo(foraDoBuild));
        }

        [Test]
        public void FiltroDeAssemblies_MantemOsOutros()
        {
            var filtro = new FiltroDeAssemblies();

            string[] mantidos = filtro.OnFilterAssemblies(UnityEditor.BuildOptions.None,
                new[] { "a/Oculus.VR.dll", "a/meta.xr.ai.agentbridge.dll", "a/meta.xr.ai.agentbridge.telemetry.dll" });

            Assert.That(mantidos, Is.EqualTo(new[] { "a/Oculus.VR.dll" }));
        }

        [TestCase("\0Meta.XR.AI.AgentBridge\0", 1)]
        [TestCase("x\0Meta.XR.AI.AgentBridge.Sse\0", 2)]
        [TestCase("attribute\0DMeta.XR.AI.AgentBridge\0", -1)]
        [TestCase("\0Meta.XR.AI.AgentBridgeX\0", -1)]
        [TestCase("\0Oculus.VR\0", -1)]
        public void MetadadosDoIl2cpp_SoContamNomesDeCodigo(string metadados, int esperado)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(metadados);

            Assert.That(InspetorDoApk.IndiceDeNomeDeCodigo(bytes, System.Text.Encoding.ASCII.GetBytes("Meta.XR.AI.AgentBridge")),
                Is.EqualTo(esperado));
        }

        private const string Inicializadores =
            "{\"root\":[{\"assemblyName\":\"Meta.XR.ImmersiveDebugger.DevAgent\",\"nameSpace\":\"N\",\"className\":\"RuntimeSettings\",\"methodName\":\"Init\",\"loadTypes\":4,\"isUnityClass\":false}," +
            "{\"assemblyName\":\"Unity.InputSystem\",\"nameSpace\":\"UnityEngine.InputSystem\",\"className\":\"InputSystem\",\"methodName\":\"RuntimeInitialize\",\"loadTypes\":1,\"isUnityClass\":true}]}";

        [Test]
        public void Inicializadores_SaemOsDosAssemblysForaDoBuild_EOsOutrosFicamIguais()
        {
            string limpo = InicializadoresDoBuild.Limpar(Inicializadores, FiltroDeAssemblies.EhForaDoBuild, out int removidos);

            Assert.That(removidos, Is.EqualTo(1));
            Assert.That(limpo, Does.Not.Contain("DevAgent"));
            Assert.That(limpo, Does.Contain("\"assemblyName\":\"Unity.InputSystem\""));
            Assert.That(limpo, Does.Contain("\"methodName\":\"RuntimeInitialize\",\"loadTypes\":1,\"isUnityClass\":true"));
        }

        [Test]
        public void Inicializadores_OrfaosSaoOsQueNaoEstaoNoApp()
        {
            const string assemblies = "{\"names\":[\"Unity.InputSystem.dll\",\"Oculus.VR.dll\"],\"types\":[16,16]}";

            Assert.That(InicializadoresDoBuild.Orfaos(Inicializadores, assemblies),
                Is.EqualTo(new[] { "Meta.XR.ImmersiveDebugger.DevAgent" }));
            string limpo = InicializadoresDoBuild.Limpar(Inicializadores, FiltroDeAssemblies.EhForaDoBuild, out _);
            Assert.That(InicializadoresDoBuild.Orfaos(limpo, assemblies), Is.Empty);
        }

        [Test]
        public void InicializadorOrfao_ReprovaOApk()
        {
            var relatorio = Limpo();
            relatorio.InicializadoresOrfaos.Add("Meta.XR.BuildingBlocks.AIBlocks");

            Assert.That(relatorio.Problemas(Pacote, VersionCode), Has.Some.Contains("Meta.XR.BuildingBlocks.AIBlocks"));
        }

        [Test]
        public void VersionCode_PorDataEHoraUtc()
        {
            Assert.That(VersaoDoBuild.VersionCodePara(new DateTime(2026, 11, 8, 15, 30, 0, DateTimeKind.Utc)),
                Is.EqualTo(263121530));
            Assert.That(VersaoDoBuild.VersionCodePara(new DateTime(2099, 12, 31, 23, 59, 0, DateTimeKind.Utc)),
                Is.LessThan(VersaoDoBuild.MaiorVersionCode));
        }

        [Test]
        public void DeployConf_LeChaveValor()
        {
            var valores = ConfiguracaoDoEnvio.LerChaveValor(
                "# comentário\nKEYSTORE_PATH=~/x.keystore\n\nKEY_ALIAS = \"lab\"\nAGE_GROUP='TEENS_AND_ADULTS'\nsemigual\n");

            Assert.That(valores["KEYSTORE_PATH"], Is.EqualTo("~/x.keystore"));
            Assert.That(valores["KEY_ALIAS"], Is.EqualTo("lab"));
            Assert.That(valores["AGE_GROUP"], Is.EqualTo("TEENS_AND_ADULTS"));
            Assert.That(valores.Count, Is.EqualTo(3));
        }

        [Test]
        public void SegredosTemNomesProprios_NaoOsDoMissaoCoracao()
        {
            Assert.That(ConfiguracaoDoEnvio.VariavelDoAppSecret, Is.Not.EqualTo("META_APP_SECRET"));
            Assert.That(ConfiguracaoDoEnvio.ItemDoKeychainDoAppSecret, Is.Not.EqualTo("META_APP_SECRET"));
            Assert.That(ConfiguracaoDoEnvio.AppId, Is.EqualTo("1398124376723979"));
        }

        [Test]
        public void GitIgnoreBloqueiaKeystoreEDeployConf()
        {
            string gitignore = File.ReadAllText(".gitignore");

            foreach (string padrao in new[] { "*.keystore", "*.jks", "*.p12", "deploy.conf", "*.apk" })
                Assert.That(gitignore, Does.Contain(padrao));
        }
    }
}
