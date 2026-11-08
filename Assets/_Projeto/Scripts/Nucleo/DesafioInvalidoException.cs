using System;

namespace LaboratorioParalelepipedo.Nucleo
{
    /// <summary>
    /// Erro nos dados de um desafio, com mensagem clara em português para o professor que o escreveu.
    /// </summary>
    public sealed class DesafioInvalidoException : ArgumentException
    {
        /// <summary>Id do desafio com problema (pode ser vazio, se o próprio id faltar).</summary>
        public string DesafioId { get; }

        public DesafioInvalidoException(string desafioId, string problema)
            : base(string.IsNullOrWhiteSpace(desafioId)
                ? $"Desafio sem id: {problema}"
                : $"Desafio \"{desafioId}\": {problema}")
        {
            DesafioId = desafioId ?? string.Empty;
        }
    }
}
