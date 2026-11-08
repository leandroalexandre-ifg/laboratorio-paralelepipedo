using System.Collections.Generic;
using LaboratorioParalelepipedo.Nucleo;
using LaboratorioParalelepipedo.Nucleo.Dados;
using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>
    /// Carrega textos.json e desafios.json uma vez, no Awake, e os entrega aos outros componentes da cena.
    /// Se um arquivo tiver erro, registra a mensagem completa no log e segue com dados vazios
    /// (os painéis mostram "[chave]" no lugar do texto).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class DadosDoLaboratorio : MonoBehaviour
    {
        [SerializeField] private TextAsset _textos;
        [SerializeField] private TextAsset _desafios;

        private CatalogoDeTextos _catalogo;
        private IReadOnlyList<Desafio> _listaDeDesafios;

        /// <summary>Textos do aluno. Nunca é nulo.</summary>
        public CatalogoDeTextos Catalogo
        {
            get
            {
                Carregar();
                return _catalogo;
            }
        }

        /// <summary>Desafios na ordem do arquivo. Nunca é nulo.</summary>
        public IReadOnlyList<Desafio> Desafios
        {
            get
            {
                Carregar();
                return _listaDeDesafios;
            }
        }

        /// <summary>Liga os arquivos (usado pelo gerador da cena e pelos testes).</summary>
        public void Configurar(TextAsset textos, TextAsset desafios)
        {
            _textos = textos;
            _desafios = desafios;
            _catalogo = null;
            _listaDeDesafios = null;
        }

        private void Awake() => Carregar();

        private void Carregar()
        {
            if (_catalogo != null) return;

            try
            {
                _catalogo = CarregadorDeDados.CarregarTextos(_textos);
            }
            catch (DadosInvalidosException e)
            {
                Debug.LogError(e.Message, this);
                _catalogo = CatalogoDeTextos.Vazio();
            }

            try
            {
                _listaDeDesafios = CarregadorDeDados.CarregarDesafios(_desafios);
            }
            catch (DadosInvalidosException e)
            {
                Debug.LogError(e.Message, this);
                _listaDeDesafios = new Desafio[0];
            }
        }
    }
}
