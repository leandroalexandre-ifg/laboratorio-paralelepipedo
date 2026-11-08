using System;
using System.Collections.Generic;
using System.Text;

namespace LaboratorioParalelepipedo.Nucleo.Dados
{
    /// <summary>Tipo de um nó do JSON bruto.</summary>
    internal enum TipoDeNoJson
    {
        Objeto,
        Lista,
        Texto,
        Numero,
        Logico,
        Nulo,
    }

    /// <summary>Par chave → valor de um objeto JSON, com a posição da chave no arquivo.</summary>
    internal readonly struct MembroJson
    {
        public MembroJson(string chave, NoJson valor, int linha)
        {
            Chave = chave;
            Valor = valor;
            Linha = linha;
        }

        public string Chave { get; }
        public NoJson Valor { get; }
        public int Linha { get; }
    }

    /// <summary>Nó do JSON bruto: guarda as chaves na ordem do arquivo (o JsonUtility não as mostra).</summary>
    internal sealed class NoJson
    {
        public TipoDeNoJson Tipo { get; }
        public string Texto { get; }
        public List<MembroJson> Membros { get; }
        public List<NoJson> Itens { get; }

        private NoJson(TipoDeNoJson tipo, string texto = null)
        {
            Tipo = tipo;
            Texto = texto;
            if (tipo == TipoDeNoJson.Objeto) Membros = new List<MembroJson>();
            if (tipo == TipoDeNoJson.Lista) Itens = new List<NoJson>();
        }

        public static NoJson Objeto() => new NoJson(TipoDeNoJson.Objeto);
        public static NoJson Lista() => new NoJson(TipoDeNoJson.Lista);
        public static NoJson DeTexto(string texto) => new NoJson(TipoDeNoJson.Texto, texto);
        public static NoJson DeNumero(string texto) => new NoJson(TipoDeNoJson.Numero, texto);
        public static NoJson Logico(string texto) => new NoJson(TipoDeNoJson.Logico, texto);
        public static NoJson Nulo() => new NoJson(TipoDeNoJson.Nulo);

        /// <summary>Valor de texto da chave no objeto (ex.: o "id"), ou null.</summary>
        public string TextoDe(string chave)
        {
            if (Membros == null) return null;
            foreach (var membro in Membros)
                if (membro.Chave == chave && membro.Valor.Tipo == TipoDeNoJson.Texto)
                    return membro.Valor.Texto;
            return null;
        }
    }

    /// <summary>JSON mal formado, com linha, coluna e motivo em português.</summary>
    internal sealed class JsonMalFormadoException : Exception
    {
        public int Linha { get; }
        public int Coluna { get; }
        public string Motivo { get; }

        public JsonMalFormadoException(int linha, int coluna, string motivo)
            : base($"linha {linha}, coluna {coluna}: {motivo}")
        {
            Linha = linha;
            Coluna = coluna;
            Motivo = motivo;
        }
    }

    /// <summary>
    /// Analisador JSON mínimo (RFC 8259) em C# puro, só para conferir a estrutura e as chaves dos arquivos de
    /// dados antes do JsonUtility, que ignora chaves desconhecidas sem avisar.
    /// </summary>
    internal sealed class AnalisadorJson
    {
        private readonly string _texto;
        private int _i;

        private AnalisadorJson(string texto)
        {
            _texto = texto;
            if (_texto.Length > 0 && _texto[0] == '\uFEFF') _i = 1; // BOM do UTF-8
        }

        /// <exception cref="JsonMalFormadoException">Se o texto não for um JSON válido.</exception>
        public static NoJson Analisar(string texto)
        {
            var analisador = new AnalisadorJson(texto ?? string.Empty);
            analisador.PularEspacos();
            var raiz = analisador.LerValor();
            analisador.PularEspacos();
            if (analisador._i < analisador._texto.Length)
                throw analisador.Erro("há conteúdo sobrando depois do fim do JSON.");
            return raiz;
        }

        private NoJson LerValor()
        {
            if (_i >= _texto.Length) throw Erro("o arquivo terminou antes da hora.");
            char c = _texto[_i];
            switch (c)
            {
                case '{': return LerObjeto();
                case '[': return LerLista();
                case '"': return NoJson.DeTexto(LerTexto());
                case 't': return LerPalavra("true", NoJson.Logico("true"));
                case 'f': return LerPalavra("false", NoJson.Logico("false"));
                case 'n': return LerPalavra("null", NoJson.Nulo());
                default:
                    if (c == '-' || EhDigito(c)) return LerNumero();
                    throw Erro($"valor inválido começando com \"{c}\" (textos vão entre aspas).");
            }
        }

