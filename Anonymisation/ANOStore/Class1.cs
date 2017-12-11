namespace ANOStore
{
    /// <summary>
    /// Required so that msbuild bundles ANOStore.Database.dll along with this dll everywhere it goes
    /// </summary>
    public class Class1
    {
        public static ANOStore.Database.Class1 DbClass { get; set; }
    }
}
