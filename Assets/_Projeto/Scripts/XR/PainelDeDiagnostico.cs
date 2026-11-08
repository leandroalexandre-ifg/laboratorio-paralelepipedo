using System.Collections.Generic;
using System.Text;
using LaboratorioParalelepipedo.Apresentacao;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using XRInputDevice = UnityEngine.XR.InputDevice;
using XRInputDevices = UnityEngine.XR.InputDevices;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Painel preso ao mundo com o estado técnico dos controles, para achar por que eles não respondem no óculos
    /// sem acesso aos logs: loader XR, runtime OpenXR, dispositivos do Input System (o perfil de interação de cada
    /// mão), InputDevices do XR, OVRInput, o mapa de ações que o OVRInput usa (MetaQuestActionMap), as ações do
    /// projeto, o OVRManager, os raios e os últimos erros do log. Os rótulos são nomes técnicos (não é texto para o
    /// aluno); só o título vem do textos.json. Atualiza 2 vezes por segundo, nunca a cada quadro.
    /// </summary>
    public sealed class PainelDeDiagnostico : MonoBehaviour
    {
        private const float Intervalo = 0.5f;
        private const int ErrosGuardados = 4;

        [SerializeField] private TMP_Text _texto;
        [SerializeField] private DadosDoLaboratorio _dados;
        [SerializeField] private string _chaveDoTitulo = Nucleo.Dados.ChavesDeTexto.DiagnosticoTitulo;
        [SerializeField] private RaioDoControle _raioEsquerdo;
        [SerializeField] private RaioDoControle _raioDireito;

        private readonly StringBuilder _linhas = new StringBuilder(2048);
        private readonly List<XRInputDevice> _dispositivosXr = new List<XRInputDevice>();
        private readonly Queue<string> _erros = new Queue<string>();
        private int _totalDeErros;
        private float _proximaAtualizacao;
        private string _titulo = "Diagnóstico";

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(TMP_Text texto, DadosDoLaboratorio dados, RaioDoControle esquerdo, RaioDoControle direito)
        {
            _texto = texto;
            _dados = dados;
            _raioEsquerdo = esquerdo;
            _raioDireito = direito;
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
            if (_erros.Count == ErrosGuardados) _erros.Dequeue();
            _erros.Enqueue(mensagem.Length > 140 ? mensagem.Substring(0, 140) : mensagem);
        }

        private void Update()
        {
            if (Time.unscaledTime < _proximaAtualizacao) return;
            _proximaAtualizacao = Time.unscaledTime + Intervalo;
            Atualizar();
        }

        /// <summary>Monta o texto do painel (público para os testes no Editor).</summary>
        public void Atualizar()
        {
            if (_texto == null) return;
            var s = _linhas;
            s.Clear();
            s.Append(_titulo).Append("   v").Append(Application.version).Append('\n');

            // XR e OpenXR
            var gerais = XRGeneralSettings.Instance;
            var loader = gerais != null && gerais.Manager != null ? gerais.Manager.activeLoader : null;
            s.Append("Loader XR: ").Append(loader != null ? loader.name : "nenhum")
                .Append(" | OpenXR: ").Append(OpenXRRuntime.name).Append(' ').Append(OpenXRRuntime.version).Append('\n');

            // OVRManager e OVRInput
            s.Append("OVRManager: ").Append(OVRManager.instance != null ? "sim" : "NÃO")
                .Append(" | OVRPlugin iniciado: ").Append(OVRPlugin.initialized ? "sim" : "NÃO")
                .Append(" | HMD: ").Append(OVRManager.isHmdPresent ? "sim" : "não").Append('\n');
            s.Append("OVRInput conectados: ").Append(OVRInput.GetConnectedControllers())
                .Append(" | ativo: ").Append(OVRInput.GetActiveController())
                .Append(" | posição rastreada E/D: ")
                .Append(OVRInput.GetControllerPositionTracked(OVRInput.Controller.LTouch) ? "sim" : "não").Append('/')
                .Append(OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch) ? "sim" : "não").Append('\n');

            // Mapa de ações que o OVRInput usa com o Input System instalado
            var mapa = MetaQuestActionMap.Instance.ActionMap;
            s.Append("MetaQuestActionMap: ").Append(mapa.enabled ? "habilitado" : "DESABILITADO")
                .Append(" (").Append(mapa.actions.Count).Append(" ações)\n");
            Acao(s, "  E IsTracked", MetaQuestActionMap.QuestTouchDevice + MetaQuestActionMap.LeftHand + MetaQuestActionMap.IsTracked);
            Acao(s, "  D IsTracked", MetaQuestActionMap.QuestTouchDevice + MetaQuestActionMap.RightHand + MetaQuestActionMap.IsTracked);
            var doProjeto = InputSystem.actions;
            s.Append("Ações do projeto: ").Append(doProjeto == null ? "nenhuma" : doProjeto.enabled ? "habilitadas" : "desabilitadas").Append('\n');

            // Input System: um dispositivo por perfil de interação ativo em cada mão
            s.Append("Input System (").Append(InputSystem.devices.Count).Append(" dispositivos):\n");
            foreach (var dispositivo in InputSystem.devices)
            {
                if (!(dispositivo is UnityEngine.InputSystem.XR.XRController) &&
                    !(dispositivo is UnityEngine.InputSystem.XR.XRHMD)) continue;
                s.Append("  ").Append(dispositivo.layout);
                foreach (var uso in dispositivo.usages) s.Append(" {").Append(uso).Append('}');
                s.Append(dispositivo.enabled ? "" : " (desabilitado)").Append('\n');
            }

            // InputDevices do XR (camada nativa do Unity)
            XRInputDevices.GetDevices(_dispositivosXr);
            s.Append("XR InputDevices (").Append(_dispositivosXr.Count).Append("):\n");
            foreach (var dispositivo in _dispositivosXr)
                s.Append("  ").Append(dispositivo.name).Append(" [").Append(dispositivo.characteristics).Append(']')
                    .Append(dispositivo.isValid ? "" : " inválido").Append('\n');

            // Nossos raios
            s.Append("Raios E/D no alvo: ").Append(_raioEsquerdo != null && _raioEsquerdo.NoAlvo ? "sim" : "não").Append('/')
                .Append(_raioDireito != null && _raioDireito.NoAlvo ? "sim" : "não").Append('\n');

            s.Append("Erros no log: ").Append(_totalDeErros).Append('\n');
            foreach (string erro in _erros) s.Append("  ").Append(erro).Append('\n');

            _texto.SetText(s);
        }

        private static void Acao(StringBuilder s, string rotulo, string nome)
        {
            var acao = MetaQuestActionMap.GetAction(nome);
            s.Append(rotulo).Append(": ");
            if (acao == null)
            {
                s.Append("não existe\n");
                return;
            }

            s.Append(acao.enabled ? "habilitada" : "DESABILITADA").Append(", ").Append(acao.controls.Count)
                .Append(" controle(s) ligado(s), valor ").Append(acao.ReadValue<float>().ToString("0")).Append('\n');
        }
    }
}
