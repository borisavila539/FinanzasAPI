namespace Core.DTOs
{
    public class MedidasInsertDTOs
    {
		public int id { get; set; }
		public int idMasterOrden { get; set; }
		public int idMedida {get;set;}
		public int lavadoID { get; set; }
		public string idTalla { get;set;}
		public string Medida { get;set;}
		public string MedidaNumerador { get; set; }
		public string Diferencia { get; set;}
		public int usuarioID { get; set; }
		public int moduloId { get; set; }
		public int? version { get; set; }

	}
}
