using System;

namespace Test.Datas
{
    [Serializable]
    public class TestReferenceType : IEquatable<TestReferenceType>
    {
        public int value;

        #region IEquatable<TestReferenceType>

        public bool Equals(TestReferenceType other)
        {
            if( other == null )
            {
                return false;
            }

            return value == other.value;
        }

        #endregion
    }
}
