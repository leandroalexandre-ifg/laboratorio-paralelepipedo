using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// Build de release do Quest para o canal de teste ALPHA (adaptado do ReleasePipeline do Missão Coração):
    /// <list type="number">
    /// <item>lê a configuração local e os segredos (fora do projeto);</item>
    /// <item>ativa o perfil Meta Quest e aplica, só durante o build, o versionCode automático e a assinatura com
    /// o keystore próprio;</item>
    /// <item>gera Builds/LaboratorioParalelepipedo-&lt;versionCode&gt;.apk;</item>
    /// <item>devolve o perfil ao que estava (o git fica limpo; senhas nunca são gravadas);</item>
    /// <item>confere o APK (<see cref="RelatorioDoApk.Problemas"/>);</item>
    /// <item>conforme o modo, valida no servidor da Meta (--validate) e, depois de validar, envia ao ALPHA.</item>
    /// </list>
    /// </summary>
    public static class PipelineDeRelease
    {
        public const string Canal = "ALPHA";
        private const string Historico = "Builds/envios.txt";

        public enum Modo
        {
            /// <summary>Só gera e confere o APK assinado (não fala com a Meta).</summary>
            Gerar,
            /// <summary>Gera, confere e valida no servidor da Meta (--validate), sem enviar.</summary>
            Validar,
            /// <summary>Gera, confere, valida e envia ao ALPHA (no Editor, pede confirmação antes).</summary>
            Enviar,
        }

        /// <summary>Gera o APK de release, confere e, conforme o modo, valida e envia.</summary>
        public static bool Rodar(Modo modo)
        {
            bool falaComALoja = modo != Modo.Gerar;
            var configuracao = ConfiguracaoDoEnvio.Carregar(falaComALoja, out string erro);
            if (configuracao == null) return Falhar(erro);

            string senha = ConfiguracaoDoEnvio.LerSegredo(ConfiguracaoDoEnvio.VariavelDaSenhaDoKeystore,
                ConfiguracaoDoEnvio.ItemDoKeychainDoKeystore, out erro);
            if (senha == null) return Falhar(erro);

            BuildProfile perfil = BuildProfile.GetBuildProfileAtPath(ConfiguracaoDoProjeto.CaminhoDoPerfil);
            if (perfil == null) return Falhar($"Perfil não encontrado em '{ConfiguracaoDoProjeto.CaminhoDoPerfil}'.");

            int versionCode = VersaoDoBuild.Agora();
            string caminhoDoApk = $"Builds/LaboratorioParalelepipedo-{versionCode}.apk";
            string pacote;

            BuildProfile perfilAnterior = BuildProfile.GetActiveBuildProfile();
            if (perfilAnterior != perfil) BuildProfile.SetActiveBuildProfile(perfil);
            int versionCodeOriginal = PlayerSettings.Android.bundleVersionCode;
            bool keystoreProprioOriginal = PlayerSettings.Android.useCustomKeystore;
            string keystoreOriginal = PlayerSettings.Android.keystoreName;
            string aliasOriginal = PlayerSettings.Android.keyaliasName;
            bool gerou;
            try
            {
                pacote = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
                PlayerSettings.Android.bundleVersionCode = versionCode;
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = configuracao.CaminhoDoKeystore;
                PlayerSettings.Android.keystorePass = senha;
                PlayerSettings.Android.keyaliasName = configuracao.AliasDaChave;
                // Keystore PKCS12 (padrão do keytool): a senha da chave é a mesma do keystore.
                PlayerSettings.Android.keyaliasPass = senha;

                Debug.Log($"[Release] {pacote} {PlayerSettings.bundleVersion} (versionCode {versionCode}), " +
                          $"assinado com o alias '{configuracao.AliasDaChave}'.");
                gerou = ScriptDeBuild.GerarComPerfil(perfil, caminhoDoApk);
            }
            finally
            {
                PlayerSettings.Android.bundleVersionCode = versionCodeOriginal;
                PlayerSettings.Android.useCustomKeystore = keystoreProprioOriginal;
                PlayerSettings.Android.keystoreName = keystoreOriginal;
                PlayerSettings.Android.keyaliasName = aliasOriginal;
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
                if (perfilAnterior != perfil) BuildProfile.SetActiveBuildProfile(perfilAnterior);
                AssetDatabase.SaveAssets();
            }

            if (!gerou) return Falhar("O build de release falhou (veja os erros acima).");

            RelatorioDoApk relatorio = Conferir(caminhoDoApk, pacote, versionCode);
            if (relatorio == null) return false;
            if (modo == Modo.Gerar)
            {
                RegistrarNoHistorico(relatorio, $"gerado (sem envio): {caminhoDoApk}");
                return true;
            }

            return MandarParaALoja(configuracao, caminhoDoApk, relatorio, modo == Modo.Enviar);
        }

        /// <summary>
        /// Envia um APK já gerado e conferido, sem refazer o build: confere de novo, valida e envia. Assim vai para
        /// a loja exatamente o APK que foi testado.
        /// </summary>
        public static bool EnviarApkExistente(string caminhoDoApk)
        {
            if (string.IsNullOrEmpty(caminhoDoApk) || !File.Exists(caminhoDoApk))
                return Falhar($"APK não encontrado: '{caminhoDoApk}'.");
            var configuracao = ConfiguracaoDoEnvio.Carregar(true, out string erro);
            if (configuracao == null) return Falhar(erro);

            var primeiro = InspetorDoApk.Inspecionar(caminhoDoApk, out erro);
            if (primeiro == null) return Falhar(erro);
            var relatorio = Conferir(caminhoDoApk, ConfiguracaoDoProjeto.PackageName, primeiro.VersionCode);
            return relatorio != null && MandarParaALoja(configuracao, caminhoDoApk, relatorio, enviar: true);
        }

        /// <summary>Confere o APK e devolve o relatório, ou null (com o erro no log) se não puder ir para a loja.</summary>
        public static RelatorioDoApk Conferir(string caminhoDoApk, string pacote, int versionCode)
        {
            var relatorio = InspetorDoApk.Inspecionar(caminhoDoApk, out string erro);
            if (relatorio == null)
            {
                Falhar(erro);
                return null;
            }

            var problemas = relatorio.Problemas(pacote, versionCode);
            if (problemas.Count > 0)
            {
                Falhar($"O APK não está pronto para a loja:\n  - {string.Join("\n  - ", problemas)}");
                return null;
            }

            Debug.Log($"[Release] APK conferido: {caminhoDoApk} — assinado por \"{relatorio.Assinante}\", " +
                      $"versionCode {relatorio.VersionCode}, targetSdk {relatorio.TargetSdk}, " +
                      $"permissões: [{string.Join(", ", relatorio.Permissoes)}], sem AgentBridge nem telemetria.");
            return relatorio;
        }

        // Sempre valida na Meta primeiro; só envia se a validação passar e enviar for true.
        private static bool MandarParaALoja(ConfiguracaoDoEnvio configuracao, string caminhoDoApk,
            RelatorioDoApk relatorio, bool enviar)
        {
            string appSecret = ConfiguracaoDoEnvio.LerSegredo(ConfiguracaoDoEnvio.VariavelDoAppSecret,
                ConfiguracaoDoEnvio.ItemDoKeychainDoAppSecret, out string erro);
            if (appSecret == null) return Falhar(erro);

            if (!RodarOvrPlatformUtil(configuracao, appSecret, caminhoDoApk, relatorio, soValidar: true))
            {
                RegistrarNoHistorico(relatorio, "FALHOU na validação da Meta");
                return Falhar("A validação da Meta recusou o APK. Veja a saída acima e Docs/DEPLOY.md.");
            }

            Debug.Log($"[Release] Validação da Meta OK: {caminhoDoApk} (versionCode {relatorio.VersionCode}).");
            if (!enviar)
            {
                RegistrarNoHistorico(relatorio, $"validado (sem envio): {caminhoDoApk}");
                return true;
            }

            if (!Application.isBatchMode && !EditorUtility.DisplayDialog("Enviar para o canal ALPHA",
                    $"Enviar a versão {relatorio.VersionName} (versionCode {relatorio.VersionCode}) de " +
                    $"{relatorio.Pacote} para o canal {Canal}?", "Enviar", "Cancelar"))
            {
                Debug.Log("[Release] Envio cancelado. O APK validado ficou em " + caminhoDoApk);
                return true;
            }

            if (!RodarOvrPlatformUtil(configuracao, appSecret, caminhoDoApk, relatorio, soValidar: false))
            {
                RegistrarNoHistorico(relatorio, "FALHOU no envio");
                return Falhar("O envio falhou. Veja a saída acima e Docs/DEPLOY.md.");
            }

            RegistrarNoHistorico(relatorio, $"enviado para {Canal}");
            Debug.Log($"[Release] Enviado para o canal {Canal}: versionCode {relatorio.VersionCode}. " +
                      "No óculos, a atualização aparece na Biblioteca em alguns minutos.");
            return true;
        }

        private static bool RodarOvrPlatformUtil(ConfiguracaoDoEnvio configuracao, string appSecret,
            string caminhoDoApk, RelatorioDoApk relatorio, bool soValidar)
        {
            var argumentos = new List<string>
            {
                "upload-quest-build",
                "--app-id", ConfiguracaoDoEnvio.AppId,
                "--app-secret", appSecret,
                "--apk", Path.GetFullPath(caminhoDoApk),
                "--channel", Canal,
                "--age-group", configuracao.FaixaEtaria,
                "--notes", $"{ConfiguracaoDoProjeto.NomeDoProduto} {relatorio.VersionName} " +
                           $"(versionCode {relatorio.VersionCode}, commit {Commit()})",
                "--disable-progress-bar",
            };
            if (soValidar) argumentos.Add("--validate");

            string[] segredos = { appSecret };
            string acao = soValidar ? "Validando na Meta (sem enviar)" : "Enviando";
            Debug.Log($"[Release] {acao}: " +
                      ProcessoExterno.Descrever(configuracao.CaminhoDoOvrPlatformUtil, argumentos, segredos));
            var resultado = ProcessoExterno.Rodar(configuracao.CaminhoDoOvrPlatformUtil, argumentos, null, segredos);
            Debug.Log($"[Release] Saída do ovr-platform-util (código {resultado.CodigoDeSaida}):\n{resultado.Saida}");
            return resultado.CodigoDeSaida == 0;
        }

        // Commit atual (e "+alterações" se houver arquivos não commitados), para as notas do envio.
        private static string Commit()
        {
            try
            {
                var head = ProcessoExterno.Rodar("git", new[] { "rev-parse", "--short", "HEAD" }, null, null);
                var status = ProcessoExterno.Rodar("git", new[] { "status", "--porcelain" }, null, null);
                string commit = head.CodigoDeSaida == 0 ? head.Saida.Trim() : "desconhecido";
                return status.CodigoDeSaida == 0 && status.Saida.Trim().Length > 0 ? commit + "+alterações" : commit;
            }
            catch (Exception)
            {
                return "desconhecido";
            }
        }

        private static void RegistrarNoHistorico(RelatorioDoApk relatorio, string resultado)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Historico));
            File.AppendAllText(Historico, $"{DateTime.Now:yyyy-MM-dd HH:mm}\t{relatorio.VersionCode}\t" +
                                          $"{relatorio.VersionName}\t{Commit()}\t{resultado}\n");
        }

        private static bool Falhar(string mensagem)
        {
            Debug.LogError($"[Release] FALHA: {mensagem}");
            return false;
        }
    }
}
