namespace ReactUI.Server.Models
{
    public class Catalogue
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public string Description { get; set; }

        public Catalogue() { }
        public Catalogue(int id, string name, string description)
        {
            Name = name;
            ID = id;
            Description = description;
        }


    }
}
