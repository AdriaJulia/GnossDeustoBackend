using API_DISCOVER.Models;
using API_DISCOVER.Models.Entities;
using API_DISCOVER.Models.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System;
using System.Collections;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using API_DISCOVER.Models.Log;
using VDS.RDF;
using System.Collections.Generic;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;
using API_DISCOVER.Models.Entities.Discover;
using API_DISCOVER.Utility;
using System.Linq;
using VDS.RDF.Query;
using VDS.RDF.Update;
using System.Threading;

namespace API_DISCOVER
{
    /// <summary>
    /// Clase para ejecutar las tareas de descubrimiento
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Discover
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="serviceScopeFactory">Factor�a de servicios</param>
        public Discover(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Procesa un item de descubrimiento
        /// </summary>
        /// <param name="itemIDstring">Identificador del item</param>
        /// <returns></returns>
        public bool ProcessItem(string itemIDstring)
        {
            Guid itemID = JsonConvert.DeserializeObject<Guid>(itemIDstring);
            try
            {
                DiscoverItemBDService discoverItemBDService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DiscoverItemBDService>();
                ProcessDiscoverStateJobBDService processDiscoverStateJobBDService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ProcessDiscoverStateJobBDService>();
                CallCronApiService callCronApiService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CallCronApiService>();
                CallEtlApiService callEtlApiService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CallEtlApiService>();

                DiscoverItem discoverItem = discoverItemBDService.GetDiscoverItemById(itemID);

                if (discoverItem != null)
                {
                    //Aplicamos el proceso de descubrimiento
                    DiscoverResult resultado = Init(discoverItem, callEtlApiService);
                    Process(discoverItem, resultado,
                        discoverItemBDService,
                        callCronApiService,
                        processDiscoverStateJobBDService
                        );
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                //Se ha producido un error al aplicar el descubrimiento
                //Modificamos los datos del DiscoverItem que ha fallado
                DiscoverItemBDService discoverItemBDService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DiscoverItemBDService>();
                DiscoverItem discoverItemBBDD = discoverItemBDService.GetDiscoverItemById(itemID);
                discoverItemBBDD.UpdateError($"{ex.Message}\n{ex.StackTrace}\n");
                discoverItemBDService.ModifyDiscoverItem(discoverItemBBDD);

                if (!string.IsNullOrEmpty(discoverItemBBDD.JobID))
                {
                    //Si viene de una tarea actualizamos su estado de descubrimiento
                    ProcessDiscoverStateJobBDService processDiscoverStateJobBDService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ProcessDiscoverStateJobBDService>();
                    ProcessDiscoverStateJob processDiscoverStateJob = processDiscoverStateJobBDService.GetProcessDiscoverStateJobByIdJob(discoverItemBBDD.JobID);
                    if (processDiscoverStateJob != null)
                    {
                        processDiscoverStateJob.State = "Error";
                        processDiscoverStateJobBDService.ModifyProcessDiscoverStateJob(processDiscoverStateJob);
                    }
                    else
                    {
                        processDiscoverStateJob = new ProcessDiscoverStateJob() { State = "Error", JobId = discoverItemBBDD.JobID };
                        processDiscoverStateJobBDService.AddProcessDiscoverStateJob(processDiscoverStateJob);
                    }
                }
            }
            return true;

        }


        /// <summary>
        /// Realiza el proceso completo de desubrimiento sobre un RDF
        /// </summary>
        /// <param name="pDiscoverItem">Item de descubrimiento</param>
        /// <param name="pCallEtlApiService">Servicio para hacer llamadas a los m�todos del controlador etl del API_CARGA </param>
        /// <returns>DiscoverResult con los datos del descubrimiento</returns>
        private DiscoverResult Init(DiscoverItem pDiscoverItem, CallEtlApiService pCallEtlApiService)
        {
            #region Cargamos configuraciones
            ConfigSparql ConfigSparql = new ConfigSparql();
            string SGI_SPARQLEndpoint = ConfigSparql.GetEndpoint();
            string SGI_SPARQLGraph = ConfigSparql.GetGraph();
            string SGI_SPARQLQueryParam = ConfigSparql.GetQueryParam();
            string SGI_SPARQLUsername = ConfigSparql.GetUsername();
            string SGI_SPARQLPassword = ConfigSparql.GetPassword();
            string Unidata_SPARQLEndpoint = ConfigSparql.GetUnidataEndpoint();
            string Unidata_SPARQLGraph = ConfigSparql.GetUnidataGraph();
            string Unidata_SPARQLQueryParam = ConfigSparql.GetUnidataQueryParam();
            string Unidata_SPARQLUsername = ConfigSparql.GetUnidataUsername();
            string Unidata_SPARQLPassword = ConfigSparql.GetUnidataPassword();
            ConfigService ConfigService = new ConfigService();
            float MaxScore = ConfigService.GetMaxScore();
            float MinScore = ConfigService.GetMinScore();
            string UnidataDomain = ConfigService.GetUnidataDomain();
            ConfigScopus ConfigScopus = new ConfigScopus();
            string ScopusApiKey = ConfigScopus.GetScopusApiKey();
            string ScopusUrl = ConfigScopus.GetScopusUrl();
            ConfigCrossref ConfigCrossref = new ConfigCrossref();
            string CrossrefUserAgent = ConfigCrossref.GetCrossrefUserAgent();
            ConfigWOS ConfigWOS = new ConfigWOS();
            string WOSAuthorization = ConfigWOS.GetWOSAuthorization();
            #endregion

            DiscoverUtility discoverUtility = new DiscoverUtility();

            DateTime discoverInitTime = DateTime.Now;

            //Cargamos la ontolog�a           
            RohGraph ontologyGraph = pCallEtlApiService.CallGetOntology();

            //Cargamos datos del RDF
            RohGraph dataGraph = new RohGraph();
            Dictionary<string, HashSet<string>> discardDissambiguations = new Dictionary<string, HashSet<string>>();
            if (!string.IsNullOrEmpty(pDiscoverItem.DiscoverRdf))
            {
                //Si tenemos valor en DiscoverRdf, trabajamos con este RDF, ya que estamos reprocesando un rdf validado
                dataGraph.LoadFromString(pDiscoverItem.DiscoverRdf, new RdfXmlParser());
                if (pDiscoverItem.DiscardDissambiguations != null)
                {
                    foreach (DiscoverItem.DiscardDissambiguation discardDissambiguation in pDiscoverItem.DiscardDissambiguations)
                    {
                        if (!discardDissambiguations.ContainsKey(discardDissambiguation.IDOrigin))
                        {
                            discardDissambiguations.Add(discardDissambiguation.IDOrigin, new HashSet<string>());
                        }
                        discardDissambiguations[discardDissambiguation.IDOrigin].UnionWith(discardDissambiguation.DiscardCandidates);
                    }
                }
            }
            else
            {
                dataGraph.LoadFromString(pDiscoverItem.Rdf, new RdfXmlParser());
            }


            //Cargamos el razonador para inferir datos en la ontolog�a
            RohRdfsReasoner reasoner = new RohRdfsReasoner();
            reasoner.Initialise(ontologyGraph);

            //Cargamos los datos con inferencia
            RohGraph dataInferenceGraph = dataGraph.Clone();
            reasoner.Apply(dataInferenceGraph);

            //Datos para trabajar con la reconciliaci�n
            ReconciliationData reconciliationData = new ReconciliationData();

            //Datos para trabajar con el descubrimiento de enlaces
            DiscoverLinkData discoverLinkData = new DiscoverLinkData();

            //Almacenamos las entidades con dudas acerca de su reonciliaci�n
            Dictionary<string, Dictionary<string, float>> reconciliationEntitiesProbability = new Dictionary<string, Dictionary<string, float>>();

            if (!pDiscoverItem.DissambiguationProcessed)
            {
                //Obtenemos los nombres de todas las personas que haya cargadas en la BBDD
                //Clave ID,
                //Valor nombre
                Dictionary<string, string> personsWithName = discoverUtility.LoadPersonWithName(SGI_SPARQLEndpoint, SGI_SPARQLGraph, SGI_SPARQLQueryParam, SGI_SPARQLUsername, SGI_SPARQLPassword);

                bool hasChanges = true;

                //Cache del proceso de descubrimiento
                DiscoverCache discoverCache = new DiscoverCache();

                //Se realizar�n este proceso iterativamente hasta que no haya ning�n cambio en lo que a reconciliaciones se refiere
                while (hasChanges)
                {
                    hasChanges = false;

                    //Preparamos los datos para proceder con la reconciliazci�n
                    discoverUtility.PrepareData(dataGraph, reasoner, out dataInferenceGraph,
                        out Dictionary<string, HashSet<string>> entitiesRdfTypes,
                        out Dictionary<string, string> entitiesRdfType,
                        out Dictionary<string, List<DisambiguationData>> disambiguationDataRdf, false);

                    //Carga los scores de las personas
                    //Aqu� se almacenar�n los nombres de las personas del RDF, junto con los candidatos de la BBDD y su score
                    Dictionary<string, Dictionary<string, float>> namesScore = new Dictionary<string, Dictionary<string, float>>();
                    discoverUtility.LoadNamesScore(ref namesScore, personsWithName, dataInferenceGraph, discoverCache, MinScore, MaxScore);

                    //0.- Macamos como reconciliadas aquellas que ya est�n cargadas en la BBDD con los mismos identificadores
                    List<string> entidadesCargadas = discoverUtility.LoadEntitiesDB(entitiesRdfType.Keys.ToList().Except(reconciliationData.reconciliatedEntityList.Keys.Union(reconciliationData.reconciliatedEntityList.Values)), SGI_SPARQLEndpoint, SGI_SPARQLGraph, SGI_SPARQLQueryParam, SGI_SPARQLUsername, SGI_SPARQLPassword).Keys.ToList();
                    foreach (string entitiID in entidadesCargadas)
                    {
                        reconciliationData.reconciliatedEntityList.Add(entitiID, entitiID);
                        reconciliationData.reconciliatedEntitiesWithSubject.Add(entitiID);
                    }

                    //1.- Realizamos reconciliaci�n con los identificadores configurados (y el roh:identifier) y marcamos como reconciliadas las entidades seleccionadas para no intentar reconciliarlas posteriormente
                    discoverUtility.ReconciliateIDs(ref hasChanges, ref reconciliationData, entitiesRdfType, disambiguationDataRdf, discardDissambiguations, ontologyGraph, ref dataGraph, discoverCache, SGI_SPARQLEndpoint, SGI_SPARQLQueryParam, SGI_SPARQLGraph, SGI_SPARQLUsername, SGI_SPARQLPassword);

                    //2.- Realizamos la reconciliaci�n con los datos del Propio RDF
                    discoverUtility.ReconciliateRDF(ref hasChanges, ref reconciliationData, ontologyGraph, ref dataGraph, reasoner, discardDissambiguations, discoverCache, MinScore, MaxScore);

                    //3.- Realizamos la reconciliaci�n con los datos de la BBDD
                    discoverUtility.ReconciliateBBDD(ref hasChanges, ref reconciliationData, out reconciliationEntitiesProbability, ontologyGraph, ref dataGraph, reasoner, namesScore, discardDissambiguations, discoverCache, MinScore, MaxScore, SGI_SPARQLEndpoint, SGI_SPARQLQueryParam, SGI_SPARQLGraph, SGI_SPARQLUsername, SGI_SPARQLPassword);

                    //4.- Realizamos la reconciliaci�n con los datos de las integraciones externas
                    discoverUtility.ExternalIntegration(ref hasChanges, ref reconciliationData, ref discoverLinkData, ref reconciliationEntitiesProbability, ref dataGraph, reasoner, namesScore, ontologyGraph, out Dictionary<string, ReconciliationData.ReconciliationScore> entidadesReconciliadasConIntegracionExternaAux, discardDissambiguations, discoverCache, ScopusApiKey, ScopusUrl, CrossrefUserAgent, WOSAuthorization, MinScore, MaxScore, SGI_SPARQLEndpoint, SGI_SPARQLQueryParam, SGI_SPARQLGraph, SGI_SPARQLUsername, SGI_SPARQLPassword);

                    //Eliminamos de las probabilidades aquellos que ya est�n reconciliados
                    foreach (string key in reconciliationData.reconciliatedEntityList.Keys)
                    {
                        reconciliationEntitiesProbability.Remove(key);
                    }
                }

                //5.-Realizamos la detecci�n de equivalencias con Unidata
                discoverUtility.EquivalenceDiscover(ontologyGraph, ref dataGraph, reasoner, discoverCache, ref reconciliationEntitiesProbability, discardDissambiguations, UnidataDomain, MinScore, MaxScore, Unidata_SPARQLEndpoint, Unidata_SPARQLQueryParam, Unidata_SPARQLGraph, Unidata_SPARQLUsername, Unidata_SPARQLPassword);
            }

            DateTime discoverEndTime = DateTime.Now;
            DiscoverResult resultado = new DiscoverResult(dataGraph, dataInferenceGraph, ontologyGraph, reconciliationData, reconciliationEntitiesProbability, discoverInitTime, discoverEndTime, discoverLinkData);

            return resultado;
        }


        /// <summary>
        /// Procesa los resultados del descubrimiento
        /// </summary>
        /// <param name="pDiscoverItem">Objeto con los datos de com procesar el proeso de descubrimiento</param>
        /// <param name="pDiscoverResult">Resultado de la aplicaci�n del descubrimiento</param>
        /// <param name="pDiscoverItemBDService">Clase para gestionar las operaciones de las tareas de descubrimiento</param>
        /// <param name="pCallCronApiService">Servicio para hacer llamadas a los m�todos del apiCron</param>
        /// <param name="pProcessDiscoverStateJobBDService">Clase para gestionar los estados de descubrimiento de las tareas</param>
        /// <returns></returns>
        public void Process(DiscoverItem pDiscoverItem, DiscoverResult pDiscoverResult, DiscoverItemBDService pDiscoverItemBDService, CallCronApiService pCallCronApiService, ProcessDiscoverStateJobBDService pProcessDiscoverStateJobBDService)
        {
            #region Cargamos configuraciones
            ConfigSparql ConfigSparql = new ConfigSparql();
            string SGI_SPARQLEndpoint = ConfigSparql.GetEndpoint();
            string SGI_SPARQLGraph = ConfigSparql.GetGraph();
            string SGI_SPARQLQueryParam = ConfigSparql.GetQueryParam();
            string SGI_SPARQLUsername = ConfigSparql.GetUsername();
            string SGI_SPARQLPassword = ConfigSparql.GetPassword();
            string Unidata_SPARQLEndpoint = ConfigSparql.GetUnidataEndpoint();
            string Unidata_SPARQLGraph = ConfigSparql.GetUnidataGraph();
            string Unidata_SPARQLQueryParam = ConfigSparql.GetUnidataQueryParam();
            string Unidata_SPARQLUsername = ConfigSparql.GetUnidataUsername();
            string Unidata_SPARQLPassword = ConfigSparql.GetUnidataPassword();
            ConfigService ConfigService = new ConfigService();
            string UnidataDomain = ConfigService.GetUnidataDomain();
            string UnidataUriTransform = ConfigService.GetUnidataUriTransform();
            #endregion

            /*
             En funci�n del resultado obtenido se realiza una de las siguientes acciones:
                Si para alguna entidad hay m�s de un candidato que supere el umbral m�ximo o hay alguna entidad que supere el umbral m�nimo pero no alcance el m�ximo se agregar� el RDF a una BBDD junto con todos los datos necesarios para que el administrador decida como proceder.
                Si no estamos en el punto anterior
                    Se obtienen las entidades principales del RDF y se eliminan todos los triples que haya en la BBDD en los que aparezcan como sujeto u objeto.
                    Se eliminan todos los triples cuyo sujeto y predicado est�n en el RDF a cargar y est�n marcados como monovaluados.
                    Se vuelcan los triples a la BBDD.


             */

            if (pDiscoverItem.Publish)
            {
                if (pDiscoverResult.discoveredEntitiesProbability.Count > 0)
                {
                    //Hay dudas en la desambiguaci�n, por lo que lo actualizamos en la BBDD con su estado correspondiente    
                    pDiscoverItem.UpdateDissambiguationProblems(
                        pDiscoverResult.discoveredEntitiesProbability,
                        pDiscoverResult.reconciliationData.reconciliatedEntitiesWithBBDD.Values.Select(x => x.uri).Union(pDiscoverResult.reconciliationData.reconciliatedEntitiesWithIds.Values).Union(pDiscoverResult.reconciliationData.reconciliatedEntitiesWithSubject).Union(pDiscoverResult.reconciliationData.reconciliatedEntitiesWithExternalIntegration.Values.Select(x => x.uri)).ToList(),
                        pDiscoverResult.GetDataGraphRDF());
                    pDiscoverItemBDService.ModifyDiscoverItem(pDiscoverItem);
                }
                else
                {
                    //Creamos los SameAs hacia unidata para las entidades que NO lo tengan hacia Unidata                       
                    pDiscoverResult.dataGraph = AsioPublication.CreateUnidataSameAs(pDiscoverResult.dataGraph, UnidataDomain, UnidataUriTransform);

                    //Publicamos en el SGI
                    AsioPublication asioPublication = new AsioPublication(SGI_SPARQLEndpoint, SGI_SPARQLQueryParam, SGI_SPARQLGraph, SGI_SPARQLUsername, SGI_SPARQLPassword);
                    //TODO urisfactory
                    asioPublication.PublishRDF(pDiscoverResult.dataGraph, pDiscoverResult.ontologyGraph, new KeyValuePair<string, string>("http://graph.um.es/res/agent/discover", "Algoritmos de descubrimiento"), pDiscoverResult.start, pDiscoverResult.end, pDiscoverResult.discoverLinkData);

                    //Publicamos en UNIDATA
                    AsioPublication asioPublicationUnidata = new AsioPublication(Unidata_SPARQLEndpoint, Unidata_SPARQLQueryParam, Unidata_SPARQLGraph, Unidata_SPARQLUsername, Unidata_SPARQLPassword);
                    // Prepara el grafo para su carga en Unidata, para ello coge las URIs de Unidata del SameAs y la aplica a los sujetos y los antiguos sujetos se agregan al SameAs
                    RohGraph unidataGraph = AsioPublication.TransformUrisToUnidata(pDiscoverResult.dataGraph, UnidataDomain, UnidataUriTransform);
                    //TODO urisfactory
                    asioPublicationUnidata.PublishRDF(unidataGraph, pDiscoverResult.ontologyGraph, new KeyValuePair<string, string>("http://graph.um.es/res/agent/discover", "Algoritmos de descubrimiento"), pDiscoverResult.start, pDiscoverResult.end, pDiscoverResult.discoverLinkData);


                    //Lo marcamos como procesado en la BBDD y eliminamos sus metadatos
                    pDiscoverItem.UpdateProcessed();
                    pDiscoverItemBDService.ModifyDiscoverItem(pDiscoverItem);
                }

                //Actualizamos el estado de descubrimiento de la tarea si el estado encolado esta en estado Success o Error (ha finalizado)
                string statusQueueJob = pCallCronApiService.GetJob(pDiscoverItem.JobID).State;
                if ((statusQueueJob == "Failed" || statusQueueJob == "Succeeded"))
                {
                    ProcessDiscoverStateJob processDiscoverStateJob = pProcessDiscoverStateJobBDService.GetProcessDiscoverStateJobByIdJob(pDiscoverItem.JobID);
                    string state;
                    //Actualizamos a error si existen items en estado error o con problemas de desambiguaci�n 
                    if (pDiscoverItemBDService.ExistsDiscoverItemsErrorOrDissambiguatinProblems(pDiscoverItem.JobID))
                    {
                        state = "Error";
                    }
                    else if (pDiscoverItemBDService.ExistsDiscoverItemsPending(pDiscoverItem.JobID))
                    {
                        //Actualizamos a 'Pending' si a�n existen items pendientes
                        state = "Pending";
                    }
                    else
                    {
                        //Actualizamos a Success si no existen items en estado error ni con problemas de desambiguaci�n y no hay ninguno pendiente
                        state = "Success";
                    }
                    if (processDiscoverStateJob != null)
                    {
                        processDiscoverStateJob.State = state;
                        pProcessDiscoverStateJobBDService.ModifyProcessDiscoverStateJob(processDiscoverStateJob);
                    }
                    else
                    {
                        processDiscoverStateJob = new ProcessDiscoverStateJob() { State = state, JobId = pDiscoverItem.JobID };
                        pProcessDiscoverStateJobBDService.AddProcessDiscoverStateJob(processDiscoverStateJob);
                    }
                }
            }
            else
            {
                //Actualizamos en BBDD
                DiscoverItem discoverItemBD = pDiscoverItemBDService.GetDiscoverItemById(pDiscoverItem.ID);

                //Reporte de descubrimiento
                string discoverReport = "Time processed (seconds): " + pDiscoverResult.secondsProcessed + "\n";
                if (pDiscoverResult.reconciliationData.reconciliatedEntitiesWithSubject != null && pDiscoverResult.reconciliationData.reconciliatedEntitiesWithSubject.Count > 0)
                {
                    discoverReport += "Entities discover with the same uri: " + pDiscoverResult.reconciliationData.reconciliatedEntitiesWithSubject.Count + "\n";
                    foreach (string uri in pDiscoverResult.reconciliationData.reconciliatedEntitiesWithSubject)
                    {
                        discoverReport += "\t" + uri + "\n";
                    }
                }
                if (pDiscoverResult.reconciliationData.reconciliatedEntitiesWithIds != null && pDiscoverResult.reconciliationData.reconciliatedEntitiesWithIds.Count > 0)
                {
                    discoverReport += "Entities discover with some common identifier: " + pDiscoverResult.reconciliationData.reconciliatedEntitiesWithIds.Count + "\n";
                    foreach (string uri in pDiscoverResult.reconciliationData.reconciliatedEntitiesWithIds.Keys)
                    {
                        discoverReport += "\t" + uri + " --> " + pDiscoverResult.reconciliationData.reconciliatedEntitiesWithIds[uri] + "\n";
                    }
                }
                if (pDiscoverResult.reconciliationData.reconciliatedEntitiesWithBBDD != null && pDiscoverResult.reconciliationData.reconciliatedEntitiesWithBBDD.Count > 0)
                {
                    discoverReport += "Entities discover with reconciliation config: " + pDiscoverResult.reconciliationData.reconciliatedEntitiesWithBBDD.Count + "\n";
                    foreach (string uri in pDiscoverResult.reconciliationData.reconciliatedEntitiesWithBBDD.Keys)
                    {
                        discoverReport += "\t" + uri + " --> " + pDiscoverResult.reconciliationData.reconciliatedEntitiesWithBBDD[uri] + "\n";
                    }
                }
                if (pDiscoverResult.reconciliationData.reconciliatedEntitiesWithExternalIntegration != null && pDiscoverResult.reconciliationData.reconciliatedEntitiesWithExternalIntegration.Count > 0)
                {
                    discoverReport += "Entities discover with external integrations config: " + pDiscoverResult.reconciliationData.reconciliatedEntitiesWithExternalIntegration.Count + "\n";
                    foreach (string uri in pDiscoverResult.reconciliationData.reconciliatedEntitiesWithExternalIntegration.Keys)
                    {
                        discoverReport += "\t" + uri + " --> " + pDiscoverResult.reconciliationData.reconciliatedEntitiesWithExternalIntegration[uri] + "\n";
                    }
                }
                if (pDiscoverResult.discoverLinkData != null && pDiscoverResult.discoverLinkData.entitiesProperties != null && pDiscoverResult.discoverLinkData.entitiesProperties.Count > 0)
                {
                    discoverReport += "Entities with identifiers obtained with External integration: " + pDiscoverResult.discoverLinkData.entitiesProperties.Count + "\n";
                    foreach (string uri in pDiscoverResult.discoverLinkData.entitiesProperties.Keys)
                    {
                        foreach (DiscoverLinkData.PropertyData property in pDiscoverResult.discoverLinkData.entitiesProperties[uri])
                        {
                            foreach (string value in property.valueProvenance.Keys)
                            {
                                foreach (string provenance in property.valueProvenance[value])
                                {
                                    discoverReport += "\t" + uri + " - " + property.property + " - " + value + " --> " + provenance + "\n";
                                }
                            }
                        }
                    }
                }
                discoverItemBD.UpdateReport(pDiscoverResult.discoveredEntitiesProbability, pDiscoverResult.GetDataGraphRDF(), discoverReport);
                pDiscoverItemBDService.ModifyDiscoverItem(discoverItemBD);
            }
        }

        /// <summary>
        /// Aplica el descubrimiento sobre las entidades cargadas en el SGI
        /// </summary>
        /// <param name="pSecondsSleep">Segundos para dormir despu�s de procesar una entidad</param>
        public void ApplyDiscoverLoadedEntities(int pSecondsSleep)
        {
            CallEtlApiService callEtlApiService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<CallEtlApiService>();

            #region Cargamos configuraciones
            ConfigSparql ConfigSparql = new ConfigSparql();
            string SGI_SPARQLEndpoint = ConfigSparql.GetEndpoint();
            string SGI_SPARQLGraph = ConfigSparql.GetGraph();
            string SGI_SPARQLQueryParam = ConfigSparql.GetQueryParam();
            string SGI_SPARQLUsername = ConfigSparql.GetUsername();
            string SGI_SPARQLPassword = ConfigSparql.GetPassword();
            string Unidata_SPARQLEndpoint = ConfigSparql.GetUnidataEndpoint();
            string Unidata_SPARQLGraph = ConfigSparql.GetUnidataGraph();
            string Unidata_SPARQLQueryParam = ConfigSparql.GetUnidataQueryParam();
            string Unidata_SPARQLUsername = ConfigSparql.GetUnidataUsername();
            string Unidata_SPARQLPassword = ConfigSparql.GetUnidataPassword();

            ConfigService ConfigService = new ConfigService();
            string UnidataDomain = ConfigService.GetUnidataDomain();
            string UnidataUriTransform = ConfigService.GetUnidataUriTransform();
            float MaxScore = ConfigService.GetMaxScore();
            float MinScore = ConfigService.GetMinScore();

            ConfigScopus ConfigScopus = new ConfigScopus();
            string ScopusApiKey = ConfigScopus.GetScopusApiKey();
            string ScopusUrl = ConfigScopus.GetScopusUrl();
            ConfigCrossref ConfigCrossref = new ConfigCrossref();
            string CrossrefUserAgent = ConfigCrossref.GetCrossrefUserAgent();
            ConfigWOS ConfigWOS = new ConfigWOS();
            string WOSAuthorization = ConfigWOS.GetWOSAuthorization();
            #endregion

            DiscoverUtility discoverUtility = new DiscoverUtility();

            //Cargar todas las personas en la lista de manera aleatoria.
            List<string> personList = discoverUtility.GetPersonList(SGI_SPARQLEndpoint, SGI_SPARQLGraph, SGI_SPARQLQueryParam, SGI_SPARQLUsername, SGI_SPARQLPassword);
            List<string> randomPersonList = GetRandomOrderList(personList);

            foreach (string person in randomPersonList)
            {
                try
                {
                    //Hora de inicio de la ejecuci�n
                    DateTime startTime = DateTime.Now;

                    //Obtener el RohGraph de una �nica persona.
                    RohGraph dataGraph = discoverUtility.GetDataGraphPersonLoadedForDiscover(person, SGI_SPARQLEndpoint, SGI_SPARQLGraph, SGI_SPARQLQueryParam, SGI_SPARQLUsername, SGI_SPARQLPassword);
                    //Clonamos el grafo original para hacer luego comprobaciones
                    RohGraph originalDataGraph = dataGraph.Clone();
                    bool hasChanges = false;
                    //Dictionary<string, string> discoveredEntityList = new Dictionary<string, string>();
                    Dictionary<string, Dictionary<string, float>> discoveredEntitiesProbability = new Dictionary<string, Dictionary<string, float>>();
                    Dictionary<string, ReconciliationData.ReconciliationScore> entidadesReconciliadasConIntegracionExternaAux;
                    Dictionary<string, HashSet<string>> discardDissambiguations = new Dictionary<string, HashSet<string>>();
                    DiscoverCache discoverCache = new DiscoverCache();
                    RohGraph ontologyGraph = callEtlApiService.CallGetOntology();
                    RohRdfsReasoner reasoner = new RohRdfsReasoner();
                    reasoner.Initialise(ontologyGraph);
                    RohGraph dataInferenceGraph = dataGraph.Clone();
                    reasoner.Apply(dataInferenceGraph);
                    Dictionary<string, string> personsWithName = discoverUtility.LoadPersonWithName(SGI_SPARQLEndpoint, SGI_SPARQLGraph, SGI_SPARQLQueryParam, SGI_SPARQLUsername, SGI_SPARQLPassword);

                    //Aqu� se almacenar�n los nombres de las personas del RDF, junto con los candidatos de la BBDD y su score
                    Dictionary<string, Dictionary<string, float>> namesScore = new Dictionary<string, Dictionary<string, float>>();
                    discoverUtility.LoadNamesScore(ref namesScore, personsWithName, dataInferenceGraph, discoverCache, MinScore, MaxScore);

                    //Obtenci�n de la integraci�n externa
                    ReconciliationData reconciliationData = new ReconciliationData();
                    DiscoverLinkData discoverLinkData = new DiscoverLinkData();
                    Dictionary<string, List<DiscoverLinkData.PropertyData>> integration = discoverUtility.ExternalIntegration(ref hasChanges, ref reconciliationData, ref discoverLinkData, ref discoveredEntitiesProbability, ref dataGraph, reasoner, namesScore, ontologyGraph, out entidadesReconciliadasConIntegracionExternaAux, discardDissambiguations, discoverCache, ScopusApiKey, ScopusUrl, CrossrefUserAgent, WOSAuthorization, MinScore, MaxScore, SGI_SPARQLEndpoint, SGI_SPARQLGraph, SGI_SPARQLQueryParam, SGI_SPARQLUsername, SGI_SPARQLPassword, false);

                    //Limpiamos 'integration' para no insertar triples en caso de que ya est�n cargados
                    foreach (string entity in integration.Keys.ToList())
                    {
                        foreach (DiscoverLinkData.PropertyData propertyData in integration[entity].ToList())
                        {
                            string p = propertyData.property;
                            HashSet<string> objetos = new HashSet<string>(propertyData.valueProvenance.Keys.ToList());
                            foreach (string o in objetos)
                            {
                                if (((SparqlResultSet)originalDataGraph.ExecuteQuery($@"ASK
                                        WHERE 
                                        {{
                                            ?s ?p ?o.
                                            FILTER(?s=<{entity}>)
                                            FILTER(?p=<{p}>)
                                            FILTER(str(?o)='{o}')
                                        }}")).Result)
                                {
                                    //Elimiamos el valor porque ya estaba cargado
                                    propertyData.valueProvenance.Remove(o);
                                }
                            }
                            if (propertyData.valueProvenance.Count == 0)
                            {
                                integration[entity].Remove(propertyData);
                            }
                        }
                        if (integration[entity].Count == 0)
                        {
                            integration.Remove(entity);
                        }
                    }


                    //Creaci�n de dataGraph con el contenido de 'integration' + RdfTypes + SameAS
                    RohGraph dataGraphIntegration = new RohGraph();
                    foreach (string sujeto in integration.Keys)
                    {
                        IUriNode s = dataGraphIntegration.CreateUriNode(UriFactory.Create(sujeto));

                        //Agregamos SameAs y RDFType de las entidades
                        SparqlResultSet sparqlResultSet = (SparqlResultSet)dataGraph.ExecuteQuery("select ?rdftype ?sameas where {?s a ?rdftype. OPTIONAL{?s <http://www.w3.org/2002/07/owl#sameAs> ?sameAS} FILTER(?s=<" + sujeto + ">)}");
                        foreach (SparqlResult sparqlResult in sparqlResultSet.Results)
                        {
                            string rdfType = sparqlResult["rdftype"].ToString();
                            IUriNode pRdfType = dataGraphIntegration.CreateUriNode(UriFactory.Create("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));
                            IUriNode oRdfType = dataGraphIntegration.CreateUriNode(UriFactory.Create(rdfType));
                            dataGraphIntegration.Assert(new Triple(s, pRdfType, oRdfType));
                            if (sparqlResult.Variables.Contains("sameas"))
                            {
                                string sameas = sparqlResult["sameas"].ToString();
                                IUriNode pSameAs = dataGraphIntegration.CreateUriNode(UriFactory.Create("http://www.w3.org/2002/07/owl#sameAs"));
                                IUriNode oSameAs = dataGraphIntegration.CreateUriNode(UriFactory.Create(sameas));
                                dataGraphIntegration.Assert(new Triple(s, pSameAs, oSameAs));
                            }
                        }

                        foreach (DiscoverLinkData.PropertyData propertyData in integration[sujeto])
                        {
                            foreach (string valor in propertyData.valueProvenance.Keys)
                            {
                                IUriNode p = dataGraphIntegration.CreateUriNode(UriFactory.Create(propertyData.property));
                                if (Uri.IsWellFormedUriString(valor, UriKind.Absolute))
                                {
                                    IUriNode uriNode = dataGraphIntegration.CreateUriNode(UriFactory.Create(propertyData.property));
                                    dataGraphIntegration.Assert(new Triple(s, p, uriNode));
                                }
                                else
                                {
                                    ILiteralNode literalNode = dataGraphIntegration.CreateLiteralNode(valor, new Uri("http://www.w3.org/2001/XMLSchema#string"));
                                    dataGraphIntegration.Assert(new Triple(s, p, literalNode));
                                }

                                foreach (string org in propertyData.valueProvenance[valor])
                                {
                                    //TODO urisfactory
                                    //Agregamos los datos de las organizaciones
                                    SparqlResultSet sparqlResultSetOrgs = (SparqlResultSet)dataGraph.ExecuteQuery("select ?s ?p ?o where {?s ?p ?o. FILTER(?s=<http://graph.um.es/res/organization/" + org + ">)}");
                                    foreach (SparqlResult sparqlResult in sparqlResultSetOrgs.Results)
                                    {
                                        INode sOrg = dataGraphIntegration.CreateUriNode(UriFactory.Create(sparqlResult["s"].ToString()));
                                        INode pOrg = dataGraphIntegration.CreateUriNode(UriFactory.Create(sparqlResult["p"].ToString()));
                                        if (sparqlResult["o"] is UriNode)
                                        {
                                            INode oOrg = dataGraphIntegration.CreateUriNode(UriFactory.Create(sparqlResult["o"].ToString()));
                                            dataGraphIntegration.Assert(new Triple(sOrg, pOrg, oOrg));
                                        }
                                        else if (sparqlResult["o"] is LiteralNode)
                                        {
                                            INode oOrg = dataGraphIntegration.CreateLiteralNode(((LiteralNode)sparqlResult["o"]).Value, ((LiteralNode)sparqlResult["o"]).DataType);
                                            dataGraphIntegration.Assert(new Triple(sOrg, pOrg, oOrg));
                                        }

                                    }
                                }
                            }
                        }
                    }
                    //Hora fin de la ejecuci�n
                    DateTime endTime = DateTime.Now;
                    if (integration.Count > 0)
                    {
                        //Si hay datos nuevos los cargamos

                        //Publicamos en el SGI
                        AsioPublication asioPublication = new AsioPublication(SGI_SPARQLEndpoint, SGI_SPARQLQueryParam, SGI_SPARQLGraph, SGI_SPARQLUsername, SGI_SPARQLPassword);
                        //TODO urisfactory
                        asioPublication.PublishRDF(dataGraphIntegration, null, new KeyValuePair<string, string>("http://graph.um.es/res/agent/discover", "Algoritmos de descubrimiento"), startTime, endTime, discoverLinkData);


                        //Preparamos los datos para cargarlos en Unidata
                        RohGraph unidataGraph = dataGraphIntegration.Clone();
                        #region Si no tiene un sameAs apuntando a Unidata lo eliminamos, no hay que cargar la entidad
                        SparqlResultSet sparqlResultSet = (SparqlResultSet)unidataGraph.ExecuteQuery("select ?s ?rdftype ?sameas where {?s a ?rdftype. OPTIONAL{?s <http://www.w3.org/2002/07/owl#sameAs> ?sameAS} }");
                        Dictionary<string, bool> entidadesConSameAsUnidata = new Dictionary<string, bool>();
                        foreach (SparqlResult sparqlResult in sparqlResultSet.Results)
                        {
                            string s = sparqlResult["s"].ToString();
                            if (!entidadesConSameAsUnidata.ContainsKey(s))
                            {
                                entidadesConSameAsUnidata.Add(s, false);
                            }
                            if (sparqlResult.Variables.Contains("sameas"))
                            {
                                if (sparqlResult["sameas"].ToString().StartsWith(UnidataDomain))
                                {
                                    entidadesConSameAsUnidata[s] = true;
                                }
                            }
                        }
                        TripleStore store = new TripleStore();
                        store.Add(unidataGraph);
                        foreach (string entity in entidadesConSameAsUnidata.Keys)
                        {
                            if (!entidadesConSameAsUnidata[entity])
                            {
                                //Cambiamos candidato.Key por entityID
                                SparqlUpdateParser parser = new SparqlUpdateParser();
                                SparqlUpdateCommandSet delete = parser.ParseFromString(@"DELETE { ?s ?p ?o. }
                                                                                WHERE 
                                                                                {
                                                                                    ?s ?p ?o. 
                                                                                    FILTER(?s = <" + entity + @">)
                                                                                
                                                                                }");
                                LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
                                processor.ProcessCommandSet(delete);
                            }
                        }
                        #endregion

                        //Si hay triples para cargar en Unidata procedemos
                        if (unidataGraph.Triples.ToList().Count > 0)
                        {
                            //Publicamos en UNIDATA
                            AsioPublication asioPublicationUnidata = new AsioPublication(Unidata_SPARQLEndpoint, Unidata_SPARQLQueryParam, Unidata_SPARQLGraph, Unidata_SPARQLUsername, Unidata_SPARQLPassword);
                            // Prepara el grafo para su carga en Unidata, para ello coge las URIs de Unidata del SameAs y la aplica a los sujetos y los antiguos sujetos se agregan al SameAs
                            unidataGraph = AsioPublication.TransformUrisToUnidata(unidataGraph, UnidataDomain, UnidataUriTransform);
                            //TODO urisfactory
                            asioPublicationUnidata.PublishRDF(unidataGraph, null, new KeyValuePair<string, string>("http://graph.um.es/res/agent/discover", "Algoritmos de descubrimiento"), startTime, endTime, discoverLinkData);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                }
                Thread.Sleep(pSecondsSleep * 1000);
            }
        }


        /// <summary>
        /// Desordena un listado que se le pasa por par�metro.
        /// </summary>
        /// <param name="pList">Lista</param>
        /// <returns>Lista desordenada</returns>
        private List<string> GetRandomOrderList(List<string> pList)
        {
            List<string> rngList = new List<string>();
            List<string> personListAux = new List<string>();
            Random rng = new Random();
            personListAux.AddRange(pList);
            while (personListAux.Count > 0)
            {
                int val = rng.Next(0, personListAux.Count - 1);
                rngList.Add(personListAux[val]);
                personListAux.RemoveAt(val);
            }
            return rngList;
        }
    }
}
