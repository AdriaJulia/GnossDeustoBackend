﻿// Copyright (c) UTE GNOSS - UNIVERSIDAD DE DEUSTO
// Licenciado bajo la licencia GPL 3. Ver https://www.gnu.org/licenses/gpl-3.0.html
// Proyecto Hércules ASIO Backend SGI. Ver https://www.um.es/web/hercules/proyectos/asio
// Clase para la programación de tareas
using CronConfigure.Filters;
using CronConfigure.Models.Entitties;
using Hangfire;
using Hangfire.Server;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CronConfigure.Models.Services
{
    [ExcludeFromCodeCoverage]
    ///<summary>
    ///Clase para la programación de tareas
    ///</summary>
    public class ProgramingMethodsService : IProgramingMethodService
    {
        private CallApiService _serviceApi;
        private HangfireEntityContext _context;
        readonly TokenBearer _token;

        public ProgramingMethodsService(CallApiService serviceApi, HangfireEntityContext context, CallTokenService tokenService)
        {
            _serviceApi = serviceApi;
            _context = context;
            if (tokenService != null)
            {
                _token = tokenService.CallTokenCarga();
            }
        }

        [AutomaticRetry(Attempts = 0, DelaysInSeconds = new int[] { 3600 })]
        [ProlongExpirationTime]
        ///<summary>
        ///Método para la sincronización de repositorios
        ///</summary>
        ///<param name="idRepositoryGuid">identificador del repositorio a sincronizar</param>
        ///<param name="fecha">Fecha desde la que se quiere sincronizar</param>
        /// <param name="set">tipo del objeto, usado para filtrar por agrupaciones</param>
        /// <param name="codigo_objeto">codigo del objeto a sincronizar, es necesario pasar el parametro set si se quiere pasar este parámetro</param>
        public string PublishRepositories(Guid idRepositoryGuid, PerformContext context, DateTime? fecha = null, string pSet = null, string codigoObjeto = null)
        {
            string idRepository = idRepositoryGuid.ToString();
            string idJob = context.BackgroundJob.Id;
            DateTime fechaJob = context.BackgroundJob.CreatedAt;
            var discover = _context.ProcessDiscoverStateJob.FirstOrDefault(item => item.JobId.Equals(idJob));
            if (discover == null)
            {
                ProcessDiscoverStateJob discoveryState = new ProcessDiscoverStateJob()
                {
                    Id = Guid.NewGuid(),
                    JobId = idJob,
                    State = "Pending"
                };
                _context.ProcessDiscoverStateJob.Add(discoveryState);
                _context.SaveChanges();
            }
            try
            {
                object objeto = new
                {
                    repository_identifier = idRepositoryGuid,
                    codigo_objeto = codigoObjeto,
                    fecha_from = fecha,
                    set = pSet,
                    job_id = idJob,
                    job_created_date = fechaJob
                };
                string result = _serviceApi.CallPostApi($"sync/execute", objeto, _token);///{idRepository}
                result = JsonConvert.DeserializeObject<string>(result);

                #region Actualizamos ProcessDiscoverStateJob
                string state;
                //Actualizamos a error si existen items en estado error o con problemas de desambiguación 
                if (_context.DiscoverItem.Any(x => x.JobID == idJob && (x.Status == DiscoverItem.DiscoverItemStatus.Error.ToString() || x.Status == DiscoverItem.DiscoverItemStatus.ProcessedDissambiguationProblem.ToString())))
                {
                    state = "Error";
                }
                else if (_context.DiscoverItem.Any(x => x.JobID == idJob && (x.Status == DiscoverItem.DiscoverItemStatus.Pending.ToString())))
                {
                    //Actualizamos a 'Pending' si aún existen items pendientes
                    state = "Pending";
                }
                else
                {
                    //Actualizamos a Success si no existen items en estado error ni con problemas de desambiguación y no hay ninguno pendiente
                    state = "Success";
                }
                ProcessDiscoverStateJob processDiscoverStateJob = _context.ProcessDiscoverStateJob.FirstOrDefault(item => item.JobId.Equals(idJob));
                if (processDiscoverStateJob != null)
                {
                    processDiscoverStateJob.State = state;
                }
                else
                {
                    processDiscoverStateJob = new ProcessDiscoverStateJob() { State = state, JobId = idJob };
                    _context.ProcessDiscoverStateJob.Add(processDiscoverStateJob);
                }
                _context.SaveChanges();
                #endregion


                return result;
            }
            catch (Exception ex)
            {
                string timeStamp = CreateTimeStamp();
                CreateLoggin(timeStamp, idRepository);
                Log.Error($"{ex.Message}\n{ex.StackTrace}\n");
                throw new Exception(ex.Message);
                //return ex.Message;
            }
        }

        ///<summary>
        ///Método para programar una sincronización recurrente
        ///</summary>
        ///<param name="idRepository">identificador del repositorio a sincronizar</param>
        ///<param name="nombreCron">Nombre de la tarea recurrente</param>
        ///<param name="cronExpression">expresión de recurrencia</param>
        ///<param name="fecha">Fecha desde la que se quiere sincronizar</param>
        ///<param name="set">tipo del objeto, usado para filtrar por agrupaciones</param>
        ///<param name="codigoObjeto">codigo del objeto a sincronizar, es necesario pasar el parametro set si se quiere pasar este parámetro</param>
        public static void ProgramRecurringJob(Guid idRepository, string nombreCron, string cronExpression, DateTime? fecha = null, string set = null, string codigoObjeto = null)
        {
            ConfigUrlService serviceUrl = new ConfigUrlService();
            CallApiService serviceApi = new CallApiService(serviceUrl);
            ProgramingMethodsService service = new ProgramingMethodsService(serviceApi, null, null);

            RecurringJob.AddOrUpdate(nombreCron, () => service.PublishRepositories(idRepository, null, fecha, set, codigoObjeto), cronExpression);
        }

        ///<summary>
        ///Método para programar una tarea
        ///</summary>
        ///<param name="idRepository">identificador del repositorio a sincronizar</param>
        ///<param name="fechaInicio">Fecha de la ejecución</param>
        ///<param name="fecha">Fecha desde la que se quiere sincronizar</param>
        ///<param name="set">tipo del objeto, usado para filtrar por agrupaciones</param>
        ///<param name="codigoObjeto">codigo del objeto a sincronizar, es necesario pasar el parametro set si se quiere pasar este parámetro</param>
        public string ProgramPublishRepositoryJob(Guid idRepository, DateTime fechaInicio, DateTime? fecha = null, string set = null, string codigoObjeto = null)
        {
            string id = BackgroundJob.Schedule(() => PublishRepositories(idRepository, null, fecha, set, codigoObjeto), fechaInicio);
            JobRepository jobRepository = new JobRepository()
            {
                IdJob = id,
                IdRepository = idRepository,
                FechaEjecucion = fechaInicio
            };
            _context.JobRepository.Add(jobRepository);
            _context.SaveChanges();
            return id;
        }

        ///<summary>
        ///Método para programar una tarea con una sincronización recurrente
        ///</summary>
        ///<param name="idRepository">identificador del repositorio a sincronizar</param>
        ///<param name="nombreCron">Nombre de la tarea recurrente</param>
        ///<param name="cronExpression">expresión de recurrencia</param>
        ///<param name="fechaInicio">Fecha en la que se ejecutará la tarea y se activará la tarea recurrente</param>
        ///<param name="fecha">Fecha desde la que se quiere sincronizar</param>
        ///<param name="set">tipo del objeto, usado para filtrar por agrupaciones</param>
        ///<param name="codigoObjeto">codigo del objeto a sincronizar, es necesario pasar el parametro set si se quiere pasar este parámetro</param>
        public void ProgramPublishRepositoryRecurringJob(Guid idRepository, string nombreCron, string cronExpression, DateTime fechaInicio, DateTime? fecha = null, string set = null, string codigoObjeto = null)
        {
            string id = BackgroundJob.Schedule(() => ProgramRecurringJob(idRepository, nombreCron, cronExpression, fecha, set, codigoObjeto), fechaInicio);
            JobRepository jobRepository = new JobRepository()
            {
                IdJob = $"{id}_{nombreCron}_{cronExpression}",
                IdRepository = idRepository,
                FechaEjecucion = fechaInicio
            };
            _context.JobRepository.Add(jobRepository);
            _context.SaveChanges();
        }

        ///<summary>
        ///Creación de log
        ///</summary>
        ///<param name="pTimestamp">String de fecha</param>
        ///<param name="id">Identificador del job</param>
        private void CreateLoggin(string pTimestamp, string id)
        {
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.File($"logs/job_{id}/log_{pTimestamp}.txt").CreateLogger();
        }

        ///<summary>
        ///Creación de una cadena de texto con la fecha actual
        ///</summary>
        private string CreateTimeStamp()
        {
            DateTime time = DateTime.Now;
            string month = time.Month.ToString();
            if (month.Length == 1)
            {
                month = $"0{month}";
            }
            string day = time.Day.ToString();
            if (day.Length == 1)
            {
                day = $"0{day}";
            }
            string timeStamp = $"{time.Year.ToString()}{month}{day}";
            return timeStamp;
        }
    }
}
