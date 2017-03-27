using Nitrolize.Extensions;

namespace Nitrolize.Types.Input
{
    public class UpdateInputType<T> : InputType<T> where T : class
    {
        public UpdateInputType() : base()
        {
            this.Name = $"update{typeof(T).Name}Input".ToFirstLower();
        }
    }
}
