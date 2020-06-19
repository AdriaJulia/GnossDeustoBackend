![](../Docs/media/CabeceraDocumentosMD.png)

# Sobre FrontEnd de Carga

Este m�dulo constituye el interfaz Web de administraci�n de las cargas de datos en la plataforma H�rcules ASIO. Esta aplicaci�n web realizada mediante el patr�n MVC (m�delo-vista-controlador) esta formado por 5 controladores, que a su vez estos controladores se comunican con los diferentes apis creados en este proyecto. Estos controladores dividen su �mbito en la gesti�n de errores, de los repositorios, de los shapes, de las tareas y la gesti�n de la factoria de uris:
  - ErrorController: Gestiona los errores para devolver una p�gina 404 o 500 seg�n la excepci�n que se genera.
  - JobController: Controlador que gestiona las operaciones (listado, obtenci�n de tareas, ejecuci�n, ...) que se realizan en la web con cualquier tipo de tarea.
  - RepositoryConfigController: Controlador encargado de gestionar las operaciones con los repositorios. Creaci�n, modificaci�n, eliminaci�n y listado.
  - ShapeConfigController: Encargado de gestionar las operaciones con los shapes. Creaci�n, modificaci�n, eliminaci�n y listado.
  - UrisFactoryController: Este controlador es el encargado de gestionar todas las tareas que se pueden realizar con el api de UrisFactory, tanto la obtenci�n de uris como las operaciones con el fichero de configuraci�n de las uris.

Hay una versi�n disponible en el entorno de pruebas de la Universidad de Murcia, en esta direcci�n: 

http://herc-as-front-desa.atica.um.es/carga-web.


En esta carpeta est� disponible el [Manual de Usuario del FrontEnd](https://github.com/HerculesCRUE/GnossDeustoBackend/blob/master/FrontEndCarga/Manual%20de%20usuario.md).
