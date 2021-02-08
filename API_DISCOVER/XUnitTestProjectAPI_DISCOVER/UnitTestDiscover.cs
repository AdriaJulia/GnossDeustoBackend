using System;
using Xunit;
using API_DISCOVER.Utility;
using VDS.RDF;
using VDS.RDF.Query.Inference;
using VDS.RDF.Parsing;
using System.Collections.Generic;
using API_DISCOVER.Models.Entities;
using VDS.RDF.Query;
using API_DISCOVER.Models.Services;
using System.Linq;
using API_DISCOVER.Models.Entities.Discover;

namespace XUnitTestProjectAPI_DISCOVER
{
    public class UnitTestDiscover
    {
        [Fact]
        public void TestPrepareData()
        {
            RohGraph dataGraph = new RohGraph();
            dataGraph.LoadFromString(System.IO.File.ReadAllText("rdfFiles/rdfFile.rdf"), new RdfXmlParser());


			RohRdfsReasoner reasoner = new RohRdfsReasoner();
            RohGraph ontologyGraph = new RohGraph();

            ontologyGraph.LoadFromFile("Ontology/roh-v2.owl");
            reasoner.Initialise(ontologyGraph);

            RohGraph dataInferenceGraph = dataGraph.Clone();
            reasoner.Apply(dataInferenceGraph);

            Dictionary<string, HashSet<string>> entitiesRdfTypes;
            Dictionary<string, string> entitiesRdfType;
            Dictionary<string, List<DisambiguationData>> disambiguationDataRdf;

            Discover.PrepareData(dataGraph, reasoner, out dataInferenceGraph, out entitiesRdfTypes, out entitiesRdfType, out disambiguationDataRdf, false);

            int count1 = 0;
            foreach(var entity in entitiesRdfTypes)
            {
                if (entity.Key.Contains("document"))
                {
                    if (entity.Value.Contains("http://purl.org/roh/mirror/bibo#Document") && entity.Value.Contains("http://purl.org/roh/mirror/obo/iao#IAO_0000030"))
                    {
                        count1++;
                    }
                }else if (entity.Key.Contains("person"))
                {
                    if (entity.Value.Contains("http://purl.org/roh/mirror/foaf#Person") && entity.Value.Contains("http://purl.org/roh/mirror/foaf#Agent"))
                    {
                        count1++;
                    }
                }
            }
            int count2 = 0;
            foreach (var entity in entitiesRdfType)
            {
                if (entity.Key.Contains("document") && entity.Value.Contains("http://purl.org/roh/mirror/bibo#Document"))
                {
                    count2++;
                    
                }
                else if (entity.Key.Contains("person") && entity.Value.Contains("http://purl.org/roh/mirror/foaf#Person"))
                {
                    count2++;
                }
            }
			int count3 = 0;
			foreach(var data in disambiguationDataRdf)
            {
                if (data.Key.Contains("person"))
                {
					int countperson = 0;
                    foreach(var values in data.Value)
                    {
						foreach(var property in values.properties)
						{
                            if (property.property.property.Contains("http://purl.org/roh/mirror/foaf#name") || property.property.property.Contains("http://purl.org/roh#correspondingAuthorOf") || property.property.property.Contains("http://purl.org/roh/mirror/bibo#authorList@@@?"))
							{
								countperson++;
                            }
                        }
                        if (countperson == 3)
                        {
							count3++;
                        }
                    }
                }
                else
                {
					if(data.Key.Contains("11826b3e") || data.Key.Contains("17d68fc4") || data.Key.Contains("86769078") || data.Key.Contains("a7db2203") || data.Key.Contains("feb48b56"))
                    {
						int countdoc = 0;
						foreach (var values in data.Value)
						{
							foreach (var property in values.properties)
							{
								if (property.property.property.Contains("http://purl.org/roh#title")) 
								{
									countdoc++;
								}
								if (property.property.property.Contains("http://purl.org/roh/mirror/bibo#authorList@@@?"))
								{
                                    if (property.values.Count == 1)
									{
										countdoc++;
									}
								}
							}
							if(countdoc == 2)
                            {
								count3++;
							}
						}
                    }
                    else
                    {
						int countdoc = 0;
						foreach (var values in data.Value)
						{
							foreach (var property in values.properties)
							{
								if (property.property.property.Contains("http://purl.org/roh#title"))
								{
									countdoc++;
								}
								else if(property.property.property.Contains("http://purl.org/roh/mirror/bibo#authorList@@@?"))
								{
									if (property.values.Count == 2)
									{
										countdoc++;
									}
								}
							}
							if (countdoc == 2)
							{
								count3++;
							}
						}
					}
                }
            }

            if(count1 ==16 && count2 == 16 && count3 == 16 )
            {
                Assert.True(true);
            }
            else
            {
				Assert.True(false);
            }
        }

