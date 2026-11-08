using TMPro;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Mostra a versão do app (Application.version, a "versionName" do APK) no texto do catálogo com a chave dada,
    /// por exemplo "Versão {0}". Assim dá para conferir no óculos qual build está rodando. Montado uma vez, no Start.
    /// </summary>
    public sealed class TextoDaVersao : MonoBehaviour
    {
        [SerializeField] private DadosDoLaboratorio _dados;
        [SerializeField] private TMP_Text _texto;
        [SerializeField] private string _chave = Nucleo.Dados.ChavesDeTexto.AppVersao;

        /// <summary>Chave do texto com o lugar da versão ({0}).</summary>
        public string Chave => _chave;

        /// <summary>Liga o componente (usado pelo gerador da cena e pelos testes).</summary>
        public void Configurar(DadosDoLaboratorio dados, TMP_Text texto, string chave)
        {
            _dados = dados;
            _texto = texto;
            _chave = chave;
        }

        private void Start() => Atualizar();

        /// <summary>Lê o catálogo e escreve o texto com a versão.</summary>
        public void Atualizar()
        {
            if (_dados == null || _texto == null) return;
            _texto.text = string.Format(_dados.Catalogo.Obter(_chave), Application.version);
        }
    }
}
