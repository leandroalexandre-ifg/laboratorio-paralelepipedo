using LaboratorioParalelepipedo.Nucleo.Interacao;
using TMPro;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Dica curta logo acima de um controle, virada para o aluno: "Aperte o gatilho para escolher" com o raio num
    /// vértice ou botão, "Segure o botão lateral para pegar" com o raio na caixa, "Solte para ir até lá" ou "Aqui não dá" ao mirar o teleporte (regra em <see cref="DicasDosControles"/>,
    /// textos no textos.json). O texto só é trocado quando a dica muda; a cada quadro só a posição e a direção.
    /// </summary>
    public sealed class DicaDaMao : MonoBehaviour
    {
        private const float AlturaAcimaDaMao = 0.09f;

        [SerializeField] private DadosDoLaboratorio _dados;
        [SerializeField, Tooltip("Âncora do controle: a dica fica logo acima dela.")]
        private Transform _mao;
        [SerializeField, Tooltip("CenterEyeAnchor: a dica fica virada para a cabeça.")]
        private Transform _cabeca;
        [SerializeField, Tooltip("Filho com o texto e o fundo: liga e desliga com a dica.")]
        private GameObject _conteudo;
        [SerializeField] private TMP_Text _texto;
        [SerializeField] private Transform _fundo;

        private string _chaveMostrada;
        private bool _iniciada;

        /// <summary>O raio desta mão está numa coisa que se pega.</summary>
        public bool ApontandoParaPegar { get; set; }

        /// <summary>O raio desta mão está num alvo do gatilho (vértice, botão).</summary>
        public bool ApontandoParaEscolher { get; set; }

        /// <summary>Esta mão está segurando algo.</summary>
        public bool Segurando { get; set; }

        /// <summary>Esta mão está mirando o teleporte.</summary>
        public bool MirandoTeleporte { get; private set; }

        /// <summary>O destino do teleporte desta mão vale.</summary>
        public bool TeleporteValido { get; private set; }

        /// <summary>Chave do texto mostrado agora (<c>null</c>: nenhuma dica).</summary>
        public string ChaveMostrada => _chaveMostrada;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(DadosDoLaboratorio dados, Transform mao, Transform cabeca, GameObject conteudo,
            TMP_Text texto, Transform fundo)
        {
            _dados = dados;
            _mao = mao;
            _cabeca = cabeca;
            _conteudo = conteudo;
            _texto = texto;
            _fundo = fundo;
        }

        /// <summary>Começa ou para de mirar o teleporte, dizendo se o destino vale.</summary>
        public void Mirar(bool mirando, bool valido)
        {
            MirandoTeleporte = mirando;
            TeleporteValido = valido;
        }

        private void LateUpdate() => Atualizar();

        /// <summary>Escolhe a dica e a põe acima da mão (público para os testes no Editor).</summary>
        public void Atualizar()
        {
            string chave = DicasDosControles.Chave(MirandoTeleporte, TeleporteValido, ApontandoParaPegar, Segurando, ApontandoParaEscolher);
            if (!_iniciada || !ReferenceEquals(chave, _chaveMostrada)) Mostrar(chave);
            if (chave == null || _mao == null) return;

            Vector3 posicao = _mao.position + Vector3.up * AlturaAcimaDaMao;
            Vector3 paraLonge = _cabeca != null ? posicao - _cabeca.position : _mao.forward;
            paraLonge.y = 0f;
            if (paraLonge.sqrMagnitude < 1e-6f) paraLonge = Vector3.forward;
            transform.SetPositionAndRotation(posicao, Quaternion.LookRotation(paraLonge, Vector3.up));
        }

        private void Mostrar(string chave)
        {
            _iniciada = true;
            _chaveMostrada = chave;
            if (_conteudo != null) _conteudo.SetActive(chave != null);
            if (chave == null || _texto == null || _dados == null) return;

            _texto.text = _dados.Catalogo.Obter(chave);
            if (_fundo == null) return;
            _texto.ForceMeshUpdate();
            Vector2 tamanho = _texto.GetRenderedValues(false);
            _fundo.localScale = new Vector3(tamanho.x + 0.03f, tamanho.y + 0.02f, 0.002f);
        }
    }
}