        private NoJson LerObjeto()
        {
            var objeto = NoJson.Objeto();
            _i++; // {
            PularEspacos();
            if (Atual == '}')
            {
                _i++;
                return objeto;
            }

            while (true)
            {
                PularEspacos();
                if (Atual == '}') throw Erro("vírgula sobrando antes de \"}\".");
                if (Atual != '"') throw Erro("esperava o nome de uma chave entre aspas.");
                int linha = Posicao(_i).linha;
                string chave = LerTexto();
                PularEspacos();
                if (Atual != ':') throw Erro($"esperava \":\" depois da chave \"{chave}\".");
                _i++;
                PularEspacos();
                objeto.Membros.Add(new MembroJson(chave, LerValor(), linha));
                PularEspacos();
                if (Atual == ',')
                {
                    _i++;
                    continue;
                }

                if (Atual == '}')
                {
                    _i++;
                    return objeto;
                }

                throw Erro("falta uma vírgula entre os campos ou um \"}\" para fechar o objeto.");
            }
        }

        private NoJson LerLista()
        {
            var lista = NoJson.Lista();
            _i++; // [
            PularEspacos();
            if (Atual == ']')
            {
                _i++;
                return lista;
            }

            while (true)
            {
                PularEspacos();
                if (Atual == ']') throw Erro("vírgula sobrando antes de \"]\".");
                lista.Itens.Add(LerValor());
                PularEspacos();
                if (Atual == ',')
                {
                    _i++;
                    continue;
                }

                if (Atual == ']')
                {
                    _i++;
                    return lista;
                }

                throw Erro("falta uma vírgula entre os itens ou um \"]\" para fechar a lista.");
            }
        }

        private string LerTexto()
        {
            int inicio = _i;
            _i++; // "
            var sb = new StringBuilder();
            while (_i < _texto.Length)
            {
                char c = _texto[_i];
                if (c == '"')
                {
                    _i++;
                    return sb.ToString();
                }

                if (c == '\n' || c == '\r') throw Erro("o texto não pode ter quebra de linha (feche as aspas).");
                if (c < ' ') throw Erro("caractere de controle dentro de um texto.");
                if (c == '\\')
                {
                    _i++;
                    if (_i >= _texto.Length) break;
                    char e = _texto[_i];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 >= _texto.Length || !int.TryParse(_texto.Substring(_i + 1, 4),
                                    System.Globalization.NumberStyles.HexNumber, null, out int codigo))
                                throw Erro("\\u precisa de 4 dígitos hexadecimais.");
                            sb.Append((char)codigo);
                            _i += 4;
                            break;
                        default: throw Erro($"sequência de escape inválida \"\\{e}\".");
                    }

                    _i++;
                    continue;
                }

                sb.Append(c);
                _i++;
            }

            _i = inicio;
            throw Erro("o texto não tem aspas de fechamento.");
        }

        private NoJson LerNumero()
        {
            int inicio = _i;
            if (Atual == '-') _i++;
            if (!EhDigito(Atual)) throw Erro("número inválido.");
            if (Atual == '0') _i++;
            else
                while (EhDigito(Atual)) _i++;
            if (Atual == '.')
            {
                _i++;
                if (!EhDigito(Atual)) throw Erro("número inválido (use ponto e dígitos depois dele, ou aspas).");
                while (EhDigito(Atual)) _i++;
            }

            if (Atual == 'e' || Atual == 'E')
            {
                _i++;
                if (Atual == '+' || Atual == '-') _i++;
                if (!EhDigito(Atual)) throw Erro("número inválido.");
                while (EhDigito(Atual)) _i++;
            }

            if (Atual == ',' && _i + 1 < _texto.Length && EhDigito(_texto[_i + 1]))
                throw Erro("número com vírgula decimal fora de aspas; escreva-o entre aspas, como \"2,5\".");
            return NoJson.DeNumero(_texto.Substring(inicio, _i - inicio));
        }

        private NoJson LerPalavra(string palavra, NoJson no)
        {
            if (string.CompareOrdinal(_texto, _i, palavra, 0, palavra.Length) != 0)
                throw Erro("valor inválido (textos vão entre aspas).");
            _i += palavra.Length;
            return no;
        }

        private static bool EhDigito(char c) => c >= '0' && c <= '9';

        private char Atual => _i < _texto.Length ? _texto[_i] : '\0';

        private void PularEspacos()
        {
            while (_i < _texto.Length && (_texto[_i] == ' ' || _texto[_i] == '\t' || _texto[_i] == '\n' ||
                                          _texto[_i] == '\r'))
                _i++;
        }

        private (int linha, int coluna) Posicao(int indice)
        {
            int linha = 1, coluna = 1;
            for (int k = 0; k < indice && k < _texto.Length; k++)
            {
                if (_texto[k] == '\n')
                {
                    linha++;
                    coluna = 1;
                }
                else coluna++;
            }

            return (linha, coluna);
        }

        private JsonMalFormadoException Erro(string motivo)
        {
            var (linha, coluna) = Posicao(_i);
            return new JsonMalFormadoException(linha, coluna, motivo);
        }
    }
}
