using TMPro;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Mostra num TextMeshPro os textos do catálogo com as chaves dadas, um por linha.
    /// O texto é montado uma vez, no Start; nada roda por frame.
    /// </summary>
    public sealed class TextoDoCatalogo : MonoBehaviour
    {
        [SerializeField] private DadosDoLaboratorio _dados;
        [SerializeField] private TMP_Text _texto;
        [SerializeField] private string[] _chaves = new string[0];

        /// <summary>Chaves mostradas, na ordem.</summary>
        public string[] Chaves => _chaves;

        /// <summary>Liga o componente (usado pelo gerador da cena e pelos testes).</summary>
        public void Configurar(DadosDoLaboratorio dados, TMP_Text texto, params string[] chaves)
        {
            _dados = dados;
            _texto = texto;
            _chaves = chaves;
        }

        private void Start() => Atualizar();

        /// <summary>Lê o catálogo e escreve o texto.</summary>
        public void Atualizar()
        {
            if (_dados == null || _texto == null)
            {
                Debug.LogError($"{name}: TextoDoCatalogo sem DadosDoLaboratorio ou sem TextMeshPro ligado.", this);
                return;
            }

            var catalogo = _dados.Catalogo;
            var linhas = new string[_chaves.Length];
            for (int i = 0; i < _chaves.Length; i++) linhas[i] = catalogo.Obter(_chaves[i]);
            _texto.text = string.Join("\n", linhas);
        }
    }
}
