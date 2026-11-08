using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>Converte o conteúdo de textos.json em <see cref="CatalogoDeTextos"/>, conferindo os dados.</summary>
    public static class LeitorDeTextos
    {
        public const int VersaoSuportada = 1;

        /// <exception cref="DadosInvalidosException">Com todos os erros do arquivo, se houver algum.</exception>
        public static CatalogoDeTextos Ler(ArquivoDeTextosDto arquivo, string nomeDoArquivo = "textos.json")
        {
            if (arquivo == null)
                throw new DadosInvalidosException(nomeDoArquivo, "o arquivo está vazio.");

            var erros = new List<string>();
            if (arquivo.versao != VersaoSuportada)
                erros.Add($"\"versao\" deve ser {VersaoSuportada} (veio {arquivo.versao}).");

            var textos = new Dictionary<string, string>();
            var posicaoDaChave = new Dictionary<string, int>();
            if (arquivo.textos == null || arquivo.textos.Length == 0)
                erros.Add("a lista \"textos\" está vazia ou faltando.");
            else
                for (int i = 0; i < arquivo.textos.Length; i++)
                {
                    int posicao = i + 1;
                    TextoDto item = arquivo.textos[i];
                    string chave = item?.chave?.Trim();
                    if (string.IsNullOrEmpty(chave))
                    {
                        erros.Add($"texto na posição {posicao}: falta a \"chave\".");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(item.texto))
                        erros.Add($"chave \"{chave}\": o \"texto\" está vazio.");
                    else if (posicaoDaChave.TryGetValue(chave, out int anterior))
                        erros.Add($"chave \"{chave}\" repetida (posições {anterior} e {posicao}).");
                    else
                    {
                        textos[chave] = item.texto;
                        posicaoDaChave[chave] = posicao;
                    }
                }

            if (erros.Count > 0) throw new DadosInvalidosException(nomeDoArquivo, erros);
            return new CatalogoDeTextos(textos);
        }
    }
}
