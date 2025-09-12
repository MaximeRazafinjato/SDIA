# Configuration de la Base de Données SDIA

## État Actuel
L'application utilise actuellement une base de données **InMemory** pour le développement. Les données sont réinitialisées à chaque redémarrage de l'application.

## Utilisateurs de Test
Les utilisateurs suivants sont créés automatiquement au démarrage :
- **Admin** : admin@sdia.com / Admin123!
- **Manager** : manager@sdia.com / Manager123!
- **User** : user@sdia.com / User123!

## Configuration SQL Server (Future)

### Problème Rencontré
La connexion à SQL Server LocalDB a échoué avec l'erreur suivante :
- Erreur de connexion : "Logon failed for login 'AzureAD\MaximeRAZAFINJATO' due to trigger execution"
- Cette erreur indique un problème de configuration des triggers de connexion sur LocalDB

### Pour Activer SQL Server

1. **Vérifier LocalDB**
   ```bash
   sqllocaldb info
   sqllocaldb start MSSQLLocalDB
   ```

2. **Modifier Program.cs**
   Remplacer la configuration InMemory :
   ```csharp
   // De :
   builder.Services.AddDbContext<SimpleSDIADbContext>(options =>
       options.UseInMemoryDatabase("SDIA_DevDB"));
   
   // Par :
   var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
       ?? "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=SDIA_DB;Integrated Security=True;TrustServerCertificate=True;";
   
   builder.Services.AddDbContext<SimpleSDIADbContext>(options =>
       options.UseSqlServer(connectionString));
   ```

3. **Ajouter EnsureCreated pour SQL Server**
   ```csharp
   // Dans Program.cs, section Database initialization
   await dbContext.Database.EnsureCreatedAsync();
   ```

### Solutions Possibles pour l'Erreur de Connexion

1. **Créer une nouvelle instance LocalDB**
   ```bash
   sqllocaldb create SDIAInstance
   sqllocaldb start SDIAInstance
   ```
   Puis utiliser : `Data Source=(LocalDB)\\SDIAInstance`

2. **Utiliser SQL Server Express**
   - Installer SQL Server Express
   - Utiliser : `Server=.\\SQLEXPRESS;Database=SDIA_DB;Integrated Security=True;`

3. **Utiliser une connexion SQL Authentication**
   - Créer un utilisateur SQL dans SSMS
   - Utiliser : `Server=(localdb)\\MSSQLLocalDB;Database=SDIA_DB;User Id=sdia_user;Password=your_password;`

## Visibilité dans SSMS

Pour voir la base de données dans SQL Server Management Studio :
1. Se connecter à : `(localdb)\MSSQLLocalDB` ou `(localdb)\SDIAInstance`
2. La base de données `SDIA_DB` apparaîtra après la première connexion réussie
3. Note : Avec InMemory, la base n'est pas visible dans SSMS

## Recommandation
Pour le développement, l'utilisation d'InMemory est suffisante. Pour la production ou les tests d'intégration, configurer SQL Server Express ou Azure SQL Database serait préférable.