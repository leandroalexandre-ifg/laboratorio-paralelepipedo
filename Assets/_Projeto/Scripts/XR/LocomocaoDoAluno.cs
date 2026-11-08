using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using UnityEngine;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Teleporte e giro por passos com os analógicos dos dois controles. Para a frente: arco até o chão (verde-água
    /// onde vale, laranja onde não vale); ao soltar, a cabeça chega ao destino sem mudar a direção do olhar. Para o
    /// lado: giro de <see cref="MovimentoDoRig.AnguloDoGiroEmGraus"/>° em torno da cabeça, com um escurecimento de
    /// ~100 ms só no instante do giro. Só o rig se move; a câmera, nunca. Regras no núcleo
    /// (<see cref="RegrasDoTeleporte"/>, <see cref="ComandoDoAnalogico"/>). Sem alocação por frame.
    /// </summary>
    public sealed class LocomocaoDoAluno : MonoBehaviour
    {
        private const int PontosDoArco = 80;
        private const int VagaDaMesa = 0;
        private const int VagaDaCaixa = 1;

        [SerializeField, Tooltip("Raiz do OVRCameraRig: é ele que o teleporte e o giro movem.")]
        private Transform _rig;
        [SerializeField, Tooltip("CenterEyeAnchor: a cabeça do aluno.")]
        private Transform _cabeca;
        [SerializeField] private RaioDoControle _raioEsquerdo;
        [SerializeField] private RaioDoControle _raioDireito;

        [Header("Teleporte")]
        [SerializeField] private LineRenderer _arco;
        [SerializeField] private Renderer _marcador;
        [SerializeField] private Material _materialValido;
        [SerializeField] private Material _materialInvalido;

        [Header("Giro")]
        [SerializeField, Tooltip("Esfera escura presa à cabeça, desligada fora do giro.")]
        private TelaEscura _telaEscura;

        [Header("Sala (metros)")]
        [SerializeField] private Vector2 _centroDaSala = Vector2.zero;
        [SerializeField] private Vector2 _salaPorDentro = new Vector2(7.9f, 7.9f);
        [SerializeField] private Vector3 _centroDaMesa = new Vector3(0f, 0f, 1f);
        [SerializeField] private Vector2 _tamanhoDaMesa = new Vector2(1.2f, 0.8f);
        [SerializeField] private float _alturaDaMesa = 0.76f;
        [SerializeField] private CaixaPegavel _caixa;

        private readonly ComandoDoAnalogico _analogicoEsquerdo = new ComandoDoAnalogico();
        private readonly ComandoDoAnalogico _analogicoDireito = new ComandoDoAnalogico();
        private readonly EscurecimentoDoGiro _escurecimentoDoGiro = new EscurecimentoDoGiro();
        private readonly Vetor3[] _pontosDoNucleo = new Vetor3[PontosDoArco];
        private RegrasDoTeleporte _regras;
        private ResultadoDoTeleporte _destino;
        private RaioDoControle _mirando;
        private int _corMostrada = -1;   // -1: nenhuma; 0: inválido; 1: válido
        private double _giroPendente;

        /// <summary>Regras do teleporte desta sala (para os testes no Editor).</summary>
        public RegrasDoTeleporte Regras => _regras ?? (_regras = CriarRegras());

        private void Awake()
        {
            _regras = CriarRegras();
            MostrarArco(false);
        }

        private RegrasDoTeleporte CriarRegras()
        {
            var regras = new RegrasDoTeleporte(
                new AreaRetangular(_centroDaSala.x, _centroDaSala.y, _salaPorDentro.x, _salaPorDentro.y), 2);
            regras.DefinirObstaculo(VagaDaMesa, new ObstaculoDoTeleporte(
                new AreaRetangular(_centroDaMesa.x, _centroDaMesa.z, _tamanhoDaMesa.x, _tamanhoDaMesa.y),
                0, _alturaDaMesa, MotivoDoTeleporte.NaMesa));
            return regras;
        }

        private void Update()
        {
            // Recentralizando (tela escurecendo ou escura): nada de teleporte nem giro até clarear.
            if (_telaEscura != null && _telaEscura.OpacidadeDaRecentralizacao > 0f)
            {
                if (_mirando != null) PararDeMirar();
                _analogicoEsquerdo.Reiniciar();
                _analogicoDireito.Reiniciar();
                AtualizarEscurecimento(Time.deltaTime);
                return;
            }

            Processar(_analogicoEsquerdo, OVRInput.Controller.LTouch, _raioEsquerdo);
            Processar(_analogicoDireito, OVRInput.Controller.RTouch, _raioDireito);
            if (_mirando != null)
            {
                AtualizarArco();
                if (_mirando.Dica != null) _mirando.Dica.Mirar(true, _destino.Valido);
            }
            AtualizarEscurecimento(Time.deltaTime);
        }

        private void Processar(ComandoDoAnalogico analogico, OVRInput.Controller controle, RaioDoControle raio)
        {
            if (raio == null) return;
            if (!OVRInput.IsControllerConnected(controle))
            {
                if (_mirando == raio) PararDeMirar();
                analogico.Reiniciar();
                return;
            }

            Vector2 eixo = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controle);
            switch (analogico.Atualizar(eixo.x, eixo.y))
            {
                case AcaoDoAnalogico.ComecarAMirar:
                    if (_mirando == null)
                    {
                        _mirando = raio;
                        raio.Ocultar(true);
                    }
                    break;
                case AcaoDoAnalogico.Teleportar:
                    if (_mirando == raio)
                    {
                        if (_destino.Valido) Teleportar(_destino.Pouso);
                        PararDeMirar();
                    }
                    break;
                case AcaoDoAnalogico.CancelarMira:
                    if (_mirando == raio) PararDeMirar();
                    break;
                case AcaoDoAnalogico.GirarParaADireita:
                    PedirGiro(MovimentoDoRig.AnguloDoGiroEmGraus);
                    break;
                case AcaoDoAnalogico.GirarParaAEsquerda:
                    PedirGiro(-MovimentoDoRig.AnguloDoGiroEmGraus);
                    break;
            }
        }

        /// <summary>Traça o arco a partir de um controle e mostra o destino (público para os testes no Editor).</summary>
        public ResultadoDoTeleporte Mirar(Vector3 origem, Vector3 direcao)
        {
            if (_caixa != null) Regras.DefinirObstaculo(VagaDaCaixa, _caixa.ObstaculoNoPiso());
            _destino = Regras.TracarArco(ConversaoDeVetor.ParaNucleo(origem), ConversaoDeVetor.ParaNucleo(direcao), _pontosDoNucleo);

            if (_arco != null)
            {
                _arco.positionCount = _destino.QuantidadeDePontos;
                for (int i = 0; i < _destino.QuantidadeDePontos; i++)
                    _arco.SetPosition(i, ConversaoDeVetor.ParaUnity(_pontosDoNucleo[i]));
            }

            if (_marcador != null)
                _marcador.transform.position = ConversaoDeVetor.ParaUnity(_destino.Pouso) + Vector3.up * 0.005f;
            MostrarArco(true);
            return _destino;
        }

        private void AtualizarArco()
        {
            Transform mao = _mirando.transform;
            Mirar(mao.position, mao.forward);
        }

        private void PararDeMirar()
        {
            if (_mirando != null)
            {
                _mirando.Ocultar(false);
                if (_mirando.Dica != null) _mirando.Dica.Mirar(false, false);
            }
            _mirando = null;
            MostrarArco(false);
        }

        private void MostrarArco(bool visivel)
        {
            if (_arco != null) _arco.enabled = visivel;
            if (_marcador != null) _marcador.enabled = visivel && _destino.Motivo != MotivoDoTeleporte.SemPouso;
            if (!visivel) return;

            bool valido = _destino.Valido;
            int cor = valido ? 1 : 0;
            if (cor == _corMostrada) return;
            _corMostrada = cor;
            Material material = valido ? _materialValido : _materialInvalido;
            if (_arco != null) _arco.sharedMaterial = material;
            if (_marcador != null) _marcador.sharedMaterial = material;
        }

        /// <summary>Leva a cabeça ao destino, sem girar (público para os testes no Editor).</summary>
        public void Teleportar(Vetor3 destino)
        {
            Vetor3 rig = MovimentoDoRig.Teleportar(ConversaoDeVetor.ParaNucleo(_rig.position),
                ConversaoDeVetor.ParaNucleo(_cabeca.position), destino);
            _rig.position = ConversaoDeVetor.ParaUnity(rig);
        }

        private void PedirGiro(double graus)
        {
            if (!_escurecimentoDoGiro.Iniciar()) return;
            _giroPendente = graus;
        }

        private void AtualizarEscurecimento(float segundos)
        {
            if (!_escurecimentoDoGiro.Ativo) return;
            double opacidade = _escurecimentoDoGiro.Avancar(segundos, out bool girarAgora);
            if (girarAgora) Girar(_giroPendente);
            if (_telaEscura != null) _telaEscura.OpacidadeDoGiro = _escurecimentoDoGiro.Ativo ? (float)opacidade : 0f;
        }

        /// <summary>Gira o rig em torno da cabeça (público para os testes no Editor).</summary>
        public void Girar(double graus)
        {
            Vetor3 rig = MovimentoDoRig.GirarEmTornoDaCabeca(ConversaoDeVetor.ParaNucleo(_rig.position),
                ConversaoDeVetor.ParaNucleo(_cabeca.position), graus);
            _rig.SetPositionAndRotation(ConversaoDeVetor.ParaUnity(rig), Quaternion.Euler(0f, (float)graus, 0f) * _rig.rotation);
        }
    }
}
