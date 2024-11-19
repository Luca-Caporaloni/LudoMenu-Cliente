# 🎲 Ludo Game

Un proyecto basado en el clásico juego de mesa **Ludo**, desarrollado en **C#** con un enfoque en la lógica del juego, conexión a base de datos, y soporte para múltiples jugadores.

## 🚀 Características

- **Tablero interactivo**: Casillas generadas dinámicamente con colores y efectos visuales.
- **Lógica del juego**: Implementación completa de reglas, incluyendo lanzamiento de dados, movimientos y colisiones.
- **Casillas especiales**: Incluyen efectos como "Pierde turno", "Retrocede 10", "Avanza 10", y "Pierde ficha".
- **Soporte multijugador**: Gestión de jugadores conectados con asignación automática de colores.
- **Conexión a base de datos**: Registro de sesiones de juego, jugadores y estado del juego.
- **Interfaz gráfica intuitiva**: Desarrollada con Windows Forms para una experiencia amigable.

## 📋 Requisitos

### Software necesario
- **Visual Studio** (recomendado 2019 o superior).
- .NET Framework (compatible con tu versión de Visual Studio).
- SQL Server para la base de datos.

### Base de datos
Crea una base de datos para almacenar los datos del juego utilizando las siguientes tablas básicas:
```sql
CREATE TABLE Players (
    PlayerID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100),
    Color NVARCHAR(50),
    SessionID INT,
    IsOnline BIT
);

CREATE TABLE Games (
    GameID INT IDENTITY PRIMARY KEY,
    SessionID INT,
    CurrentTurn INT,
    IsGameOver BIT
);
```

## 📂 Estructura del proyecto

```plaintext
LudoGame/
├── BL/                   # Lógica del juego
├── DAL/                  # Acceso a datos
├── LudoMenu/             # Interfaz de usuario y lógica principal
├── Resources/            # Recursos gráficos (opcional)
├── README.md             # Este archivo
└── .gitignore            # Archivos ignorados por Git
```

## ⚙️ Configuración

1. Clona el repositorio:
   ```bash
   git clone https://github.com/Luca-Caporaloni/LudoMenu-Cliente.git
   cd ludo-game
   ```

2. Configura la cadena de conexión en el archivo principal del proyecto (`LudoGame.cs`):
   ```csharp
   string connectionString = "Data Source=localhost;Initial Catalog=LudoDB;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";
   ```

3. Compila el proyecto en Visual Studio.

4. Ejecuta la aplicación.

## 🕹️ Cómo jugar

1. Lanza los dados usando el botón **"Tirar Dado"**.
2. Sigue las reglas del juego:
   - Si sacas un 6, puedes mover una ficha fuera de la casa o tirar de nuevo.
   - Mueve las fichas estratégicamente para llegar a la casilla 100 antes que tus oponentes.
3. ¡Disfruta y compite para ganar!

## 👥 Colaboradores

- **Luca** - *Desarrollador principal*
- **Franco** - *Desarrollador principal*
- **Cuello** - *Desarrollador principal*

## 📜 Licencia

Este proyecto está bajo la licencia MIT - consulta el archivo [LICENSE](LICENSE) para más detalles.

## 📞 Contacto

Si tienes preguntas o sugerencias, no dudes en contactarme en:  
🌐 [GitHub](https://github.com/Luca-Caporaloni)

--- 
