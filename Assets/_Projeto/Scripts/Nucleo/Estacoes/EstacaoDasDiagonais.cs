using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>
    /// Fluxo da Estação 2 (Diagonais), sem cena: abertura → passo 1 (ligar E e G, a diagonal d da base) → passo 2
    /// (ligar E e C, a diagonal D) → pergunta → acertou → fechamento → próximo desafio ou refazer. Vale ligar os
    /// vértices nos dois sentidos (G → E, C → E). Vértice errado não penaliza: só pede outra tentativa. Usa os desafios
    /// de resposta da estação de diagonais cuja grandeza é a diagonal D, na ordem do desafios.json; depois do último,
    /// volta ao primeiro. As opções da pergunta são embaralhadas com uma semente (repetível nos testes).
    /// </summary>
    public sealed class EstacaoDasDiagonais
    {
        private static readonly string[] SegmentoDaBase = { "E", "G" };
        private static readonly string[] SegmentoDaCaixa = { "E", "C" };

        private readonly IReadOnlyList<DesafioDeResposta> _desafios;
        private readonly int _semente;
        private readonly List<OpcaoDeResposta> _opcoes = new List<OpcaoDeResposta>(4);
        private int _rodada;

        public EtapaDasDiagonais Etapa { get; private set; } = EtapaDasDiagonais.Abertura;

        /// <summary>Posição do desafio atual na lista da estação (0 = primeiro).</summary>
        public int IndiceDoDesafio { get; private set; }

        public DesafioDeResposta Desafio => _desafios[IndiceDoDesafio];

        public Paralelepipedo Caixa => Desafio.Caixa;

        /// <summary>Quantos desafios a estação tem.</summary>
        public int QuantidadeDeDesafios => _desafios.Count;

        /// <summary>Primeira ponta já escolhida no passo atual (nula se nenhuma).</summary>
        public string VerticeEscolhido { get; private set; }

        /// <summary>Último vértice errado tocado (nulo depois de um acerto ou de mudar de passo).</summary>
        public string VerticeErrado { get; private set; }

        /// <summary>Vértice que a instrução pede agora: E antes de escolher, a outra ponta depois.</summary>
        public string VerticeEsperado
        {
            get
            {
                string[] segmento = SegmentoAtual;
                if (segmento == null) return null;
                if (VerticeEscolhido == null) return segmento[0];
                return VerticeEscolhido == segmento[0] ? segmento[1] : segmento[0];
            }
        }

        /// <summary>Opções da pergunta, embaralhadas (vazia antes da pergunta).</summary>
        public IReadOnlyList<OpcaoDeResposta> Opcoes => _opcoes;

        /// <summary>Última opção escolhida (−1 se nenhuma neste desafio).</summary>
        public int OpcaoEscolhida { get; private set; } = -1;

        /// <summary>Chave da dica da última resposta errada (nula se não há erro a mostrar).</summary>
        public string ChaveDaDica { get; private set; }

        /// <summary>Respostas erradas neste desafio (só para o painel; nada de penalidade).</summary>
        public int Erros { get; private set; }

        public EstacaoDasDiagonais(IReadOnlyList<DesafioDeResposta> desafios, int semente = 0)
        {
            if (desafios == null) throw new ArgumentNullException(nameof(desafios));
            if (desafios.Count == 0)
                throw new ArgumentException("A estação de diagonais precisa de pelo menos um desafio sobre a diagonal D.", nameof(desafios));
            _desafios = desafios;
            _semente = semente;
        }

        /// <summary>Desafios que a estação usa: resposta, estação de diagonais e grandeza diagonal D, na ordem dada.</summary>
        public static List<DesafioDeResposta> DesafiosDaEstacao(IEnumerable<Desafio> todos)
        {
            var lista = new List<DesafioDeResposta>();
            if (todos == null) return lista;
            foreach (var desafio in todos)
                if (desafio is DesafioDeResposta resposta && resposta.Estacao == Estacao.Diagonais &&
                    resposta.Grandeza == Grandeza.Diagonal)
                    lista.Add(resposta);
            return lista;
        }

        private string[] SegmentoAtual => Etapa == EtapaDasDiagonais.DiagonalDaBase ? SegmentoDaBase
            : Etapa == EtapaDasDiagonais.DiagonalDaCaixa ? SegmentoDaCaixa : null;

        /// <summary>"Começar": vai ao passo 1 do primeiro desafio. Só vale na abertura.</summary>
        public void Comecar()
        {
            if (Etapa != EtapaDasDiagonais.Abertura) return;
            IniciarDesafio(0);
        }

        /// <summary>Gatilho num vértice (nome de A a H) durante os passos 1 e 2.</summary>
        public ResultadoDoToque TocarVertice(string nome)
        {
            string[] segmento = SegmentoAtual;
            if (segmento == null || string.IsNullOrEmpty(nome)) return ResultadoDoToque.Ignorado;

            bool ehPonta = nome == segmento[0] || nome == segmento[1];
            if (!ehPonta)
            {
                VerticeErrado = nome;
                return ResultadoDoToque.Errado;
            }

            if (VerticeEscolhido == null)
            {
                VerticeEscolhido = nome;
                VerticeErrado = null;
                return ResultadoDoToque.Escolheu;
            }

            if (nome == VerticeEscolhido) return ResultadoDoToque.Ignorado;

            VerticeEscolhido = null;
            VerticeErrado = null;
            if (Etapa == EtapaDasDiagonais.DiagonalDaBase) Etapa = EtapaDasDiagonais.DiagonalDaCaixa;
            else
            {
                Etapa = EtapaDasDiagonais.Pergunta;
                MontarOpcoes();
            }
            return ResultadoDoToque.Ligou;
        }

        /// <summary>Escolhe a opção <paramref name="indice"/> da pergunta. Verdadeiro se acertou.</summary>
        public bool Responder(int indice)
        {
            if (Etapa != EtapaDasDiagonais.Pergunta || indice < 0 || indice >= _opcoes.Count) return false;

            OpcaoEscolhida = indice;
            OpcaoDeResposta opcao = _opcoes[indice];
            if (opcao.Correta)
            {
                ChaveDaDica = null;
                Etapa = EtapaDasDiagonais.Acertou;
                return true;
            }

            Erros++;
            ChaveDaDica = opcao.ChaveDaDica;
            return false;
        }

        /// <summary>Depois da frase de acerto, vai ao fechamento.</summary>
        public void Concluir()
        {
            if (Etapa == EtapaDasDiagonais.Acertou) Etapa = EtapaDasDiagonais.Fechamento;
        }

        /// <summary>"Próximo desafio": troca a caixa (o seguinte da lista, voltando ao primeiro) e recomeça o passo 1.</summary>
        public void ProximoDesafio()
        {
            if (Etapa != EtapaDasDiagonais.Fechamento) return;
            IniciarDesafio((IndiceDoDesafio + 1) % _desafios.Count);
        }

        /// <summary>"Refazer": mesma caixa, de volta ao passo 1.</summary>
        public void Refazer()
        {
            if (Etapa != EtapaDasDiagonais.Fechamento) return;
            IniciarDesafio(IndiceDoDesafio);
        }

        private void IniciarDesafio(int indice)
        {
            IndiceDoDesafio = indice;
            Etapa = EtapaDasDiagonais.DiagonalDaBase;
            VerticeEscolhido = null;
            VerticeErrado = null;
            ChaveDaDica = null;
            OpcaoEscolhida = -1;
            Erros = 0;
            _opcoes.Clear();
            _rodada++;
        }

        private void MontarOpcoes()
        {
            _opcoes.Clear();
            DesafioDeResposta desafio = Desafio;
            _opcoes.Add(new OpcaoDeResposta(ContasDaDiagonal.Diagonal(Caixa), desafio.RespostaCorreta, true, null));
            foreach (Medida distrator in desafio.Distratores)
                _opcoes.Add(new OpcaoDeResposta(distrator.Texto, distrator.Valor, false,
                    DicaDoDistrator.Chave(Caixa, distrator.Valor, desafio.Tolerancia)));

            // Fisher–Yates com semente: a mesma rodada do mesmo desafio dá sempre a mesma ordem.
            var aleatorio = new Random(unchecked(_semente * 7919 + IndiceDoDesafio * 104729 + _rodada * 31));
            for (int i = _opcoes.Count - 1; i > 0; i--)
            {
                int j = aleatorio.Next(i + 1);
                (_opcoes[i], _opcoes[j]) = (_opcoes[j], _opcoes[i]);
            }
        }
    }
}
