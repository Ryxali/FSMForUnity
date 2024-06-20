using UnityEngine.UIElements;

namespace FSMForUnity.Editor
{
    internal struct StyleWithDefault<T>
    {
        public readonly CustomStyleProperty<T> property;
        public readonly T defaultValue;

        public StyleWithDefault(string propertyName, T defaultValue)
        {
            this.defaultValue = defaultValue;
            property = new CustomStyleProperty<T>(propertyName);
        }
    }
}
