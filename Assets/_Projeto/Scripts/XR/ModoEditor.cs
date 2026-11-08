#if UNITY_EDITOR
using LaboratorioParalelepipedo.Apresentacao;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace LaboratorioParalelepipedo.XR
{
    /// <summary>
    /// Só no Editor e só sem óculos conectado: põe o olhar na altura de um aluno de pé, gira a visão com o
    /// botão direito do mouse e usa o clique esquerdo como o gatilho do raio (chama <see cref="IAlvoDoRaio"/>
    /// no objeto apontado). O código inteiro fica em #if UNITY_EDITOR e o objeto da cena tem a tag EditorOnly:
    /// nada disto vai para o APK. Com o óculos, não faz nada (o rastreamento manda na câmera).
    /// </summary>
    public sealed class ModoEditor : MonoBehaviour
    {
        [SerializeField, Tooltip("TrackingSpace do OVRCameraRig: é ele que sobe e gira, não a câmera.")]
        private Transform _espacoDeRastreamento;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _alturaDosOlhos = 1.4f;
        [SerializeField, Tooltip("Graus por pixel do mouse.")]
        private float _sensibilidade = 0.15f;
        [SerializeField] private float _alcanceDoRaio = 10f;

        private OlharDoMouse _olhar;

        /// <summary>Liga o componente (usado pelo gerador da cena).</summary>
        public void Configurar(Transform espacoDeRastreamento, Camera camera)
        {
            _espacoDeRastreamento = espacoDeRastreamento;
            _camera = camera;
        }

        private void Start()
        {
            if (XRSettings.isDeviceActive || _espacoDeRastreamento == null || _camera == null)
            {
                enabled = false;
                return;
            }

            _espacoDeRastreamento.localPosition = new Vector3(0f, _alturaDosOlhos, 0f);
            Debug.Log("[Modo Editor] Sem óculos: olhos a 1,4 m. Botão direito do mouse olha em volta; " +
                      "clique esquerdo é o gatilho do raio.", this);
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                _olhar.Mover(delta.x, delta.y, _sensibilidade);
                _espacoDeRastreamento.localRotation = Quaternion.Euler(_olhar.Inclinacao, _olhar.Giro, 0f);
            }

            if (mouse.leftButton.wasPressedThisFrame) Acionar(mouse.position.ReadValue());
        }

        private void Acionar(Vector2 posicaoNaTela)
        {
            Ray raio = _camera.ScreenPointToRay(posicaoNaTela);
            if (!Physics.Raycast(raio, out RaycastHit acerto, _alcanceDoRaio)) return;

            var alvo = acerto.collider.GetComponentInParent<IAlvoDoRaio>();
            if (alvo != null) alvo.AoAcionar();
        }
    }
}
#endif