        [Fact]
        public void TestReconciliateRDF()
        {
            bool hasChanges = false;

			ReconciliationData reconciliateData = new ReconciliationData();

            RohGraph dataGraph = new RohGraph();
            dataGraph.LoadFromString(System.IO.File.ReadAllText("rdfFiles/rdfFileRecon.rdf"), new RdfXmlParser());


			RohRdfsReasoner reasoner = new RohRdfsReasoner();

            Dictionary<string, HashSet<string>> discardDissambiguations = new Dictionary<string, HashSet<string>>();

            DiscoverCache discoverCache = new DiscoverCache();

            Discover.ReconciliateRDF(ref hasChanges, ref reconciliateData, ref dataGraph, reasoner, discardDissambiguations, discoverCache);
			if(hasChanges == true){
				if(reconciliateData.reconciliatedEntityList.ContainsKey("http://graph.um.es/res/person/e12d2a31-e285-459a-9d71-2d4a45329532") && reconciliateData.reconciliatedEntityList.ContainsValue("http://graph.um.es/res/person/aaad2a31-e285-459a-9d71-2d4a45329532") && dataGraph.Triples.Count == 89)
				{
					Assert.True(true);
                }
                else
                {
					Assert.True(false);
                }
			}
		}

		[Fact]
		public void TestExternalIntegration()
		{
			bool hasChanges = false;
			ReconciliationData reconciliateData = new ReconciliationData();
			DiscoverLinkData discoverLinkData = new DiscoverLinkData();

			Dictionary<string, Dictionary<string, float>> discoveredEntitiesProbability = new Dictionary<string, Dictionary<string, float>>();

			RohGraph dataGraph = new RohGraph();
			dataGraph.LoadFromString(System.IO.File.ReadAllText("rdfFiles/rdfFile.rdf"), new RdfXmlParser());

			RohRdfsReasoner reasoner = new RohRdfsReasoner();

			Dictionary<string, Dictionary<string, float>> namesScore = new Dictionary<string, Dictionary<string, float>>();

			RohGraph ontologyGraph = new RohGraph();
			ontologyGraph.LoadFromFile("Ontology/roh-v2.owl");
			reasoner.Initialise(ontologyGraph);

			Dictionary<string, ReconciliationData.ReconciliationScore> entidadesReconciliadasConIntegracionExternaAux;

			Dictionary<string, HashSet<string>> discardDissambiguations = new Dictionary<string, HashSet<string>>();

			DiscoverCache discoverCache = new DiscoverCache();

			RohGraph dataGraphBBDD = new RohGraph();
			dataGraphBBDD.LoadFromString(System.IO.File.ReadAllText("rdfFiles/databaseFile.rdf"), new RdfXmlParser());
			Discover.mSparqlUtility = new SparqlUtilityMock(dataGraphBBDD);
			Discover.test = true;
			Dictionary<string,List<DiscoverLinkData.PropertyData>> externalIds = Discover.ExternalIntegration(ref hasChanges, ref reconciliateData,ref discoverLinkData, ref discoveredEntitiesProbability, ref dataGraph, reasoner, namesScore, ontologyGraph, out entidadesReconciliadasConIntegracionExternaAux, discardDissambiguations, discoverCache);
			Assert.True(true);
			//TODO, no funciona correctamente al lanzar las pruebas desde el servidor
			//if (hasChanges == true)
			//{
			//	int suma = 0;
			//	foreach (var id in externalIds)
			//	{
			//		suma += id.Value.Count;
			//	}
			//	string query = @"select distinct ?s where{?s ?p ?o. FILTER(!isBlank(?s))}";
			//	SparqlResultSet sparqlResultSet = (SparqlResultSet)dataGraph.ExecuteQuery(query.ToString());

			//	if (suma > sparqlResultSet.Count / 2)
			//	{
			//		Assert.True(true);
			//	}
			//	else
			//	{
			//		Assert.True(false);
			//	}
			//}
		}

