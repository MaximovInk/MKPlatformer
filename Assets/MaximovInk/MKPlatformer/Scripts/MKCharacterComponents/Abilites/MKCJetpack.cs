using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCJetpack : MKControllerComponent
    {
        [SerializeField]
        private MKCharacterButton _button = new MKCharacterButton() { ID = "Jetpack" };

    }
}