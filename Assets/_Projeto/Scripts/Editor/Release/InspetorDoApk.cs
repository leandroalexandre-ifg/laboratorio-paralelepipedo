using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// Abre o APK com as ferramentas do Android SDK que vêm com o Unity (aapt2 e apksigner), procura classes
    /// proibidas nos .dex e nomes proibidos nos metadados do IL2CPP, e monta o <see cref="RelatorioDoApk"/>.
    /// </summary>
    public static class InspetorDoApk
    {
        private const string MetadadosDoIl2cpp = "assets/bin/Data/Managed/Metadata/global-metadata.dat";

        public static RelatorioDoApk Inspecionar(string caminhoDoApk, out string erro)
        {
            string android = BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, BuildOptions.None);
            string buildTools = BuildToolsMaisNovo(Path.Combine(android, "SDK", "build-tools"));
            if (buildTools == null)
            {
                erro = $"build-tools do Android SDK não encontrado em {android}/SDK.";
                return null;
            }

            string javaHome = Path.Combine(android, "OpenJDK");
            var ambiente = new Dictionary<string, string>
            {
                ["JAVA_HOME"] = javaHome,
                ["PATH"] = Path.Combine(javaHome, "bin") + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH"),
            };

            var badging = ProcessoExterno.Rodar(Path.Combine(buildTools, "aapt2"),
                new[] { "dump", "badging", caminhoDoApk }, ambiente, null);
            if (badging.CodigoDeSaida != 0)
            {
                erro = $"aapt2 falhou ({badging.CodigoDeSaida}):\n{badging.Saida}";
                return null;
            }

            var assinatura = ProcessoExterno.Rodar(Path.Combine(buildTools, "apksigner"),
                new[] { "verify", "--print-certs", caminhoDoApk }, ambiente, null);
            if (assinatura.CodigoDeSaida != 0)
            {
                erro = $"apksigner: assinatura inválida ({assinatura.CodigoDeSaida}):\n{assinatura.Saida}";
                return null;
            }

            var manifesto = ProcessoExterno.Rodar(Path.Combine(buildTools, "aapt2"),
                new[] { "dump", "xmltree", "--file", "AndroidManifest.xml", caminhoDoApk }, ambiente, null);
            if (manifesto.CodigoDeSaida != 0)
            {
                erro = $"aapt2 não conseguiu ler o manifesto ({manifesto.CodigoDeSaida}):\n{manifesto.Saida}";
                return null;
            }

            var arquivos = new List<string>();
            var classes = new List<string>();
            var codigo = new List<string>();
            using (FileStream fluxo = File.OpenRead(caminhoDoApk))
            using (var apk = new ZipArchive(fluxo, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry item in apk.Entries)
                {
                    arquivos.Add(item.FullName);
                    if (item.FullName.StartsWith("classes", StringComparison.Ordinal) &&
                        item.FullName.EndsWith(".dex", StringComparison.Ordinal))
                        Procurar(item, RegrasDePrivacidade.PrefixosDeClassesProibidas, classes, IndiceDe);
                    else if (item.FullName == MetadadosDoIl2cpp)
                        Procurar(item, RegrasDePrivacidade.NomesProibidosNoCodigo, codigo, IndiceDeNomeDeCodigo);
                }
            }

            if (!arquivos.Contains(MetadadosDoIl2cpp))
            {
                erro = $"{MetadadosDoIl2cpp} não está no APK: o build é IL2CPP?";
                return null;
            }

            erro = null;
            return RelatorioDoApk.Ler(badging.Saida, assinatura.Saida, arquivos, manifesto.Saida, classes, codigo);
        }

        // Nomes de classes e tipos ficam como texto (ASCII) nas tabelas de strings: basta procurar os bytes.
        private static void Procurar(ZipArchiveEntry item, IEnumerable<string> textos, List<string> achados,
            Func<byte[], byte[], int> buscar)
        {
            byte[] bytes;
            using (Stream fluxo = item.Open())
            using (var memoria = new MemoryStream())
            {
                fluxo.CopyTo(memoria);
                bytes = memoria.ToArray();
            }

            foreach (string texto in textos)
                if (!achados.Contains(texto) && buscar(bytes, Encoding.ASCII.GetBytes(texto)) >= 0)
                    achados.Add(texto);
        }

        /// <summary>
        /// Posição de <paramref name="nome"/> como namespace ou tipo na tabela de strings do global-metadata.dat
        /// (texto entre bytes zero, ou seguido de "." quando é parte de um namespace maior), ou -1. Ignora o mesmo texto
        /// dentro de blobs de atributos, como o <c>[InternalsVisibleTo("Meta.XR.ImmersiveDebugger.DevAgent")]</c> de um
        /// assembly que fica no APK: isso é só o nome de um "amigo", não código dele.
        /// </summary>
        public static int IndiceDeNomeDeCodigo(byte[] metadados, byte[] nome)
        {
            var comZero = new byte[nome.Length + 1];
            Array.Copy(nome, 0, comZero, 1, nome.Length);
            int inicio = 0;
            while (true)
            {
                int i = IndiceDe(metadados, comZero, inicio);
                if (i < 0) return -1;
                int depois = i + comZero.Length;
                if (depois >= metadados.Length || metadados[depois] == 0 || metadados[depois] == (byte)'.') return i + 1;
                inicio = i + 1;
            }
        }

        /// <summary>Posição da primeira ocorrência de <paramref name="agulha"/> em <paramref name="palheiro"/>, ou -1.</summary>
        public static int IndiceDe(byte[] palheiro, byte[] agulha) => IndiceDe(palheiro, agulha, 0);

        private static int IndiceDe(byte[] palheiro, byte[] agulha, int inicio)
        {
            for (int i = inicio; i <= palheiro.Length - agulha.Length; i++)
            {
                int k = 0;
                while (k < agulha.Length && palheiro[i + k] == agulha[k]) k++;
                if (k == agulha.Length) return i;
            }

            return -1;
        }

        private static string BuildToolsMaisNovo(string pasta)
        {
            if (!Directory.Exists(pasta)) return null;
            string maisNovo = null;
            Version versaoMaisNova = null;
            foreach (string subpasta in Directory.GetDirectories(pasta))
            {
                if (!Version.TryParse(Path.GetFileName(subpasta), out Version versao)) continue;
                if (versaoMaisNova != null && versao <= versaoMaisNova) continue;
                maisNovo = subpasta;
                versaoMaisNova = versao;
            }

            return maisNovo;
        }
    }
}
