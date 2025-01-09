using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;

var builder = DistributedApplication.CreateBuilder(args);

var databaseName = "EscrowDb";

var postgres = builder
    .AddPostgres("postgres")
    // Set the name of the default database to auto-create on container startup.
    .WithEnvironment("POSTGRES_DB", databaseName);

var database = postgres.AddDatabase(databaseName);

/*builder.AddProject<Escrow.Api>("api")
    .WithReference(database)
    .WaitFor(database);*/

builder.Build().Run();