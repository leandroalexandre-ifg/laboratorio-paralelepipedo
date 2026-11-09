using LaboratorioParalelepipedo.Nucleo.Interacao;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// A caixa que o aluno pega com o botão lateral para olhar por todos os lados. Ao soltar, volta à mesa em
    /// <see cref="TransicaoSuave.DuracaoDoRetornoDaCaixa"/> segundos, com começo e fim suaves e na orientação
    /// original (a convenção dos vértices A–H não se perde). Acende as arestas quando um raio aponta para ela.
    /// Sem alocação por frame.
    /// </summary>
    [RequireComponent(typeof(VisualDaCaixa), typeof(BoxCollider))]
    public sealed class CaixaPegavel : MonoBehaviour, IPegavel, IDestacavel
    {
        [SerializeField] private Material _materialDaArestaDestacada;

        private readonly TransicaoSuave _retorno = new TransicaoSuave(TransicaoSuave.DuracaoDoRetornoDaCaixa);
        private VisualDaCaixa _visual;
        private BoxCollider _colisor;
        private Vector3 _posicaoNaMesa;
        private Quaternion _rotacaoNaMesa;
        private Transform _mao;
        private Vector3 _posicaoNaMao;
        private Quaternion _rotacaoNaMao;
        private Vector3 _posicaoAoSoltar;
        private Quaternion _rotacaoAoSoltar;
        private MeshRenderer[] _arestas;
        private Material _materialDaAresta;
        private int _raiosApontando;
        private bool _posicaoGuardada;

        /// <summary>Se está voltando para a mesa agora.</summary>
        public bool Voltando => _retorno.Ativa;

        /// <summary>Se alguma mão está segurando.</summary>
        public bool NaMao => _mao != null;

        /// <summary>Se algum raio aponta para ela agora.</summary>
        public bool Destacada => _raiosApontando > 0;

        /// <summary>Liga o material de destaque (usado pelo gerador da cena e pelos testes).</summary>
        public void Configurar(Material arestaDestacada) => _materialDaArestaDestacada = arestaDestacada;

        private void Awake() => GuardarPosicaoNaMesa();

        /// <summary>Guarda a posição e a orientação atuais como as da mesa, para onde a caixa volta ao ser solta.</summary>
        public void GuardarPosicaoNaMesa()
        {
            Preparar();
            _posicaoNaMesa = transform.position;
            _rotacaoNaMesa = transform.rotation;
            _posicaoGuardada = true;
        }

        private void Preparar()
        {
            if (_visual == null) _visual = GetComponent<VisualDaCaixa>();
            if (_colisor == null) _colisor = GetComponent<BoxCollider>();
        }

        /// <summary>Faz o colisor cobrir a caixa desenhada (chamar depois de cada <see cref="VisualDaCaixa.Construir"/>).</summary>
        public void AjustarColisor()
        {
            Preparar();
            Vector3 tamanho = _visual.TamanhoNaCena;
            _colisor.size = tamanho;
            _colisor.center = new Vector3(0f, tamanho.y / 2, 0f);
            _arestas = null;
        }

        /// <summary>Posição da caixa na mesa (para onde ela volta ao ser solta).</summary>
        /// Antes do Awake (no Editor parado), é a posição atual, que é a da mesa.
        public Vector3 PosicaoNaMesa => _posicaoGuardada ? _posicaoNaMesa : transform.position;

        /// <summary>Orientação da caixa na mesa.</summary>
        public Quaternion RotacaoNaMesa => _posicaoGuardada ? _rotacaoNaMesa : transform.rotation;

        /// <summary>Se o botão lateral consegue pegar a caixa (desligado com a caixa aberta na Estação 3).</summary>
        public bool Pegavel => _colisor == null || _colisor.enabled;

        /// <summary>
        /// Liga ou desliga a pega: desligada, o colisor some (o raio passa) e, se estava na mão, a caixa volta à mesa
        /// na hora.
        /// </summary>
        public void DefinirPegavel(bool pegavel)
        {
            Preparar();
            if (!pegavel && _mao != null)
            {
                _mao = null;
                _retorno.Parar();
                transform.SetPositionAndRotation(_posicaoNaMesa, _rotacaoNaMesa);
            }
            if (_colisor != null && _colisor.enabled != pegavel) _colisor.enabled = pegavel;
        }

        public void Pegar(Transform mao)
        {
            if (mao == null || !Pegavel) return;
            _retorno.Parar();
            _mao = mao;
            Quaternion inversa = Quaternion.Inverse(mao.rotation);
            _posicaoNaMao = inversa * (transform.position - mao.position);
            _rotacaoNaMao = inversa * transform.rotation;
        }

        public void Soltar(Transform mao)
        {
            if (mao == null || mao != _mao) return;
            _mao = null;
            _posicaoAoSoltar = transform.position;
            _rotacaoAoSoltar = transform.rotation;
            _retorno.Iniciar();
        }

        public bool EstaNaMaoDe(Transform mao) => mao != null && mao == _mao;

        /// <summary>Avança a volta para a mesa (o LateUpdate chama; público para os testes no Editor).</summary>
        public void Avancar(float segundos)
        {
            if (_mao != null)
            {
                transform.SetPositionAndRotation(_mao.position + _mao.rotation * _posicaoNaMao, _mao.rotation * _rotacaoNaMao);
                return;
            }

            if (!_retorno.Ativa) return;
            float fracao = (float)_retorno.Avancar(segundos);
            transform.SetPositionAndRotation(Vector3.LerpUnclamped(_posicaoAoSoltar, _posicaoNaMesa, fracao),
                Quaternion.SlerpUnclamped(_rotacaoAoSoltar, _rotacaoNaMesa, fracao));
        }

        private void LateUpdate() => Avancar(Time.deltaTime);

        public void Destacar(bool apontado)
        {
            int antes = _raiosApontando;
            _raiosApontando = Mathf.Max(0, _raiosApontando + (apontado ? 1 : -1));
            if ((antes > 0) == (_raiosApontando > 0) || _materialDaArestaDestacada == null) return;

            if (_arestas == null || _arestas.Length == 0 || _arestas[0] == null)
            {
                Transform grupo = transform.Find(VisualDaCaixa.GrupoArestas);
                _arestas = grupo != null ? grupo.GetComponentsInChildren<MeshRenderer>() : new MeshRenderer[0];
                if (_arestas.Length > 0 && _arestas[0].sharedMaterial != _materialDaArestaDestacada)
                    _materialDaAresta = _arestas[0].sharedMaterial;
            }

            Material material = _raiosApontando > 0 ? _materialDaArestaDestacada : _materialDaAresta;
            foreach (var aresta in _arestas) aresta.sharedMaterial = material;
        }

        /// <summary>Volume da caixa para as regras do teleporte (o chão embaixo dela não vale).</summary>
        public ObstaculoDoTeleporte ObstaculoNoPiso()
        {
            Preparar();
            Bounds limites = _colisor.bounds;
            return new ObstaculoDoTeleporte(
                new AreaRetangular(limites.center.x, limites.center.z, limites.size.x, limites.size.z),
                limites.min.y, limites.max.y, MotivoDoTeleporte.NaCaixa);
        }
    }
}
