namespace Core.DTOs
{
    public class MaestroOrdenes
    {
        public int id { get; set; }
        public string prodmasterrefid { get; set; }
        public string prodmasterid { get; set; }
        public string itemid { get; set; }
        public string loweststatus { get; set; }
        public int posted { get; set; }
        public int? userid { get; set; }

    }
}
