# Play Economy
Creating a small economy system following the tutorial provided by [freeCodeCamp](https://www.youtube.com/watch?v=CqCDOosvZIk) and created by [Julio Casal](https://www.youtube.com/channel/UCw8aBxRvQ2ksWNFuO5eHdmA).

## What is microservices?
Microservices architecture (often shortened to microservices) refers to an architectural style for developing applications. Microservices allow a large application to be separated into smaller independent parts, with each part having its own realm of responsibility. [Google Cloud ☁️](https://cloud.google.com/learn/what-is-microservices-architecture#:~:text=Microservices%20architecture%20\(often%20shortened%20to,its%20own%20realm%20of%20responsibility.)

## Technologies
- RabbitMQ
- MongoDB
- .NET WebApi
- Docker

## How it works?
We use docker to start 2 containers:
- MongoDB database;
- RabbitMQ instance.

Then we start the services with `dotnet run` and you can see the magic happens 🪄.
When we create an item on the Catalog it will send a message to the message-broker (RabbitMQ) with the new item and the consumers will receive that message and do something with it, in this case we create a collection in the Inventory database named `catalogitems`.

## Give a Star! ⭐
If you liked the project please give a star! It will help a lot.