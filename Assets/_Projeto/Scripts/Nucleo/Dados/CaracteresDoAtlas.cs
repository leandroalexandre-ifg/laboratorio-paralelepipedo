using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>
    /// Caracteres que o atlas estático da fonte precisa ter: todos os valores de texto dos JSON de dados
    /// mais os caracteres extras (Assets/_Projeto/Fontes/caracteres-extras.txt). Mesma regra do
    /// Tools/verificar_glifos.py.
    /// </summary>
    public static class CaracteresDoAtlas
    {
        /// <summary>Caracteres distintos, em ordem de código, sem caracteres de controle.</summary>
        /// <exception cref="DadosInvalidosException">Se algum JSON estiver mal formado.</exception>
        public static string Coletar(string extras, params string[] jsons)
        {
            var conjunto = new SortedSet<char>();
            Adicionar(conjunto, extras);
            foreach (string json in jsons)
            {
                NoJson raiz;
                try
                {
                    raiz = AnalisadorJson.Analisar(json ?? string.Empty);
                }
                catch (JsonMalFormadoException e)
                {
                    throw new DadosInvalidosException("JSON de dados",
                        $"o JSON está mal formado na linha {e.Linha}, coluna {e.Coluna}: {e.Motivo}");
                }

                AdicionarTextos(conjunto, raiz);
            }

            var resultado = new StringBuilder(conjunto.Count);
            foreach (char c in conjunto) resultado.Append(c);
            return resultado.ToString();
        }

        /// <summary>Caracteres de <paramref name="texto"/> que não estão em <paramref name="atlas"/>, sem repetir.</summary>
        public static string QueFaltam(string texto, string atlas)
        {
            var faltando = new SortedSet<char>();
            foreach (char c in texto ?? string.Empty)
                if (!EhControle(c) && atlas.IndexOf(c) < 0)
                    faltando.Add(c);

            var resultado = new StringBuilder();
            foreach (char c in faltando) resultado.Append(c);
            return resultado.ToString();
        }

        private static void AdicionarTextos(SortedSet<char> conjunto, NoJson no)
        {
            switch (no.Tipo)
            {
                case TipoDeNoJson.Texto:
                case TipoDeNoJson.Numero:
                    Adicionar(conjunto, no.Texto);
                    break;
                case TipoDeNoJson.Objeto:
                    foreach (var membro in no.Membros) AdicionarTextos(conjunto, membro.Valor);
                    break;
                case TipoDeNoJson.Lista:
                    foreach (var item in no.Itens) AdicionarTextos(conjunto, item);
                    break;
            }
        }

        private static void Adicionar(SortedSet<char> conjunto, string texto)
        {
            foreach (char c in texto ?? string.Empty)
                if (!EhControle(c))
                    conjunto.Add(c);
        }

        private static bool EhControle(char c)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            return categoria == UnicodeCategory.Control || categoria == UnicodeCategory.Format ||
                   categoria == UnicodeCategory.Surrogate || categoria == UnicodeCategory.PrivateUse ||
                   categoria == UnicodeCategory.OtherNotAssigned;
        }
    }
}
