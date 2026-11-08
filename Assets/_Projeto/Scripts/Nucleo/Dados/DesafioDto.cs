using System;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>
    /// Um desafio de desafios.json, como o professor escreve. Medidas, distratores e tolerância são texto
    /// (ex.: "2,5", "√13"), lidos pelo <see cref="InterpretadorDeMedida"/>.
    /// Campos comuns: id, tipo ("resposta", o padrão, ou "montarFace"), estacao, bncc, enunciado, dimensoes.
    /// Só em "resposta": grandeza, distratores, tolerancia. Só em "montarFace": face.
    /// </summary>
    [Serializable]
    public sealed class DesafioDto
    {
        public string id;
        public string tipo;
        public string estacao;
        public string bncc;
        public string enunciado;
        public DimensoesDto dimensoes;
        public string grandeza;
        public string[] distratores;
        public string tolerancia;
        public string face;
    }
}
