using System.ComponentModel.DataAnnotations;

namespace DatingAppApi.Entities
{
    public class Group
    {
        // generating empty constructor to satisfy entity framework when creating schema in our db
        public Group()
        {

        }
        //A constructor is generated here to make this class slightly easier to use
        public Group(string name)
        {
            Name = name;
        }

        [Key]
        public string Name { get; set; } //name of the group
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}
