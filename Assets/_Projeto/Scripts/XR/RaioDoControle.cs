using System.Collections.Generic;
using LaboratorioParalelepipedo.Apresentacao;
using LaboratorioParalelepipedo.Nucleo.Interacao;
using UnityEngine;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Raio reto que sai de um controle Touch (há um em cada mão). Acende o alvo para onde aponta
    /// (<see cref="IDestacavel"/>) com uma vibração leve; o gatilho confirma (<see cref="IAlvoDoRaio"/>) com outra;
    /// o botão lateral segura um <see cref="IPegavel"/> enquanto estiver apertado. Some com o controle desligado e
    /// enquanto a mesma mão mira o teleporte. Um alvo do gatilho (vértice, botão) ao longo do raio vale mais que o
    /// corpo da caixa: os vértices de trás ficam dentro do colisor da caixa e o raio os escolhe através dela.
    /// Sem alocação por frame.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class RaioDoControle : MonoBehaviour
    {
        [SerializeField] private OVRInput.Controller _controle = OVRInput.Controller.RTouch;
        [SerializeField] private LineRenderer _linha;
        [SerializeField] private Material _materialLivre;
        [SerializeField] private Material _materialNoAlvo;
        [SerializeField, Tooltip("Modelo simples do controle: some quando o controle está desligado.")]
        private GameObject _modelo;
        [SerializeField, Tooltip("Dica acima do controle (opcional).")]
        private DicaDaMao _dica;
        [SerializeField] private float _alcance = 6f;
        [SerializeField] private LayerMask _camadas = ~0;

        private const int MaximoDeAcertos = 16;

        private readonly RaycastHit[] _acertos = new RaycastHit[MaximoDeAcertos];
        // Se o colisor tem um IAlvoDoRaio acima dele: a busca na hierarquia só roda na primeira vez de cada colisor.
        private readonly Dictionary<Collider, bool> _ehAlvo = new Dictionary<Collider, bool>(64);
        private readonly BotaoComHisterese _gatilho = new BotaoComHisterese();
        private readonly BotaoComHisterese _botaoLateral = new BotaoComHisterese();
        private Collider _colisorApontado;
        private IAlvoDoRaio _alvo;
        private IPegavel _pegavel;
        private IDestacavel _destacavel;
        private IPegavel _naMao;
        private bool _oculto;
        private float _fimDaVibracao = -1f;

        /// <summary>Controle deste raio.</summary>
        public OVRInput.Controller Controle => _controle;

        /// <summary>Se o raio aponta para algo que acende ou se pega.</summary>
        public bool NoAlvo => _alvo != null || _pegavel != null;

        /// <summary>Dica acima deste controle (pode ser nula).</summary>
        public DicaDaMao Dica => _dica;

        /// <summary>Liga a dica acima do controle (usado pelo gerador da cena).</summary>
        public void ConfigurarDica(DicaDaMao dica) => _dica = dica;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(OVRInput.Controller controle, LineRenderer linha, Material livre, Material noAlvo, GameObject modelo)
        {
            _controle = controle;
            _linha = linha;
            _materialLivre = livre;
            _materialNoAlvo = noAlvo;
            _modelo = modelo;
        }

        /// <summary>Esconde o raio (a mesma mão está mirando o teleporte) ou o mostra de novo.</summary>
        public void Ocultar(bool oculto)
        {
            _oculto = oculto;
            if (oculto) Apontar(null);
        }

        private void Awake()
        {
            _linha.positionCount = 2;
            _linha.useWorldSpace = true;
            _linha.sharedMaterial = _materialLivre;
        }

        private void OnDisable()
        {
            Apontar(null);
            Largar();
            PararDeVibrar();
            _gatilho.Reiniciar();
            _botaoLateral.Reiniciar();
            if (_linha != null) _linha.enabled = false;
            AtualizarDica();
        }

        private void Update()
        {
            if (_fimDaVibracao >= 0f && Time.unscaledTime >= _fimDaVibracao) PararDeVibrar();

            bool conectado = OVRInput.IsControllerConnected(_controle);
            if (_modelo != null && _modelo.activeSelf != conectado) _modelo.SetActive(conectado);
            if (!conectado)
            {
                if (_linha.enabled) OnDisable();
                return;
            }

            // A outra mão pegou a caixa: esta deixa de segurar.
            if (_naMao != null && !_naMao.EstaNaMaoDe(transform)) _naMao = null;

            bool mostrar = !_oculto && _naMao == null;
            _linha.enabled = mostrar;
            if (mostrar)
            {
                Vector3 inicio = transform.position;
                Vector3 fim = Mirar(new Ray(inicio, transform.forward));
                _linha.SetPosition(0, inicio);
                _linha.SetPosition(1, fim);
            }
            else Apontar(null);

            if (_gatilho.Atualizar(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, _controle)) == TransicaoDoBotao.Apertou)
                Confirmar();

            switch (_botaoLateral.Atualizar(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, _controle)))
            {
                case TransicaoDoBotao.Apertou:
                    if (_pegavel != null)
                    {
                        _naMao = _pegavel;
                        Apontar(null);
                        _naMao.Pegar(transform);
                    }
                    break;
                case TransicaoDoBotao.Soltou:
                    Largar();
                    break;
            }

            AtualizarDica();
        }

        private void AtualizarDica()
        {
            if (_dica == null) return;
            _dica.ApontandoParaPegar = _pegavel != null;
            _dica.ApontandoParaEscolher = _alvo != null;
            _dica.Segurando = _naMao != null;
        }

        /// <summary>Acha o que o raio atinge e devolve onde ele termina (público para os testes no Editor).</summary>
        public Vector3 Mirar(Ray raio)
        {
            int total = Physics.RaycastNonAlloc(raio, _acertos, _alcance, _camadas, QueryTriggerInteraction.Ignore);
            int escolhido = -1, maisPerto = -1;
            for (int i = 0; i < total; i++)
            {
                if (maisPerto < 0 || _acertos[i].distance < _acertos[maisPerto].distance) maisPerto = i;
                if (EhAlvo(_acertos[i].collider) && (escolhido < 0 || _acertos[i].distance < _acertos[escolhido].distance))
                    escolhido = i;
            }
            if (escolhido < 0) escolhido = maisPerto;

            if (escolhido >= 0)
            {
                // Componentes procurados só quando o objeto atingido muda.
                Collider colisor = _acertos[escolhido].collider;
                if (colisor != _colisorApontado) Apontar(colisor);
                return _acertos[escolhido].point;
            }

            Apontar(null);
            return raio.GetPoint(_alcance);
        }

        private bool EhAlvo(Collider colisor)
        {
            if (_ehAlvo.TryGetValue(colisor, out bool ehAlvo)) return ehAlvo;
            if (_ehAlvo.Count >= 256) _ehAlvo.Clear();   // colisores de caixas antigas (cada desafio cria 8 vértices)
            ehAlvo = colisor.GetComponentInParent<IAlvoDoRaio>() != null;
            _ehAlvo[colisor] = ehAlvo;
            return ehAlvo;
        }

        /// <summary>Aperto do gatilho: confirma o alvo, com vibração (público para os testes no Editor).</summary>
        public void Confirmar()
        {
            if (_alvo == null) return;
            Vibrar(Vibracao.AoConfirmar);
            _alvo.AoAcionar();
        }

        private void Largar()
        {
            if (_naMao == null) return;
            _naMao.Soltar(transform);
            _naMao = null;
        }

        private void Apontar(Collider colisor)
        {
            if (colisor == _colisorApontado) return;
            _colisorApontado = colisor;

            IAlvoDoRaio alvo = colisor != null ? colisor.GetComponentInParent<IAlvoDoRaio>() : null;
            IPegavel pegavel = colisor != null ? colisor.GetComponentInParent<IPegavel>() : null;
            IDestacavel destacavel = colisor != null ? colisor.GetComponentInParent<IDestacavel>() : null;

            if (destacavel != _destacavel)
            {
                _destacavel?.Destacar(false);
                _destacavel = destacavel;
                _destacavel?.Destacar(true);
            }

            bool acendeu = (alvo != null || pegavel != null) && (alvo != _alvo || pegavel != _pegavel);
            _alvo = alvo;
            _pegavel = pegavel;
            if (_linha != null) _linha.sharedMaterial = NoAlvo ? _materialNoAlvo : _materialLivre;
            if (acendeu) Vibrar(Vibracao.AoAcender);
        }

        private void Vibrar(PulsoDeVibracao pulso)
        {
            if (!Application.isPlaying) return;
            OVRInput.SetControllerVibration(pulso.Frequencia, pulso.Forca, _controle);
            _fimDaVibracao = Time.unscaledTime + Mathf.Min(pulso.Duracao, Vibracao.DuracaoMaxima);
        }

        private void PararDeVibrar()
        {
            if (_fimDaVibracao < 0f) return;
            OVRInput.SetControllerVibration(0f, 0f, _controle);
            _fimDaVibracao = -1f;
        }
    }
}
