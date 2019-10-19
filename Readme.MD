Not the RabbitMQ Library we deserve, but the one that we need 

This library assumes you have at leas some knowledge about how RabbitMQ works. 

As opposed to other frameworks it DOES NOT try to abstract away the details of using RabbitMQ properly.
But on the other hand focuses on a more fluent and readable API for the different message flows and patterns.

If you need to learn more about RabbitMQ in general please take a look at my udemy course:
https://www.udemy.com/course/rabbitmq-from-start-to-finish/?referralCode=AC4E4E5264FFDC3B1A9C

In general you have the following options (more in the documentation)

Connect to the Broker with
IBunny bunny = Bunny.ConnectSingle("amqp://guest:guest@localhost:5672/%2F");
// also available with parameters instead of amqp uri
or connect to a cluster
IBunny bunny = Bunny.ClusterConnect().AddNode("amqp://guest:guest@localhost:5672/%2F")
                      .AddNode("amqp://guest:guest@localhost:5673/%2F")
                      .Connect();

With a bunny (representing a single connection, that does encapsulate ConnectionRetry)
you can access the Declarations to create your RabbitMQ Entities:

IQueue queue = bunny.Setup().Queue();
IExchange exchange = bunny.Setup().Exchange();

Those are Builder interfaces and allow to specify all properties that those Entities have in place.

You can also remove some of those entities or check if they exist
bool deleted = await bunny.Setup().DeleteQueueAsync("queue-name");
bool exists = await bunny.Setup().QueueExistsAsync("queue-name");


If you still need to access the underlying Channel (IModel of the RabbitMQ.Client):
IModel channel = bunny.Channel();

You can also create a new channel by passing the newOne flag set to true
IModel freshChannel =bunny.Channel(newOne:true);

If you need to access the underlying COnnection use
IConnection connection = bunny.Connection;
