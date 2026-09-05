# Mini Souls 2D

## Integrantes
- Santiago Llarull

## Descripción
El presente proyecto propone un videojuego de combate táctico en 2D para PC, utilizando una perspectiva *top-down* (vista superior). El jugador controlará a un personaje dentro de una arena cerrada, con el objetivo principal de enfrentar y derrotar a un enemigo tipo Jefe (*Boss*).

--Video https://youtu.be/FwLm2FJKm8A

La jugabilidad se centrará en la administración estratégica de recursos limitados, principalmente la vida y la estamina, obligando al jugador a decidir con precisión cuándo atacar, bloquear o esquivar. La experiencia está fuertemente inspirada en la filosofía de los juegos *soulslike*: cada error es penalizado de forma estricta, por lo que la victoria no depende de atacar sin parar, sino de la toma de decisiones bajo presión y el aprendizaje progresivo de los patrones del enemigo.

## Tecnologías Utilizadas y Previstas
- **Lenguaje:** C#
- **Framework de desarrollo:** MonoGame (versión `3.8.1.303`) con soporte específico para Windows mediante **MonoGame.Framework.WindowsDX**
- **Plataforma / Runtime:** SDK de .NET 8.0
- **Entorno de desarrollo:** Visual Studio
- **Base de Datos y Persistencia (Previsto):** SQLite y ADO.NET (tecnologías planificadas para el registro local de estadísticas, historial de partidas y métricas de rendimiento del jugador).

## Cómo compilar y ejecutar

1. **Instalar el SDK de .NET:** Es un requisito estricto contar con el **SDK de .NET 8.0** instalado en tu sistema. Podés descargarlo desde la página oficial de Microsoft.
2. **Instalar Visual Studio:** Asegurarse de marcar la opción de "Desarrollo de escritorio con .NET" durante la instalación.
3. **Instalar extensiones y plantillas:** Instalar las extensiones oficiales de MonoGame en Visual Studio para asegurar la compatibilidad.
4. **Clonar el repositorio:** Abrir una terminal de comandos en la carpeta donde se desea guardar el proyecto y ejecutar:
   ```bash
   git clone [https://github.com/santiago-llarull/proyecto_final_MiniSoul2D_santiago_llarull.git](https://github.com/santiago-llarull/proyecto_final_MiniSoul2D_santiago_llarull.git)
