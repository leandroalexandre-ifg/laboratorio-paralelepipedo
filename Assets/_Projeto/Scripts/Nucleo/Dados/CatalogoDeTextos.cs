using System;
using System.Collections.Generic;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>Textos mostrados ao aluno, por chave (ver <see cref="ChavesDeTexto"/>).</summary>
    public sealed class CatalogoDeTextos
    {
        private readonly Dictionary<string, string> _textos;

        internal CatalogoDeTextos(Dictionary<string, string> textos)
        {
            _textos = textos ?? throw new ArgumentNullException(nameof(textos));
        }

        public int Quantidade => _textos.Count;

        public IEnumerable<string> Chaves => _textos.Keys;

        public bool Contem(string chave) => chave != null && _textos.ContainsKey(chave);

        public bool TentarObter(string chave, out string texto)
        {
            texto = null;
            return chave != null && _textos.TryGetValue(chave, out texto);
        }

        /// <summary>
        /// Texto da chave. Se a chave faltar, devolve "[chave]" para o erro aparecer na tela sem travar o app
        /// (os testes garantem que o textos.json tem todas as chaves).
        /// </summary>
        public string Obter(string chave) => TentarObter(chave, out var texto) ? texto : $"[{chave}]";
    }
}
