# Sobre FrontEndCarga

Accesible en pruebas en esta dirección: http://herc-as-front-desa.atica.um.es/carga-web.

## Manual
Este proyecto es una interfaz web desarrollada con el patr�n MVC. Al acceder a la web vemos un men� en la parte superior desde la que se puede acceder a los repositorios, los shapes y las operaciones del urisFactory.
### P�gina principal - listado de repositorios
La p�gina de inicio de la web es el listado de los repositorios, desde este listado se puede crear un repositorio nuevo con el bot�n + que se encuentra por la mitad de la p�gina. ![](img/repositorios.png)
Al acceder a un repositorio podemos ver los shapes que tiene vinculados ese repositorio, as� como las tareas de sincronizaci�n programadas y ejecutadas que ha tenido el repositorio, como se ve en la siguiente imagen.
![](img/repositorio.png)
Desde esta pantalla se pueden crear tanto nuevos shapes asociados al repositorio, como nuevas sincronizaciones. Adem�s se puede editar o eliminar dicho repositorio as� como la informaci�n asociada a �l (sincronizaciones y shapes).
 - Shapes: Listado global de shapes.
 - factor�a de uris: Interfaz desde la que se puede:
	 - Obtener una uri.
	 - Descargar el archivo el archivo de configuración de las uris.
	 - Reemplazar el archivo de configuración de uris.
	 - Eliminar una estructura de uris.
	 - Añadir una nueva estructura de uris.
