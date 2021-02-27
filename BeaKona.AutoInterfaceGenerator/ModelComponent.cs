namespace BeaKona.AutoInterfaceGenerator
{
    internal abstract class ModelComponent
    {
        public ModelComponent(Model model)
        {
            this.Model = model;
        }

        public Model Model { get; }
    }
}