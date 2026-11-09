namespace LaTiendita.Domain.Entities
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? LineasSuministro { get; set; } // Lo que decía en el formulario: "Líneas que suministra"
    }
}