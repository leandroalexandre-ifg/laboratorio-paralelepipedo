using System;
using System.Text;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using LaboratorioParalelepipedo.Nucleo.Estacoes;
using TMPro;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Liga a <see cref="EstacaoDosElementos"/> (núcleo) à cena: o botão "Começar" do título, os vértices da caixa como
    /// alvos do gatilho (os da ajuda pulsam), o desenho da face em montagem, o painel da estação (instrução em
    /// destaque no alto, objetivo e desafio, vértices tocados em letra grande) e os botões da pergunta e do
    /// fechamento. No fechamento, "Ir para a Estação 2" esconde este painel e começa a <see cref="EstacaoDasDiagonaisNaCena"/>.
    /// Toda regra fica no núcleo; aqui só se mostra o estado. O texto do painel é montado só quando algo muda.
    /// </summary>
    public sealed class EstacaoDosElementosNaCena : MonoBehaviour
    {
        /// <summary>Tempo da frase de acerto antes do próximo desafio (ou do fechamento).</summary>
        public const float EsperaDoAcerto = 2f;

        /// <summary>Entre os nomes dos vértices tocados ("A – B – F").</summary>
        private const string Separador = " – ";

        [SerializeField] private DadosDoLaboratorio _dados;
        [SerializeField] private VisualDaCaixa _visual;
        [SerializeField] private CaixaPegavel _caixa;
        [SerializeField] private DesenhoDaFace _desenho;
        [SerializeField, Tooltip("A estação que começa ao sair desta.")]
        private EstacaoDasDiagonaisNaCena _proximaEstacao;

        [Header("Painéis")]
        [SerializeField] private BotaoDoPainel _comecar;
        [SerializeField] private GameObject _painelDaEstacao;
        [SerializeField, Tooltip("Instrução do desafio, dica ou acerto: no alto, grande e em destaque.")]
        private TMP_Text _instrucao;
        [SerializeField, Tooltip("Objetivo da estação e o número do desafio.")]
        private TMP_Text _texto;
        [SerializeField, Tooltip("Vértices já tocados, em letra grande.")]
        private TMP_Text _tocados;
        [SerializeField] private BotaoDoPainel[] _opcoes = new BotaoDoPainel[0];
        [SerializeField] private BotaoDoPainel _irParaAProxima;
        [SerializeField] private BotaoDoPainel _refazer;
        [SerializeField, Tooltip("Fundo do painel: cresce para baixo quando aparecem os botões.")]
        private Transform _fundo;
        [SerializeField] private float _topoDoFundo, _baseSoTexto, _baseComOpcoes, _baseNoFechamento;

        [Header("Vértices")]
        [SerializeField] private Material _verticeNormal;
        [SerializeField] private Material _verticeApontado;
        [SerializeField] private Material _verticeErrado;
        [SerializeField, Tooltip("Raio do colisor de cada vértice, em metros (maior que a esfera: fácil de mirar).")]
        private float _raioDoAlvo = 0.035f;

        [SerializeField] private int _semente = 1;

        private readonly StringBuilder _linhas = new StringBuilder(512);
        private EstacaoDosElementos _estacao;
        private AlvoDeVertice[] _alvos = new AlvoDeVertice[0];
        private Paralelepipedo _caixaMontada;
        private float _ateContinuar = -1f;
        private bool _saiu;
        private string _corDaDica, _corDoAcerto;

        /// <summary>A estação do núcleo (para a prévia, os testes e o diagnóstico).</summary>
        public EstacaoDosElementos Estacao => _estacao;

        /// <summary>Se o aluno já foi para a Estação 2.</summary>
        public bool Saiu => _saiu;

        /// <summary>Texto atual do painel da estação.</summary>
        public string TextoDoPainel => $"{Lido(_instrucao)}\n{Lido(_texto)}\n{Lido(_tocados)}";

        /// <summary>Só a instrução em destaque (para os testes).</summary>
        public string Instrucao => Lido(_instrucao);

        /// <summary>Altura atual do fundo do painel, em metros (para os testes).</summary>
        public float AlturaDoFundo => _fundo != null ? _fundo.localScale.y : 0f;

        /// <summary>Vértices da caixa atual que o gatilho consegue escolher (para o diagnóstico: o normal é 8).</summary>
        public int VerticesEscolhiveis
        {
            get
            {
                if (_saiu) return 0;
                int total = 0;
                for (int i = 0; i < _alvos.Length; i++)
                    if (_alvos[i] != null && _alvos[i].isActiveAndEnabled && _alvos[i].transform.IsChildOf(_visual.transform))
                        total++;
                return total;
            }
        }

        private static string Lido(TMP_Text texto) => texto != null ? texto.text : string.Empty;

        /// <summary>O aluno acertou um desafio (a vibração do controle escuta isto).</summary>
        public event Action Acertou;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(DadosDoLaboratorio dados, VisualDaCaixa visual, CaixaPegavel caixa, DesenhoDaFace desenho,
            EstacaoDasDiagonaisNaCena proximaEstacao, BotaoDoPainel comecar, GameObject painelDaEstacao, TMP_Text instrucao,
            TMP_Text texto, TMP_Text tocados, BotaoDoPainel[] opcoes, BotaoDoPainel irParaAProxima, BotaoDoPainel refazer,
            Material verticeNormal, Material verticeApontado, Material verticeErrado)
        {
            _dados = dados;
            _visual = visual;
            _caixa = caixa;
            _desenho = desenho;
            _proximaEstacao = proximaEstacao;
            _comecar = comecar;
            _painelDaEstacao = painelDaEstacao;
            _instrucao = instrucao;
            _texto = texto;
            _tocados = tocados;
            _opcoes = opcoes;
            _irParaAProxima = irParaAProxima;
            _refazer = refazer;
            _verticeNormal = verticeNormal;
            _verticeApontado = verticeApontado;
            _verticeErrado = verticeErrado;
        }

        /// <summary>Liga o fundo que acompanha o conteúdo (usado pelo gerador da cena): topo fixo, base por etapa.</summary>
        public void ConfigurarFundo(Transform fundo, float topo, float baseSoTexto, float baseComOpcoes, float baseNoFechamento)
        {
            _fundo = fundo;
            _topoDoFundo = topo;
            _baseSoTexto = baseSoTexto;
            _baseComOpcoes = baseComOpcoes;
            _baseNoFechamento = baseNoFechamento;
        }

        private void Start() => Inicializar();

        /// <summary>Cria a estação com os desafios do desafios.json e mostra a abertura (público para a prévia e os testes).</summary>
        public void Inicializar()
        {
            if (_estacao != null) return;
            _corDaDica = "#" + ColorUtility.ToHtmlStringRGB(PaletaDoLaboratorio.DestaqueLaranja);
            _corDoAcerto = "#" + ColorUtility.ToHtmlStringRGB(PaletaDoLaboratorio.Teleporte);

            var desafios = EstacaoDosElementos.DesafiosDaEstacao(_dados.Desafios);
            if (desafios.Count == 0)
            {
                Debug.LogError("[Estação 1] O desafios.json não tem desafio de elementos.", this);
                return;
            }

            _estacao = new EstacaoDosElementos(desafios, _semente);
            if (_comecar != null) _comecar.Acionado += AoComecar;
            for (int i = 0; i < _opcoes.Length; i++)
            {
                _opcoes[i].Indice = i;
                _opcoes[i].Acionado += AoEscolherOpcao;
            }
            if (_irParaAProxima != null) _irParaAProxima.Acionado += AoIrParaAProxima;
            if (_refazer != null) _refazer.Acionado += AoRefazer;

            MontarCaixa();
            Mostrar();
        }

        private void Update()
        {
            if (_ateContinuar < 0f) return;
            _ateContinuar -= Time.deltaTime;
            if (_ateContinuar < 0f) ContinuarAgora();
        }

        // Botões e vértices --------------------------------------------------------------------------------------

        private void AoComecar(BotaoDoPainel botao) => Comecar();

        /// <summary>"Começar" (público para a prévia e os testes).</summary>
        public void Comecar()
        {
            _estacao.Comecar();
            Mostrar();
        }

        private void AoTocarVertice(AlvoDeVertice alvo) => TocarVertice(alvo.Nome);

        /// <summary>Gatilho num vértice (público para a prévia e os testes).</summary>
        public ResultadoDoToque TocarVertice(string nome)
        {
            if (_saiu) return ResultadoDoToque.Ignorado;
            ResultadoDoToque resultado = _estacao.TocarVertice(nome);
            if (resultado == ResultadoDoToque.Errado) Alvo(nome)?.Piscar();
            if (resultado == ResultadoDoToque.Ligou) AoAcertar();
            if (resultado != ResultadoDoToque.Ignorado) Mostrar();
            return resultado;
        }

        private void AoEscolherOpcao(BotaoDoPainel botao) => Responder(botao.Indice);

        /// <summary>Escolhe a opção <paramref name="indice"/> (público para a prévia e os testes).</summary>
        public bool Responder(int indice)
        {
            bool certa = _estacao.Responder(indice);
            if (indice >= 0 && indice < _opcoes.Length && _estacao.OpcaoEscolhida == indice)
                _opcoes[indice].DefinirEstado(certa ? EstadoDoBotao.Certo : EstadoDoBotao.Errado);
            if (certa) AoAcertar();
            Mostrar();
            return certa;
        }

        private void AoAcertar()
        {
            _ateContinuar = EsperaDoAcerto;
            Acertou?.Invoke();
        }

        /// <summary>Vai ao próximo desafio (ou ao fechamento) já, sem esperar a frase de acerto (público para a prévia e os testes).</summary>
        public void ContinuarAgora()
        {
            _ateContinuar = -1f;
            _estacao.Continuar();
            MontarCaixa();
            Mostrar();
        }

        private void AoIrParaAProxima(BotaoDoPainel botao) => IrParaAProximaEstacao();

        /// <summary>"Ir para a Estação 2": esconde esta estação e começa a próxima (público para a prévia e os testes).</summary>
        public void IrParaAProximaEstacao()
        {
            if (_estacao.Etapa != EtapaDosElementos.Fechamento || _saiu) return;
            _saiu = true;
            _desenho.Limpar();
            for (int i = 0; i < _alvos.Length; i++)
            {
                if (_alvos[i] == null) continue;
                _alvos[i].Acionado -= AoTocarVertice;
                _alvos[i].DefinirPulsando(false);
                _alvos[i].DefinirEscolhido(false);
            }
            _alvos = new AlvoDeVertice[0];
            _caixaMontada = null;
            Mostrar();
            if (_proximaEstacao != null) _proximaEstacao.Comecar();
        }

        private void AoRefazer(BotaoDoPainel botao) => Refazer();

        /// <summary>"Refazer": a estação inteira de novo (público para a prévia e os testes).</summary>
        public void Refazer()
        {
            if (_saiu) return;
            _estacao.Refazer();
            MontarCaixa();
            Mostrar();
        }

        // Caixa ---------------------------------------------------------------------------------------------------

        // A caixa só é refeita quando as medidas mudam; entre desafios da mesma caixa, só o desenho recomeça.
        private void MontarCaixa()
        {
            Paralelepipedo caixa = _estacao.Caixa;
            _ateContinuar = -1f;
            if (_caixaMontada != null && MesmasMedidas(_caixaMontada, caixa) && _alvos.Length > 0)
            {
                _desenho.Construir();
                return;
            }

            _caixaMontada = caixa;
            _visual.Construir(caixa);
            if (_caixa != null) _caixa.AjustarColisor();
            _desenho.Construir();

            _alvos = new AlvoDeVertice[caixa.Vertices.Count];
            float diametro = _visual.DiametroDoVertice;
            for (int i = 0; i < caixa.Vertices.Count; i++)
            {
                string nome = caixa.Vertices[i].Nome;
                Transform esfera = _visual.Vertice(nome);
                var colisor = esfera.gameObject.AddComponent<SphereCollider>();
                colisor.radius = _raioDoAlvo / diametro;   // a esfera tem escala = diâmetro e raio local 0,5
                var alvo = esfera.gameObject.AddComponent<AlvoDeVertice>();
                alvo.Configurar(nome, esfera.GetComponent<Renderer>(), _verticeNormal, _verticeApontado, _verticeErrado);
                alvo.Acionado += AoTocarVertice;
                _alvos[i] = alvo;
            }
        }

        private static bool MesmasMedidas(Paralelepipedo x, Paralelepipedo y) => x.A == y.A && x.B == y.B && x.C == y.C;

        private AlvoDeVertice Alvo(string nome)
        {
            for (int i = 0; i < _alvos.Length; i++)
                if (_alvos[i] != null && _alvos[i].Nome == nome) return _alvos[i];
            return null;
        }

        // Painel --------------------------------------------------------------------------------------------------

        private string Texto(string chave) => _dados.Catalogo.Obter(chave);

        private string Formatar(string chave, params object[] valores) => string.Format(Texto(chave), valores);

        /// <summary>Atualiza desenho, vértices, botões e o texto do painel conforme a etapa (só quando algo muda).</summary>
        private void Mostrar()
        {
            EtapaDosElementos etapa = _estacao.Etapa;
            bool aVista = etapa != EtapaDosElementos.Abertura && !_saiu;

            if (_comecar != null) _comecar.Mostrar(etapa == EtapaDosElementos.Abertura);
            if (_painelDaEstacao != null && _painelDaEstacao.activeSelf != aVista) _painelDaEstacao.SetActive(aVista);
            if (_saiu)
            {
                MostrarBotoes(etapa);
                return;
            }

            bool comFace = _estacao.DesafioDeFace != null;
            _desenho.Mostrar(comFace ? _estacao.Tocados : null, comFace ? _estacao.FaceMontada : null);
            for (int i = 0; i < _alvos.Length; i++)
            {
                if (_alvos[i] == null) continue;
                string nome = _alvos[i].Nome;
                _alvos[i].DefinirEscolhido(comFace && Contem(_estacao.Tocados, nome));
                _alvos[i].DefinirPulsando(Contem(_estacao.VerticesDeAjuda, nome));
            }

            MostrarBotoes(etapa);
            AjustarFundo(etapa);
            if (aVista) MontarTexto(etapa);
        }

        private static bool Contem(System.Collections.Generic.IReadOnlyList<string> lista, string nome)
        {
            for (int i = 0; i < lista.Count; i++)
                if (lista[i] == nome) return true;
            return false;
        }

        private bool PerguntaAVista(EtapaDosElementos etapa) =>
            !_saiu && _estacao.DesafioDePergunta != null &&
            (etapa == EtapaDosElementos.Pergunta || etapa == EtapaDosElementos.Acertou);

        private void MostrarBotoes(EtapaDosElementos etapa)
        {
            bool perguntaAVista = PerguntaAVista(etapa);
            for (int i = 0; i < _opcoes.Length; i++)
            {
                bool visivel = perguntaAVista && i < _estacao.Opcoes.Count;
                _opcoes[i].Mostrar(visivel);
                if (!visivel) continue;
                _opcoes[i].DefinirTexto(_estacao.Opcoes[i].Texto);
                // Antes de responder, tudo normal; no acerto, as erradas voltam ao normal e só a certa fica verde.
                if (_estacao.OpcaoEscolhida < 0 || (etapa == EtapaDosElementos.Acertou && i != _estacao.OpcaoEscolhida))
                    _opcoes[i].DefinirEstado(EstadoDoBotao.Normal);
            }

            bool fechamento = etapa == EtapaDosElementos.Fechamento && !_saiu;
            if (_irParaAProxima != null) _irParaAProxima.Mostrar(fechamento);
            if (_refazer != null) _refazer.Mostrar(fechamento);
        }

        // Sem grande área vazia: o painel termina logo abaixo do que está à vista.
        private void AjustarFundo(EtapaDosElementos etapa)
        {
            if (_fundo == null) return;
            float baseDoFundo = PerguntaAVista(etapa) ? _baseComOpcoes
                : etapa == EtapaDosElementos.Fechamento ? _baseNoFechamento : _baseSoTexto;
            Vector3 posicao = _fundo.localPosition;
            Vector3 escala = _fundo.localScale;
            _fundo.localPosition = new Vector3(posicao.x, (_topoDoFundo + baseDoFundo) / 2, posicao.z);
            _fundo.localScale = new Vector3(escala.x, _topoDoFundo - baseDoFundo, escala.z);
        }

        private void MontarTexto(EtapaDosElementos etapa)
        {
            var s = _linhas;
            Paralelepipedo caixa = _estacao.Caixa;
            bool fechamento = etapa == EtapaDosElementos.Fechamento;

            // Objetivo e o número do desafio.
            s.Clear();
            s.Append(Texto(ChavesDeTexto.ElementosObjetivo));
            if (!fechamento)
                s.Append("\n<b>").Append(Formatar(ChavesDeTexto.ElementosDesafio, _estacao.IndiceDoDesafio + 1,
                    _estacao.QuantidadeDeDesafios)).Append("</b>");
            Escrever(_texto, s);

            // Instrução em destaque: o enunciado (com a dica do erro), o acerto ou o fechamento.
            s.Clear();
            switch (etapa)
            {
                case EtapaDosElementos.MontarFace:
                case EtapaDosElementos.Pergunta:
                    s.Append(_estacao.Desafio.Enunciado);
                    string dica = Dica();
                    if (dica != null) s.Append("\n<size=80%><color=").Append(_corDaDica).Append('>').Append(dica).Append("</color></size>");
                    if (_estacao.VerticesDeAjuda.Count > 0)
                        s.Append("\n<size=80%>").Append(Texto(ChavesDeTexto.ElementosAjuda)).Append("</size>");
                    break;
                case EtapaDosElementos.Acertou:
                    s.Append("<size=130%><color=").Append(_corDoAcerto).Append('>');
                    if (_estacao.FaceMontada != null) s.Append(Formatar(ChavesDeTexto.ElementosFaceMontada, _estacao.FaceMontada.Nome));
                    else s.Append(Formatar(ChavesDeTexto.ElementosAcerto, _estacao.Opcoes[_estacao.OpcaoEscolhida].Texto));
                    s.Append("</color></size>");
                    break;
                case EtapaDosElementos.Fechamento:
                    s.Append("<color=").Append(_corDoAcerto).Append('>').Append(Texto(ChavesDeTexto.ElementosConcluida))
                        .Append("</color>\n").Append(Formatar(ChavesDeTexto.ElementosResumo, caixa.Vertices.Count,
                            caixa.Arestas.Count, caixa.Faces.Count));
                    break;
            }
            Escrever(_instrucao, s);

            // Vértices tocados, em letra grande: "Vértices: A – B – F".
            s.Clear();
            if (_estacao.DesafioDeFace != null && !fechamento && _estacao.Tocados.Count > 0)
            {
                _juntos.Clear();
                for (int i = 0; i < _estacao.Tocados.Count; i++)
                {
                    if (i > 0) _juntos.Append(Separador);
                    _juntos.Append(_estacao.Tocados[i]);
                }
                s.Append(Formatar(ChavesDeTexto.ElementosTocados, _juntos.ToString()));
            }
            Escrever(_tocados, s);
        }

        private readonly StringBuilder _juntos = new StringBuilder(32);

        // Dica do vértice errado (montar face) ou do distrator escolhido (pergunta); nula se não há erro a mostrar.
        private string Dica()
        {
            if (_estacao.VerticeErrado != null)
            {
                string face = _estacao.DesafioDeFace?.FaceAlvo?.Nome;
                return Formatar(_estacao.ChaveDoVerticeErrado, _estacao.VerticeErrado, face);
            }
            if (_estacao.ChaveDaDica != null)
                return Formatar(_estacao.ChaveDaDica, _estacao.Opcoes[_estacao.OpcaoEscolhida].Texto);
            return null;
        }

        // Só quando algo muda (nunca a cada quadro): o texto fica legível também para a prévia e os testes.
        private static void Escrever(TMP_Text texto, StringBuilder s)
        {
            if (texto == null) return;
            string novo = s.ToString();
            if (texto.text != novo) texto.text = novo;
        }
    }
}
