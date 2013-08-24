namespace Pixills.Net.Goofy.Modules
{
    public class ModuleLoader
    {
        public static IModule Load(string path, string name)
        {
            var fileNameAndPath = path + "/" + name + ".dll";
            var assembly = System.Reflection.Assembly.LoadFile(name);
            return (IModule)assembly.CreateInstance("Pixills.Net.Goofy.Modules.Module");
        }
    }
}
