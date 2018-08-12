using Camus.Validators;
using Test.Datas;
using UnityEngine;

namespace Test
{
    public class TestGameObject : MonoBehaviour
    {
        [NotNull, SerializeField] private TestSerializable data;
    }
}
