#!/bin/bash

>&2 echo "Connection string: $ConnectionStrings__DefaultConnection"

>&2 echo "Wait for DB"
sleep 10

>&2 echo "Run Migrations"
cd TimeVic.Migrations/
dotnet run --configuration Development

>&2 echo "Run Tests"
cd ../TimeVic.Tests.Integration.Api
dotnet test --logger trx --results-directory /var/temp .
