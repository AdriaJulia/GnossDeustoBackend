![](../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 01/10/2020                                                   |
| ------------- | ------------------------------------------------------------ |
|Titulo|Cambios en la documentaci�n| 
|Descripci�n|insercci�n de configuraciones en el appsettings|
|Versi�n|0.3|
|M�dulo|FrontEndCarga|
|Tipo|Documentaci�n|
|Cambios de la Versi�n| Se ha a�adido dos configuraciones al appsettings|


# Sobre FrontEnd de Carga

Este m�dulo constituye el interfaz Web de administraci�n de las cargas de datos en la plataforma H�rcules ASIO. Esta aplicaci�n web realizada mediante el patr�n MVC (m�delo-vista-controlador) esta formado por diferentes controladores que se comunican con los diferentes apis creados en este proyecto. Estos controladores dividen su �mbito en la gesti�n de errores, de los repositorios, de los shapes, de las tareas, la gesti�n de la factoria de uris, etc.:
  - ErrorController: Gestiona los errores para devolver una p�gina 404 o 500 seg�n la excepci�n que se genera.
  - JobController: Controlador que gestiona las operaciones (listado, obtenci�n de tareas, ejecuci�n, ...) que se realizan en la web con cualquier tipo de tarea.
  - RepositoryConfigController: Controlador encargado de gestionar las operaciones con los repositorios. Creaci�n, modificaci�n, eliminaci�n y listado.
  - ShapeConfigController: Encargado de gestionar las operaciones con los shapes. Creaci�n, modificaci�n, eliminaci�n y listado.
  - UrisFactoryController: Este controlador es el encargado de gestionar todas las tareas que se pueden realizar con el api de UrisFactory, tanto la obtenci�n de uris como las operaciones con el fichero de configuraci�n de las uris.
  - CMSController: Controlador encargado de gestionar las p�ginas creadas por los usuarios.
  - TokenController: Encargado de obtener los tokens de acceso para los diferentes apis.
  - CheckSystemController: Se encarga de verificar que el api cron y el api carga funcionan correctamente.

Hay una versi�n disponible en el entorno de pruebas de la Universidad de Murcia, en esta direcci�n: 

https://herc-as-front-desa.atica.um.es/carga-web.


En esta carpeta est� disponible el [Manual de Usuario del FrontEnd](https://github.com/HerculesCRUE/GnossDeustoBackend/blob/master/FrontEndCarga/Manual%20de%20usuario.md).

## Configuraci�n en el appsettings.json
 >
    {
		"ConnectionStrings": {
			"PostgreConnectionmigration": ""
		},
		"Logging": {
			"LogLevel": {
				"Default": "Information",
				"Microsoft": "Warning",
				"Microsoft.Hosting.Lifetime": "Information"
			}
		},
		"Sparql": {
			"Endpoint": "http://localhost:8890/sparql",
			"QueryParam": "query"
		},
		"AllowedHosts": "*",
		"LogPath": "",
		"LogPathCarga": "",
		"LogPathCron": "",
		"Urls": "http://0.0.0.0:5103",
		"ConfigUrl": "http://herc-as-front-desa.atica.um.es/carga/",
		"ConfigUrlDocumentacion": "http://herc-as-front-desa.atica.um.es/documentacion/",
		"ConfigUrlCron": "http://herc-as-front-desa.atica.um.es/cron-config/",
		"ConfigUrlUrisFactory": "http://herc-as-front-desa.atica.um.es/uris/",
		"Authority": "http://localhost:56306/connect/token",
		"GrantType": "client_credentials",
		"Scope": "apiCarga",
		"ScopeCron": "apiCron",
		"ScopeUrisFactory": "apiUrisFactory",
		"ScopeDocumentacion": "apiGestorDocumentacion",
		"ScopeOAIPMH": "apiOAIPMH",
		"ClientId": "Web",
		"ClientIdOAIPMH": "OAIPMH",
		"ClientSecretOAIPMH": "secretOAIPMH",
		"ClientSecret": "master",
		"Proxy": "/carga-web"
	}
 - LogLevel.Default: Nivel de error por defecto
 - LogLevel.Microsoft: Nivel de error para los errores propios de Microsoft
 - LogLevel.Microsoft.Hosting.Lifetime: Nivel de error para los errores de host
 - PostgreConnectionmigration: Conexi�n con la base de datos
 - Sparql.Endpoint: URL del Endpoint Sparql
 - Sparql.QueryParam: Par�metro para la query en el Endpoint Sparql
 - LogPath: Ruta donde va a guardar los logs de la aplicaci�n
 - LogPathCarga: Ruta donde escribe los logs el apiCarga
 - LogPathCron: Ruta donde escribe los logs el apiCron
 - Urls: Url en la que se va a lanzar la aplicaci�n
 - ConfigUrlDocumentacion: Url donde est� lanzada la aplicaci�n de apiDocumentacion
 - ConfigUrl: Url donde est� lanzada la aplicaci�n API Carga
 - ConfigUrlCron: Url donde est� lanzada la aplicaci�n CronConfigure
 - ConfigUrlUrisFactory: Url donde est� lanzada la aplicaci�n UrisFactory
 - Authority: Endpoint para la llamada de obtenci�n del token
 - GrantType: Tipo de concesi�n de Oauth
 - Scope: Limitaci�n de acceso al api de carga
 - ScopeCron: Limitaci�n de acceso al api de cron
 - ScopeDocumentacion: Limitaci�n de acceso al api de documentaci�n
 - ScopeOAIPMH: Limitaci�n de acceso al api de OAIPMH
 - ScopeUrisFactory: Limitaci�n de acceso al api de urisFactory
 - ClientId: Id de cliente, en este caso se ha configurado un cliente que pueda acceder a todas las apis que usa la web
 - ClientIdOAIPMH: Id de cliente de OAIPMH
 - ClientSecretOAIPMH: "clave" de acceso del cliente de OAIPMH
 - ClientSecret: "clave" de acceso del cliente
 - Proxy: directorio virtual que se ha configurado para el proxy inverso, en caso de que no se haya configurado dejar vac�o.
 
Se puede encontrar un el appsettings usado para este servicio sin datos sensibles en: https://github.com/HerculesCRUE/GnossDeustoBackend/blob/master/FrontEndCarga/ApiCargaWebInterface/appsettings.json

## Dependencias

- **AspNetCore.Security.CAS**: versi�n 2.0.5
- **CsvHelper**: versi�n 15.0.6
- **dotNetRDF**: versi�n 2.6.0
- **Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation**: versi�n 3.1.8
- **Microsoft.EntityFrameworkCore**: versi�n 3.1.8
- **Microsoft.NETCore.App**: versi�n 2.2.8
- **Microsoft.VisualStudio.Web.CodeGeneration.Design**: versi�n 3.1.4
- **NCrontab**: versi�n 3.3.1
- **Npgsql.EntityFrameworkCore.PostgreSQL**: versi�n 3.1.4
- **Serilog.AspNetCore**: versi�n 3.2.0