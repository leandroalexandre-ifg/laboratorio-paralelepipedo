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
    /// Liga a <see cref="EstacaoDasDiagonais"/> (núcleo) à cena: o botão "Começar" do título, os vértices da caixa como
    /// alvos do gatilho, o desenho das diagonais, o painel da estação (instrução e contas linha por linha) e os botões
    /// da pergunta e do fechamento. Toda regra fica no núcleo; aqui só se mostra o estado. O texto do painel é montado
    /// só quando algo muda, nunca a cada quadro.
    /// </summary>
    public sealed class EstacaoDasDiagonaisNaCena : MonoBehaviour
    {
        /// <summary>Espera depois de ligar E e C até mostrar as opções (a subida do triângulo e um respiro).</summary>
        public const float EsperaDasOpcoes = DesenhoDasDiagonais.DuracaoDaSubida + 0.3f;

        /// <summary>Tempo da frase de acerto antes do fechamento.</summary>
        public const float EsperaDoAcerto = 2f;

        [SerializeField] private DadosDoLaboratorio _dados;
        [SerializeField] private VisualDaCaixa _visual;
        [SerializeField] private CaixaPegavel _caixa;
        [SerializeField] private DesenhoDasDiagonais _desenho;

        [Header("Painéis")]
        [SerializeField] private BotaoDoPainel _comecar;
        [SerializeField] private GameObject _painelDaEstacao;
        [SerializeField] private TMP_Text _texto;
        [SerializeField] private BotaoDoPainel[] _opcoes = new BotaoDoPainel[0];
        [SerializeField] private BotaoDoPainel _proximoDesafio;
        [SerializeField] private BotaoDoPainel _refazer;

        [Header("Vértices")]
        [SerializeField] private Material _verticeNormal;
        [SerializeField] private Material _verticeApontado;
        [SerializeField] private Material _verticeErrado;
        [SerializeField, Tooltip("Raio do colisor de cada vértice, em metros (maior que a esfera: fácil de mirar).")]
        private float _raioDoAlvo = 0.035f;

        [SerializeField] private int _semente = 1;

        private readonly StringBuilder _linhas = new StringBuilder(1024);
        private EstacaoDasDiagonais _estacao;
        private AlvoDeVertice[] _alvos = new AlvoDeVertice[0];
        private float _ateAsOpcoes = -1f;
        private float _ateOFechamento = -1f;
        private string _corDaDica, _corDoAcerto, _corAzul;

        /// <summary>A estação do núcleo (para a prévia e os testes).</summary>
        public EstacaoDasDiagonais Estacao => _estacao;

        /// <summary>Se as opções da pergunta já estão à vista.</summary>
        public bool OpcoesAVista => _opcoes.Length > 0 && _opcoes[0] != null && _opcoes[0].gameObject.activeSelf;

        /// <summary>Texto atual do painel da estação.</summary>
        public string TextoDoPainel => _texto != null ? _texto.text : null;

        /// <summary>O aluno acertou a pergunta (a vibração do controle escuta isto).</summary>
        public event Action Acertou;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(DadosDoLaboratorio dados, VisualDaCaixa visual, CaixaPegavel caixa, DesenhoDasDiagonais desenho,
            BotaoDoPainel comecar, GameObject painelDaEstacao, TMP_Text texto, BotaoDoPainel[] opcoes,
            BotaoDoPainel proximoDesafio, BotaoDoPainel refazer, Material verticeNormal, Material verticeApontado,
            Material verticeErrado)
        {
            _dados = dados;
            _visual = visual;
            _caixa = caixa;
            _desenho = desenho;
            _comecar = comecar;
            _painelDaEstacao = painelDaEstacao;
            _texto = texto;
            _opcoes = opcoes;
            _proximoDesafio = proximoDesafio;
            _refazer = refazer;
            _verticeNormal = verticeNormal;
            _verticeApontado = verticeApontado;
            _verticeErrado = verticeErrado;
        }

        private void Start() => Inicializar();

        /// <summary>Cria a estação com os desafios do desafios.json e mostra a abertura (público para a prévia e os testes).</summary>
        public void Inicializar()
        {
            if (_estacao != null) return;
            _corDaDica = "#" + ColorUtility.ToHtmlStringRGB(PaletaDoLaboratorio.DestaqueLaranja);
            _corDoAcerto = "#" + ColorUtility.ToHtmlStringRGB(PaletaDoLaboratorio.Teleporte);
            _corAzul = "#" + ColorUtility.ToHtmlStringRGB(PaletaDoLaboratorio.DestaqueAzul);

            var desafios = EstacaoDasDiagonais.DesafiosDaEstacao(_dados.Desafios);
            if (desafios.Count == 0)
            {
                Debug.LogError("[Estação 2] O desafios.json não tem desafio de diagonais sobre a diagonal D.", this);
                return;
            }

            _estacao = new EstacaoDasDiagonais(desafios, _semente);
            if (_comecar != null) _comecar.Acionado += AoComecar;
            for (int i = 0; i < _opcoes.Length; i++)
            {
                _opcoes[i].Indice = i;
                _opcoes[i].Acionado += AoEscolherOpcao;
            }
            if (_proximoDesafio != null) _proximoDesafio.Acionado += AoPedirProximo;
            if (_refazer != null) _refazer.Acionado += AoRefazer;

            MontarCaixa();
            Mostrar();
        }

        private void Update()
        {
            if (_ateAsOpcoes >= 0f)
            {
                _ateAsOpcoes -= Time.deltaTime;
                if (_ateAsOpcoes < 0f) MostrarOpcoesAgora();
            }

            if (_ateOFechamento >= 0f)
            {
                _ateOFechamento -= Time.deltaTime;
                if (_ateOFechamento < 0f) ConcluirAgora();
            }
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
            EtapaDasDiagonais antes = _estacao.Etapa;
            ResultadoDoToque resultado = _estacao.TocarVertice(nome);
            if (resultado == ResultadoDoToque.Errado) Alvo(nome)?.Piscar();
            if (resultado == ResultadoDoToque.Ligou && antes == EtapaDasDiagonais.DiagonalDaCaixa)
            {
                _desenho.MostrarPasso2(true, true);
                _ateAsOpcoes = EsperaDasOpcoes;
            }
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
            if (certa)
            {
                _ateOFechamento = EsperaDoAcerto;
                Acertou?.Invoke();
            }
            Mostrar();
            return certa;
        }

        private void AoPedirProximo(BotaoDoPainel botao) => ProximoDesafio();

        /// <summary>"Próximo desafio" (público para a prévia e os testes).</summary>
        public void ProximoDesafio()
        {
            _estacao.ProximoDesafio();
            MontarCaixa();
            Mostrar();
        }

        private void AoRefazer(BotaoDoPainel botao) => Refazer();

        /// <summary>"Refazer" (público para a prévia e os testes).</summary>
        public void Refazer()
        {
            _estacao.Refazer();
            MontarCaixa();
            Mostrar();
        }

        /// <summary>Mostra as opções já, sem esperar a subida (público para a prévia e os testes).</summary>
        public void MostrarOpcoesAgora()
        {
            _ateAsOpcoes = -1f;
            _desenho.TerminarSubida();
            Mostrar();
        }

        /// <summary>Vai ao fechamento já, sem esperar a frase de acerto (público para a prévia e os testes).</summary>
        public void ConcluirAgora()
        {
            _ateOFechamento = -1f;
            _estacao.Concluir();
            Mostrar();
        }

        // Caixa ---------------------------------------------------------------------------------------------------

        private void MontarCaixa()
        {
            Paralelepipedo caixa = _estacao.Caixa;
            _visual.Construir(caixa);
            if (_caixa != null) _caixa.AjustarColisor();

            string Rotulo(string simbolo, string valor) => string.Format(Texto(ChavesDeTexto.DiagonaisRotulo), simbolo, valor);
            _desenho.Construir(Rotulo("a", ContasDaDiagonal.A(caixa)), Rotulo("b", ContasDaDiagonal.B(caixa)),
                Rotulo("c", ContasDaDiagonal.C(caixa)), "d", "D");

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

            _ateAsOpcoes = -1f;
            _ateOFechamento = -1f;
        }

        private AlvoDeVertice Alvo(string nome)
        {
            for (int i = 0; i < _alvos.Length; i++)
                if (_alvos[i] != null && _alvos[i].Nome == nome) return _alvos[i];
            return null;
        }

        // Painel --------------------------------------------------------------------------------------------------

        private string Texto(string chave) => _dados.Catalogo.Obter(chave);

        private string Formatar(string chave, params object[] valores) => string.Format(Texto(chave), valores);

        /// <summary>Atualiza desenho, botões e o texto do painel conforme a etapa (só quando algo muda).</summary>
        private void Mostrar()
        {
            EtapaDasDiagonais etapa = _estacao.Etapa;
            bool nosPassos = etapa != EtapaDasDiagonais.Abertura;

            if (_comecar != null) _comecar.Mostrar(etapa == EtapaDasDiagonais.Abertura);
            if (_painelDaEstacao != null && _painelDaEstacao.activeSelf != nosPassos) _painelDaEstacao.SetActive(nosPassos);

            _desenho.MostrarBase(etapa == EtapaDasDiagonais.DiagonalDaBase);
            _desenho.MostrarPasso1(etapa >= EtapaDasDiagonais.DiagonalDaCaixa);
            if (etapa < EtapaDasDiagonais.Pergunta) _desenho.MostrarPasso2(false, false);
            else if (!_desenho.Passo2.activeSelf) _desenho.MostrarPasso2(true, false);

            for (int i = 0; i < _alvos.Length; i++)
                if (_alvos[i] != null) _alvos[i].DefinirEscolhido(_alvos[i].Nome == _estacao.VerticeEscolhido);

            MostrarBotoes(etapa);
            if (nosPassos) MontarTexto(etapa);
        }

        private void MostrarBotoes(EtapaDasDiagonais etapa)
        {
            bool perguntaAVista = (etapa == EtapaDasDiagonais.Pergunta && _ateAsOpcoes < 0f) || etapa == EtapaDasDiagonais.Acertou;
            for (int i = 0; i < _opcoes.Length; i++)
            {
                bool visivel = perguntaAVista && i < _estacao.Opcoes.Count;
                _opcoes[i].Mostrar(visivel);
                if (!visivel) continue;
                _opcoes[i].DefinirTexto(_estacao.Opcoes[i].Texto);
                if (_estacao.OpcaoEscolhida < 0) _opcoes[i].DefinirEstado(EstadoDoBotao.Normal);
            }

            bool fechamento = etapa == EtapaDasDiagonais.Fechamento;
            if (_proximoDesafio != null) _proximoDesafio.Mostrar(fechamento);
            if (_refazer != null) _refazer.Mostrar(fechamento);
        }

        private void MontarTexto(EtapaDasDiagonais etapa)
        {
            Paralelepipedo caixa = _estacao.Caixa;
            var s = _linhas;
            s.Clear();
            s.Append("<b>").Append(Texto(ChavesDeTexto.DiagonaisTitulo)).Append("</b>\n");

            if (etapa == EtapaDasDiagonais.Fechamento)
            {
                s.Append('\n').Append("<b>").Append(Texto(ChavesDeTexto.DiagonaisConcluida)).Append("</b>\n\n");
                s.Append(Texto(ChavesDeTexto.DiagonaisFormulaGeral)).Append('\n');
                s.Append(Formatar(ChavesDeTexto.DiagonaisAcerto, ContasDaDiagonal.Diagonal(caixa)));
                _texto.text = s.ToString();
                return;
            }

            s.Append("<size=85%>").Append(Texto(ChavesDeTexto.DiagonaisObjetivo)).Append("</size>\n");
            s.Append(Formatar(ChavesDeTexto.DiagonaisDimensoes, ContasDaDiagonal.A(caixa), ContasDaDiagonal.B(caixa),
                ContasDaDiagonal.C(caixa))).Append("\n\n");

            if (etapa == EtapaDasDiagonais.DiagonalDaBase)
            {
                Instrucao(s, ChavesDeTexto.DiagonaisPasso1);
                _texto.text = s.ToString();
                return;
            }

            // Passo 1 concluído: d² = a² + b² → d² = 4 + 9 → d = √13
            s.Append(Azul(Texto(ChavesDeTexto.DiagonaisFormulaDaBase))).Append('\n');
            s.Append(Azul(Formatar(ChavesDeTexto.DiagonaisContaDaBase, ContasDaDiagonal.QuadradoDeA(caixa),
                ContasDaDiagonal.QuadradoDeB(caixa)))).Append('\n');
            s.Append(Azul(Formatar(ChavesDeTexto.DiagonaisResultadoDaBase, ContasDaDiagonal.DiagonalDaBase(caixa)))).Append("\n\n");

            if (etapa == EtapaDasDiagonais.DiagonalDaCaixa)
            {
                Instrucao(s, ChavesDeTexto.DiagonaisPasso2);
                _texto.text = s.ToString();
                return;
            }

            // Passo 2 concluído: D² = d² + c² → D² = 13 + 36 = 49 → D = ?
            s.Append(Laranja(Texto(ChavesDeTexto.DiagonaisFormulaDaDiagonal))).Append('\n');
            s.Append(Laranja(Formatar(ChavesDeTexto.DiagonaisContaDaDiagonal, ContasDaDiagonal.QuadradoDaDiagonalDaBase(caixa),
                ContasDaDiagonal.QuadradoDeC(caixa), ContasDaDiagonal.QuadradoDaDiagonal(caixa)))).Append('\n');
            s.Append(Laranja(Texto(ChavesDeTexto.DiagonaisIncognita))).Append("\n\n");

            if (_ateAsOpcoes < 0f) s.Append("<b>").Append(_estacao.Desafio.Enunciado).Append("</b>\n");

            if (etapa == EtapaDasDiagonais.Acertou)
                s.Append("<color=").Append(_corDoAcerto).Append("><b>")
                    .Append(Formatar(ChavesDeTexto.DiagonaisAcerto, ContasDaDiagonal.Diagonal(caixa))).Append("</b></color>");
            else if (_estacao.ChaveDaDica != null)
                s.Append("<color=").Append(_corDaDica).Append('>')
                    .Append(Formatar(_estacao.ChaveDaDica, _estacao.Opcoes[_estacao.OpcaoEscolhida].Texto)).Append("</color>");

            _texto.text = s.ToString();
        }

        // Instrução do passo, ou "Agora aponte para o vértice G.", ou a dica do vértice errado.
        private void Instrucao(StringBuilder s, string chaveDoPasso)
        {
            if (_estacao.VerticeErrado != null)
                s.Append("<color=").Append(_corDaDica).Append('>')
                    .Append(Formatar(ChavesDeTexto.DiagonaisVerticeErrado, _estacao.VerticeErrado, _estacao.VerticeEsperado))
                    .Append("</color>");
            else if (_estacao.VerticeEscolhido != null)
                s.Append(Formatar(ChavesDeTexto.DiagonaisProximoVertice, _estacao.VerticeEsperado));
            else
                s.Append(Texto(chaveDoPasso));
        }

        private string Azul(string texto) => "<color=" + _corAzul + ">" + texto + "</color>";

        private string Laranja(string texto) => "<color=" + _corDaDica + ">" + texto + "</color>";
    }
}
