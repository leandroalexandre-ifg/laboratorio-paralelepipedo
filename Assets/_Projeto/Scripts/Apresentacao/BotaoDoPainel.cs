using System;
using TMPro;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Botão grande num painel, apertado com o gatilho do raio: fundo com colisor, texto e cores de estado
    /// (<see cref="EstadoDoBotao"/>). Acende em azul com o raio. Escondido, o colisor some junto.
    /// </summary>
    public sealed class BotaoDoPainel : MonoBehaviour, IAlvoDoRaio, IDestacavel
    {
        [SerializeField] private Renderer _fundo;
        [SerializeField] private TMP_Text _texto;
        [SerializeField] private Material _normal;
        [SerializeField] private Material _apontado;
        [SerializeField] private Material _certo;
        [SerializeField] private Material _errado;
        [SerializeField] private Color _corDoTextoNormal = new Color(0.106f, 0.165f, 0.227f);

        private int _apontadores;
        private EstadoDoBotao _estado;

        /// <summary>Posição do botão num grupo (ex.: a opção 0 a 3 da pergunta).</summary>
        public int Indice { get; set; }

        public EstadoDoBotao Estado => _estado;

        public string Texto => _texto != null ? _texto.text : null;

        /// <summary>O gatilho apertou este botão.</summary>
        public event Action<BotaoDoPainel> Acionado;

        public void Configurar(Renderer fundo, TMP_Text texto, Material normal, Material apontado, Material certo, Material errado,
            Color corDoTextoNormal)
        {
            _corDoTextoNormal = corDoTextoNormal;
            _fundo = fundo;
            _texto = texto;
            _normal = normal;
            _apontado = apontado;
            _certo = certo;
            _errado = errado;
            Aplicar();
        }

        public void AoAcionar()
        {
            if (_estado == EstadoDoBotao.Certo) return;
            Acionado?.Invoke(this);
        }

        public void Destacar(bool apontado)
        {
            _apontadores = Mathf.Max(0, _apontadores + (apontado ? 1 : -1));
            Aplicar();
        }

        private void OnDisable() => _apontadores = 0;

        /// <summary>Mostra ou esconde o botão (e o colisor).</summary>
        public void Mostrar(bool visivel)
        {
            if (gameObject.activeSelf != visivel) gameObject.SetActive(visivel);
        }

        public void DefinirTexto(string texto)
        {
            if (_texto != null && _texto.text != texto) _texto.text = texto;
        }

        public void DefinirEstado(EstadoDoBotao estado)
        {
            _estado = estado;
            Aplicar();
        }

        private void Aplicar()
        {
            Material material = _estado == EstadoDoBotao.Certo ? _certo
                : _estado == EstadoDoBotao.Errado ? _errado
                : _apontadores > 0 ? _apontado : _normal;
            if (_fundo != null && material != null && _fundo.sharedMaterial != material) _fundo.sharedMaterial = material;
            if (_texto != null)
            {
                Color cor = _estado == EstadoDoBotao.Normal && _apontadores == 0 ? _corDoTextoNormal : Color.white;
                if (_texto.color != cor) _texto.color = cor;
            }
        }
    }
}
