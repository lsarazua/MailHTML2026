using System;
using System.Collections.Generic;

namespace MailHTML.Dominio.Modelos
{

    public class CotizacionConfiguracionModel
    {
        public int RenglonesPorHoja { get; set; } = 5;

        public CotizacionTerminosModel Terminos { get; set; } = new CotizacionTerminosModel();
        public CotizacionPagoModel Pago { get; set; } = new CotizacionPagoModel();

        public CotizacionFooterModel Footer { get; set; } = new CotizacionFooterModel();

        public string UrlLogoPrincipal { get; set; } = "";
        public string UrlLogoFooter { get; set; } = "";
    }

    public class CotizacionTerminosModel
    {
        public List<string> Lineas { get; set; } = new List<string>();
    }

    public class CotizacionPagoModel
    {
        public string Texto { get; set; } = "";
    }

    public class CotizacionFooterModel
    {
        public string Empresa { get; set; } = "";
        public string Linea1 { get; set; } = "";
        public string Linea2 { get; set; } = "";
        public string Linea3 { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string FooterHtml { get; set; } = "";
    }
}