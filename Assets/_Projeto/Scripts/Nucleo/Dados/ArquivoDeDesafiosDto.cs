using System;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>Formato de desafios.json: <c>{ "versao": 1, "desafios": [ ... ] }</c>.</summary>
    [Serializable]
    public sealed class ArquivoDeDesafiosDto
    {
        public int versao;
        public DesafioDto[] desafios;
    }
}