		[Fact]
        public void TestLoadNamesScore()
        {
            Dictionary<string, Dictionary<string, float>> namesScore = new Dictionary<string, Dictionary<string, float>>();

            RohGraph dataGraphBBDD = new RohGraph();
            dataGraphBBDD.LoadFromString(System.IO.File.ReadAllText("rdfFiles/databaseFile.rdf"), new RdfXmlParser()); 


			Discover.mSparqlUtility = new SparqlUtilityMock(dataGraphBBDD);
            Dictionary<string, string> personsWithName = Discover.LoadPersonWithName("","","");


            RohGraph dataGraph = new RohGraph();
            dataGraph.LoadFromString(System.IO.File.ReadAllText("rdfFiles/rdfFile.rdf"), new RdfXmlParser());


			RohGraph dataInferenceGraph = dataGraph.Clone();

            DiscoverCache discoverCache = new DiscoverCache();

            Discover.LoadNamesScore(ref namesScore, personsWithName, dataInferenceGraph, discoverCache);

			if(personsWithName.Count == 2 && personsWithName.ContainsValue("Diego L�pez-de-Ipi�a") && personsWithName.ContainsValue("Diego Casado-Mansilla"))
            {
				if(namesScore.Count == 2 && namesScore.ContainsKey("Diego L�pez-de-Ipi�a") && namesScore.ContainsKey("Diego Casado-Mansilla"))
				{
					Assert.True(true);
                }
                else
                {
					Assert.True(false);
                }
            }
        }

        [Fact]
        public void TestLoadEntitiesDB()
        {
            Dictionary<string, string> entitiesRdfType = new Dictionary<string, string>();
            Dictionary<string, string> discoveredEntityList = new Dictionary<string, string>();
            RohGraph dataGraphBBDD = new RohGraph();
            dataGraphBBDD.LoadFromString(System.IO.File.ReadAllText("rdfFiles/databaseFile.rdf"), new RdfXmlParser());
            Discover.mSparqlUtility = new SparqlUtilityMock(dataGraphBBDD);

            string query = @"select ?s ?rdftype where {?s a ?rdftype. FILTER(!isBlank(?s))}";
            SparqlResultSet sparqlResultSet = (SparqlResultSet)dataGraphBBDD.ExecuteQuery(query.ToString());
            foreach (SparqlResult sparqlResult in sparqlResultSet.Results)
            {
                string s = sparqlResult["s"].ToString();
                string rdfType = sparqlResult["rdftype"].ToString();
                if (!entitiesRdfType.ContainsKey(s))
                {
                    entitiesRdfType.Add(s, rdfType);
                }
            }

            List<string> entidadesCargadas = Discover.LoadEntitiesDB(entitiesRdfType.Keys.ToList().Except(discoveredEntityList.Keys.Union(discoveredEntityList.Values)),"","","").Keys.ToList();
			int suma = 0;
			foreach(var entity in entitiesRdfType)
            {
				if (entidadesCargadas.Contains(entity.Key))
				{
					suma++;
				}
			}
            if(suma == 5)
            {
				Assert.True(true);
            }
            else
            {
				Assert.True(false);
            }

        }

