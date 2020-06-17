# Sobre FrontEndCarga

Accesible en pruebas en esta direcci�n: http://herc-as-front-desa.atica.um.es/carga-web.

## Manual
Este proyecto es una interfaz web desarrollada con el patr�n MVC. Al acceder a la web vemos un men� en la parte superior desde la que se puede acceder a los repositorios, los shapes y las operaciones del urisFactory.
### P�gina principal - listado de repositorios
La p�gina de inicio de la web es el listado de los repositorios, desde este listado se puede crear un repositorio nuevo con el bot�n + que se encuentra por la mitad de la p�gina. ![](img/repositorios.png)
Al acceder a un repositorio podemos ver los shapes que tiene vinculados ese repositorio, as� como las tareas de sincronizaci�n programadas y ejecutadas que ha tenido el repositorio, como se muestra a continuaci�n.
![](img/repositorio.png)
Desde esta pantalla se pueden crear tanto nuevos shapes asociados al repositorio, como nuevas sincronizaciones. Adem�s se puede editar o eliminar dicho repositorio as� como modificar la informaci�n asociada a �l:
 - Modificar shape.
 - Eliminar shape.
 - Eliminar tarea.
### Listado de shapes
Listado global de shapes. Desde este listado se pueden acceder a todos los shapes que hay creados
![](img/shapes.png)
Para poder acceder a su informaci�n y poder editarla o eliminar el shape tendremos que acceder a �l.
![](img/shape.png)
### Factoria de uris
Interfaz desde la que se puede:
 - Obtener una uri.
 - Descargar el archivo el archivo de configuraci�n de las uris.
 - Reemplazar el archivo de configuraci�n de uris.
 - Eliminar una estructura de uris.
 - A�adir una nueva estructura de uris.
 ![](img/urisFactory.png)
 A la hora de crear una estructura uri nos aparecer� un texto editable en el que aparece una estructura uri a modo de ayuda, como se ve en la siguiente im�gen
![](img/AddUriStructure.png)