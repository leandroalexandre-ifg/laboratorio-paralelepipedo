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
    /// controles conectados e os erros do log. Os rótulos são técnicos (não é texto para o aluno); só o título vem
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

        private readonly StringBuilder _linhas = new StringBuilder(1024);
        private int _totalDeErros;
        private string _ultimoErro;
        private float _proximaAtualizacao;
        private string _titulo = "Diagnóstico";

        /// <summary>Se o painel está à vista (o atalho dos dois analógicos alterna).</summary>
        public bool Visivel => _conteudo != null && _conteudo.activeSelf;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(TMP_Text texto, DadosDoLaboratorio dados, RecentralizacaoDoAluno recentralizacao, GameObject conteudo)
        {
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
            if (_conteudo.activeSelf) Atualizar();
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
            s.Append("Erros no log: ").Append(_totalDeErros);
            if (_ultimoErro != null) s.Append(" (último: ").Append(_ultimoErro).Append(')');

            _texto.SetText(s);
        }
    }
}
