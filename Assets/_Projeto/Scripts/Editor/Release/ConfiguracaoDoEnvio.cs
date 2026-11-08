using System;
using System.Collections.Generic;
using System.IO;

namespace LaboratorioParalelepipedo.Editor.Release
{
    /// <summary>
    /// Configuração do envio, toda fora do projeto (nada disto vai para o git), no padrão do Missão Coração:
    /// <list type="bullet">
    /// <item><c>~/.laboratorioparalelepipedo/deploy.conf</c> (CHAVE=valor, sem segredos): KEYSTORE_PATH,
    /// KEY_ALIAS, AGE_GROUP e OVR_PLATFORM_UTIL (opcional).</item>
    /// <item>Segredos em variáveis de ambiente, ou no Keychain do macOS se a variável não existir:
    /// <see cref="VariavelDaSenhaDoKeystore"/> e <see cref="VariavelDoAppSecret"/>. Os nomes são próprios deste
    /// app, para nunca pegar por engano o App Secret ou o keystore do Missão Coração.</item>
    /// </list>
    /// Passo a passo em Docs/DEPLOY.md.
    /// </summary>
    public sealed class ConfiguracaoDoEnvio
    {
        /// <summary>App ID do Laboratório do Paralelepípedo no Meta Horizon Developer Dashboard (não é segredo).</summary>
        public const string AppId = "1398124376723979";

        public const string VariavelDaSenhaDoKeystore = "LABORATORIO_KEYSTORE_PASS";
        public const string VariavelDoAppSecret = "LABORATORIO_META_APP_SECRET";
        public const string ItemDoKeychainDoKeystore = "laboratorioparalelepipedo-keystore";
        public const string ItemDoKeychainDoAppSecret = "laboratorioparalelepipedo-app-secret";

        /// <summary>Faixas etárias que o ovr-platform-util aceita em --age-group.</summary>
        public static readonly string[] FaixasEtarias = { "CHILDREN", "TEENS_AND_ADULTS", "MIXED_AGES" };

        public static string Pasta =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".laboratorioparalelepipedo");

        public static string CaminhoDoArquivo => Path.Combine(Pasta, "deploy.conf");

        public string CaminhoDoKeystore { get; private set; }
        public string AliasDaChave { get; private set; }
        public string FaixaEtaria { get; private set; }
        public string CaminhoDoOvrPlatformUtil { get; private set; }

        /// <summary>Lê o deploy.conf. Devolve null e preenche <paramref name="erro"/> se faltar algo.</summary>
        public static ConfiguracaoDoEnvio Carregar(bool paraEnviar, out string erro)
        {
            if (!File.Exists(CaminhoDoArquivo))
            {
                erro = $"Configuração não encontrada: {CaminhoDoArquivo}. Veja Docs/DEPLOY.md, \"Configuração única\".";
                return null;
            }

            var valores = LerChaveValor(File.ReadAllText(CaminhoDoArquivo));
            var configuracao = new ConfiguracaoDoEnvio
            {
                CaminhoDoKeystore = ExpandirHome(Valor(valores, "KEYSTORE_PATH")),
                AliasDaChave = Valor(valores, "KEY_ALIAS"),
                FaixaEtaria = Valor(valores, "AGE_GROUP"),
                CaminhoDoOvrPlatformUtil = ExpandirHome(Valor(valores, "OVR_PLATFORM_UTIL") ??
                                                        "~/.laboratorioparalelepipedo/bin/ovr-platform-util"),
            };

            var faltando = new List<string>();
            if (string.IsNullOrEmpty(configuracao.CaminhoDoKeystore)) faltando.Add("KEYSTORE_PATH");
            if (string.IsNullOrEmpty(configuracao.AliasDaChave)) faltando.Add("KEY_ALIAS");
            if (paraEnviar && string.IsNullOrEmpty(configuracao.FaixaEtaria)) faltando.Add("AGE_GROUP");
            if (faltando.Count > 0)
            {
                erro = $"Faltam no {CaminhoDoArquivo}: {string.Join(", ", faltando)}.";
                return null;
            }

            if (paraEnviar && Array.IndexOf(FaixasEtarias, configuracao.FaixaEtaria) < 0)
            {
                erro = $"AGE_GROUP \"{configuracao.FaixaEtaria}\" inválido; use {string.Join(", ", FaixasEtarias)}.";
                return null;
            }

            if (!File.Exists(configuracao.CaminhoDoKeystore))
            {
                erro = $"Keystore não encontrado em {configuracao.CaminhoDoKeystore}. Restaure do backup (Docs/DEPLOY.md): " +
                       "sem ele não dá para atualizar o app na loja.";
                return null;
            }

            if (paraEnviar && !File.Exists(configuracao.CaminhoDoOvrPlatformUtil))
            {
                erro = $"ovr-platform-util não encontrado em {configuracao.CaminhoDoOvrPlatformUtil}.";
                return null;
            }

            erro = null;
            return configuracao;
        }

        /// <summary>
        /// Lê um segredo: primeiro da variável de ambiente, depois do Keychain do macOS. Nunca grava nem mostra o
        /// valor no log.
        /// </summary>
        public static string LerSegredo(string variavel, string itemDoKeychain, out string erro)
        {
            string valor = Environment.GetEnvironmentVariable(variavel);
            if (!string.IsNullOrEmpty(valor))
            {
                erro = null;
                return valor;
            }

            if (File.Exists("/usr/bin/security"))
            {
                var resultado = ProcessoExterno.Rodar("/usr/bin/security",
                    new[] { "find-generic-password", "-s", itemDoKeychain, "-w" }, null, Array.Empty<string>());
                if (resultado.CodigoDeSaida == 0 && resultado.Saida.Trim().Length > 0)
                {
                    erro = null;
                    return resultado.Saida.Trim();
                }
            }

            erro = $"Segredo não encontrado: defina a variável {variavel} ou guarde no Keychain com o serviço " +
                   $"\"{itemDoKeychain}\" (Docs/DEPLOY.md).";
            return null;
        }

        /// <summary>Linhas CHAVE=valor; ignora linhas vazias e comentários (#). Tira aspas em volta do valor.</summary>
        public static Dictionary<string, string> LerChaveValor(string texto)
        {
            var valores = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string bruta in texto.Split('\n'))
            {
                string linha = bruta.Trim();
                if (linha.Length == 0 || linha.StartsWith("#", StringComparison.Ordinal)) continue;
                int igual = linha.IndexOf('=');
                if (igual <= 0) continue;
                string valor = linha.Substring(igual + 1).Trim();
                if (valor.Length >= 2 && (valor[0] == '"' && valor[valor.Length - 1] == '"' ||
                                          valor[0] == '\'' && valor[valor.Length - 1] == '\''))
                    valor = valor.Substring(1, valor.Length - 2);
                valores[linha.Substring(0, igual).Trim()] = valor;
            }

            return valores;
        }

        private static string Valor(Dictionary<string, string> valores, string chave) =>
            valores.TryGetValue(chave, out string valor) && valor.Length > 0 ? valor : null;

        private static string ExpandirHome(string caminho) =>
            caminho != null && caminho.StartsWith("~", StringComparison.Ordinal)
                ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + caminho.Substring(1)
                : caminho;
    }
}