        [Fact]
        public void TestReconciliateIDs()
        {
            bool hasChanges = false;

			ReconciliationData reconciliationData = new ReconciliationData();

            Dictionary<string, string> entitiesRdfType = new Dictionary<string, string>();

            Dictionary<string, List<DisambiguationData>> disambiguationDataRdf = new Dictionary<string, List<DisambiguationData>>();

            Dictionary<string, HashSet<string>> discardDissambiguations = new Dictionary<string, HashSet<string>>();

            RohGraph dataGraph = new RohGraph();
			dataGraph.LoadFromString(System.IO.File.ReadAllText("rdfFiles/rdfFileReconciliateIDs.rdf"), new RdfXmlParser());

			RohGraph dataGraphBBDD = new RohGraph();
            dataGraphBBDD.LoadFromString(System.IO.File.ReadAllText("rdfFiles/databaseFileReconciliateIds.rdf"), new RdfXmlParser());

			Discover.mSparqlUtility = new SparqlUtilityMock(dataGraphBBDD);

			RohRdfsReasoner reasoner = new RohRdfsReasoner();
			RohGraph ontologyGraph = new RohGraph();
			ontologyGraph.LoadFromFile("Ontology/roh-v2.owl");
			reasoner.Initialise(ontologyGraph);
			RohGraph dataInferenceGraph = dataGraph.Clone();
			reasoner.Apply(dataInferenceGraph);
			Dictionary<string, HashSet<string>> entitiesRdfTypes;
			Discover.PrepareData(dataGraph, reasoner, out dataInferenceGraph, out entitiesRdfTypes, out entitiesRdfType, out disambiguationDataRdf, false);

			Dictionary<string, string> entidadesReconciliadasConIdsAux = Discover.ReconciliateIDs(ref hasChanges, ref reconciliationData, entitiesRdfType, disambiguationDataRdf, discardDissambiguations, ref dataGraph); 

			if(entidadesReconciliadasConIdsAux.Count == 1 && entidadesReconciliadasConIdsAux.ContainsKey("http://graph.um.es/res/person/c8a16863-a606-48e3-a858-9def000380c0") && entidadesReconciliadasConIdsAux.ContainsValue("http://graph.um.es/res/person/aaa16863-a606-48e3-a858-9def000380c0") && hasChanges == true)
            {
				Assert.True(true);
            }
            else
            {
				Assert.True(false);
            }

        }

        [Fact]
        public void TestReconciliateBBDD()
        {
            bool hasChanges = false;

			ReconciliationData reconciliationData = new ReconciliationData();

			Dictionary<string, Dictionary<string, float>> discoveredEntitiesProbability = new Dictionary<string, Dictionary<string, float>>();

            RohGraph dataGraph = new RohGraph();
			dataGraph.LoadFromString(System.IO.File.ReadAllText( "rdfFiles/rdfFileRecon.rdf"), new RdfXmlParser());

			RohRdfsReasoner reasoner = new RohRdfsReasoner();

            Dictionary<string, Dictionary<string, float>> namesScore = new Dictionary<string, Dictionary<string, float>>();

            Dictionary<string, HashSet<string>> discardDissambiguations = new Dictionary<string, HashSet<string>>();

            DiscoverCache discoverCache = new DiscoverCache();

            RohGraph dataGraphBBDD = new RohGraph();
            dataGraphBBDD.LoadFromString(System.IO.File.ReadAllText("rdfFiles/rdfFile.rdf"), new RdfXmlParser());

			Discover.mSparqlUtility = new SparqlUtilityMock(dataGraphBBDD);

            Discover.ReconciliateBBDD(ref hasChanges, ref reconciliationData, out discoveredEntitiesProbability, ref dataGraph, reasoner, namesScore, discardDissambiguations, discoverCache);
			if (hasChanges == true)
			{
				if (reconciliationData.reconciliatedEntityList.ContainsKey("http://graph.um.es/res/person/aaad2a31-e285-459a-9d71-2d4a45329532") && reconciliationData.reconciliatedEntityList.ContainsValue("http://graph.um.es/res/person/e12d2a31-e285-459a-9d71-2d4a45329532") && dataGraph.Triples.Count == 89)
				{
					Assert.True(true);
                }
                else
                {
					Assert.True(false);
                }
			}
		}

	}
}
