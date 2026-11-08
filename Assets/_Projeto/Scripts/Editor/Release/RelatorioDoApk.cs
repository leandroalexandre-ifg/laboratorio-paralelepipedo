using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// O que interessa do APK para o envio à loja, lido da saída do <c>aapt2 dump badging</c>, do
    /// <c>apksigner verify --print-certs</c>, da lista de arquivos e das buscas nos .dex e nos metadados do IL2CPP.
    /// A análise é pura (só texto), coberta por testes EditMode.
    /// </summary>
    public sealed class RelatorioDoApk
    {
        public string Pacote { get; private set; }
        public int VersionCode { get; private set; }
        public string VersionName { get; private set; }
        public int TargetSdk { get; private set; }
        public bool Depuravel { get; private set; }
        public List<string> Permissoes { get; } = new List<string>();
        public string Assinante { get; private set; }
        public List<string> Arquivos { get; } = new List<string>();

        /// <summary>Saída do <c>aapt2 dump xmltree</c> do AndroidManifest.xml compilado (para achar meta-data).</summary>
        public string ArvoreDoManifesto { get; private set; } = string.Empty;

        /// <summary>Prefixos de <see cref="RegrasDePrivacidade.PrefixosDeClassesProibidas"/> achados nos .dex.</summary>
        public List<string> ClassesProibidas { get; } = new List<string>();

        /// <summary>Nomes de <see cref="RegrasDePrivacidade.NomesProibidosNoCodigo"/> achados no global-metadata.dat.</summary>
        public List<string> CodigoProibido { get; } = new List<string>();

        private static readonly Regex LinhaDoPacote =
            new Regex(@"^package: name='([^']*)' versionCode='(\d+)' versionName='([^']*)'");
        private static readonly Regex LinhaDePermissao = new Regex(@"^uses-permission: name='([^']*)'");
        private static readonly Regex LinhaDoTargetSdk = new Regex(@"^targetSdkVersion:'(\d+)'");
        private static readonly Regex LinhaDoAssinante = new Regex(@"^Signer #1 certificate DN: (.+)$");

        public static RelatorioDoApk Ler(string badging, string assinatura, IEnumerable<string> arquivos,
            string arvoreDoManifesto = "", IEnumerable<string> classesProibidas = null,
            IEnumerable<string> codigoProibido = null)
        {
            var relatorio = new RelatorioDoApk();
            foreach (string bruta in badging.Split('\n'))
            {
                string linha = bruta.TrimEnd('\r');
                Match m;
                if ((m = LinhaDoPacote.Match(linha)).Success)
                {
                    relatorio.Pacote = m.Groups[1].Value;
                    relatorio.VersionCode = int.Parse(m.Groups[2].Value);
                    relatorio.VersionName = m.Groups[3].Value;
                }
                else if ((m = LinhaDePermissao.Match(linha)).Success) relatorio.Permissoes.Add(m.Groups[1].Value);
                else if ((m = LinhaDoTargetSdk.Match(linha)).Success) relatorio.TargetSdk = int.Parse(m.Groups[1].Value);
                else if (linha.Trim() == "application-debuggable") relatorio.Depuravel = true;
            }

            foreach (string bruta in assinatura.Split('\n'))
            {
                Match m = LinhaDoAssinante.Match(bruta.TrimEnd('\r'));
                if (!m.Success) continue;
                relatorio.Assinante = m.Groups[1].Value.Trim();
                break;
            }

            relatorio.Arquivos.AddRange(arquivos);
            relatorio.ArvoreDoManifesto = arvoreDoManifesto ?? string.Empty;
            if (classesProibidas != null) relatorio.ClassesProibidas.AddRange(classesProibidas);
            if (codigoProibido != null) relatorio.CodigoProibido.AddRange(codigoProibido);
            return relatorio;
        }

        /// <summary>
        /// Lista de verificação do APK. Cada item é um motivo para não enviar; lista vazia = pode enviar.
        /// </summary>
        public List<string> Problemas(string pacoteEsperado, int versionCodeEsperado)
        {
            var problemas = new List<string>();

            if (Pacote != pacoteEsperado) problemas.Add($"Pacote '{Pacote}', esperado '{pacoteEsperado}'.");
            if (VersionCode != versionCodeEsperado)
                problemas.Add($"versionCode {VersionCode}, esperado {versionCodeEsperado}.");
            if (Depuravel) problemas.Add("O APK é depurável (application-debuggable): a loja recusa.");
            if (string.IsNullOrEmpty(Assinante)) problemas.Add("APK sem assinatura.");
            else if (Assinante.IndexOf("Android Debug", StringComparison.OrdinalIgnoreCase) >= 0)
                problemas.Add($"Assinado com o certificado de debug ({Assinante}): a loja recusa.");

            var aceitas = new HashSet<string>();
            foreach (string permissao in RegrasDePrivacidade.PermissoesAceitas)
                aceitas.Add(permissao.Replace("{package}", pacoteEsperado));
            foreach (string permissao in Permissoes)
            {
                if (Array.IndexOf(RegrasDePrivacidade.PermissoesProibidas, permissao) >= 0)
                    problemas.Add($"Permissão proibida (privacidade): {permissao}.");
                else if (!aceitas.Contains(permissao))
                    problemas.Add($"Permissão fora da lista aceita: {permissao}. Revise e, se for o caso, inclua em RegrasDePrivacidade.");
            }

            foreach (string metaData in RegrasDePrivacidade.MetaDataProibida)
                if (ArvoreDoManifesto.Contains($"\"{metaData}\""))
                    problemas.Add($"Meta-data proibida no manifesto (telemetria): {metaData}.");

            foreach (string prefixo in ClassesProibidas)
                problemas.Add($"Classes proibidas no APK (ID de publicidade/diagnóstico): {prefixo}.");
            foreach (string nome in CodigoProibido)
                problemas.Add($"Código de rede/IA do SDK no APK: {nome}. Confira o FiltroDeAssemblies.");

            bool temArm64 = false;
            foreach (string arquivo in Arquivos)
            {
                temArm64 |= arquivo.StartsWith("lib/arm64-v8a/", StringComparison.Ordinal);
                foreach (string pasta in RegrasDePrivacidade.PastasDeBibliotecasProibidas)
                    if (arquivo.StartsWith(pasta, StringComparison.Ordinal))
                        problemas.Add($"Biblioteca de 32 bits ou x86 (o Quest usa só arm64-v8a): {arquivo}.");
                foreach (string biblioteca in RegrasDePrivacidade.BibliotecasNativasProibidas)
                    if (arquivo.EndsWith("/" + biblioteca, StringComparison.Ordinal))
                        problemas.Add($"Biblioteca de telemetria no APK: {arquivo}.");
            }

            if (!temArm64) problemas.Add("Nenhuma biblioteca em lib/arm64-v8a: o Quest exige 64 bits.");
            return problemas;
        }
    }
}
