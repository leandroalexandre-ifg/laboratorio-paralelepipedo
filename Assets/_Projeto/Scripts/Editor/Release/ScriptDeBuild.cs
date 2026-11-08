using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// Build do APK do Quest a partir do perfil "Meta Quest" (adaptado do BuildScript do Missão Coração).
    /// Batchmode (o Unity fechado neste projeto):
    /// <code>
    /// # Gera e confere o APK de release assinado, sem falar com a Meta
    /// -buildTarget Android -executeMethod LaboratorioParalelepipedo.Editor.Release.ScriptDeBuild.GerarRelease
    /// # Gera, confere e valida na Meta, sem enviar
    /// -buildTarget Android -executeMethod LaboratorioParalelepipedo.Editor.Release.ScriptDeBuild.ValidarAlpha
    /// # Gera, confere, valida e envia ao ALPHA
    /// -buildTarget Android -executeMethod LaboratorioParalelepipedo.Editor.Release.ScriptDeBuild.GerarEEnviarAlpha
    /// # Envia um APK já gerado e conferido (sem refazer o build)
    /// -buildTarget Android -executeMethod LaboratorioParalelepipedo.Editor.Release.ScriptDeBuild.EnviarApkAlpha -apk Builds/x.apk
    /// </code>
    /// Processo completo em Docs/DEPLOY.md.
    /// </summary>
    public static class ScriptDeBuild
    {
        /// <summary>Cenas esperadas no build, na ordem. O perfil usa a lista global; se não bater, o build falha.</summary>
        public static readonly string[] Cenas = { "Assets/_Projeto/Cenas/Laboratorio.unity" };

        [MenuItem("Laboratório/Release/Gerar APK de release (sem enviar)")]
        public static void GerarRelease() => SairSeFalhar(Conferido() && PipelineDeRelease.Rodar(PipelineDeRelease.Modo.Gerar));

        [MenuItem("Laboratório/Release/Validar na Meta (sem enviar)")]
        public static void ValidarAlpha() => SairSeFalhar(Conferido() && PipelineDeRelease.Rodar(PipelineDeRelease.Modo.Validar));

        [MenuItem("Laboratório/Release/Gerar e enviar ao ALPHA")]
        public static void GerarEEnviarAlpha() => SairSeFalhar(Conferido() && PipelineDeRelease.Rodar(PipelineDeRelease.Modo.Enviar));

        /// <summary>Envia o APK passado em -apk (só batchmode).</summary>
        public static void EnviarApkAlpha() => SairSeFalhar(PipelineDeRelease.EnviarApkExistente(Argumento("-apk")));

        /// <summary>Gera o APK com o perfil dado em <paramref name="caminhoDoApk"/>.</summary>
        internal static bool GerarComPerfil(BuildProfile perfil, string caminhoDoApk)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(caminhoDoApk));
            var opcoes = new BuildPlayerWithProfileOptions
            {
                buildProfile = perfil,
                locationPathName = caminhoDoApk,
                options = BuildOptions.None,
            };

            Debug.Log($"[Build] Iniciando build com o perfil '{perfil.name}' → {caminhoDoApk}");
            BuildReport relatorio = BuildPipeline.BuildPlayer(opcoes);
            BuildSummary resumo = relatorio.summary;
            if (resumo.result != BuildResult.Succeeded)
            {
                var erros = new StringBuilder();
                foreach (BuildStep passo in relatorio.steps)
                foreach (BuildStepMessage mensagem in passo.messages)
                    if (mensagem.type == LogType.Error || mensagem.type == LogType.Exception)
                        erros.AppendLine($"  [{passo.name}] {mensagem.content}");
                return Falhar($"Build terminou com {resumo.result} ({resumo.totalErrors} erro(s)).\n{erros}");
            }

            long bytes = new FileInfo(caminhoDoApk).Length;
            Debug.Log($"[Build] Build concluído: {caminhoDoApk} ({bytes / (1024f * 1024f):F1} MB, " +
                      $"{resumo.totalTime.TotalSeconds:F0} s, {resumo.totalWarnings} aviso(s)).");
            return true;
        }

        // Plataforma, perfil e cenas, antes de qualquer build.
        private static bool Conferido()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                return Falhar($"A plataforma ativa é {EditorUserBuildSettings.activeBuildTarget}, não Android. " +
                              "Use -buildTarget Android.");

            BuildProfile perfil = BuildProfile.GetBuildProfileAtPath(ConfiguracaoDoProjeto.CaminhoDoPerfil);
            if (perfil == null) return Falhar($"Perfil não encontrado em '{ConfiguracaoDoProjeto.CaminhoDoPerfil}'.");

            var ativas = new List<string>();
            foreach (EditorBuildSettingsScene cena in perfil.GetScenesForBuild())
                if (cena.enabled) ativas.Add(cena.path);
            if (string.Join("|", ativas) != string.Join("|", Cenas))
                return Falhar($"As cenas do build não batem. Esperado: [{string.Join(", ", Cenas)}]; " +
                              $"no perfil: [{string.Join(", ", ativas)}].");

            foreach (string cena in Cenas)
                if (AssetDatabase.AssetPathToGUID(cena, AssetPathToGUIDOptions.OnlyExistingAssets) == string.Empty)
                    return Falhar($"Cena não encontrada: '{cena}'.");
            return true;
        }

        private static string Argumento(string nome)
        {
            string[] argumentos = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < argumentos.Length - 1; i++)
                if (argumentos[i] == nome) return argumentos[i + 1];
            return null;
        }

        private static void SairSeFalhar(bool deuCerto)
        {
            if (Application.isBatchMode) EditorApplication.Exit(deuCerto ? 0 : 1);
        }

        private static bool Falhar(string mensagem)
        {
            Debug.LogError($"[Build] FALHA: {mensagem}");
            return false;
        }
    }
}
