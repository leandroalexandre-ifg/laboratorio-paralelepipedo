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

        /// <summary>Quantas vezes o aluno foi reposto (para os testes no Editor).</summary>
        public int Reposicoes { get; private set; }

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
        }

        private void Start()
        {
            // Sem óculos (Modo Editor): nada a corrigir, a cena fica à vista.
            if (!XRSettings.isDeviceActive)
            {
                if (_telaEscura != null) _telaEscura.OpacidadeDaRecentralizacao = 0f;
                enabled = false;
            }
        }

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
                _tela.RecenteredPose -= Pedir;
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
            Pedir();
        }

        private void AoMudarAOrigem(XRInputSubsystem entrada) => Pedir();

        /// <summary>Pede uma reposição: escurece, põe o aluno de frente para a mesa quando a cabeça parar, e clareia.</summary>
        public void Pedir() => _sequencia.Pedir();

        private void Update()
        {
            // O OVRDisplay nasce com o OVRManager: assina quando existir (botão Meta segurado).
            if (_tela == null && OVRManager.display != null)
            {
                _tela = OVRManager.display;
                _tela.RecenteredPose += Pedir;
            }

            // Origem do rastreamento trocada depois do início (o OVRManager aplica a origem "chão" num quadro seguinte).
            for (int i = 0; i < _entradas.Count; i++)
            {
                TrackingOriginModeFlags origem = _entradas[i].GetTrackingOriginMode();
                if (origem == _origem) continue;
                bool primeira = _origem == TrackingOriginModeFlags.Unknown;
                _origem = origem;
                if (!primeira) Pedir();
            }

            bool rastreada = OVRManager.OVRManagerinitialized && OVRNodeStateProperties.IsHmdPresent() &&
                             OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.Head);
            Avancar(rastreada, Time.unscaledDeltaTime);
        }

        /// <summary>
        /// Um quadro: <paramref name="rastreada"/> diz se o óculos está presente e a cabeça rastreada; a âncora também
        /// precisa ter saído da pose salva (público para os testes no Editor).
        /// </summary>
        public void Avancar(bool rastreada, float segundos)
        {
            bool ancoraMexeu = _cabeca.localPosition != _posicaoSalvaDaCabeca || _cabeca.localRotation != _rotacaoSalvaDaCabeca;
            Vetor3 cabeca = ConversaoDeVetor.ParaNucleo(_cabeca.position);
            double olhar = PosicaoInicialDoAluno.DirecaoDoOlhar(ConversaoDeVetor.ParaNucleo(_cabeca.forward),
                ConversaoDeVetor.ParaNucleo(_cabeca.up));
            if (_sequencia.Avancar(rastreada && ancoraMexeu, cabeca, olhar, segundos)) Repor();
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

        private void MostrarOpacidade()
        {
            if (_telaEscura != null) _telaEscura.OpacidadeDaRecentralizacao = (float)_sequencia.Opacidade;
        }
    }
}
