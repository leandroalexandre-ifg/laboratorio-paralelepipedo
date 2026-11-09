using System.Collections.Generic;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using UnityEngine;
using UnityEngine.XR;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Põe o aluno no ponto de partida, de frente para a mesa: no início do app (a frente do espaço de rastreamento é
    /// a do Guardian, e a mesa podia aparecer às costas), ao recentralizar com o botão Meta, quando a origem do
    /// rastreamento muda e ao voltar do menu do sistema ou de uma pausa. Quem se move é o rig, como no teleporte e no
    /// giro; a câmera, nunca. Cada reposição acontece com a tela escura (<see cref="SequenciaDeRecentralizacao"/>).
    /// Nenhum movimento da cabeça, por maior que seja, dispara a reposição sozinho. Sem alocação por frame.
    /// </summary>
    public sealed class RecentralizacaoDoAluno : MonoBehaviour
    {
        [SerializeField, Tooltip("Raiz do OVRCameraRig: é ela que gira e anda.")]
        private Transform _rig;
        [SerializeField, Tooltip("CenterEyeAnchor: a cabeça do aluno.")]
        private Transform _cabeca;
        [SerializeField] private TelaEscura _telaEscura;
        [SerializeField, Tooltip("Ponto de partida no chão (x, z), olhando para +z, para a mesa.")]
        private Vector2 _pontoDePartida = Vector2.zero;

        private readonly SequenciaDeRecentralizacao _sequencia = new SequenciaDeRecentralizacao();
        private readonly List<XRInputSubsystem> _entradas = new List<XRInputSubsystem>();
        private OVRDisplay _tela;
        private TrackingOriginModeFlags _origem = TrackingOriginModeFlags.Unknown;
        private Vector3 _posicaoSalvaDaCabeca;
        private Quaternion _rotacaoSalvaDaCabeca;
        private bool _saiu;
        private bool _prontaAntes;
        private string _motivo = "início";
        private readonly string[] _eventos = new string[EventosGuardados];
        private int _totalDeEventos;

        private const int EventosGuardados = 6;

        /// <summary>Quantas vezes o aluno foi reposto (para os testes no Editor e o diagnóstico).</summary>
        public int Reposicoes { get; private set; }

        /// <summary>Fase atual da sequência (para o diagnóstico).</summary>
        public FaseDaRecentralizacao Fase => _sequencia.Fase;

        /// <summary>A cabeça estava rastreada no último quadro (para o diagnóstico).</summary>
        public bool CabecaRastreada { get; private set; }

        /// <summary>A âncora da cabeça já saiu da pose salva no último quadro (para o diagnóstico).</summary>
        public bool AncoraMexeu { get; private set; }

        /// <summary>Quantos eventos já foram registrados desde o início.</summary>
        public int TotalDeEventos => _totalDeEventos;

        /// <summary>
        /// Evento <paramref name="i"/> dos últimos guardados, do mais antigo (0) ao mais novo, com o tempo desde o
        /// início do app. Montado só quando acontece, nunca a cada quadro.
        /// </summary>
        public string Evento(int i)
        {
            int guardados = Mathf.Min(_totalDeEventos, EventosGuardados);
            if (i < 0 || i >= guardados) return null;
            return _eventos[(_totalDeEventos - guardados + i) % EventosGuardados];
        }

        /// <summary>Quantos eventos estão guardados agora (no máximo 6).</summary>
        public int EventosGuardadosAgora => Mathf.Min(_totalDeEventos, EventosGuardados);

        private void Registrar(string mensagem)
        {
            string linha = $"{Time.realtimeSinceStartup:0.0} s  {mensagem}";
            _eventos[_totalDeEventos % EventosGuardados] = linha;
            _totalDeEventos++;
            if (Application.isPlaying) Debug.Log("[Recentralização] " + linha, this);
        }

        /// <summary>Se há uma recentralização em curso (incluindo escurecer e clarear).</summary>
        public bool Ativa => _sequencia.Ativa;

        private void Awake()
        {
            // Pose salva da âncora da cabeça: enquanto ela não sair daqui, o OVRCameraRig ainda não aplicou a pose real.
            if (_cabeca != null)
            {
                _posicaoSalvaDaCabeca = _cabeca.localPosition;
                _rotacaoSalvaDaCabeca = _cabeca.localRotation;
            }
            _sequencia.ComecarNoEscuro();
            MostrarOpacidade();
            Registrar("início: tela escura, esperando a cabeça");
        }

#if UNITY_EDITOR
        private void Start()
        {
            // Só no Editor sem óculos (Modo Editor): nada a corrigir, a cena fica à vista. No Quest não há esta
            // checagem: com o OpenXR, a sessão pode ainda não estar rodando no primeiro quadro, e o componente se
            // desligaria para sempre (a mesa ficava às costas). Lá, a espera pela pose da cabeça resolve.
            if (!XRSettings.isDeviceActive)
            {
                if (_telaEscura != null) _telaEscura.OpacidadeDaRecentralizacao = 0f;
                enabled = false;
            }
        }
#endif

        private void OnEnable()
        {
            SubsystemManager.GetSubsystems(_entradas);
            for (int i = 0; i < _entradas.Count; i++) _entradas[i].trackingOriginUpdated += AoMudarAOrigem;
        }

        private void OnDisable()
        {
            for (int i = 0; i < _entradas.Count; i++) _entradas[i].trackingOriginUpdated -= AoMudarAOrigem;
            _entradas.Clear();
            if (_tela != null)
            {
                _tela.RecenteredPose -= AoRecentralizarNoSistema;
                _tela = null;
            }
        }

        // Menu do sistema: perde o foco. Óculos fora do rosto ou outro app: pausa. As duas voltas pedem a reposição.
        private void OnApplicationFocus(bool comFoco) => AoMudarDeEstado(comFoco);

        private void OnApplicationPause(bool pausado) => AoMudarDeEstado(!pausado);

        private void AoMudarDeEstado(bool ativo)
        {
            if (!ativo)
            {
                _saiu = true;
                return;
            }
            if (!_saiu) return;
            _saiu = false;
            Pedir("volta do menu");
        }

        private void AoMudarAOrigem(XRInputSubsystem entrada) => Pedir("origem do rastreamento mudou");

        private void AoRecentralizarNoSistema() => Pedir("botão Meta");

        /// <summary>Pede uma reposição: escurece, põe o aluno de frente para a mesa quando a cabeça parar, e clareia.</summary>
        public void Pedir() => Pedir("pedido");

        private void Pedir(string motivo)
        {
            _motivo = motivo;
            Registrar("pedido: " + motivo);
            _sequencia.Pedir();
        }

        private void Update()
        {
            // O OVRDisplay nasce com o OVRManager: assina quando existir (botão Meta segurado).
            if (_tela == null && OVRManager.display != null)
            {
                _tela = OVRManager.display;
                _tela.RecenteredPose += AoRecentralizarNoSistema;
            }

            // Origem do rastreamento trocada depois do início (o OVRManager aplica a origem "chão" num quadro seguinte).
            for (int i = 0; i < _entradas.Count; i++)
            {
                TrackingOriginModeFlags origem = _entradas[i].GetTrackingOriginMode();
                if (origem == _origem) continue;
                bool primeira = _origem == TrackingOriginModeFlags.Unknown;
                _origem = origem;
                if (!primeira) Pedir("origem do rastreamento mudou");
            }

            // A âncora ter saído da pose salva (checado no Avancar) já prova que o rastreamento chegou à cena.
            bool rastreada = OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.Head);
            Avancar(rastreada, Time.unscaledDeltaTime);
        }

        /// <summary>
        /// Um quadro: <paramref name="rastreada"/> diz se o óculos está presente e a cabeça rastreada; a âncora também
        /// precisa ter saído da pose salva (público para os testes no Editor).
        /// </summary>
        public void Avancar(bool rastreada, float segundos)
        {
            bool ancoraMexeu = _cabeca.localPosition != _posicaoSalvaDaCabeca || _cabeca.localRotation != _rotacaoSalvaDaCabeca;
            CabecaRastreada = rastreada;
            AncoraMexeu = ancoraMexeu;
            bool pronta = rastreada && ancoraMexeu;
            if (pronta != _prontaAntes)
            {
                _prontaAntes = pronta;
                Registrar(pronta ? "cabeça pronta" : $"cabeça sem pose (rastreada {SimNao(rastreada)}, âncora {SimNao(ancoraMexeu)})");
            }

            Vetor3 cabeca = ConversaoDeVetor.ParaNucleo(_cabeca.position);
            double olhar = PosicaoInicialDoAluno.DirecaoDoOlhar(ConversaoDeVetor.ParaNucleo(_cabeca.forward),
                ConversaoDeVetor.ParaNucleo(_cabeca.up));
            FaseDaRecentralizacao antes = _sequencia.Fase;
            if (_sequencia.Avancar(pronta, cabeca, olhar, segundos))
            {
                Repor();
                Registrar($"reposto ({_motivo}): olhava para {olhar:0}°, agora de frente para a mesa");
            }
            else if (antes == FaseDaRecentralizacao.Escura && _sequencia.Fase == FaseDaRecentralizacao.Clareando)
            {
                Registrar("clareou sem repor: a cabeça não ficou pronta em 3 s");
            }
            MostrarOpacidade();
        }

        /// <summary>Põe o aluno no ponto de partida, olhando para a mesa, agora (público para os testes no Editor).</summary>
        public void Repor()
        {
            var rig = new PoseDoRig(ConversaoDeVetor.ParaNucleo(_rig.position), _rig.eulerAngles.y);
            double olhar = PosicaoInicialDoAluno.DirecaoDoOlhar(ConversaoDeVetor.ParaNucleo(_cabeca.forward),
                ConversaoDeVetor.ParaNucleo(_cabeca.up));
            PoseDoRig nova = PosicaoInicialDoAluno.Calcular(rig, ConversaoDeVetor.ParaNucleo(_cabeca.position), olhar,
                _pontoDePartida.x, _pontoDePartida.y);
            _rig.SetPositionAndRotation(ConversaoDeVetor.ParaUnity(nova.Posicao), Quaternion.Euler(0f, (float)nova.Giro, 0f));
            // Colisores e raios já no lugar novo neste quadro.
            Physics.SyncTransforms();
            Reposicoes++;
        }

        private static string SimNao(bool valor) => valor ? "sim" : "não";

        private void MostrarOpacidade()
        {
            if (_telaEscura != null) _telaEscura.OpacidadeDaRecentralizacao = (float)_sequencia.Opacidade;
        }
    }
}
