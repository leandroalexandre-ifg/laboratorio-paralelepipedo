using System;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Vértice da caixa que o gatilho escolhe. Acende em azul com o raio, fica azul quando é a ponta escolhida,
    /// pulsa (cresce e volta, em azul) quando é o vértice que a instrução pede e pisca em laranja
    /// (<see cref="DuracaoDaPiscada"/> s) quando não é o vértice pedido, sem penalidade. Só troca o material quando
    /// o estado muda.
    /// </summary>
    public sealed class AlvoDeVertice : MonoBehaviour, IAlvoDoRaio, IDestacavel
    {
        public const float DuracaoDaPiscada = 0.9f;
        private const float MeioPeriodo = 0.15f;

        /// <summary>Um pulso (crescer e voltar) a cada <see cref="PeriodoDoPulso"/> s.</summary>
        public const float PeriodoDoPulso = 1f;

        /// <summary>Quanto a esfera cresce no auge do pulso (0,6 = 60%).</summary>
        public const float CrescimentoDoPulso = 0.6f;

        [SerializeField] private string _nome;
        [SerializeField] private Renderer _esfera;
        [SerializeField] private Material _normal;
        [SerializeField] private Material _apontado;
        [SerializeField] private Material _errado;

        private int _apontadores;
        private bool _escolhido;
        private float _piscando = -1f;
        private bool _pulsando;
        private float _tempoDoPulso;
        private Vector3 _escalaNormal;

        /// <summary>Nome do vértice (A a H).</summary>
        public string Nome => _nome;

        /// <summary>Se está piscando agora (para os testes).</summary>
        public bool Piscando => _piscando >= 0f;

        /// <summary>Se é o vértice que a instrução pede agora.</summary>
        public bool Pulsando => _pulsando;

        /// <summary>O gatilho escolheu este vértice.</summary>
        public event Action<AlvoDeVertice> Acionado;

        public void Configurar(string nome, Renderer esfera, Material normal, Material apontado, Material errado)
        {
            _nome = nome;
            _esfera = esfera;
            _normal = normal;
            _apontado = apontado;
            _errado = errado;
            _escalaNormal = transform.localScale;
            Aplicar();
        }

        public void AoAcionar() => Acionado?.Invoke(this);

        public void Destacar(bool apontado)
        {
            _apontadores = Mathf.Max(0, _apontadores + (apontado ? 1 : -1));
            Aplicar();
        }

        /// <summary>Marca (ou desmarca) como a ponta já escolhida do segmento.</summary>
        public void DefinirEscolhido(bool escolhido)
        {
            _escolhido = escolhido;
            Aplicar();
        }

        /// <summary>Liga ou desliga o pulso (o vértice que a instrução pede).</summary>
        public void DefinirPulsando(bool pulsando)
        {
            if (_pulsando == pulsando) return;
            _pulsando = pulsando;
            _tempoDoPulso = 0f;
            if (!pulsando) transform.localScale = _escalaNormal;
            Aplicar();
        }

        /// <summary>Avança o pulso e a piscada (o Update chama; público para a prévia e os testes).</summary>
        public void Avancar(float segundos)
        {
            if (_pulsando)
            {
                _tempoDoPulso = (_tempoDoPulso + segundos) % PeriodoDoPulso;
                float onda = 0.5f - 0.5f * Mathf.Cos(_tempoDoPulso / PeriodoDoPulso * 2f * Mathf.PI);
                transform.localScale = _escalaNormal * (1f + CrescimentoDoPulso * onda);
            }

            if (_piscando < 0f) return;
            _piscando += segundos;
            if (_piscando >= DuracaoDaPiscada) _piscando = -1f;
            Aplicar();
        }

        /// <summary>Pisca em laranja: não é o vértice pedido.</summary>
        public void Piscar()
        {
            _piscando = 0f;
            Aplicar();
        }

        private void Update() => Avancar(Time.deltaTime);

        private void Aplicar()
        {
            if (_esfera == null) return;
            Material material;
            if (_piscando >= 0f) material = ((int)(_piscando / MeioPeriodo)) % 2 == 0 ? _errado : _normal;
            else if (_escolhido || _apontadores > 0 || _pulsando) material = _apontado;
            else material = _normal;
            if (material != null && _esfera.sharedMaterial != material) _esfera.sharedMaterial = material;
        }
    }
}
