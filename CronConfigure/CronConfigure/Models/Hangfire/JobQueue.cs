// Copyright (c) UTE GNOSS - UNIVERSIDAD DE DEUSTO
// Licenciado bajo la licencia GPL 3. Ver https://www.gnu.org/licenses/gpl-3.0.html
// Proyecto H�rcules ASIO Backend SGI. Ver https://www.um.es/web/hercules/proyectos/asio
// Clase usada por Hangfire para el guardado de las tareas
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace CronConfigure.Models.Hangfire
{
    [ExcludeFromCodeCoverage]
    ///<summary>
    ///Clase usada por Hangfire para el guardado de las tareas
    ///</summary>
    [Table("jobqueue", Schema = "hangfire")]
    public partial class JobQueue
    {
        [Column(Order = 0)]
        public int Id { get; set; }

        public long JobId { get; set; }

        [Column(Order = 1)]
        [StringLength(50)]
        public string Queue { get; set; }

        public DateTime? FetchedAt { get; set; }
    }
}
