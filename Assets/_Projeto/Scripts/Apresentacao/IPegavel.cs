using UnityEngine;

namespace LaboratorioParalelepipedo.Apresentacao
{
    /// <summary>Objeto que o aluno pega com o botão lateral, apontando o raio para ele.</summary>
    public interface IPegavel
    {
        /// <summary>Passa a seguir a mão (o controle), mantendo a distância e o ângulo do momento em que foi pego.</summary>
        void Pegar(Transform mao);

        /// <summary>Solta, se estiver na mão dada (a outra mão pode ter pegado depois).</summary>
        void Soltar(Transform mao);

        /// <summary>Se está na mão dada agora.</summary>
        bool EstaNaMaoDe(Transform mao);
    }
}
