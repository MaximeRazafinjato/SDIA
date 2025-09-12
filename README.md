# SDIA - Système de suivi D'Inscription avec IA

## 🎯 Vue d'ensemble

SDIA est une application MVP de suivi d'inscription assistée par IA, conçue pour les organismes de formation (CFA) et facilement extensible à d'autres domaines. L'application permet de gérer efficacement les dossiers d'inscription avec un workflow complet depuis la création jusqu'à la validation.

## 🚀 État actuel du projet

### ✅ Backend (.NET 9 / C#)
- **API REST** : https://localhost:7089 (en cours d'exécution)
- **Documentation API** : https://localhost:7089/scalar/v1
- **Architecture** : Clean Architecture avec DDD
- **Base de données** : SQL Server avec Entity Framework Core 8
- **Services** : Email (SendGrid), SMS (Twilio), Stockage de fichiers

### ✅ Frontend (React 19 + TypeScript + Vite)
- **Application Web** : http://localhost:5173 (en cours d'exécution)
- **UI Framework** : Material-UI v6
- **État** : React Query + Context API
- **Formulaires** : React Hook Form + Zod

## 📋 Fonctionnalités principales

### Gestion des utilisateurs
- ✅ Authentification (JWT/Cookies)
- ✅ Gestion des rôles (Admin, Manager, User)
- ✅ Double authentification (Email + SMS)

### Gestion des inscriptions
- ✅ Dashboard avec statistiques en temps réel
- ✅ Workflow complet (Draft → Pending → Validated/Rejected)
- ✅ Relances automatiques par email/SMS
- ✅ Upload sécurisé de documents
- ✅ Formulaires dynamiques et paramétrables

### Paramétrabilité
- ✅ Templates de formulaires configurables
- ✅ Champs conditionnels (mineur/majeur)
- ✅ Fournisseurs email/SMS configurables
- ✅ Multi-organisation

## 🏗️ Architecture technique

```
SDIA/
├── SDIA/                          # Backend .NET
│   ├── SDIA.API/                 # API REST
│   ├── SDIA.Application/         # Logique métier
│   ├── SDIA.Core/                # Entités du domaine
│   ├── SDIA.Infrastructure/      # Services externes
│   ├── SDIA.SharedKernel/        # Code partagé
│   └── SDIA.IntegrationTests/    # Tests d'intégration
│
└── sdia-client/                   # Frontend React
    ├── src/
    │   ├── api/                  # Configuration Axios
    │   ├── components/           # Composants UI
    │   ├── contexts/             # Contextes React
    │   ├── pages/                # Pages de l'application
    │   └── types/                # Types TypeScript
    └── package.json
```

## 🔧 Installation et configuration

### Prérequis
- .NET 9 SDK
- Node.js 18+
- SQL Server (LocalDB ou instance complète)

### Backend
```bash
cd SDIA
dotnet restore
dotnet build
dotnet run --project SDIA.API
```

### Frontend
```bash
cd sdia-client
npm install
npm run dev
```

### Configuration
1. **Backend** : Modifier `SDIA/SDIA.API/appsettings.json`
   - Connection string SQL Server
   - Clés SendGrid et Twilio
   - Configuration JWT

2. **Frontend** : Modifier `sdia-client/.env`
   - URL de l'API backend

## 📊 Modèle de données

### Entités principales
- **Organization** : Structure cliente (CFA, etc.)
- **User** : Utilisateurs du système
- **Registration** : Dossiers d'inscription
- **FormTemplate** : Modèles de formulaires
- **Document** : Documents uploadés
- **RegistrationHistory** : Historique des modifications

## 🔐 Sécurité

- Authentification JWT avec refresh tokens
- Validation côté client (Zod) et serveur (FluentValidation)
- Upload sécurisé avec validation des types MIME
- Soft delete pour préserver l'historique
- Logging complet avec Serilog
- CORS configuré pour le développement

## 📈 Évolutions prévues

### Court terme
- [ ] Intégration IA pour l'analyse des documents
- [ ] Export des dossiers validés (PDF, Excel)
- [ ] Notifications push
- [ ] Mode hors ligne

### Moyen terme
- [ ] API d'intégration avec ERP (Ypareo)
- [ ] Application mobile
- [ ] Signature électronique
- [ ] Workflow personnalisable par organisation

## 🛠️ Technologies utilisées

### Backend
- .NET 9 / C# 13
- Entity Framework Core 8
- SQL Server
- SendGrid / Twilio
- Serilog
- FluentValidation
- MediatR
- AutoMapper

### Frontend
- React 19
- TypeScript 5
- Vite 6
- Material-UI 6
- React Query 5
- React Router 7
- Axios
- React Hook Form
- Zod
- Recharts

## 📝 Documentation API

L'API est documentée automatiquement via OpenAPI/Swagger.
Accès : https://localhost:7089/scalar/v1

### Endpoints principaux

#### Authentification
- `POST /api/auth/login` - Connexion
- `POST /api/auth/logout` - Déconnexion
- `POST /api/auth/register` - Inscription
- `GET /api/auth/me` - Utilisateur actuel

#### Inscriptions
- `GET /api/registrations` - Liste des inscriptions
- `GET /api/registrations/{id}` - Détail d'une inscription
- `POST /api/registrations` - Créer une inscription
- `PUT /api/registrations/{id}/status` - Changer le statut
- `POST /api/registrations/{id}/verify-email` - Vérifier l'email
- `POST /api/registrations/{id}/verify-phone` - Vérifier le téléphone

#### Templates de formulaires
- `GET /api/form-templates` - Liste des templates
- `GET /api/form-templates/{id}` - Détail d'un template
- `POST /api/form-templates` - Créer un template
- `PUT /api/form-templates/{id}` - Modifier un template
- `DELETE /api/form-templates/{id}` - Supprimer un template

## 👥 Équipe et contribution

Projet développé avec l'architecture ASK (AppStarterKit) pour garantir maintenabilité et évolutivité.

## 📄 Licence

Propriétaire - Tous droits réservés

---

**Application actuellement en cours d'exécution :**
- 🟢 Backend : https://localhost:7089
- 🟢 Frontend : http://localhost:5173