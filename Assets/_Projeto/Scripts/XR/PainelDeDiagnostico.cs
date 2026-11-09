using System.Text;
using LaboratorioParalelepipedo.Apresentacao;
using TMPro;
using UnityEngine;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Painel curto, preso ao mundo, separado dos créditos e escondido até o atalho do professor (os dois analógicos
    /// afundados por 3 s, <see cref="AtalhoDoDiagnostico"/>), para conferir no óculos sem acesso aos logs: a versão,
    /// a recentralização (fase, se a cabeça está pronta e os últimos eventos, com o tempo desde a abertura), os
    /// controles conectados, o estado dos botões (gatilho, lateral, analógico afundado) e os erros do log. Ao abrir,
    /// vai para a frente de onde o professor olha (preso ao mundo, não à cabeça). Os rótulos são técnicos (não é texto para o aluno); só o título vem
    /// do textos.json. Atualiza 2 vezes por segundo, nunca a cada quadro.
    /// </summary>
    public sealed class PainelDeDiagnostico : MonoBehaviour
    {
        private const float Intervalo = 0.5f;

        [SerializeField] private TMP_Text _texto;
        [SerializeField] private DadosDoLaboratorio _dados;
        [SerializeField] private string _chaveDoTitulo = Nucleo.Dados.ChavesDeTexto.DiagnosticoTitulo;
        [SerializeField] private RecentralizacaoDoAluno _recentralizacao;
        [SerializeField, Tooltip("Fundo e texto: escondidos até o atalho do professor.")]
        private GameObject _conteudo;
        [SerializeField, Tooltip("Cabeça (CenterEyeAnchor): o painel abre à frente dela.")]
        private Transform _cabeca;
        [SerializeField] private float _distanciaAoAbrir = 1.3f;
        [SerializeField, Tooltip("Estação 2: etapa e vértices escolhíveis.")]
        private EstacaoDasDiagonaisNaCena _estacao;
        [SerializeField, Tooltip("Estação 1: etapa e vértices escolhíveis.")]
        private EstacaoDosElementosNaCena _estacaoDosElementos;
        [SerializeField, Tooltip("Estação 3: etapa, pares e caixa aberta.")]
        private EstacaoDasAreasNaCena _estacaoDasAreas;

        private readonly StringBuilder _linhas = new StringBuilder(1024);
        private int _totalDeErros;
        private string _ultimoErro;
        private float _proximaAtualizacao;
        private string _titulo = "Diagnóstico";

        /// <summary>Se o painel está à vista (o atalho dos dois analógicos alterna).</summary>
        public bool Visivel => _conteudo != null && _conteudo.activeSelf;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(TMP_Text texto, DadosDoLaboratorio dados, RecentralizacaoDoAluno recentralizacao, GameObject conteudo,
            Transform cabeca = null, EstacaoDasDiagonaisNaCena estacao = null, EstacaoDosElementosNaCena estacaoDosElementos = null,
            EstacaoDasAreasNaCena estacaoDasAreas = null)
        {
            _estacaoDasAreas = estacaoDasAreas;
            _estacaoDosElementos = estacaoDosElementos;
            _cabeca = cabeca;
            _estacao = estacao;
            _texto = texto;
            _dados = dados;
            _recentralizacao = recentralizacao;
            _conteudo = conteudo;
        }

        /// <summary>Mostra ou esconde o painel. Os erros do log continuam sendo contados escondido.</summary>
        public void Alternar()
        {
            if (_conteudo == null) return;
            _conteudo.SetActive(!_conteudo.activeSelf);
            if (!_conteudo.activeSelf) return;
            PorAFrenteDaCabeca();
            Atualizar();
        }

        // Na horizontal, à frente do olhar, na altura dos olhos, virado para a cabeça.
        private void PorAFrenteDaCabeca()
        {
            if (_cabeca == null) return;
            Vector3 frente = _cabeca.forward;
            frente.y = 0f;
            if (frente.sqrMagnitude < 1e-4f) frente = Vector3.forward;
            frente.Normalize();
            transform.SetPositionAndRotation(_cabeca.position + frente * _distanciaAoAbrir, Quaternion.LookRotation(frente, Vector3.up));
        }

        private void OnEnable() => Application.logMessageReceived += AoRegistrar;

        private void OnDisable() => Application.logMessageReceived -= AoRegistrar;

        private void Start()
        {
            if (_dados != null) _titulo = _dados.Catalogo.Obter(_chaveDoTitulo);
            Atualizar();
        }

        private void AoRegistrar(string mensagem, string pilha, LogType tipo)
        {
            if (tipo != LogType.Error && tipo != LogType.Exception && tipo != LogType.Assert) return;
            _totalDeErros++;
            _ultimoErro = mensagem.Length > 80 ? mensagem.Substring(0, 80) : mensagem;
        }

        private void Update()
        {
            if (!Visivel || Time.unscaledTime < _proximaAtualizacao) return;
            _proximaAtualizacao = Time.unscaledTime + Intervalo;
            Atualizar();
        }

        /// <summary>Monta o texto do painel (público para os testes no Editor).</summary>
        public void Atualizar()
        {
            if (_texto == null) return;
            var s = _linhas;
            s.Clear();
            s.Append(_titulo).Append('\n');
            s.Append("Versão ").Append(Application.version).Append('\n');

            var r = _recentralizacao;
            if (r == null) s.Append("Recentralização: não está na cena\n");
            else
            {
                s.Append("Recentralização: ").Append(r.enabled ? r.Fase.ToString() : "DESLIGADA")
                    .Append(" | reposições: ").Append(r.Reposicoes).Append('\n');
                s.Append("Cabeça rastreada: ").Append(r.CabecaRastreada ? "sim" : "não")
                    .Append(" | âncora mexeu: ").Append(r.AncoraMexeu ? "sim" : "não").Append('\n');
                for (int i = 0; i < r.EventosGuardadosAgora; i++) s.Append("  ").Append(r.Evento(i)).Append('\n');
            }

            s.Append("Controles: ").Append(OVRInput.GetConnectedControllers()).Append('\n');
            s.Append("Gatilho E/D: ").Append(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch).ToString("0.00"))
                .Append(" / ").Append(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch).ToString("0.00"))
                .Append(" | lateral E/D: ").Append(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch).ToString("0.00"))
                .Append(" / ").Append(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch).ToString("0.00"))
                .Append('\n');
            if (_estacaoDosElementos != null && _estacaoDosElementos.Estacao != null)
                s.Append("Estação 1: ").Append(_estacaoDosElementos.Saiu ? "concluída" : _estacaoDosElementos.Estacao.Etapa.ToString())
                    .Append(" | desafio ").Append(_estacaoDosElementos.Estacao.IndiceDoDesafio + 1)
                    .Append(" | vértices escolhíveis: ").Append(_estacaoDosElementos.VerticesEscolhiveis).Append("/8\n");
            else s.Append("Estação 1: não iniciada (sem desafios?)\n");
            if (_estacao != null && _estacao.Estacao != null)
                s.Append("Estação 2: ").Append(_estacao.Saiu ? "concluída" : _estacao.Estacao.Etapa.ToString())
                    .Append(" | desafio ").Append(_estacao.Estacao.IndiceDoDesafio + 1)
                    .Append(" | vértices escolhíveis: ").Append(_estacao.VerticesEscolhiveis).Append("/8\n");
            else s.Append("Estação 2: não iniciada (sem desafios?)\n");
            if (_estacaoDasAreas != null && _estacaoDasAreas.Estacao != null)
                s.Append("Estação 3: ").Append(_estacaoDasAreas.Estacao.Etapa.ToString())
                    .Append(" | pares ").Append(_estacaoDasAreas.Estacao.ParesEncontrados.Count).Append("/3")
                    .Append(" | pergunta ").Append(_estacaoDasAreas.Estacao.IndiceDoDesafio + 1)
                    .Append(" | caixa aberta: ").Append(_estacaoDasAreas.Estacao.Aberta ? "sim" : "não").Append('\n');
            else s.Append("Estação 3: não iniciada (sem desafios?)\n");
            s.Append("Analógico afundado E/D: ").Append(AtalhoDoDiagnostico.EsquerdoAfundado ? "sim" : "não")
                .Append(" / ").Append(AtalhoDoDiagnostico.DireitoAfundado ? "sim" : "não").Append('\n');
            s.Append("Erros no log: ").Append(_totalDeErros);
            if (_ultimoErro != null) s.Append(" (último: ").Append(_ultimoErro).Append(')');

            _texto.SetText(s);
        }
    }
}
