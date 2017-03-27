using Nitrolize.Extensions;

namespace Nitrolize.Types.Input
{
    public class AddInputType<T> : InputType<T> where T : class
    {
        public AddInputType() : base(omitIdProperty: true)
        {
            this.Name = $"add{typeof(T).Name}Input".ToFirstLower();
        }
    }
}
