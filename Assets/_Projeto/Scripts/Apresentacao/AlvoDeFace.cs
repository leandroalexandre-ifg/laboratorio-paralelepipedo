using System;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Face da caixa aberta (Estação 3) que o gatilho escolhe: acende com o raio e avisa quem escuta. A cor de
    /// fundo (neutra ou a do par) é definida por quem a usa; só troca o material quando o estado muda.
    /// </summary>
    public sealed class AlvoDeFace : MonoBehaviour, IAlvoDoRaio, IDestacavel
    {
        [SerializeField] private string _nome;
        [SerializeField] private Renderer _renderizador;
        [SerializeField] private Material _apontada;

        private Material _base;
        private int _apontadores;

        /// <summary>Nome da face (ex.: "ABCD").</summary>
        public string Nome => _nome;

        /// <summary>Se algum raio aponta para ela agora.</summary>
        public bool Apontada => _apontadores > 0;

        /// <summary>O gatilho escolheu esta face.</summary>
        public event Action<AlvoDeFace> Acionado;

        public void Configurar(string nome, Renderer renderizador, Material baseDaFace, Material apontada)
        {
            _nome = nome;
            _renderizador = renderizador;
            _apontada = apontada;
            DefinirMaterial(baseDaFace);
        }

        /// <summary>Material da face quando nenhum raio aponta (neutro ou a cor do par).</summary>
        public void DefinirMaterial(Material baseDaFace)
        {
            _base = baseDaFace;
            Aplicar();
        }

        public void AoAcionar() => Acionado?.Invoke(this);

        public void Destacar(bool apontado)
        {
            _apontadores = Mathf.Max(0, _apontadores + (apontado ? 1 : -1));
            Aplicar();
        }

        private void OnDisable()
        {
            _apontadores = 0;
            Aplicar();
        }

        private void Aplicar()
        {
            if (_renderizador == null) return;
            Material material = _apontadores > 0 && _apontada != null ? _apontada : _base;
            if (material != null && _renderizador.sharedMaterial != material) _renderizador.sharedMaterial = material;
        }
    }
}
