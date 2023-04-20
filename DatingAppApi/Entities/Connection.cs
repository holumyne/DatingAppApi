namespace DatingAppApi.Entities
{
    public class Connection
    {
        // generating empty constructor to satisfy entity framework when creating schema in our db
        public Connection()
        {

        }

        //A constructor is generated here to make this class slightly easier to use
        public Connection(string connectionId, string username)
        {
            ConnectionId = connectionId;
            Username = username;
        }

        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}
