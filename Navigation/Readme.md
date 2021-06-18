This project is dependant on the Access project and database updates should be run after the Access updates are complete.

Migrations
OSX:
* Create: 1dotnet ef migrations add <name here>`
* Update: `dotnet ef database update --context "NavigationContext"`