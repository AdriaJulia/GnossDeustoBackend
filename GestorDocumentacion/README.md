![](../Docs/media/CabeceraDocumentosMD.png)

# Sobre api gestor de documentaci�n

Este m�dulo se usa para cargar p�ginas HTML y servirlas a trav�s de la web a modo de un gestor de contenidos, en el que los usuarios pueden subir p�ginas html con una ruta
determinada y luego acceder a esas p�ginas a trav�s de la web (https://herc-as-front-desa.atica.um.es/carga-web), estas operaciones est�n disponibles en el controlador Page
  - Controlador Page: contiene 4 m�todos para realizar las operaciones con las p�ginas
	 - *Get:* /Page: Que devuelve una p�gina dada su ruta.
	 - *Get:* /Page/list: Que devuelve la lista de p�ginas sin el html.
	 - *Post:* /Page/loadpage: Carga una nueva p�gina o modifica un p�gina existente
	 - *Delete:* /Page/Delete: Elimina una p�gina dando su identificador

Hay una versi�n disponible en el entorno de pruebas de la Universidad de Murcia, en esta direcci�n: 

http://herc-as-front-desa.atica.um.es/documentacion.
