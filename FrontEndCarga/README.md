# Sobre FrontEndCarga

Accesible en pruebas en esta dirección: http://herc-as-front-desa.atica.um.es/carga-web.

## Manual
Este proyecto es una interfaz web desarrollada con el patr�n MVC. Al acceder a la web vemos un men� en la parte superior desde la que se puede acceder a los repositorios, los shapes y las operaciones del urisFactory.
### P�gina principal - listado de repositorios
La página de inicio de la web es el listado de los repositorios, desde este listado se puede crear un repositorio nuevo con el botón + que se encuentra por la mitad de la página. ![](img/repositorios.png)
Al acceder a un repositorio podemos ver los shapes que tiene vinculados ese repositorio, as� como las tareas de sincronizaci�n programadas y ejecutadas que ha tenido el repositorio, como se muestra a continuaci�n.
![](img/repositorio.png)
Desde esta pantalla se pueden crear tanto nuevos shapes asociados al repositorio, como nuevas sincronizaciones. Adem�s se puede editar o eliminar dicho repositorio, adem�s de modificar la informaci�n asociada a �l (shapes y tareas):
 - Modificar shape.
 - Eliminar shape.
 - Eliminar tarea programada.
 - Eliminar tarea recurrente.
Tambi�n se puede acceder a la informaci�n de las tareas, tanto de tareas de �nica ejecuci�n como las tareas recurrentes como se muestra en los apartados siguientes.
### Vista de una tarea
En la pantalla que se muestra a contiaci�n se muestran los datos de una tarea ejecutada y un bot�n con el cual se puede volver a lanzar esta tarea. Los datos motrados son:
 - Identificador de la tarea.
 - La tarea ejecutada.
 - El estado de la tarea, que puede ser que se haya ejecutado con �xito o est� en estado fallido.
 - La fecha de la ejecuci�n en formato UTC.
 - Error que haya causado el fallo de la tarea.
![](img/JobFailDetails.png)
### Vista de una tarea recurrente
En la siguiente im�gen se muestran los datos de una tarea recurrente:
 - El nombre de la tarea recurrente.
 - La expresi�n del cron que indica la recurrencia de dicha tarea.
 - La fecha de la pr�xima ejecuci�n.
 - El identificador de la �ltima tarea ejecutada, en el caso de que se haya ejecutado alguna tarea.
 - El estado de la �ltima tarea ejecutada, en el caso de que se haya ejecutado alguna tarea.
 - Por �ltimo se muestra un litado de las tareas ejecutadas a partir de la tarea recurrente.
![](img/RecurringJobDetails.png)
### Listado de shapes
Listado global de shapes. Desde este listado se pueden acceder a todos los shapes que hay creados
![](img/shapes.png)
Para poder acceder a su informaci�n y poder editarla o eliminar el shape tendremos que acceder a �l.
![](img/shape.png)
### Factoria de uris
Interfaz desde la que se puede:
 - Obtener una uri.
 - Descargar el archivo el archivo de configuraci�n de las uris.
 - Reemplazar el archivo de configuración de uris.
 - Eliminar una estructura de uris.
 - A�adir una nueva estructura de uris.
 ![](img/urisFactory.png)
 A la hora de crear una estructura uri nos aparecer� un texto editable en el que aparece una estructura uri a modo de ayuda, como se ve en la siguiente im�gen
![](img/AddUriStructure.png)
