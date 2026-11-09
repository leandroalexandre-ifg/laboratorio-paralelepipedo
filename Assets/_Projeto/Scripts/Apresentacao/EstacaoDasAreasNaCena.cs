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
    /// Liga a <see cref="EstacaoDasAreas"/> (núcleo) à cena: começa pela Estação 2 ("Ir para a Estação 3"), mostra a
    /// caixa 4 × 3 × 2 na mesa, abre e fecha a <see cref="PlanificacaoNaCena"/> pelo botão do painel, acende os pares
    /// de faces tocados (cor do par e área no rótulo), faz as perguntas com os botões 2 × 2 e mostra a conta de cada
    /// acerto. Com a caixa aberta, ela não pode ser pega. Toda regra fica no núcleo; aqui só se mostra o estado.
    /// O texto do painel é montado só quando algo muda.
    /// </summary>
    public sealed class EstacaoDasAreasNaCena : MonoBehaviour
    {
        /// <summary>Tempo da frase do último par ou do acerto antes de seguir.</summary>
        public const float EsperaParaSeguir = 2f;

        [SerializeField] private DadosDoLaboratorio _dados;
        [SerializeField] private VisualDaCaixa _visual;
        [SerializeField] private CaixaPegavel _caixa;
        [SerializeField] private PlanificacaoNaCena _planificacao;

        [Header("Painel")]
        [SerializeField] private GameObject _painelDaEstacao;
        [SerializeField] private TMP_Text _instrucao;
        [SerializeField] private TMP_Text _texto;
        [SerializeField] private TMP_Text _contas;
        [SerializeField] private BotaoDoPainel[] _opcoes = new BotaoDoPainel[0];
        [SerializeField] private BotaoDoPainel _abrirOuFechar;
        [SerializeField] private BotaoDoPainel _refazer;
        [SerializeField] private Transform _fundo;
        [SerializeField] private float _topoDoFundo, _baseComUmaLinha;
        [SerializeField, Tooltip("Altura (y local) da 1ª e da 3ª linha de botões.")]
        private float _primeiraLinha = -0.12f, _terceiraLinha = -0.37f;

        [SerializeField] private int _semente = 1;

        private readonly StringBuilder _linhas = new StringBuilder(512);
        private EstacaoDasAreas _estacao;
        private Paralelepipedo _caixaMontada;
        private float _ateSeguir = -1f;
        private string _corDaDica, _corDoAcerto;

        public EstacaoDasAreas Estacao => _estacao;

        public string TextoDoPainel => $"{Lido(_instrucao)}\n{Lido(_texto)}\n{Lido(_contas)}";

        public string Instrucao => Lido(_instrucao);

        public float AlturaDoFundo => _fundo != null ? _fundo.localScale.y : 0f;

        private static string Lido(TMP_Text texto) => texto != null ? texto.text : string.Empty;

        /// <summary>O aluno acertou uma pergunta (a vibração do controle escuta isto).</summary>
        public event Action Acertou;

        public void Configurar(DadosDoLaboratorio dados, VisualDaCaixa visual, CaixaPegavel caixa, PlanificacaoNaCena planificacao,
            GameObject painelDaEstacao, TMP_Text instrucao, TMP_Text texto, TMP_Text contas, BotaoDoPainel[] opcoes,
            BotaoDoPainel abrirOuFechar, BotaoDoPainel refazer)
        {
            _dados = dados;
            _visual = visual;
            _caixa = caixa;
            _planificacao = planificacao;
            _painelDaEstacao = painelDaEstacao;
            _instrucao = instrucao;
            _texto = texto;
            _contas = contas;
            _opcoes = opcoes;
            _abrirOuFechar = abrirOuFechar;
            _refazer = refazer;
        }

        /// <summary>
        /// Fundo que acompanha o conteúdo: base logo abaixo da linha de botões (a 1ª; a 2ª no fechamento; a 3ª com a
        /// pergunta). <paramref name="baseComUmaLinha"/> é a base com os botões na 1ª linha.
        /// </summary>
        public void ConfigurarFundo(Transform fundo, float topo, float baseComUmaLinha, float primeiraLinha, float terceiraLinha)
        {
            _fundo = fundo;
            _topoDoFundo = topo;
            _baseComUmaLinha = baseComUmaLinha;
            _primeiraLinha = primeiraLinha;
            _terceiraLinha = terceiraLinha;
        }

        private void Start() => Inicializar();

        /// <summary>Cria a estação com os desafios do desafios.json (público para a prévia e os testes).</summary>
        public void Inicializar()
        {
            if (_estacao != null) return;
            _corDaDica = "#" + ColorUtility.ToHtmlStringRGB(PaletaDoLaboratorio.DestaqueLaranja);
            _corDoAcerto = "#" + ColorUtility.ToHtmlStringRGB(PaletaDoLaboratorio.Teleporte);

            var desafios = EstacaoDasAreas.DesafiosDaEstacao(_dados.Desafios);
            if (desafios.Count == 0)
            {
                Debug.LogError("[Estação 3] O desafios.json não tem perguntas de área.", this);
                return;
            }

            _estacao = new EstacaoDasAreas(desafios, _semente);
            for (int i = 0; i < _opcoes.Length; i++)
            {
                _opcoes[i].Indice = i;
                _opcoes[i].Acionado += AoEscolherOpcao;
            }
            if (_abrirOuFechar != null) _abrirOuFechar.Acionado += AoAbrirOuFechar;
            if (_refazer != null) _refazer.Acionado += AoRefazer;
            _planificacao.FaceTocada += AoTocarFace;
            _planificacao.Terminou += AoTerminarAnimacao;
            Mostrar();
        }

        private void Update()
        {
            if (_ateSeguir < 0f) return;
            _ateSeguir -= Time.deltaTime;
            if (_ateSeguir < 0f) ContinuarAgora();
        }

        // Comandos ------------------------------------------------------------------------------------------------

        /// <summary>Chegada pela Estação 2: a caixa da estação, fechada na mesa (público para a prévia e os testes).</summary>
        public void Comecar()
        {
            if (_estacao == null) Inicializar();
            if (_estacao == null || _estacao.Etapa != EtapaDasAreas.Abertura) return;
            _estacao.Comecar();
            MontarCaixa(forcar: true);
            Mostrar();
        }

        private void AoAbrirOuFechar(BotaoDoPainel botao) => AbrirOuFechar();

        /// <summary>"Abrir a caixa" ou "Fechar a caixa", conforme ela está (público para a prévia e os testes).</summary>
        public void AbrirOuFechar()
        {
            if (_estacao.Aberta) Fechar();
            else Abrir();
        }

        public void Abrir()
        {
            _estacao.Abrir();
            if (!_estacao.Aberta) return;
            _caixa.DefinirPegavel(false);
            _visual.MostrarCaixa(false);
            _planificacao.Abrir();
            Mostrar();
        }

        public void Fechar()
        {
            _estacao.Fechar();
            _planificacao.Fechar();
            Mostrar();
        }

        // A caixa só volta a aparecer (e a poder ser pega) quando as faces terminam de dobrar.
        private void AoTerminarAnimacao(bool aberta)
        {
            if (aberta) return;
            _visual.MostrarCaixa(true);
            _caixa.DefinirPegavel(true);
        }

        /// <summary>Termina a animação da caixa já (para a prévia e os testes).</summary>
        public void TerminarAnimacao() => _planificacao.TerminarAgora();

        private void AoTocarFace(string nome) => TocarFace(nome);

        /// <summary>Gatilho numa face aberta (público para a prévia e os testes).</summary>
        public ResultadoDoToque TocarFace(string nome)
        {
            ResultadoDoToque resultado = _estacao.TocarFace(nome);
            if (resultado != ResultadoDoToque.Ligou) return resultado;
            MostrarPar(_estacao.UltimaFace.Par);
            if (_estacao.TodosOsPares) _ateSeguir = EsperaParaSeguir;
            Mostrar();
            return resultado;
        }

        private void AoEscolherOpcao(BotaoDoPainel botao) => Responder(botao.Indice);

        public bool Responder(int indice)
        {
            bool certa = _estacao.Responder(indice);
            if (indice >= 0 && indice < _opcoes.Length && _estacao.OpcaoEscolhida == indice)
                _opcoes[indice].DefinirEstado(certa ? EstadoDoBotao.Certo : EstadoDoBotao.Errado);
            if (certa)
            {
                _ateSeguir = EsperaParaSeguir;
                Acertou?.Invoke();
            }
            Mostrar();
            return certa;
        }

        /// <summary>Segue já, sem esperar a frase do último par ou do acerto (público para a prévia e os testes).</summary>
        public void ContinuarAgora()
        {
            _ateSeguir = -1f;
            _estacao.Continuar();
            MontarCaixa(forcar: false);
            Mostrar();
        }

        private void AoRefazer(BotaoDoPainel botao) => Refazer();

        /// <summary>"Refazer": caixa fechada de novo, nenhum par achado (público para a prévia e os testes).</summary>
        public void Refazer()
        {
            _estacao.Refazer();
            MontarCaixa(forcar: true);
            Mostrar();
        }

        // Caixa ---------------------------------------------------------------------------------------------------

        // Refaz a caixa e a planificação quando as medidas mudam (ou ao forçar); a planificação nova volta ao estado da
        // estação (aberta ou fechada, com os pares já achados).
        private void MontarCaixa(bool forcar)
        {
            Paralelepipedo caixa = _estacao.Caixa;
            _ateSeguir = -1f;
            if (!forcar && _caixaMontada != null && caixa.A == _caixaMontada.A && caixa.B == _caixaMontada.B &&
                caixa.C == _caixaMontada.C)
                return;

            _caixaMontada = caixa;
            _caixa.DefinirPegavel(true);
            _visual.Construir(caixa);
            _caixa.AjustarColisor();
            _planificacao.Construir(caixa, _caixa.PosicaoNaMesa, _caixa.RotacaoNaMesa, _visual.MetrosPorUnidade);
            foreach (Face face in caixa.Faces)
            {
                var (maior, menor) = ContasDaArea.Lados(face);
                _planificacao.DefinirRotulo(face.Nome, $"<size=70%>{face.Nome}</size>\n{Formatar(ChavesDeTexto.AreasLadosDaFace, maior, menor)}");
            }
            foreach (ParDeFaces par in _estacao.ParesEncontrados) MostrarPar(par);
            if (_estacao.Aberta)
            {
                _caixa.DefinirPegavel(false);
                _visual.MostrarCaixa(false);
                _planificacao.Abrir();
                _planificacao.TerminarAgora();
            }
        }

        // O par achado: as duas faces com a cor dele e a área no rótulo ("4 · 3 = 12").
        private void MostrarPar(ParDeFaces par)
        {
            _planificacao.DefinirPar(par, true);
            foreach (Face face in _estacao.Caixa.Faces)
            {
                if (face.Par != par) continue;
                var (maior, menor) = ContasDaArea.Lados(face);
                _planificacao.DefinirRotulo(face.Nome, $"<size=70%>{face.Nome}</size>\n" +
                    Formatar(ChavesDeTexto.AreasAreaDaFace, maior, menor, ContasDaArea.Area(face)));
            }
        }

        // Painel --------------------------------------------------------------------------------------------------

        private string Texto(string chave) => _dados.Catalogo.Obter(chave);

        private string Formatar(string chave, params object[] valores) => string.Format(Texto(chave), valores);

        private void Mostrar()
        {
            EtapaDasAreas etapa = _estacao.Etapa;
            bool aVista = etapa != EtapaDasAreas.Abertura;
            if (_painelDaEstacao != null && _painelDaEstacao.activeSelf != aVista) _painelDaEstacao.SetActive(aVista);
            MostrarBotoes(etapa);
            AjustarFundo(etapa);
            if (aVista) MontarTexto(etapa);
        }

        private bool PerguntaAVista(EtapaDasAreas etapa) => etapa == EtapaDasAreas.Pergunta || etapa == EtapaDasAreas.Acertou;

        private void MostrarBotoes(EtapaDasAreas etapa)
        {
            bool pergunta = PerguntaAVista(etapa);
            for (int i = 0; i < _opcoes.Length; i++)
            {
                bool visivel = pergunta && i < _estacao.Opcoes.Count;
                _opcoes[i].Mostrar(visivel);
                if (!visivel) continue;
                _opcoes[i].DefinirTexto(_estacao.Opcoes[i].Texto);
                if (_estacao.OpcaoEscolhida < 0 || (etapa == EtapaDasAreas.Acertou && i != _estacao.OpcaoEscolhida))
                    _opcoes[i].DefinirEstado(EstadoDoBotao.Normal);
            }

            if (_abrirOuFechar != null)
            {
                _abrirOuFechar.Mostrar(etapa != EtapaDasAreas.Abertura);
                _abrirOuFechar.DefinirTexto(Texto(_estacao.Aberta ? ChavesDeTexto.AreasFechar : ChavesDeTexto.AreasAbrir));
                Vector3 posicao = _abrirOuFechar.transform.localPosition;
                if (!Mathf.Approximately(posicao.y, LinhaDosBotoes(etapa)))
                    _abrirOuFechar.transform.localPosition = new Vector3(posicao.x, LinhaDosBotoes(etapa), posicao.z);
            }
            if (_refazer != null)
            {
                _refazer.Mostrar(etapa == EtapaDasAreas.Fechamento);
                Vector3 posicao = _refazer.transform.localPosition;
                if (!Mathf.Approximately(posicao.y, LinhaDosBotoes(etapa)))
                    _refazer.transform.localPosition = new Vector3(posicao.x, LinhaDosBotoes(etapa), posicao.z);
            }
        }

        // Linha dos botões: a 1ª (caixa fechada e pares), a 3ª (abaixo das opções) ou a 2ª no fechamento, para as três
        // fórmulas caberem grandes acima deles.
        private float LinhaDosBotoes(EtapaDasAreas etapa) =>
            PerguntaAVista(etapa) ? _terceiraLinha
            : etapa == EtapaDasAreas.Fechamento ? (_primeiraLinha + _terceiraLinha) / 2
            : _primeiraLinha;

        private void AjustarFundo(EtapaDasAreas etapa)
        {
            if (_fundo == null) return;
            float baseDoFundo = _baseComUmaLinha + (LinhaDosBotoes(etapa) - _primeiraLinha);
            Vector3 posicao = _fundo.localPosition;
            Vector3 escala = _fundo.localScale;
            _fundo.localPosition = new Vector3(posicao.x, (_topoDoFundo + baseDoFundo) / 2, posicao.z);
            _fundo.localScale = new Vector3(escala.x, _topoDoFundo - baseDoFundo, escala.z);
        }

        private void MontarTexto(EtapaDasAreas etapa)
        {
            var s = _linhas;
            Paralelepipedo caixa = _estacao.Caixa;

            // Objetivo e onde o aluno está (pares ou pergunta).
            s.Clear();
            s.Append(Texto(ChavesDeTexto.AreasObjetivo));
            if (etapa == EtapaDasAreas.CaixaFechada || etapa == EtapaDasAreas.Pares)
                s.Append("\n<b>").Append(Formatar(ChavesDeTexto.AreasParesContagem, _estacao.ParesEncontrados.Count,
                    EstacaoDasAreas.QuantidadeDePares)).Append("</b>");
            else if (PerguntaAVista(etapa))
                s.Append("\n<b>").Append(Formatar(ChavesDeTexto.AreasPergunta, _estacao.IndiceDoDesafio + 1,
                    _estacao.QuantidadeDeDesafios)).Append("</b>\n").Append(Texto(ChavesDeTexto.AreasPares));
            Escrever(_texto, s);

            // Instrução em destaque.
            s.Clear();
            switch (etapa)
            {
                case EtapaDasAreas.CaixaFechada:
                    s.Append(Texto(ChavesDeTexto.AreasInstrucao));
                    break;
                case EtapaDasAreas.Pares:
                    if (_estacao.UltimaFace != null)
                    {
                        Face face = _estacao.UltimaFace;
                        var (maior, menor) = ContasDaArea.Lados(face);
                        s.Append("<color=").Append(_corDoAcerto).Append('>')
                            .Append(Formatar(ChavesDeTexto.AreasParEncontrado, face.Nome, _estacao.UltimaParalela.Nome,
                                Formatar(ChavesDeTexto.AreasAreaDaFace, maior, menor, ContasDaArea.Area(face))))
                            .Append("</color>");
                    }
                    // Depois do 1º par, a frase do par fica no destaque e a instrução desce para a linha das contas.
                    else s.Append(Texto(_estacao.Aberta ? ChavesDeTexto.AreasParesInstrucao : ChavesDeTexto.AreasInstrucao));
                    break;
                case EtapaDasAreas.Pergunta:
                    s.Append(_estacao.Desafio.Enunciado);
                    if (_estacao.ChaveDaDica != null)
                        s.Append("\n<size=80%><color=").Append(_corDaDica).Append('>')
                            .Append(Formatar(_estacao.ChaveDaDica, _estacao.Opcoes[_estacao.OpcaoEscolhida].Texto))
                            .Append("</color></size>");
                    break;
                case EtapaDasAreas.Acertou:
                    s.Append("<size=130%><color=").Append(_corDoAcerto).Append('>').Append(Texto(ChavesDeTexto.AreasAcerto))
                        .Append("</color></size>");
                    break;
                case EtapaDasAreas.Fechamento:
                    s.Append("<color=").Append(_corDoAcerto).Append('>').Append(Texto(ChavesDeTexto.AreasConcluida)).Append("</color>");
                    break;
            }
            Escrever(_instrucao, s);

            // A conta do acerto, com os números; no fechamento, as três fórmulas.
            s.Clear();
            if (etapa == EtapaDasAreas.Acertou) s.Append(Conta(caixa, _estacao.Desafio.Grandeza));
            else if (etapa == EtapaDasAreas.Pares && _estacao.UltimaFace != null && !_estacao.TodosOsPares)
                s.Append(Texto(_estacao.Aberta ? ChavesDeTexto.AreasParesInstrucao : ChavesDeTexto.AreasInstrucao));
            else if (etapa == EtapaDasAreas.Fechamento)
                s.Append(Texto(ChavesDeTexto.AreasFormulaDaBase)).Append('\n')
                    .Append(Texto(ChavesDeTexto.AreasFormulaLateral)).Append('\n')
                    .Append(Texto(ChavesDeTexto.AreasFormulaTotal));
            Escrever(_contas, s);
        }

        private string Conta(Paralelepipedo caixa, Grandeza grandeza)
        {
            switch (grandeza)
            {
                case Grandeza.AreaDaBase:
                    return Formatar(ChavesDeTexto.AreasContaDaBase, ContasDaArea.A(caixa), ContasDaArea.B(caixa),
                        ContasDaArea.AreaDaBase(caixa));
                case Grandeza.AreaLateral:
                    return Formatar(ChavesDeTexto.AreasContaLateral, ContasDaArea.AC(caixa), ContasDaArea.BC(caixa),
                        ContasDaArea.AreaLateral(caixa));
                default:
                    return Formatar(ChavesDeTexto.AreasContaTotal, ContasDaArea.AB(caixa), ContasDaArea.AC(caixa),
                        ContasDaArea.BC(caixa), ContasDaArea.AreaTotal(caixa));
            }
        }

        private static void Escrever(TMP_Text texto, StringBuilder s)
        {
            if (texto == null) return;
            string novo = s.ToString();
            if (texto.text != novo) texto.text = novo;
        }
    }
}
