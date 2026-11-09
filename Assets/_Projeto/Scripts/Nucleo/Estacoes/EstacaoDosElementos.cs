using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>
    /// Fluxo da Estação 1 (Elementos), sem cena: abertura → desafios na ordem do desafios.json (montar face ou
    /// pergunta de contagem) → fechamento → Estação 2 ou refazer. Montar face: vale qualquer vértice inicial e os dois
    /// sentidos; a face fica pronta no 4º vértice. Vértice que não continua a face pisca e só esse toque é descartado
    /// (os certos ficam), sem penalidade. Depois de <see cref="ErrosAteAAjuda"/> erros seguidos, os vértices da face
    /// pedida (ou de uma face que continua o que já foi tocado) passam a pulsar, sem indicar a ordem. Pergunta: as
    /// opções são embaralhadas com uma semente (repetível nos testes) e cada distrator tem a dica do erro que imita.
    /// </summary>
    public sealed class EstacaoDosElementos
    {
        /// <summary>Erros seguidos ao montar a face até os vértices dela pulsarem.</summary>
        public const int ErrosAteAAjuda = 2;

        private readonly IReadOnlyList<Desafio> _desafios;
        private readonly int _semente;
        private readonly List<string> _tocados = new List<string>(5);
        private readonly List<string> _ajuda = new List<string>(4);
        private readonly List<OpcaoDeResposta> _opcoes = new List<OpcaoDeResposta>(4);
        private ValidadorDeFace _validador;
        private int _rodada;

        public EtapaDosElementos Etapa { get; private set; } = EtapaDosElementos.Abertura;

        /// <summary>Posição do desafio atual na lista da estação (0 = primeiro).</summary>
        public int IndiceDoDesafio { get; private set; }

        public Desafio Desafio => _desafios[IndiceDoDesafio];

        public Paralelepipedo Caixa => Desafio.Caixa;

        /// <summary>Quantos desafios a estação tem.</summary>
        public int QuantidadeDeDesafios => _desafios.Count;

        /// <summary>Desafio atual de montar face (nulo se o atual é uma pergunta).</summary>
        public DesafioDeMontarFace DesafioDeFace => Desafio as DesafioDeMontarFace;

        /// <summary>Desafio atual de pergunta (nulo se o atual é de montar face).</summary>
        public DesafioDeResposta DesafioDePergunta => Desafio as DesafioDeResposta;

        /// <summary>Vértices já aceitos na face em montagem, em ordem.</summary>
        public IReadOnlyList<string> Tocados => _tocados;

        /// <summary>Face que os toques formaram (nula até o 4º vértice).</summary>
        public Face FaceMontada { get; private set; }

        /// <summary>Último vértice errado tocado (nulo depois de um toque certo).</summary>
        public string VerticeErrado { get; private set; }

        /// <summary>Chave da dica do último vértice errado (fora da face pedida ou não continua a face).</summary>
        public string ChaveDoVerticeErrado { get; private set; }

        /// <summary>Erros seguidos ao montar a face (zera num toque certo).</summary>
        public int ErrosSeguidos { get; private set; }

        /// <summary>Vértices que pulsam como ajuda (vazia até <see cref="ErrosAteAAjuda"/> erros seguidos).</summary>
        public IReadOnlyList<string> VerticesDeAjuda => _ajuda;

        /// <summary>Opções da pergunta, embaralhadas (vazia fora da pergunta).</summary>
        public IReadOnlyList<OpcaoDeResposta> Opcoes => _opcoes;

        /// <summary>Última opção escolhida (−1 se nenhuma neste desafio).</summary>
        public int OpcaoEscolhida { get; private set; } = -1;

        /// <summary>Chave da dica da última resposta errada (nula se não há erro a mostrar).</summary>
        public string ChaveDaDica { get; private set; }

        /// <summary>Erros neste desafio (só para o painel; nada de penalidade).</summary>
        public int Erros { get; private set; }

        /// <summary>Se o desafio atual é o último da estação.</summary>
        public bool NoUltimoDesafio => IndiceDoDesafio == _desafios.Count - 1;

        public EstacaoDosElementos(IReadOnlyList<Desafio> desafios, int semente = 0)
        {
            if (desafios == null) throw new ArgumentNullException(nameof(desafios));
            if (desafios.Count == 0)
                throw new ArgumentException("A estação de elementos precisa de pelo menos um desafio.", nameof(desafios));
            foreach (var desafio in desafios)
                if (!Aceita(desafio))
                    throw new ArgumentException(
                        $"O desafio '{desafio?.Id}' não é de montar face nem de contagem da estação de elementos.",
                        nameof(desafios));
            _desafios = desafios;
            _semente = semente;
        }

        /// <summary>Desafios que a estação usa: da estação de elementos, de montar face ou de contagem, na ordem dada.</summary>
        public static List<Desafio> DesafiosDaEstacao(IEnumerable<Desafio> todos)
        {
            var lista = new List<Desafio>();
            if (todos == null) return lista;
            foreach (var desafio in todos)
                if (Aceita(desafio))
                    lista.Add(desafio);
            return lista;
        }

        private static bool Aceita(Desafio desafio) =>
            desafio != null && desafio.Estacao == Estacao.Elementos &&
            (desafio is DesafioDeMontarFace || (desafio is DesafioDeResposta resposta && resposta.Grandeza.EhContagem()));

        /// <summary>"Começar": vai ao primeiro desafio. Só vale na abertura.</summary>
        public void Comecar()
        {
            if (Etapa != EtapaDosElementos.Abertura) return;
            IniciarDesafio(0);
        }

        /// <summary>Gatilho num vértice (nome de A a H) ao montar a face.</summary>
        public ResultadoDoToque TocarVertice(string nome)
        {
            if (Etapa != EtapaDosElementos.MontarFace || string.IsNullOrEmpty(nome) || Caixa.ObterVertice(nome) == null)
                return ResultadoDoToque.Ignorado;
            if (_tocados.Contains(nome)) return ResultadoDoToque.Ignorado;   // já faz parte da face: nada muda

            Face alvo = DesafioDeFace.FaceAlvo;
            _tocados.Add(nome);
            bool continua = alvo != null ? _validador.PodeContinuar(alvo, _tocados) : _validador.PodeContinuar(_tocados);
            if (!continua)
            {
                _tocados.RemoveAt(_tocados.Count - 1);   // só o toque errado é descartado
                VerticeErrado = nome;
                ChaveDoVerticeErrado = alvo != null && !TemOVertice(alvo, nome)
                    ? Dados.ChavesDeTexto.ElementosForaDaFace
                    : Dados.ChavesDeTexto.ElementosNaoEhFace;
                Erros++;
                ErrosSeguidos++;
                if (ErrosSeguidos >= ErrosAteAAjuda) AtualizarAjuda();
                return ResultadoDoToque.Errado;
            }

            VerticeErrado = null;
            ChaveDoVerticeErrado = null;
            ErrosSeguidos = 0;
            if (_ajuda.Count > 0) AtualizarAjuda();   // a ajuda continua, para a face que segue o que já foi tocado

            if (!_validador.TentarIdentificar(_tocados, out Face face)) return ResultadoDoToque.Escolheu;
            FaceMontada = face;
            _ajuda.Clear();
            Etapa = EtapaDosElementos.Acertou;
            return ResultadoDoToque.Ligou;
        }

        /// <summary>Escolhe a opção <paramref name="indice"/> da pergunta. Verdadeiro se acertou.</summary>
        public bool Responder(int indice)
        {
            if (Etapa != EtapaDosElementos.Pergunta || indice < 0 || indice >= _opcoes.Count) return false;
            OpcaoEscolhida = indice;
            OpcaoDeResposta opcao = _opcoes[indice];
            if (opcao.Correta)
            {
                ChaveDaDica = null;
                Etapa = EtapaDosElementos.Acertou;
                return true;
            }

            Erros++;
            ChaveDaDica = opcao.ChaveDaDica;
            return false;
        }

        /// <summary>Depois da frase de acerto: o próximo desafio, ou o fechamento depois do último.</summary>
        public void Continuar()
        {
            if (Etapa != EtapaDosElementos.Acertou) return;
            if (NoUltimoDesafio) Etapa = EtapaDosElementos.Fechamento;
            else IniciarDesafio(IndiceDoDesafio + 1);
        }

        /// <summary>"Refazer": a estação inteira de novo, do primeiro desafio.</summary>
        public void Refazer()
        {
            if (Etapa != EtapaDosElementos.Fechamento) return;
            IniciarDesafio(0);
        }

        private static bool TemOVertice(Face face, string nome)
        {
            foreach (Vertice vertice in face.Vertices)
                if (vertice.Nome == nome) return true;
            return false;
        }

        private void IniciarDesafio(int indice)
        {
            IndiceDoDesafio = indice;
            _validador = new ValidadorDeFace(Caixa);
            _tocados.Clear();
            _ajuda.Clear();
            _opcoes.Clear();
            FaceMontada = null;
            VerticeErrado = null;
            ChaveDoVerticeErrado = null;
            ChaveDaDica = null;
            OpcaoEscolhida = -1;
            Erros = 0;
            ErrosSeguidos = 0;
            _rodada++;
            Etapa = Desafio is DesafioDeMontarFace ? EtapaDosElementos.MontarFace : EtapaDosElementos.Pergunta;
            if (Etapa == EtapaDosElementos.Pergunta) MontarOpcoes();
        }

        // A face pedida; ou, valendo qualquer face, a primeira que continua o que já foi tocado.
        private void AtualizarAjuda()
        {
            _ajuda.Clear();
            Face face = DesafioDeFace.FaceAlvo;
            if (face == null)
                foreach (Face candidata in Caixa.Faces)
                    if (_validador.PodeContinuar(candidata, _tocados))
                    {
                        face = candidata;
                        break;
                    }
            if (face == null) return;
            foreach (Vertice vertice in face.Vertices) _ajuda.Add(vertice.Nome);
        }

        private void MontarOpcoes()
        {
            DesafioDeResposta desafio = DesafioDePergunta;
            _opcoes.Add(new OpcaoDeResposta(FormatoDeConta.Numero(desafio.RespostaCorreta), desafio.RespostaCorreta, true, null));
            foreach (Medida distrator in desafio.Distratores)
                _opcoes.Add(new OpcaoDeResposta(distrator.Texto, distrator.Valor, false,
                    DicaDaContagem.Chave(Caixa, desafio.Grandeza, distrator.Valor)));

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
