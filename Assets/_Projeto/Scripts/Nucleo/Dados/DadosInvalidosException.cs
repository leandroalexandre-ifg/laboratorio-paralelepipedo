using System;
using System.Collections.Generic;
using System.Text;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>
    /// Erros encontrados ao ler um arquivo de dados (textos ou desafios). Traz todos os erros de uma vez,
    /// para o professor corrigir o arquivo inteiro sem precisar tentar várias vezes.
    /// </summary>
    public sealed class DadosInvalidosException : Exception
    {
        public string Arquivo { get; }
        public IReadOnlyList<string> Erros { get; }

        public DadosInvalidosException(string arquivo, IReadOnlyList<string> erros)
            : base(Montar(arquivo, erros))
        {
            Arquivo = arquivo;
            Erros = erros;
        }

        public DadosInvalidosException(string arquivo, string erro) : this(arquivo, new[] { erro })
        {
        }

        private static string Montar(string arquivo, IReadOnlyList<string> erros)
        {
            var texto = new StringBuilder();
            texto.Append(arquivo).Append(erros.Count == 1 ? " tem 1 erro:" : $" tem {erros.Count} erros:");
            foreach (var erro in erros) texto.Append("\n- ").Append(erro);
            return texto.ToString();
        }
    }
}
