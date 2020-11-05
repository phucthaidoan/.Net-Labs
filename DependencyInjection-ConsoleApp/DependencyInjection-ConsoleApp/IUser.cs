namespace DependencyInjection_ConsoleApp
{
    public interface IUser
    {
        string Name { get; set; }

        void TruncateName(string name);
    }
}
