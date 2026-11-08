using System;
using System.Collections.Generic;
using System.Reflection;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>
    /// Confere as chaves do JSON bruto, antes do JsonUtility (que ignora chaves desconhecidas sem avisar).
    /// Chave desconhecida vira erro com o nome da chave, o desafio (ou texto) e a chave válida mais parecida
    /// (ex.: "grandesa" → "grandeza?"). As chaves válidas vêm dos campos dos DTOs.
    /// </summary>
    public static class VerificadorDeChaves
    {
        private static readonly string[] ChavesDoArquivoDeDesafios = CamposDe(typeof(ArquivoDeDesafiosDto));
        private static readonly string[] ChavesDoDesafio = CamposDe(typeof(DesafioDto));
        private static readonly string[] ChavesDasDimensoes = CamposDe(typeof(DimensoesDto));
        private static readonly string[] ChavesDoArquivoDeTextos = CamposDe(typeof(ArquivoDeTextosDto));
        private static readonly string[] ChavesDoTexto = CamposDe(typeof(TextoDto));

        /// <summary>Chaves válidas de um desafio, na ordem dos campos do <see cref="DesafioDto"/>.</summary>
        public static IReadOnlyList<string> ChavesValidasDoDesafio => ChavesDoDesafio;

        /// <returns>Os erros de chave encontrados (lista vazia se estiver tudo certo).</returns>
        /// <exception cref="DadosInvalidosException">Se o JSON estiver mal formado.</exception>
        public static IReadOnlyList<string> VerificarDesafios(string json, string nomeDoArquivo = "desafios.json")
        {
            var raiz = Analisar(json, nomeDoArquivo);
            var erros = new List<string>();
            ConferirChaves(raiz, ChavesDoArquivoDeDesafios, "No início do arquivo", null, erros);

            var desafios = Membro(raiz, "desafios");
            if (desafios?.Tipo != TipoDeNoJson.Lista) return erros;
            for (int i = 0; i < desafios.Itens.Count; i++)
            {
                var desafio = desafios.Itens[i];
                if (desafio.Tipo != TipoDeNoJson.Objeto) continue;
                string id = desafio.TextoDe("id")?.Trim();
                string rotulo = string.IsNullOrEmpty(id) ? $"Desafio na posição {i + 1}" : $"Desafio \"{id}\"";
                ConferirChaves(desafio, ChavesDoDesafio, rotulo, null, erros);

                var dimensoes = Membro(desafio, "dimensoes");
                if (dimensoes?.Tipo == TipoDeNoJson.Objeto)
                    ConferirChaves(dimensoes, ChavesDasDimensoes, rotulo, "dimensoes", erros);
            }

            return erros;
        }

        /// <returns>Os erros de chave encontrados (lista vazia se estiver tudo certo).</returns>
        /// <exception cref="DadosInvalidosException">Se o JSON estiver mal formado.</exception>
        public static IReadOnlyList<string> VerificarTextos(string json, string nomeDoArquivo = "textos.json")
        {
            var raiz = Analisar(json, nomeDoArquivo);
            var erros = new List<string>();
            ConferirChaves(raiz, ChavesDoArquivoDeTextos, "No início do arquivo", null, erros);

            var textos = Membro(raiz, "textos");
            if (textos?.Tipo != TipoDeNoJson.Lista) return erros;
            for (int i = 0; i < textos.Itens.Count; i++)
            {
                var texto = textos.Itens[i];
                if (texto.Tipo != TipoDeNoJson.Objeto) continue;
                string chave = texto.TextoDe("chave")?.Trim();
                string rotulo = string.IsNullOrEmpty(chave) ? $"Texto na posição {i + 1}" : $"Texto \"{chave}\"";
                ConferirChaves(texto, ChavesDoTexto, rotulo, null, erros);
            }

            return erros;
        }

        /// <summary>
        /// Chave válida mais parecida com a digitada, ou null se nenhuma for parecida o bastante.
        /// Compara sem diferenciar maiúsculas (o JsonUtility diferencia: "Grandeza" é ignorado).
        /// </summary>
        public static string MaisParecida(string digitada, IReadOnlyList<string> validas)
        {
            if (string.IsNullOrEmpty(digitada)) return null;
            string alvo = digitada.ToLowerInvariant();
            int limite = Math.Max(1, alvo.Length / 3);
            string melhor = null;
            int menor = int.MaxValue;
            foreach (string valida in validas)
            {
                int distancia = Distancia(alvo, valida.ToLowerInvariant());
                if (distancia < menor)
                {
                    menor = distancia;
                    melhor = valida;
                }
            }

            return menor <= limite && menor < alvo.Length ? melhor : null;
        }

        private static NoJson Analisar(string json, string nomeDoArquivo)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new DadosInvalidosException(nomeDoArquivo, "o arquivo está vazio.");

            NoJson raiz;
            try
            {
                raiz = AnalisadorJson.Analisar(json);
            }
            catch (JsonMalFormadoException e)
            {
                throw new DadosInvalidosException(nomeDoArquivo,
                    $"o JSON está mal formado na linha {e.Linha}, coluna {e.Coluna}: {e.Motivo}");
            }

            if (raiz.Tipo != TipoDeNoJson.Objeto)
                throw new DadosInvalidosException(nomeDoArquivo,
                    "o JSON está mal formado: o arquivo deve ser um objeto, começando com \"{\".");
            return raiz;
        }

        private static void ConferirChaves(
            NoJson objeto, string[] validas, string rotulo, string dentroDe, List<string> erros)
        {
            var vistas = new HashSet<string>(StringComparer.Ordinal);
            string onde = dentroDe == null ? string.Empty : $" em \"{dentroDe}\"";
            foreach (var membro in objeto.Membros)
            {
                if (!vistas.Add(membro.Chave))
                {
                    erros.Add($"{rotulo}: chave \"{membro.Chave}\" repetida{onde} (linha {membro.Linha}).");
                    continue;
                }

                if (Array.IndexOf(validas, membro.Chave) >= 0) continue;

                string parecida = MaisParecida(membro.Chave, validas);
                string dica = parecida != null
                    ? $"Você quis dizer \"{parecida}\"?"
                    : $"Chaves válidas: {string.Join(", ", validas)}.";
                erros.Add($"{rotulo}: chave desconhecida \"{membro.Chave}\"{onde} (linha {membro.Linha}). {dica}");
            }
        }

        private static NoJson Membro(NoJson objeto, string chave)
        {
            foreach (var membro in objeto.Membros)
                if (membro.Chave == chave)
                    return membro.Valor;
            return null;
        }

        private static string[] CamposDe(Type tipo)
        {
            var campos = tipo.GetFields(BindingFlags.Public | BindingFlags.Instance);
            Array.Sort(campos, (x, y) => x.MetadataToken.CompareTo(y.MetadataToken)); // ordem da declaração
            var nomes = new string[campos.Length];
            for (int i = 0; i < campos.Length; i++) nomes[i] = campos[i].Name;
            return nomes;
        }

        /// <summary>Distância de edição (Levenshtein) entre dois textos.</summary>
        private static int Distancia(string a, string b)
        {
            var anterior = new int[b.Length + 1];
            var atual = new int[b.Length + 1];
            for (int j = 0; j <= b.Length; j++) anterior[j] = j;
            for (int i = 1; i <= a.Length; i++)
            {
                atual[0] = i;
                for (int j = 1; j <= b.Length; j++)
                {
                    int custo = a[i - 1] == b[j - 1] ? 0 : 1;
                    atual[j] = Math.Min(Math.Min(atual[j - 1] + 1, anterior[j] + 1), anterior[j - 1] + custo);
                }

                (anterior, atual) = (atual, anterior);
            }

            return anterior[b.Length];
        }
    }
}
