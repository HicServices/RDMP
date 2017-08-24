namespace ReusableLibraryCode
{
    public interface IHasDependencies
    {
        IHasDependencies[] GetObjectsThisDependsOn();
        IHasDependencies[] GetObjectsDependingOnThis();
    }

}