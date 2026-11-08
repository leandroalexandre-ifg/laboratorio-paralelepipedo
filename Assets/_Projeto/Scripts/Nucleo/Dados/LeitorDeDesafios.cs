using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>
    /// Converte o conteúdo de desafios.json em desafios, conferindo cada campo.
    /// Reúne todos os erros do arquivo numa só <see cref="DadosInvalidosException"/>.
    /// Os nomes das listas fechadas (tipo, estação, grandeza) não diferenciam maiúsculas de minúsculas.
    /// </summary>
    public static class LeitorDeDesafios
    {
        public const int VersaoSuportada = 1;

        public const string TipoResposta = "resposta";
        public const string TipoMontarFace = "montarFace";

        private static readonly (string nome, TipoDeDesafio valor)[] Tipos =
        {
            (TipoResposta, TipoDeDesafio.Resposta),
            (TipoMontarFace, TipoDeDesafio.MontarFace),
        };

        private static readonly (string nome, Estacao valor)[] Estacoes =
        {
            ("elementos", Estacao.Elementos),
            ("diagonais", Estacao.Diagonais),
            ("areas", Estacao.Areas),
            ("volume", Estacao.Volume),
        };

        private static readonly (string nome, Grandeza valor)[] GrandezasDoJson =
        {
            ("diagonalDaBase", Grandeza.DiagonalDaBase),
            ("diagonal", Grandeza.Diagonal),
            ("areaDaBase", Grandeza.AreaDaBase),
            ("areaLateral", Grandeza.AreaLateral),
            ("areaTotal", Grandeza.AreaTotal),
            ("volume", Grandeza.Volume),
            ("numeroDeVertices", Grandeza.NumeroDeVertices),
            ("numeroDeArestas", Grandeza.NumeroDeArestas),
            ("numeroDeFaces", Grandeza.NumeroDeFaces),
        };

        /// <exception cref="DadosInvalidosException">Com todos os erros do arquivo, se houver algum.</exception>
        public static IReadOnlyList<Desafio> Ler(ArquivoDeDesafiosDto arquivo, string nomeDoArquivo = "desafios.json")
        {
            if (arquivo == null)
                throw new DadosInvalidosException(nomeDoArquivo, "o arquivo está vazio.");

            var erros = new List<string>();
            if (arquivo.versao != VersaoSuportada)
                erros.Add($"\"versao\" deve ser {VersaoSuportada} (veio {arquivo.versao}).");

            var desafios = new List<Desafio>();
            if (arquivo.desafios == null || arquivo.desafios.Length == 0)
                erros.Add("a lista \"desafios\" está vazia ou faltando.");
            else
            {
                var posicaoDoId = new Dictionary<string, int>(StringComparer.Ordinal);
                for (int i = 0; i < arquivo.desafios.Length; i++)
                {
                    var desafio = LerDesafio(arquivo.desafios[i], i + 1, posicaoDoId, erros);
                    if (desafio != null) desafios.Add(desafio);
                }
            }

            if (erros.Count > 0) throw new DadosInvalidosException(nomeDoArquivo, erros);
            return desafios;
        }

        /// <summary>Nomes aceitos no campo "grandeza", na ordem da lista fechada.</summary>
        public static IEnumerable<string> NomesDasGrandezas()
        {
            foreach (var (nome, _) in GrandezasDoJson) yield return nome;
        }

        private static Desafio LerDesafio(
            DesafioDto dto, int posicao, Dictionary<string, int> posicaoDoId, List<string> erros)
        {
            if (dto == null)
            {
                erros.Add($"Desafio na posição {posicao}: está vazio.");
                return null;
            }

            string id = dto.id?.Trim();
            string rotulo = string.IsNullOrEmpty(id) ? $"Desafio na posição {posicao}" : $"Desafio \"{id}\"";
            int errosAntes = erros.Count;
            void Erro(string problema) => erros.Add($"{rotulo}: {problema}");

            if (string.IsNullOrEmpty(id))
                Erro("falta o \"id\".");
            else if (posicaoDoId.TryGetValue(id, out int anterior))
                Erro($"id repetido (já usado na posição {anterior}).");
            else
                posicaoDoId[id] = posicao;

            TipoDeDesafio tipo = TipoDeDesafio.Resposta;
            if (Presente(dto.tipo) && !TentarNome(Tipos, dto.tipo, out tipo))
                Erro($"tipo \"{dto.tipo.Trim()}\" desconhecido. Use {Lista(Tipos)}.");

            Estacao estacao = default;
            if (!Presente(dto.estacao))
                Erro($"falta a \"estacao\" ({Lista(Estacoes)}).");
            else if (!TentarNome(Estacoes, dto.estacao, out estacao))
                Erro($"estação \"{dto.estacao.Trim()}\" desconhecida. Use {Lista(Estacoes)}.");

            Medida a = LerDimensao(dto.dimensoes?.a, "a", Erro);
            Medida b = LerDimensao(dto.dimensoes?.b, "b", Erro);
            Medida c = LerDimensao(dto.dimensoes?.c, "c", Erro);

            Grandeza grandeza = default;
            var distratores = new List<Medida>();
            double? tolerancia = null;
            bool temDistratores = dto.distratores != null && dto.distratores.Length > 0;

            if (tipo == TipoDeDesafio.Resposta)
            {
                if (!Presente(dto.grandeza))
                    Erro($"falta a \"grandeza\" ({Lista(GrandezasDoJson)}).");
                else if (!TentarNome(GrandezasDoJson, dto.grandeza, out grandeza))
                    Erro($"grandeza \"{dto.grandeza.Trim()}\" desconhecida. Use {Lista(GrandezasDoJson)}.");

                if (Presente(dto.face))
                    Erro(NaoPertence("face", TipoResposta));

                if (temDistratores)
                    for (int i = 0; i < dto.distratores.Length; i++)
                    {
                        if (InterpretadorDeMedida.TentarInterpretar(dto.distratores[i], out var d, out var erro))
                            distratores.Add(d);
                        else
                            Erro($"distrator {i + 1}: {erro}");
                    }

                if (Presente(dto.tolerancia))
                {
                    if (InterpretadorDeMedida.TentarInterpretar(dto.tolerancia, out var t, out var erro))
                        tolerancia = t.Valor;
                    else
                        Erro($"tolerância: {erro}");
                }
            }
            else
            {
                if (Presente(dto.grandeza)) Erro(NaoPertence("grandeza", TipoMontarFace));
                if (temDistratores) Erro(NaoPertence("distratores", TipoMontarFace));
                if (Presente(dto.tolerancia)) Erro(NaoPertence("tolerancia", TipoMontarFace));
            }

            if (erros.Count > errosAntes) return null;

            try
            {
                return tipo == TipoDeDesafio.MontarFace
                    ? new DesafioDeMontarFace(id, estacao, dto.bncc?.Trim(), dto.enunciado, a, b, c, dto.face)
                    : new DesafioDeResposta(id, estacao, dto.bncc?.Trim(), dto.enunciado, a, b, c, grandeza,
                        distratores, tolerancia);
            }
            catch (DesafioInvalidoException e)
            {
                erros.Add(e.Message);
                return null;
            }
        }

        private static Medida LerDimensao(string texto, string nome, Action<string> erro)
        {
            if (!Presente(texto))
            {
                erro($"falta a dimensão {nome} em \"dimensoes\".");
                return default;
            }

            if (InterpretadorDeMedida.TentarInterpretar(texto, out var medida, out var problema))
                return medida;
            erro($"dimensão {nome}: {problema}");
            return default;
        }

        private static bool Presente(string texto) => !string.IsNullOrWhiteSpace(texto);

        private static string NaoPertence(string campo, string tipo) =>
            $"o campo \"{campo}\" não pertence a desafios do tipo \"{tipo}\".";

        private static bool TentarNome<T>((string nome, T valor)[] lista, string texto, out T valor)
        {
            string limpo = texto.Trim();
            foreach (var (nome, v) in lista)
                if (string.Equals(nome, limpo, StringComparison.OrdinalIgnoreCase))
                {
                    valor = v;
                    return true;
                }

            valor = default;
            return false;
        }

        private static string Lista<T>((string nome, T valor)[] lista)
        {
            var nomes = new string[lista.Length];
            for (int i = 0; i < lista.Length; i++) nomes[i] = $"\"{lista[i].nome}\"";
            return string.Join(", ", nomes);
        }
    }
}
