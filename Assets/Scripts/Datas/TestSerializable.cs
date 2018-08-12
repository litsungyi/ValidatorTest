using System;
using Camus.Validators;

namespace Test.Datas
{
    [Serializable]
    public class TestSerializable
    {
        // NOTE: Value Type will never be NULL
        public TestValueType valueType;
        // NOTE: Value type cannot be null
        //public TestValueType valueType_Null = null;

        // NOTE: Value Type will never be NULL
        [NotNull] public TestValueType notNullValueType;
        // NOTE: Value type cannot be null
        //[NotNull] public TestValueType notNullValueType_Null = null;
  
        // NOTE: Nullable is not Serialzable and will not display in inspector
        public TestValueType? nullableValueType;
        public TestValueType? nullableValueType_Null = null;
        [NotNull] public TestValueType? notNullNullableValueType;
        [NotNull] public TestValueType? notNullNullableValueType_Null = null;

        // NOTE: Reference
        public TestReferenceType referenceType;
        public TestReferenceType referenceType_Null = null;
        [NotNull]public TestReferenceType notNullReferenceType;
        [NotNull]public TestReferenceType notNullReferenceType_Null = null;
    }
}
