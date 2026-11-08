using System;

namespace LaboratorioParalelepipedo.Nucleo.Interacao
{
    /// <summary>
    /// Lê o analógico de um controle quadro a quadro. Para a frente: mira o teleporte, que acontece ao soltar
    /// (para trás cancela). Para o lado: um único passo de giro por empurrão; o próximo só depois de o analógico
    /// voltar ao centro. Enquanto mira, o lado não gira. Sem alocação.
    /// </summary>
    public sealed class ComandoDoAnalogico
    {
        /// <summary>Quanto o analógico precisa ir (0 a 1) para valer como empurrão.</summary>
        public const double LimiarDeAcionamento = 0.7;

        /// <summary>Abaixo disto o analógico conta como de volta ao centro.</summary>
        public const double LimiarDeRepouso = 0.3;

        private bool _esperandoOCentro;

        /// <summary>Se o aluno está mirando o teleporte com este controle.</summary>
        public bool Mirando { get; private set; }

        /// <summary>Avança um quadro com a posição do analógico (x: direita +, y: frente +).</summary>
        public AcaoDoAnalogico Atualizar(double x, double y)
        {
            double tamanho = Math.Sqrt(x * x + y * y);

            if (Mirando)
            {
                if (y < -LimiarDeAcionamento)
                {
                    Mirando = false;
                    _esperandoOCentro = true;
                    return AcaoDoAnalogico.CancelarMira;
                }

                if (tamanho >= LimiarDeRepouso) return AcaoDoAnalogico.Nenhuma;
                Mirando = false;
                return AcaoDoAnalogico.Teleportar;
            }

            if (_esperandoOCentro)
            {
                if (tamanho < LimiarDeRepouso) _esperandoOCentro = false;
                return AcaoDoAnalogico.Nenhuma;
            }

            if (y > LimiarDeAcionamento && Math.Abs(x) < y)
            {
                Mirando = true;
                return AcaoDoAnalogico.ComecarAMirar;
            }

            if (Math.Abs(x) > LimiarDeAcionamento && Math.Abs(y) < Math.Abs(x))
            {
                _esperandoOCentro = true;
                return x > 0 ? AcaoDoAnalogico.GirarParaADireita : AcaoDoAnalogico.GirarParaAEsquerda;
            }

            return AcaoDoAnalogico.Nenhuma;
        }

        /// <summary>Esquece a mira e a espera (controle desconectado, por exemplo).</summary>
        public void Reiniciar()
        {
            Mirando = false;
            _esperandoOCentro = false;
        }
    }
}
