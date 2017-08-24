namespace ReusableLibraryCode
{
    public interface IHasFullyQualifiedNameToo:IHasRuntimeName
    {
        string GetFullyQualifiedName();
    }
}