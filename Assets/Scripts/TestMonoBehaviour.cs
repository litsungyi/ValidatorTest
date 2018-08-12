using Camus.Validators;
using UnityEngine;

namespace Test
{
    public class TestMonoBehaviour : MonoBehaviour
    {
        [NotNull, SerializeField] private GameObject notNullPrivateItem;
        [NotNull, SerializeField] private GameObject notNullPrivateItem_Null;
        [SerializeField] private GameObject privateItem;
        [SerializeField] private GameObject privateItem_Null;
        [NotNull] public GameObject notNullPublicItem;
        [NotNull] public GameObject notNullPublicItem_Null;
        public GameObject publicItem;
        public GameObject publicItem_Null;
    }
}
