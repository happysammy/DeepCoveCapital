# DeepCoveCapital
Provides the source code for Deep Cove Capital, a trading platform which connects to various Cryptocurrency exchanges. Developed around a central app to be deployed on a server, with websocket and REST API tracking of status via Desktop (WPF) and web applications.

# Architecture of Deep Cove Capital Application
This application is designed using a multitude of archiectural principles. The software is designed using a loosley coupled framework, which was inspired by "ENTER THE GITHUB REPO HERE" as well as the open source framework of QuantConnect's LEAN framework. This architecture is separated into six main components: Core, Data, Exchanges, Infrastructure, Strategies, and Trade Executions. Each of these compnents are described below. TODO: Add a diagram of how the projects interact with one another, in infrastructure at the center of the architectural framework.

# DeepCoveCapital.Core
The Core of the application contains all of the base classes and enums that will be used throughout the solution. This project is completely standalone and could be used as the base class for future, unrelated projects.

# DeepCoveCapital.Data
This project contains all of the data classes which will be held in memory when doing trade calculations. It will also hold the database which will allow for the application to push completed trades and track the PnL of the strategy. This project references the Core project only and will be used by the main infrastructure project. 
