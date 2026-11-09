using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo.Estacoes
{
    /// <summary>
    /// Fluxo da Estação 3 (Áreas), sem cena: abertura → caixa fechada → "Abrir a caixa" → achar os 3 pares de faces
    /// paralelas (tocar uma face acende o par dela, com a área) → perguntas na ordem do desafios.json (área da base,
    /// lateral, total) → fechamento → refazer. "Fechar a caixa" e "Abrir a caixa" valem a qualquer momento depois de
    /// começar; os pares só são achados com a caixa aberta. As opções são embaralhadas com uma semente (repetível
    /// nos testes) e cada distrator tem a dica do erro que imita (<see cref="DicaDaArea"/>).
    /// </summary>
    public sealed class EstacaoDasAreas
    {
        /// <summary>Pares de faces paralelas de uma caixa.</summary>
        public const int QuantidadeDePares = 3;

        private readonly IReadOnlyList<DesafioDeResposta> _desafios;
        private readonly int _semente;
        private readonly List<ParDeFaces> _pares = new List<ParDeFaces>(QuantidadeDePares);
        private readonly List<OpcaoDeResposta> _opcoes = new List<OpcaoDeResposta>(4);
        private int _rodada;

        public EtapaDasAreas Etapa { get; private set; } = EtapaDasAreas.Abertura;

        /// <summary>Se a caixa está aberta (a planificação à vista).</summary>
        public bool Aberta { get; private set; }

        /// <summary>Posição da pergunta atual na lista da estação (0 = primeira).</summary>
        public int IndiceDoDesafio { get; private set; }

        public DesafioDeResposta Desafio => _desafios[IndiceDoDesafio];

        /// <summary>A caixa da pergunta atual (antes das perguntas, a da primeira).</summary>
        public Paralelepipedo Caixa => Desafio.Caixa;

        public int QuantidadeDeDesafios => _desafios.Count;

        /// <summary>Pares já achados, na ordem em que o aluno os achou.</summary>
        public IReadOnlyList<ParDeFaces> ParesEncontrados => _pares;

        /// <summary>Se os 3 pares já foram achados.</summary>
        public bool TodosOsPares => _pares.Count == QuantidadeDePares;

        /// <summary>Última face tocada que revelou um par (nula antes do primeiro).</summary>
        public Face UltimaFace { get; private set; }

        /// <summary>A face paralela à <see cref="UltimaFace"/>.</summary>
        public Face UltimaParalela => UltimaFace != null ? Paralela(Caixa, UltimaFace) : null;

        public IReadOnlyList<OpcaoDeResposta> Opcoes => _opcoes;

        /// <summary>Última opção escolhida (−1 se nenhuma nesta pergunta).</summary>
        public int OpcaoEscolhida { get; private set; } = -1;

        /// <summary>Chave da dica da última resposta errada (nula se não há erro a mostrar).</summary>
        public string ChaveDaDica { get; private set; }

        /// <summary>Respostas erradas nesta pergunta (só para o painel; nada de penalidade).</summary>
        public int Erros { get; private set; }

        public bool NaUltimaPergunta => IndiceDoDesafio == _desafios.Count - 1;

        public EstacaoDasAreas(IReadOnlyList<DesafioDeResposta> desafios, int semente = 0)
        {
            if (desafios == null) throw new ArgumentNullException(nameof(desafios));
            if (desafios.Count == 0)
                throw new ArgumentException("A estação de áreas precisa de pelo menos um desafio de área.", nameof(desafios));
            foreach (var desafio in desafios)
                if (!Aceita(desafio))
                    throw new ArgumentException($"O desafio '{desafio?.Id}' não é uma pergunta de área da estação de áreas.",
                        nameof(desafios));
            _desafios = desafios;
            _semente = semente;
        }

        /// <summary>Desafios que a estação usa: perguntas de área (base, lateral, total) da estação de áreas, na ordem dada.</summary>
        public static List<DesafioDeResposta> DesafiosDaEstacao(IEnumerable<Desafio> todos)
        {
            var lista = new List<DesafioDeResposta>();
            if (todos == null) return lista;
            foreach (var desafio in todos)
                if (desafio is DesafioDeResposta resposta && Aceita(resposta))
                    lista.Add(resposta);
            return lista;
        }

        private static bool Aceita(DesafioDeResposta desafio) =>
            desafio != null && desafio.Estacao == Estacao.Areas &&
            (desafio.Grandeza == Grandeza.AreaDaBase || desafio.Grandeza == Grandeza.AreaLateral ||
             desafio.Grandeza == Grandeza.AreaTotal);

        /// <summary>A face paralela a <paramref name="face"/> (a outra do mesmo par).</summary>
        public static Face Paralela(Paralelepipedo caixa, Face face)
        {
            foreach (Face outra in caixa.Faces)
                if (outra.Par == face.Par && outra.Nome != face.Nome)
                    return outra;
            return null;
        }

        /// <summary>Chegou à estação: a caixa fechada na mesa. Só vale na abertura.</summary>
        public void Comecar()
        {
            if (Etapa != EtapaDasAreas.Abertura) return;
            Recomecar();
        }

        /// <summary>"Abrir a caixa". Na primeira vez, começa a busca dos pares.</summary>
        public void Abrir()
        {
            if (Etapa == EtapaDasAreas.Abertura) return;
            Aberta = true;
            if (Etapa == EtapaDasAreas.CaixaFechada) Etapa = EtapaDasAreas.Pares;
        }

        /// <summary>"Fechar a caixa": as faces dobram de volta; a etapa não muda.</summary>
        public void Fechar()
        {
            if (Etapa == EtapaDasAreas.Abertura) return;
            Aberta = false;
        }

        /// <summary>Gatilho numa face (nome na convenção, ex.: "ABCD") com a caixa aberta, na busca dos pares.</summary>
        public ResultadoDoToque TocarFace(string nome)
        {
            if (Etapa != EtapaDasAreas.Pares || !Aberta || TodosOsPares || string.IsNullOrEmpty(nome))
                return ResultadoDoToque.Ignorado;
            Face face = Caixa.ObterFace(nome);
            if (face == null || _pares.Contains(face.Par)) return ResultadoDoToque.Ignorado;
            _pares.Add(face.Par);
            UltimaFace = face;
            return ResultadoDoToque.Ligou;
        }

        /// <summary>Se o par de faces já foi achado.</summary>
        public bool ParEncontrado(ParDeFaces par) => _pares.Contains(par);

        /// <summary>Escolhe a opção <paramref name="indice"/> da pergunta. Verdadeiro se acertou.</summary>
        public bool Responder(int indice)
        {
            if (Etapa != EtapaDasAreas.Pergunta || indice < 0 || indice >= _opcoes.Count) return false;
            OpcaoEscolhida = indice;
            OpcaoDeResposta opcao = _opcoes[indice];
            if (opcao.Correta)
            {
                ChaveDaDica = null;
                Etapa = EtapaDasAreas.Acertou;
                return true;
            }

            Erros++;
            ChaveDaDica = opcao.ChaveDaDica;
            return false;
        }

        /// <summary>
        /// Depois dos 3 pares, vai à primeira pergunta; depois de um acerto, à próxima pergunta (ou ao fechamento,
        /// na última).
        /// </summary>
        public void Continuar()
        {
            if (Etapa == EtapaDasAreas.Pares && TodosOsPares) IniciarPergunta(0);
            else if (Etapa == EtapaDasAreas.Acertou)
            {
                if (NaUltimaPergunta) Etapa = EtapaDasAreas.Fechamento;
                else IniciarPergunta(IndiceDoDesafio + 1);
            }
        }

        /// <summary>"Refazer": a estação inteira de novo, com a caixa fechada e nenhum par achado.</summary>
        public void Refazer()
        {
            if (Etapa != EtapaDasAreas.Fechamento) return;
            Recomecar();
        }

        private void Recomecar()
        {
            Etapa = EtapaDasAreas.CaixaFechada;
            Aberta = false;
            IndiceDoDesafio = 0;
            _pares.Clear();
            UltimaFace = null;
            _opcoes.Clear();
            OpcaoEscolhida = -1;
            ChaveDaDica = null;
            Erros = 0;
        }

        private void IniciarPergunta(int indice)
        {
            IndiceDoDesafio = indice;
            Etapa = EtapaDasAreas.Pergunta;
            OpcaoEscolhida = -1;
            ChaveDaDica = null;
            Erros = 0;
            _rodada++;
            MontarOpcoes();
        }

        private void MontarOpcoes()
        {
            _opcoes.Clear();
            DesafioDeResposta desafio = Desafio;
            _opcoes.Add(new OpcaoDeResposta(FormatoDeConta.Numero(desafio.RespostaCorreta), desafio.RespostaCorreta, true, null));
            foreach (Medida distrator in desafio.Distratores)
                _opcoes.Add(new OpcaoDeResposta(distrator.Texto, distrator.Valor, false,
                    DicaDaArea.Chave(Caixa, desafio.Grandeza, distrator.Valor, desafio.Tolerancia)));

            // Fisher–Yates com semente: a mesma rodada da mesma pergunta dá sempre a mesma ordem.
            var aleatorio = new Random(unchecked(_semente * 7919 + IndiceDoDesafio * 104729 + _rodada * 31));
            for (int i = _opcoes.Count - 1; i > 0; i--)
            {
                int j = aleatorio.Next(i + 1);
                (_opcoes[i], _opcoes[j]) = (_opcoes[j], _opcoes[i]);
            }
        }
    }
}
