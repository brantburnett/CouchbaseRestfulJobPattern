# Couchbase Restful Job Pattern

This repository provides an example of a C# pattern for handling long running jobs in a REST microservice or other cloud environment, using Couchbase as the backing data store. However, this pattern could be used for any distributed data storage mechanism capable of atomic writes for lock management.

## Prerequisites

1. [Visual Studio](https://visualstudio.microsoft.com/) (sorry, no VSCode at the moment)
2. [Docker for Windows](https://docs.docker.com/docker-for-windows/install/)
3. Docker must be configured for Linux containers (the default)
4. The drive where this project lives must be [shared within Docker](https://docs.docker.com/docker-for-windows/#shared-drives)

## Running the Sample

Just set the startup project in Visual Studio to be the "docker-compose" project and hit F5. A copy of Couchbase is built into the docker-compose project and will be started and configured within a Linux container.

**Note:** The first time you run the project it may take a while as it downloads all the required Docker images and configures Couchbase.